using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using JboxTransfer.Extensions;
using JboxTransfer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;

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
        private CommonResult<MemoryStream> chunkRes;
        public SyncTaskState State { get; set; }
        public string Message { get; set; }

        private JboxDownloadSession jbox;
        private TboxUploadSession tbox;


        private List<string> sha256_list;

        private CRC64 crc64;
        public PauseTokenSource pts;

        public double Progress { get; set; }

        public FileSyncTask(string path, string hash, long size) {
            this.path = path;
            this.jboxhash = hash;
            this.size = size;
            this.succChunk = 0;
            this.chunkCount = (int)(this.size / ChunkSize);
            this.chunkCount += this.chunkCount * ChunkSize == this.size ? 0 : 1;
            jbox = new JboxDownloadSession(path, size);
            tbox = new TboxUploadSession(path, size);
            State = SyncTaskState.Wait;
            sha256_list = new List<string>();
            crc64 = new CRC64();
            pts = new PauseTokenSource();
        }

        public void Start()
        {
            Task.Run(internalStart);
        }

        public void internalStart()
        {
            var inst_pts = pts;
            State = SyncTaskState.Running;
            if (inst_pts.IsPaused) 
                return;

            var res1 = tbox.PrepareForUpload();
            if (!res1.success)
            {
                Message = res1.result;
                return;
            }

            if (inst_pts.IsPaused)
                return;

            var res2 = tbox.GetNextPartNumber();
            if (!res2.Success)
            {
                Message = res2.Message;
                return;
            }
            curChunk = res2.Result;

            if (inst_pts.IsPaused)
                return;

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

                        res1 = tbox.EnsureNoExpire(curChunk.PartNumber);
                        if (!res1.success)
                            throw new Exception(res1.result);

                        if (inst_pts.IsPaused)
                            return;

                        chunkRes = jbox.GetChunk(curChunk.PartNumber);
                        if (!chunkRes.Success)
                            throw new Exception(chunkRes.Message);

                        if (inst_pts.IsPaused)
                            return;

                        chunkRes.Result.Position = 0;
                        var res3 = tbox.Upload(chunkRes.Result, curChunk.PartNumber);
                        if (res3.success)
                            break;
                        else
                            throw new Exception(res3.result);
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
                    Message = ex.Message;
                    return;
                }

                chunkRes.Result.Position = 0;
                sha256_list.Add(HashHelper.SHA256Hash(chunkRes.Result));
                crc64.TransformBlock(chunkRes.Result.ToArray(), 0, (int)chunkRes.Result.Length);
                tbox.CompletePart(curChunk);
                succChunk++;

                if (inst_pts.IsPaused)
                    return;

                res2 = tbox.GetNextPartNumber();
                if (!res2.Success)
                {
                    Message = res2.Message;
                    return;
                }
                curChunk = res2.Result;
            }

            var actualHash = HashHelper.MD5Hash(string.Join(',', sha256_list));
            var actualcrc64 = crc64.TransformFinalBlock();


            if (succChunk == chunkCount && curChunk.PartNumber == 0)//&& 
            {
                var res4 = tbox.Confirm();
                if (!res4.Success)
                {
                    Message = res4.Message;
                    return;
                }
                if (actualcrc64.ToString() != res4.Result.Crc64 || jboxhash != actualHash)
                {
                    Message = $"hash不匹配";
                    return;
                }
                State = SyncTaskState.Complete;
                Message = "同步完成";
            }
        }

        public string GetProgressStr()
        {
            var down = (succChunk == chunkCount ? size : (succChunk * ChunkSize + jbox.Progress)).PrettyPrint();
            var up = (succChunk == chunkCount ? size : (succChunk * ChunkSize + tbox.Progress)).PrettyPrint();
            var all = size.PrettyPrint();
            return $"{down} / {up} / {all}";
        }

        public void Parse()
        {
            pts.Pause();
            State = SyncTaskState.Parse;
            if (curChunk != null)
                curChunk.Uploading = false;
        }

        public void Resume()
        {
            if (State != SyncTaskState.Parse)
                return;
            pts = new PauseTokenSource();
            pts.Resume();
            Task.Run(internalStart);
        }

        //public delegate void OnUpdate(SyncTaskState state);
    }
}
