using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Message;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Models.Sync;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Db;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;
using Teru.Code.Services;

namespace JboxTransfer.Core.Modules.Sync
{
    public class SyncTaskCollection
    {
        private const int MaxErrorTaskCount = 99;
        private const int MaxCompletedTaskCount = 99;
        private const int MaxPendingTaskCount = 20;

        private List<IBaseSyncTask> ListCompleted { get; set; }
        private List<IBaseSyncTask> ListError { get; set; }
        private List<IBaseSyncTask> ListCurrent { get; set; }
        private int UserId { get; set; }
        private UserPreference Preference { get; set; }

        private object addTaskLock = new object();

        private LoopWorker checker;

        public bool IsBusy { get; set; }
        public bool IsTooManyError { get; set; }
        public bool HasMoreTasks { get; set; }
        public string Message { get; set; }

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SyncTaskCollection(SystemUser user, IServiceScopeFactory serviceScopeFactory)
        {
            UserId = user.Id;
            Preference = JsonConvert.DeserializeObject<UserPreference>(user.Preference);
            ListCompleted = new List<IBaseSyncTask>();
            ListError = new List<IBaseSyncTask>();
            ListCurrent = new List<IBaseSyncTask>();
            _serviceScopeFactory = serviceScopeFactory;

            checker = new LoopWorker();
            checker.Interval = 1000;
            checker.CanRun += () => true;
            checker.Go += Checker_Go;
            checker.StartRun();

            InitTasksFromDb();
        }

        #region Init
        private async Task InitTasksFromDb()
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

                await LoadErrorTasksFromDb(db);
                await LoadCompleteTasksFromDb(db);

                // 上次未完成直接被关闭的任务
                db.SyncTasks
                    .Where(db => db.UserId == UserId)
                    .Where(x => x.State == SyncTaskDbState.Busy || x.State == SyncTaskDbState.Pending)
                    .ExecuteUpdate(x => x.SetProperty(model => model.State, (_) => SyncTaskDbState.Idle));

                await LoadIdleTasksFromDb(db);
            }
        }

        private async Task LoadErrorTasksFromDb(DefaultDbContext db)
        {
            try
            {
                List<SyncTaskDbModel> items = await db.SyncTasks
                    .Where(x => x.UserId == UserId)
                    .Where(x => x.State == SyncTaskDbState.Error)
                    .OrderByDescending(x => x.UpdateTime)
                    .ToListAsync();

                if (items == null || items.Count == 0)
                    return;

                foreach (var item in items)
                {
                    IBaseSyncTask task2;
                    if (item.Type == SyncTaskType.File)
                    {
                        task2 = new FileSyncTask(_serviceScopeFactory);
                        task2.Init(item);
                    }
                    else
                    {
                        task2 = new FolderSyncTask(_serviceScopeFactory);
                        task2.Init(item);
                    }
                    ListError.Add(task2);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        private async Task LoadCompleteTasksFromDb(DefaultDbContext db)
        {
            try
            {
                List<SyncTaskDbModel> items = await db.SyncTasks
                    .Where(x => x.UserId == UserId)
                    .Where(x => x.State == SyncTaskDbState.Done)
                    .OrderByDescending(x => x.UpdateTime)
                    .ToListAsync();

                if (items == null || items.Count == 0)
                    return;

                foreach (var item in items)
                {
                    IBaseSyncTask task2;
                    if (item.Type == SyncTaskType.File)
                    {
                        task2 = new FileSyncTask(_serviceScopeFactory);
                        task2.Init(item);
                    }
                    else
                    {
                        task2 = new FolderSyncTask(_serviceScopeFactory);
                        task2.Init(item);
                    }
                    ListCompleted.Add(task2);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        private async Task LoadIdleTasksFromDb(DefaultDbContext db)
        {
            Monitor.Enter(addTaskLock);
            try
            {
                int availableCount = MaxPendingTaskCount - ListCurrent.Count;
                if (availableCount > 0)
                {
                    List<SyncTaskDbModel> items = await db.SyncTasks
                        .Where(x => x.UserId == UserId)
                        .Where(x => x.State == SyncTaskDbState.Idle)
                        .Include(x => x.User)
                        .OrderByDescending(x => x.UpdateTime) //Todo: 到底应该怎么排序
                        .OrderBy(x => x.Order)
                        .Take(availableCount)
                        .ToListAsync();

                    if (items == null || items.Count == 0)
                    {
                        HasMoreTasks = false;
                        return;
                    }
                    else if (items.Count == availableCount)
                    {
                        HasMoreTasks = true;
                    }
                    foreach (var item in items)
                    {
                        item.State = SyncTaskDbState.Pending;
                        item.UpdateTime = DateTime.Now;
                        db.Update(item);
                    }
                    await db.SaveChangesAsync();

                    foreach (var item in items)
                    {
                        AddToCurrentInternal(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            finally
            {
                Monitor.Exit(addTaskLock);
            }
        }

        private void AddToCurrentInternal(SyncTaskDbModel item, bool setTop = false)
        {
            IBaseSyncTask task2;
            if (item.Type == SyncTaskType.File)
            {
                task2 = new FileSyncTask(_serviceScopeFactory);
                task2.Init(item);
            }
            else
            {
                task2 = new FolderSyncTask(_serviceScopeFactory);
                task2.Init(item);
            }
            if (setTop)
                ListCurrent.Insert(0, task2);
            else
                ListCurrent.Add(task2);
        }
        #endregion

        #region update
        private TaskState Checker_Go(CancellationTokenSource cts)
        {
            try
            {
                UpdateList();
                if (IsBusy && !IsTooManyError)
                    UpdateStartNew();
            }
            catch( Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return TaskState.Started;
        }

        public void UpdateFromDb()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

                    LoadIdleTasksFromDb(db).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void UpdateStartNew()
        {
            while (ListCurrent.Count(
                x => x.State == SyncTaskState.Running ||
                x.State == SyncTaskState.Error ||
                x.State == SyncTaskState.Complete
                ) < Preference.ConcurrencyCount)
            {
                var x = ListCurrent.FirstOrDefault(x => (x.State == SyncTaskState.Wait || x.State == SyncTaskState.Pause) && x.IsUserPause == false);
                if (x == null)
                    return;
                if (x.State == SyncTaskState.Wait)
                    x.Start();
                else
                    x.Resume();
            }
        }

        private void UpdateList()
        {
            for (int i = ListCurrent.Count - 1; i >= 0; i--)
            {
                var task = ListCurrent[i];
                if (task.State == SyncTaskState.Complete)
                {
                    ListCurrent.Remove(task);
                    ListCompleted.Insert(0, task);
                }
                else if (task.State == SyncTaskState.Error)
                {
                    ListCurrent.Remove(task);
                    ListError.Insert(0, task);
                    CheckTooManyErrors();
                }
            }
            if (ListCompleted.Count > MaxCompletedTaskCount)
            {
                ListCompleted.RemoveRange(MaxCompletedTaskCount, ListCompleted.Count - MaxCompletedTaskCount);
            }
        }

        private void CheckTooManyErrors()
        {
            if (ListError.Count > MaxErrorTaskCount)
            {
                IsTooManyError = true;
                Message = "错误过多，队列被迫终止。请先处理出错的项目。";
            }
            else
            {
                IsTooManyError = false;
                Message = "";
            }
        }

        public void UpdatePreference(UserPreference preference)
        {
            Preference = preference;
        }
        #endregion

        #region State control
        public CommonResult StartAll()
        {
            if (IsTooManyError)
            {
                return new CommonResult(false, Message);
            }
            IsBusy = true;
            foreach (var task in ListCurrent)
            {
                task.IsUserPause = false;
            }
            return new CommonResult(true, "");
        }

        public CommonResult PauseAll()
        {
            IsBusy = false;
            foreach (var task in ListCurrent)
            {
                task.Pause();
                task.IsUserPause = true;
            }
            return new CommonResult(true, "");
        }

        public CommonResult CancelAll()
        {
            IsBusy = false;
            foreach (var task in ListCurrent)
            {
                task.Cancel();
            }
            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                db.SyncTasks
                    .Where(x => x.UserId == UserId)
                    .Where(x => x.State == SyncTaskDbState.Idle || x.State == SyncTaskDbState.Busy || x.State == SyncTaskDbState.Pending)
                    .ExecuteDelete();
            }
            ListCurrent.Clear();
            return new CommonResult(true, "");
        }

        public CommonResult RestartAllError(bool keepProgress)
        {
            try
            {
                for (int i = ListError.Count - 1; i >= 0; i--)
                {
                    var item = ListError[i];
                    item.Recover(true);
                    ListError.Remove(item);
                }
                CheckTooManyErrors();
                return new CommonResult(true, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult CancelAllError()
        {
            try
            {
                foreach (var task in ListError)
                {
                    task.Cancel();
                }
                ListError = new List<IBaseSyncTask>();
                CheckTooManyErrors();
                return new CommonResult(true, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }
        
        public CommonResult DeleteAllDone()
        {
            try
            {
                ListCompleted = new List<IBaseSyncTask>();
                return new CommonResult(true, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult StartOne(int syncTaskId)
        {
            if (IsTooManyError)
            {
                return new CommonResult(false, Message);
            }
            try
            {
                var task = ListCurrent.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    if (task.State == SyncTaskState.Pause)
                    {
                        task.Resume();
                        task.IsUserPause = false;
                    }
                    else if (task.State == SyncTaskState.Wait)
                        task.Start();
                    return new CommonResult(true, "");
                }
                else
                {
                    return new CommonResult(false, "找不到任务，请确保任务在传输列表中");
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }        
        
        public CommonResult PauseOne(int syncTaskId)
        {
            try
            {
                var task = ListCurrent.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    if (task.State == SyncTaskState.Running || task.State == SyncTaskState.Wait)
                    {
                        task.Pause();
                        task.IsUserPause = true;
                    }
                    return new CommonResult(true, "");
                }
                else
                {
                    return new CommonResult(false, "找不到任务，请确保任务在传输列表中");
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult CancelOne(int syncTaskId) // both in list or db supported
        {
            try
            {
                var task = ListCurrent.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    task.Cancel();
                    ListCurrent.Remove(task);
                    return new CommonResult(true, "");
                }
                else
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                        var taskDb = db.SyncTasks.FirstOrDefault(x => x.Id == syncTaskId);
                        if (taskDb != null)
                        {
                            taskDb.State = SyncTaskDbState.Cancel;
                            db.Update(taskDb);
                            int changed = db.SaveChanges(); //todo: 检查
                            return new CommonResult(true, "");
                        }
                        else
                        {
                            return new CommonResult(false, "找不到任务");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult RestartOneError(int syncTaskId, bool keepProgress)
        {
            try
            {
                var task = ListError.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    task.Recover(true);
                    ListError.Remove(task);
                    CheckTooManyErrors();
                    return new CommonResult(true, "");
                }
                else
                {
                    return new CommonResult(false, "找不到任务，请确保任务在错误列表中");
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        } 
        
        public CommonResult CancelOneError(int syncTaskId)
        {
            try
            {
                var task = ListError.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    task.Cancel();
                    ListError.Remove(task);
                    CheckTooManyErrors();
                    return new CommonResult(true, "");
                }
                else
                {
                    return new CommonResult(false, "找不到任务，请确保任务在错误列表中");
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public CommonResult SetTop(int syncTaskId) // both in list or db supported
        {
            try
            {
                var task = ListCurrent.FirstOrDefault(x => x.SyncTaskId == syncTaskId);
                if (task != null)
                {
                    ListCurrent.Remove(task);
                    ListCurrent.Insert(0, task);
                    return new CommonResult(true, "");
                }
                else
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                        var taskDb = db.SyncTasks.FirstOrDefault(x => x.Id == syncTaskId);
                        if (taskDb != null && taskDb.State == SyncTaskDbState.Idle)
                        {
                            taskDb.Order = db.GetMinOrder() - 1;
                            taskDb.State = SyncTaskDbState.Pending;
                            taskDb.UpdateTime = DateTime.Now;
                            db.Update(taskDb);
                            db.SaveChanges();

                            AddToCurrentInternal(taskDb, true); //todo: 和前面的冲突怎么办

                            return new CommonResult(true, "");
                        }
                        else
                        {
                            return new CommonResult(false, "找不到任务或任务状态冲突");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }
        #endregion

        #region Output
        public CommonResult<SyncTaskListOutputDto> GetCurrentListInfo()
        {
            List<SyncTaskOutputDto> list = new List<SyncTaskOutputDto>();
            foreach (var task in ListCurrent)
            {
                list.Add(new SyncTaskOutputDto()
                {
                    Id = task.SyncTaskId,
                    FileName = task.FileName,
                    FilePath = task.FilePath,
                    ParentPath = task.ParentPath,
                    Progress = task.Progress,
                    TotalBytes = task.TotalBytes,
                    DownloadedBytes = task.DownloadedBytes,
                    UploadedBytes = task.UploadedBytes,
                    State = task.State,
                    Message = task.Message,
                    Type = task.Type,
                });
            }
            var outputDto = new SyncTaskListOutputDto(list);
            outputDto.HasMore = this.HasMoreTasks;
            outputDto.IsTooManyError = this.IsTooManyError;
            outputDto.Message = this.Message;
            outputDto.RunningCount = this.ListCurrent.Count;
            outputDto.CompletedCount = this.ListCompleted.Count;
            outputDto.ErrorCount = this.ListError.Count;
            return new (true, "", outputDto);
        }        
        
        public CommonResult<SyncTaskListOutputDto> GetCompletedListInfo()
        {
            List<SyncTaskOutputDto> list = new List<SyncTaskOutputDto>();
            foreach (var task in ListCompleted)
            {
                list.Add(new SyncTaskOutputDto()
                {
                    Id = task.SyncTaskId,
                    FileName = task.FileName,
                    FilePath = task.FilePath,
                    ParentPath = task.ParentPath,
                    Progress = task.Progress,
                    TotalBytes = task.TotalBytes,
                    DownloadedBytes = task.DownloadedBytes,
                    UploadedBytes = task.UploadedBytes,
                    State = task.State,
                    Message = task.Message,
                    Type = task.Type,
                });
            }
            var outputDto = new SyncTaskListOutputDto(list);
            outputDto.HasMore = ListCompleted.Count >= MaxCompletedTaskCount;
            outputDto.RunningCount = this.ListCurrent.Count;
            outputDto.CompletedCount = this.ListCompleted.Count;
            outputDto.ErrorCount = this.ListError.Count;
            return new(true, "", outputDto);
        }

        public CommonResult<SyncTaskListOutputDto> GetErrorListInfo()
        {
            List<SyncTaskOutputDto> list = new List<SyncTaskOutputDto>();
            foreach (var task in ListError)
            {
                list.Add(new SyncTaskOutputDto()
                {
                    Id = task.SyncTaskId,
                    FileName = task.FileName,
                    FilePath = task.FilePath,
                    ParentPath = task.ParentPath,
                    Progress = task.Progress,
                    TotalBytes = task.TotalBytes,
                    DownloadedBytes = task.DownloadedBytes,
                    UploadedBytes = task.UploadedBytes,
                    State = task.State,
                    Message = task.Message,
                    Type = task.Type,
                });
            }
            var outputDto = new SyncTaskListOutputDto(list);
            outputDto.RunningCount = this.ListCurrent.Count;
            outputDto.CompletedCount = this.ListCompleted.Count;
            outputDto.ErrorCount = this.ListError.Count;
            return new(true, "", outputDto);
        }
        #endregion
    }
}
