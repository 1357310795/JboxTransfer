using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Output;
using JboxTransfer.Core.Models.Sync;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Db;
using JboxTransfer.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public List<IBaseSyncTask> ListCompleted { get; set; }
        public List<IBaseSyncTask> ListError { get; set; }
        public List<IBaseSyncTask> ListCurrent { get; set; }
        public SystemUser User { get; set; }

        private object addTaskLock = new object();

        LoopWorker checker;

        public bool IsBusy { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SyncTaskCollection(SystemUser user, IServiceScopeFactory serviceScopeFactory)
        {
            User = user;
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
                var userInfoProvider = scope.ServiceProvider.GetRequiredService<SystemUserInfoProvider>();
                userInfoProvider.SetUser(User);
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

                //await LoadPendingTasksFromDb(db);
                await LoadErrorTasksFromDb(db);
                await LoadCompleteTasksFromDb(db);

                // 上次未完成直接被关闭的的任务
                db.SyncTasks
                    .Where(db => db.UserId == User.Id)
                    .Where(x => x.State == SyncTaskDbState.Busy)
                    .ExecuteUpdate(x => x.SetProperty(model => model.State, (_) => SyncTaskDbState.Idle));
            }
        }

        private async Task LoadErrorTasksFromDb(DefaultDbContext db)
        {
            try
            {
                List<SyncTaskDbModel> items = await db.SyncTasks
                    .Where(x => x.UserId == User.Id)
                    .Where(x => x.State == SyncTaskDbState.Error)
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
                    .Where(x => x.UserId == User.Id)
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

        private async Task LoadPendingTasksFromDb(DefaultDbContext db)
        {
            Monitor.Enter(addTaskLock);
            try
            {
                if (ListCurrent.Count < 99)
                {
                    List<SyncTaskDbModel> items = await db.SyncTasks
                        .Where(x => x.UserId == User.Id)
                        .Where(x => x.State == SyncTaskDbState.Idle)
                        .Include(x => x.User)
                        .OrderBy(x => x.Order)
                        .Take(99 - ListCurrent.Count)
                        .ToListAsync();

                    if (items == null || items.Count == 0)
                        return;
                    foreach (var item in items)
                    {
                        item.State = SyncTaskDbState.Busy;
                        db.Update(item);
                    }
                    await db.SaveChangesAsync();

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
                        ListCurrent.Add(task2);
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
        #endregion

        #region Background daemon
        private TaskState Checker_Go(CancellationTokenSource cts)
        {
            try
            {
                UpdateList();
                if (IsBusy)
                    UpdateStartNew();
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var userInfoProvider = scope.ServiceProvider.GetRequiredService<SystemUserInfoProvider>();
                    userInfoProvider.SetUser(User);
                    var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

                    LoadPendingTasksFromDb(db).GetAwaiter().GetResult();
                }
                return TaskState.Started;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return TaskState.Started;
            }
        }

        private void UpdateStartNew()
        {
            while (ListCurrent.Count(
                x => x.State == SyncTaskState.Running ||
                x.State == SyncTaskState.Error ||
                x.State == SyncTaskState.Complete
                ) < GlobalConfigService.Config.TaskConfig.ThreadCount)
            {
                Debug.WriteLine("begin start new");
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
        }

        private void CheckTooManyErrors()
        {
            if (ListError.Count > 99)
            {
                //ErrorNum = "99+";
                IsBusy = false;
                Message = "错误过多，队列被迫终止。请先处理出错的项目。";
            }
        }
        #endregion

        #region State control
        public CommonResult StartAll()
        {
            if (ListError.Count > 99)
            {
                return new CommonResult(false, "错误过多，无法启动队列。请先前往“已停止”选项卡处理出错的项目。");
            }
            IsBusy = true;
            var t = GlobalConfigService.Config.TaskConfig.ThreadCount;
            foreach (var task in ListCurrent)
            {
                task.IsUserPause = false;
            }
            //foreach (var task in ListCurrent)
            //{
            //    t--;
            //    if (task.State == SyncTaskState.Running)
            //        continue;
            //    task.Resume();
            //    if (t == 0)
            //        break;
            //}
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
                db.SyncTasks.Where(x => x.UserId == User.Id).ExecuteDelete();
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
                return new CommonResult(true, "");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }
        
        public CommonResult DeleteAllComplete()
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
        #endregion

        #region Output
        public CommonResult<List<SyncTaskOutputDto>> GetCurrentListInfo()
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
                    state = task.State
                });
            }
            return new CommonResult<List<SyncTaskOutputDto>>(true, "", list);
        }
        #endregion
    }
}
