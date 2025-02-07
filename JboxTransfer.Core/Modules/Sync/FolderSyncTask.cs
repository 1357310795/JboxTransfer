using JboxTransfer.Core.Models.Jbox;
using System.Diagnostics;
using JboxTransfer.Core.Modules.Jbox;
using JboxTransfer.Core.Modules.Tbox;
using Microsoft.Extensions.DependencyInjection;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Sync;
using JboxTransfer.Core.Modules.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace JboxTransfer.Core.Modules.Sync
{
    public class FolderSyncTask : IBaseSyncTask
    {
        private const int RetryTimes = 3;
        private string path;
        private Exception ex;
        private int total;
        private int succ;
        private int page;
        private JboxItemInfo info;
        public SyncTaskState State { get; set; }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public int SyncTaskId { get; private set; }

        public PauseTokenSource pts;

        public double Progress
        {
            get
            {
                if (total == 0) return 0;
                return succ / (double)total;
            }
        }

        public long TotalBytes => total;
        public long DownloadedBytes => succ;
        public long UploadedBytes => succ;

        public string FileName => GetName();
        public string FilePath => GetPath();
        public string ParentPath => GetParentPath();
        public SyncTaskType Type => SyncTaskType.Folder;

        public bool IsUserPause { get; set; }

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FolderSyncTask(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        //Only for debug
        public void Init(string path, string hash, long size)
        {
            this.path = path;
            page = 0;
            total = succ = 0;
            State = SyncTaskState.Wait;
            pts = new PauseTokenSource();
        }

        public void Init(SyncTaskDbModel dbModel)
        {
            this.SyncTaskId = dbModel.Id;
            if (dbModel.RemainParts == null)
            {
                page = 0;
                succ = 0;
            }
            else
            {
                page = int.Parse(dbModel.RemainParts);
                succ = page * 50;
            }
            total = (int)dbModel.Size;
            path = dbModel.FilePath;
            State = dbModel.State == SyncTaskDbState.Error ? SyncTaskState.Error : SyncTaskState.Wait;
            this.Message = dbModel.Message;
            pts = new PauseTokenSource();
        }

        public string GetName()
        {
            var name = path.Split('/').Last();
            return name == "" ? "根目录" : name;
        }

        public string GetPath()
        {
            return path;
        }

        public string GetParentPath()
        {
            var s = path.Split('/');
            var p = string.Join("/", s.Take(s.Length - 1));
            return p == "" ? "/" : p;
        }

        public string GetProgressStr()
        {
            return $"{succ} / {total}";
        }

        public string GetProgressTextTooltip()
        {
            return $"""
                已处理的子文件/文件夹：{succ}
                总数：{total}
                """;
        }

        public void Start()
        {
            State = SyncTaskState.Running;
            Task.Run(async () => { await internalStartWrap(pts); });
        }

        public void Pause()
        {
            pts.Pause();
            State = SyncTaskState.Pause;
        }

        public void Resume()
        {
            if (State != SyncTaskState.Pause && State != SyncTaskState.Error)
                return;
            pts = new PauseTokenSource();
            pts.Resume();
            State = SyncTaskState.Running;
            Task.Run(async () => { await internalStartWrap(pts); });
        }

        public void Cancel()
        {
            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                pts.Pause();
                State = SyncTaskState.Wait;
                db.SyncTasks
                    .Where(x => x.Id == SyncTaskId)
                    .ExecuteUpdate(call => call
                    .SetProperty(x => x.State, x => SyncTaskDbState.Cancel)
                    .SetProperty(x => x.UpdateTime, x => DateTime.Now)
                    .SetProperty(x => x.Message, x => "已取消"));
                Message = "已取消";
            }
        }

        public void Recover(bool keepProgress)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                if (keepProgress)
                {
                    db.SyncTasks
                        .Where(x => x.Id == SyncTaskId)
                        .ExecuteUpdate(call => call
                        .SetProperty(x => x.State, x => SyncTaskDbState.Idle)
                        .SetProperty(x => x.UpdateTime, x => DateTime.Now)
                        .SetProperty(x => x.Message, x => null));
                }
                else
                {
                    db.SyncTasks
                        .Where(x => x.Id == SyncTaskId)
                        .ExecuteUpdate(call => call
                        .SetProperty(x => x.State, x => SyncTaskDbState.Idle)
                        .SetProperty(x => x.UpdateTime, x => DateTime.Now)
                        .SetProperty(x => x.ConfirmKey, x => null)
                        .SetProperty(x => x.RemainParts, x => null)
                        .SetProperty(x => x.Message, x => null));
                }
            }
        }

        private async Task internalStartWrap(PauseTokenSource inst_pts)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

                try
                {
                    Monitor.Enter(this);
                    if (inst_pts.IsPaused)
                    {
                        State = SyncTaskState.Pause;
                        return;
                    }
                    await internalStart(scope, pts);
                    if (inst_pts.IsPaused)
                    {
                        State = SyncTaskState.Pause;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    //log
                    Debug.WriteLine(ex);
                    Message = $"{ex.Message}";
                    State = SyncTaskState.Error;
                    db.SyncTasks
                        .Where(x => x.Id == SyncTaskId)
                        .ExecuteUpdate(call => call
                        .SetProperty(x => x.State, x => SyncTaskDbState.Error)
                        .SetProperty(x => x.UpdateTime, x => DateTime.Now)
                        .SetProperty(x => x.Message, x => Message));
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public async Task internalStart(AsyncServiceScope scope, PauseTokenSource inst_pts)
        {
            var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            var dbModel = db.SyncTasks
                .Include(x => x.User)
                .Where(x => x.Id == SyncTaskId)
                .First();
            var user = scope.ServiceProvider.GetRequiredService<SystemUserInfoProvider>();
            user.SetUser(dbModel.User);
            var tbox = scope.ServiceProvider.GetRequiredService<TboxService>();
            var jbox = scope.ServiceProvider.GetRequiredService<JboxService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FolderSyncTask>>();

            State = SyncTaskState.Running;

            if (inst_pts.IsPaused)
                return;

            //Create Folder in tbox
            var res0 = tbox.CreateDirectory(path);
            if (!res0.Success)
            {
                if (res0.Result == null || res0.Result.Code != "SameNameDirectoryOrFileExists" && res0.Result.Code != "RootDirectoryNotAllowed")
                {
                    State = SyncTaskState.Error;
                    Message = $"创建文件夹失败：{res0.Result.Message}";
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.UpdateTime = DateTime.Now;
                    dbModel.Message = Message;
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }
            }

            while (true)
            {
                var t = RetryTimes;
                while (t-- > 0)
                {
                    try
                    {
                        if (inst_pts.IsPaused)
                            return;

                        var res = jbox.GetJboxFolderInfo(path, page);
                        info = res.Result;

                        if (!res.Success)
                            throw new Exception($"获取文件夹信息失败：{res.Message}");

                        if (info.Content.Length == 0)
                            break;

                        Monitor.Enter(db.insertLock);
                        if (inst_pts.IsPaused)
                        {
                            Monitor.Exit(db.insertLock);
                            return;
                        }
                        var order = db.GetMinOrder() - 1;

                        //db.ChangeTracker.DetectChanges();
                        //Console.WriteLine(db.ChangeTracker.DebugView.LongView);
                        //Console.WriteLine("---------------------------------------");

                        //EF Core 的 SaveChanges 本身就是事务
                        foreach (var item in info.Content.Where(x => x.IsDir == true))
                            db.Add(new SyncTaskDbModel(user.GetUser(), SyncTaskType.Folder, item.Path, 0, order));
                        foreach (var item in info.Content.Where(x => x.IsDir == false))
                            db.Add(new SyncTaskDbModel(user.GetUser(), SyncTaskType.File, item.Path, item.Bytes, order) { MD5_Ori = item.Hash });

                        succ += info.Content.Length;
                        total = (int)info.ContentSize;
                        dbModel.Size = total;
                        dbModel.RemainParts = page.ToString();
                        dbModel.UpdateTime = DateTime.Now;
                        db.Update(dbModel);

                        //db.ChangeTracker.DetectChanges();
                        //Console.WriteLine(db.ChangeTracker.DebugView.LongView);
                        db.SaveChanges();

                        Monitor.Exit(db.insertLock);

                        page++;
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"同步文件夹出错：{ex}");
                        this.ex = ex;
                    }
                }
                if (t <= 0)
                {
                    State = SyncTaskState.Error;
                    Message = ex.Message;
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.UpdateTime = DateTime.Now;
                    dbModel.Message = Message;
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }
                if (info.Content.Length == 0)
                    break;
            }
            State = SyncTaskState.Complete;
            Message = "同步完成";
            dbModel.State = SyncTaskDbState.Done;
            dbModel.UpdateTime = DateTime.Now;
            dbModel.Message = Message;
            db.Update(dbModel);
            db.SaveChanges();
        }
    }
}
