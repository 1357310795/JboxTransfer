using JboxTransfer.Core.Extensions;
using JboxTransfer.Core.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Teru.Code.Models;
using JboxTransfer.Core.Modules.Tbox;
using JboxTransfer.Core.Modules.Jbox;
using Microsoft.Extensions.DependencyInjection;
using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Sync;
using JboxTransfer.Core.Modules.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JboxTransfer.Core.Models.Message;
using MassTransit;
using Nito.AsyncEx;
using JboxTransfer.Core.Models.Exceptions;
using System.Net;

namespace JboxTransfer.Core.Modules.Sync
{
    public class FileSyncTask : IBaseSyncTask
    {
        public const long ChunkSize = 4 * 1024 * 1024;
        private const int RetryTimes = 10;
        private string path;
        private string jboxhash;
        private long size;
        private TboxUploadPartSession curChunk;
        private int chunkCount;
        private int succChunk;
        private Exception ex;
        public SyncTaskState State { get; set; }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private SyncTaskErrorCause errorCause;
        public SyncTaskErrorCause ErrorCause
        {
            get { return errorCause; }
            set { errorCause = value; }
        }

        public int ChunkFactor { get; set; }

        public bool IsUserPause { get; set; }

        private JboxDownloadSession? jbox;
        private TboxUploadSession? tbox;

        public int SyncTaskId { get; private set; }
        public int UserId { get; private set; }

        private CRC64 crc64;
        private MD5 md5;

        private AsyncLock _taskLock = new AsyncLock();

        public PauseTokenSource pts;

        public double Progress
        {
            get
            {
                if (size == 0) return 0;
                return ((double)DownloadedBytes + (double)UploadedBytes) / (2*(double)TotalBytes); 
            }
        }

        public long TotalBytes => size;
        public long DownloadedBytes => succChunk == chunkCount ? size : (succChunk * ChunkSize + (jbox?.Progress ?? 0));
        public long UploadedBytes => succChunk == chunkCount ? size : (succChunk * ChunkSize + (tbox?.Progress ?? 0));

        public string FileName => GetName();
        public string FilePath => GetPath();
        public string ParentPath => GetParentPath();
        public SyncTaskType Type => SyncTaskType.File;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FileSyncTask(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            pts = new PauseTokenSource();
        }

        public void Init(SyncTaskDbModel dbModel)
        {
            this.SyncTaskId = dbModel.Id;
            this.UserId = dbModel.UserId;
            this.path = dbModel.FilePath;
            this.jboxhash = dbModel.MD5_Ori;
            this.size = dbModel.Size;
            this.Message = dbModel.Message;
            this.ErrorCause = dbModel.ErrorCause;
            this.ChunkFactor = dbModel.ChunkFactor;
            this.chunkCount = size.GetChunkCount();

            if (dbModel.ConfirmKey == null)
            {
                crc64 = new CRC64();
                md5 = new MD5();
                this.succChunk = 0;
            }
            else
            {
                var remain = JsonConvert.DeserializeObject<List<int>>(dbModel.RemainParts);
                crc64 = CRC64.FromValue((ulong)dbModel.CRC64_Part);
                md5 = MD5.Create(JsonConvert.DeserializeObject<MD5StateStorage>(dbModel.MD5_Part));
                this.succChunk = this.chunkCount - remain.Count;
            }
            State = dbModel.State == SyncTaskDbState.Error ? SyncTaskState.Error : SyncTaskState.Wait;
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
            var down = (succChunk == chunkCount ? size : (succChunk * ChunkSize + jbox.Progress)).PrettyPrint();
            var up = (succChunk == chunkCount ? size : (succChunk * ChunkSize + tbox.Progress)).PrettyPrint();
            var all = size.PrettyPrint();
            return $"{down} / {up} / {all}";
        }

        public string GetProgressTextTooltip()
        {
            var down = (succChunk == chunkCount ? size : (succChunk * ChunkSize + jbox.Progress));
            var up = (succChunk == chunkCount ? size : (succChunk * ChunkSize + tbox.Progress));
            var all = size;
            return $"""
                已下载：{down.PrettyPrint()} ({down} bytes)
                已上传：{up.PrettyPrint()} ({up} bytes)
                总大小：{all.PrettyPrint()} ({all} bytes)
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
            if (curChunk != null)
                curChunk.Uploading = false;
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
            Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                    pts.Pause();
                    State = SyncTaskState.Wait;
                    if (curChunk != null)
                        curChunk.Uploading = false;
                    db.SyncTasks
                        .Where(x => x.Id == SyncTaskId)
                        .ExecuteUpdate(call => call
                        .SetProperty(x => x.State, x => SyncTaskDbState.Cancel)
                        .SetProperty(x => x.UpdateTime, x => DateTime.Now)
                        .SetProperty(x => x.Message, x => "已取消"));
                    Message = "已取消";

                    ISendEndpointProvider sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
                    var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:add_task_from_db"));
                    await endpoint.Send(new NewTaskCheckMessage() { UserId = this.UserId });
                }
            });
        }

        public void Recover(bool keepProgress)
        {
            Task.Run(async () =>
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

                    ISendEndpointProvider sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
                    var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:add_task_from_db"));
                    await endpoint.Send(new NewTaskCheckMessage() { UserId = this.UserId });
                }
            });
        }

        private async Task internalStartWrap(PauseTokenSource inst_pts)
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                try
                {
                    _taskLock.Lock();
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
                catch(Exception ex)
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
                    _taskLock.ReleaseLock();
                    ISendEndpointProvider sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
                    var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:add_task_from_db"));
                    await endpoint.Send(new NewTaskCheckMessage() { UserId = this.UserId });
                }
            }
        }

        private async Task internalStart(AsyncServiceScope scope, PauseTokenSource pts)
        {
            MemoryStream chunkRes = null;
            CancellationToken ct = pts.CurrentCancellationToken;

            var db = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            var dbModel = db.SyncTasks
                .Include(x => x.User)
                .Where(x => x.Id == SyncTaskId)
                .First();
            var userInfoProvider = scope.ServiceProvider.GetRequiredService<SystemUserInfoProvider>();
            userInfoProvider.SetUser(dbModel.User);
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileSyncTask>>();

            if (jbox == null)
            {
                jbox = scope.ServiceProvider.GetRequiredService<JboxDownloadSession>();
                jbox.Init(path, size);
            }
            if (tbox == null)
            {
                if (dbModel.ConfirmKey == null)
                {
                    tbox = scope.ServiceProvider.GetRequiredService<TboxUploadSession>();
                    tbox.Init(path, size);
                }
                else
                {
                    tbox = scope.ServiceProvider.GetRequiredService<TboxUploadSession>();
                    tbox.Init(path, size, dbModel.ConfirmKey, JsonConvert.DeserializeObject<List<int>>(dbModel.RemainParts));
                }
            }
            UserPreference preference = JsonConvert.DeserializeObject<UserPreference>(dbModel.User.Preference);

            State = SyncTaskState.Running;

            dbModel.State = SyncTaskDbState.Busy;
            dbModel.Message = "";
            dbModel.ErrorCause = SyncTaskErrorCause.None;
            dbModel.UpdateTime = DateTime.Now;
            db.Update(dbModel);
            db.SaveChanges();

            if (pts.IsPaused) 
                return;

            var res0 = tbox.EnsureDirectoryExists(ct);
            if (!res0.success)
            {
                if (ct.IsCancellationRequested) return;
                State = SyncTaskState.Error;
                dbModel.State = SyncTaskDbState.Error;
                dbModel.Message = Message = res0.result;
                dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.Tbox;
                dbModel.UpdateTime = DateTime.Now;
                db.Update(dbModel);
                db.SaveChanges();
                return;
            }

            if (pts.IsPaused)
                return;

            var res1 = tbox.PrepareForUpload(ct);
            if (!res1.Success)
            {
                if (ct.IsCancellationRequested) return;
                State = SyncTaskState.Error;
                dbModel.State = SyncTaskDbState.Error;
                dbModel.UpdateTime = DateTime.Now;
                dbModel.Message = Message = res1.Message;
                dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.Tbox;
                db.Update(dbModel);
                db.SaveChanges();
                return;
            }

            if (pts.IsPaused)
                return;

            var res2 = tbox.GetNextPartNumber();
            if (!res2.Success)
            {
                if (ct.IsCancellationRequested) return;
                State = SyncTaskState.Error;
                dbModel.State = SyncTaskDbState.Error;
                dbModel.UpdateTime = DateTime.Now;
                dbModel.Message = Message = res2.Message;
                dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.Tbox;
                db.Update(dbModel);
                db.SaveChanges();
                return;
            }
            curChunk = res2.Result;

            if (pts.IsPaused)
                return;

            dbModel.ConfirmKey = tbox.ConfirmKey;
            dbModel.RemainParts = JsonConvert.SerializeObject(tbox.RemainParts.Select(x => x.PartNumber));
            dbModel.CRC64_Part = (long)crc64.GetValue();
            dbModel.MD5_Part = JsonConvert.SerializeObject(md5.GetValue());
            dbModel.UpdateTime = DateTime.Now;
            db.Update(dbModel);
            db.SaveChanges();

            while (curChunk.PartNumber != 0)
            {
                int t = RetryTimes;
                jbox.ClearProgress();
                tbox.ClearProgress();
                while (t-- > 0)
                {
                    try
                    {
                        if (pts.IsPaused)
                            return;

                        var res3 = tbox.EnsureNoExpire(curChunk.PartNumber, ct);
                        if (!res3.success)
                            throw new TboxException($"{res3.result}");

                        if (pts.IsPaused)
                            return;

                        try
                        {
                            chunkRes = await jbox.GetChunk(curChunk.PartNumber, ct);
                        }
                        catch (TimeoutException ex)
                        {
                            throw new JboxException($"下载块 {curChunk.PartNumber} 发生错误：{ex.Message}【服务繁忙，请于空闲时段重试】");
                        }                        
                        catch (TaskCanceledException ex)
                        {
                            throw new JboxException($"下载块 {curChunk.PartNumber} 发生错误：{ex.Message}【服务繁忙，请于空闲时段重试】");
                        }                        
                        catch (UnauthorizedAccessException ex)
                        {
                            throw new UnauthorizedAccessException($"【认证失败，请尝试重启 JboxTransfer】");
                        }                     
                        catch (WebException ex)
                        {
                            throw new WebException($"下载块 {curChunk.PartNumber} 发生错误：{ex.Message}【请检查您的网络连接】");
                        }
                        catch
                        {
                            throw;
                        }

                        if (pts.IsPaused)
                            return;

                        chunkRes.Position = 0;
                        var res4 = tbox.Upload(chunkRes, curChunk.PartNumber, ct);
                        if (res4.success)
                            break;
                        else
                            throw new TboxException($"上传块 {curChunk.PartNumber} 发生错误：{res3.result}");
                    }
                    catch (Exception ex)
                    {
                        if (ct.IsCancellationRequested) return;
                        logger.LogWarning($"同步文件出错：{ex}");
                        this.ex = ex;
                        await Task.Delay(100, ct);
                    }
                }
                if (pts.IsPaused)
                    return;
                if (t <= 0)
                {
                    tbox.ResetPartNumber(curChunk);
                    State = SyncTaskState.Error;
                    chunkRes = null;
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.UpdateTime = DateTime.Now;
                    dbModel.Message = Message = ex.Message;
                    dbModel.ErrorCause = ErrorCause = (ex.GetType().Name) switch {
                        nameof(JboxException) => SyncTaskErrorCause.Jbox,
                        nameof(TboxException) => SyncTaskErrorCause.Tbox,
                        nameof(WebException) => SyncTaskErrorCause.LocalNetwork,
                        nameof(UnauthorizedAccessException) => SyncTaskErrorCause.Auth,
                        _ => SyncTaskErrorCause.Other,
                    };
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }

                chunkRes.Position = 0;
                if (chunkRes.Length > 0)
                {
                    var sha256 = HashHelper.SHA256Hash(chunkRes);
                    md5.MD5Hash_Proc(Encoding.Default.GetBytes((curChunk.PartNumber == 1 ? "" : ",") + sha256));
                }
                crc64.TransformBlock(chunkRes.ToArray(), 0, (int)chunkRes.Length);
                tbox.CompletePart(curChunk);
                succChunk++;
                jbox.ClearProgress();
                tbox.ClearProgress();
                dbModel.CRC64_Part = (long)crc64.GetValue();
                dbModel.MD5_Part = JsonConvert.SerializeObject(md5.GetValue());
                dbModel.RemainParts = JsonConvert.SerializeObject(tbox.RemainParts.Select(x=>x.PartNumber));
                dbModel.UpdateTime = DateTime.Now;
                db.Update(dbModel);
                db.SaveChanges();

                if (pts.IsPaused)
                    return;

                res2 = tbox.GetNextPartNumber();
                if (!res2.Success)
                {
                    if (ct.IsCancellationRequested) return;
                    State = SyncTaskState.Error;
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.Message = Message = $"获取下一分块发生错误，当前分块为 {res2.Message}";
                    dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.Tbox;
                    dbModel.UpdateTime = DateTime.Now;
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }
                curChunk = res2.Result;
            }
            chunkRes = null;

            var actualHash_bytes = md5.MD5Hash_Finish();
            StringBuilder sub = new StringBuilder();
            foreach (var t in actualHash_bytes)
            {
                sub.Append(t.ToString("x2"));
            }
            var actualHash = sub.ToString();
            var actualcrc64 = crc64.TransformFinalBlock();

            if (succChunk == chunkCount && curChunk.PartNumber == 0)//&& 
            {
                if (jboxhash != actualHash)
                {
                    State = SyncTaskState.Error;
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.UpdateTime = DateTime.Now;
                    dbModel.Message = Message = $"下载流校验值不匹配";
                    dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.DownHash;
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }
                var res4 = tbox.Confirm(actualcrc64, preference.ConflictResolutionStrategy, ct);
                if (!res4.Success)
                {
                    if (ct.IsCancellationRequested) return;
                    State = SyncTaskState.Error;
                    dbModel.State = SyncTaskDbState.Error;
                    dbModel.UpdateTime = DateTime.Now;
                    dbModel.Message = Message = $"{res4.Message}";
                    dbModel.ErrorCause = ErrorCause = 
                        (res4.Message.Contains("different crc64 value") ? SyncTaskErrorCause.UpHash : SyncTaskErrorCause.Tbox);
                    db.Update(dbModel);
                    db.SaveChanges();
                    return;
                }
                State = SyncTaskState.Complete;
                dbModel.Message = Message = "同步完成";
                dbModel.State = SyncTaskDbState.Done;
                dbModel.UpdateTime = DateTime.Now;
                dbModel.ErrorCause = ErrorCause = SyncTaskErrorCause.None;
                db.Update(dbModel);
                db.SaveChanges();

                db.UserStats
                    .Where(x => x.UserId == UserId)
                    .ExecuteUpdate(call => call
                    .SetProperty(x => x.TotalTransferredBytes, x => x.TotalTransferredBytes + dbModel.Size));
            }
        }
    }
}
