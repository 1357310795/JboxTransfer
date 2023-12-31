﻿using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using JboxTransfer.Extensions;
using JboxTransfer.Models;
using JboxTransfer.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Text;
using Teru.Code.Models;
using MD5 = JboxTransfer.Core.Modules.MD5;

namespace JboxTransfer.Modules.Sync
{
    public class FileSyncTask : IBaseTask
    {
        public const long ChunkSize = 4 * 1024 * 1024;
        private const int RetryTimes = 3;
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
            set { message = value; dbModel.Message = value; }
        }


        private JboxDownloadSession jbox;
        private TboxUploadSession tbox;

        private SyncTaskDbModel dbModel;

        private CRC64 crc64;
        private MD5 md5;

        public PauseTokenSource pts;

        public double Progress
        {
            get
            {
                if (size == 0) return 0;
                var up = (succChunk == chunkCount ? size : (succChunk * ChunkSize + tbox.Progress));
                var all = size;
                return (double)up / (double)all; 
            }
        }

        //public FileSyncTask(string path, string hash, long size) {
        //    this.path = path;
        //    this.jboxhash = hash;
        //    this.size = size;
        //    this.succChunk = 0;
        //    this.chunkCount = size.GetChunkCount();
        //    jbox = new JboxDownloadSession(path, size);
        //    tbox = new TboxUploadSession(path, size);
        //    State = SyncTaskState.Wait;
        //    md5 = new MD5();
        //    crc64 = new CRC64();
        //    pts = new PauseTokenSource();
        //}

        public FileSyncTask(SyncTaskDbModel dbModel)
        {
            this.dbModel = dbModel;
            this.path = dbModel.FilePath;
            this.jboxhash = dbModel.MD5_Ori;
            this.size = dbModel.Size;
            this.chunkCount = size.GetChunkCount();
            jbox = new JboxDownloadSession(path, size);

            if (dbModel.ConfirmKey == null)
            {
                tbox = new TboxUploadSession(path, size);
                crc64 = new CRC64();
                md5 = new MD5();
                this.succChunk = 0;
            }
            else
            {
                var remain = JsonConvert.DeserializeObject<List<int>>(dbModel.RemainParts);
                tbox = new TboxUploadSession(path, size, dbModel.ConfirmKey, remain);
                crc64 = CRC64.FromValue((ulong)dbModel.CRC64_Part);
                md5 = MD5.Create(JsonConvert.DeserializeObject<MD5StateStorage>(dbModel.MD5_Part));
                this.succChunk = this.chunkCount - remain.Count;
            }
            State = dbModel.State == 2 ? SyncTaskState.Error : SyncTaskState.Wait;
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
            Task.Run(() => { internalStartWrap(pts); });
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
            Task.Run(() => { internalStartWrap(pts); });
        }

        public void Cancel()
        {
            pts.Pause();
            State = SyncTaskState.Wait;
            if (curChunk != null)
                curChunk.Uploading = false;
            dbModel.State = 4;
            Message = "已取消";
            DbService.db.Update(dbModel);
        }

        public void Recover(bool keepProgress)
        {
            if (keepProgress)
            {
                dbModel.State = 0;
                DbService.db.Update(dbModel);
            }
            else
            {
                dbModel.ConfirmKey = null;
                dbModel.State = 0;
                dbModel.RemainParts = null;
                DbService.db.Update(dbModel);
            }
        }

        private void internalStartWrap(PauseTokenSource inst_pts)
        {
            try
            {
                Monitor.Enter(this);
                if (inst_pts.IsPaused)
                {
                    Monitor.Exit(this);
                    State = SyncTaskState.Pause;
                    return;
                }
                internalStart(pts);
                if (inst_pts.IsPaused)
                {
                    Monitor.Exit(this);
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
                dbModel.State = 2;
                DbService.db.Update(dbModel);
            }
            Monitor.Exit(this);
        }

        private void internalStart(PauseTokenSource inst_pts)
        {
            CommonResult<MemoryStream> chunkRes = null;
            State = SyncTaskState.Running;
            
            if (inst_pts.IsPaused) 
                return;

            var res0 = tbox.EnsureDirectoryExists();
            if (!res0.success)
            {
                State = SyncTaskState.Error;
                dbModel.State = 2;
                Message = res0.result;
                DbService.db.Update(dbModel);
                return;
            }

            if (inst_pts.IsPaused)
                return;

            var res1 = tbox.PrepareForUpload();
            if (!res1.Success)
            {
                State = SyncTaskState.Error;
                dbModel.State = 2;
                Message = res1.Message;
                DbService.db.Update(dbModel);
                return;
            }

            if (inst_pts.IsPaused)
                return;

            var res2 = tbox.GetNextPartNumber();
            if (!res2.Success)
            {
                State = SyncTaskState.Error;
                dbModel.State = 2;
                Message = res2.Message;
                DbService.db.Update(dbModel);
                return;
            }
            curChunk = res2.Result;

            if (inst_pts.IsPaused)
                return;

            dbModel.ConfirmKey = tbox.ConfirmKey;
            dbModel.RemainParts = JsonConvert.SerializeObject(tbox.RemainParts.Select(x => x.PartNumber));
            dbModel.CRC64_Part = (long)crc64.GetValue();
            dbModel.MD5_Part = JsonConvert.SerializeObject(md5.GetValue());
            DbService.db.Update(dbModel);

            while (curChunk.PartNumber != 0)
            {
                int t = RetryTimes;
                jbox.ClearProgress();
                tbox.ClearProgress();
                while (t-- > 0)
                {
                    try
                    {
                        if (inst_pts.IsPaused)
                            return;

                        var res3 = tbox.EnsureNoExpire(curChunk.PartNumber);
                        if (!res3.success)
                            throw new Exception($"{res3.result}");

                        if (inst_pts.IsPaused)
                            return;

                        chunkRes = jbox.GetChunk(curChunk.PartNumber);
                        if (!chunkRes.Success)
                            throw new Exception($"下载块 {curChunk.PartNumber} 发生错误：{chunkRes.Message}");

                        if (inst_pts.IsPaused)
                            return;

                        chunkRes.Result.Position = 0;
                        var res4 = tbox.Upload(chunkRes.Result, curChunk.PartNumber);
                        if (res4.success)
                            break;
                        else
                            throw new Exception($"上传块 {curChunk.PartNumber} 发生错误：{res3.result}");
                    }
                    catch (Exception ex)
                    {
                        this.ex = ex;
                    }
                }
                if (inst_pts.IsPaused)
                    return;
                if (t <= 0)
                {
                    tbox.ResetPartNumber(curChunk);
                    State = SyncTaskState.Error;
                    chunkRes = null;
                    dbModel.State = 2;
                    Message = ex.Message;
                    DbService.db.Update(dbModel);
                    return;
                }

                chunkRes.Result.Position = 0;
                if (chunkRes.Result.Length > 0)
                {
                    var sha256 = HashHelper.SHA256Hash(chunkRes.Result);
                    md5.MD5Hash_Proc(Encoding.Default.GetBytes((curChunk.PartNumber == 1 ? "" : ",") + sha256));
                }
                crc64.TransformBlock(chunkRes.Result.ToArray(), 0, (int)chunkRes.Result.Length);
                tbox.CompletePart(curChunk);
                succChunk++;
                dbModel.CRC64_Part = (long)crc64.GetValue();
                dbModel.MD5_Part = JsonConvert.SerializeObject(md5.GetValue());
                dbModel.RemainParts = JsonConvert.SerializeObject(tbox.RemainParts.Select(x=>x.PartNumber));
                DbService.db.Update(dbModel);

                if (inst_pts.IsPaused)
                    return;

                res2 = tbox.GetNextPartNumber();
                if (!res2.Success)
                {
                    Message = $"获取下一分块发生错误，当前分块为 {res2.Message}";
                    State = SyncTaskState.Error;
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
                    Message = $"下载流校验值不匹配";
                    State = SyncTaskState.Error;
                    dbModel.State = 2;
                    DbService.db.Update(dbModel);
                    return;
                }
                var res4 = tbox.Confirm(actualcrc64);
                if (!res4.Success)
                {
                    Message = $"{res4.Message}";
                    State = SyncTaskState.Error;
                    dbModel.State = 2;
                    DbService.db.Update(dbModel);
                    return;
                }
                dbModel.State = 3;
                Message = "同步完成";
                State = SyncTaskState.Complete;
                DbService.db.Update(dbModel);
            }
        }

    }
}
