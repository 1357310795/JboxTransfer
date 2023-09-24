using JboxTransfer.Core.Helpers;
using JboxTransfer.Extensions;
using JboxTransfer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private string hash;
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

        public CancellationTokenSource cts;
        public CancellationToken ct;

        public double Progress { get; set; }

        public FileSyncTask(string path, string hash, long size) {
            this.path = path;
            this.hash = hash;
            this.size = size;
            this.succChunk = 0;
            this.chunkCount = (int)(this.size / ChunkSize);
            this.chunkCount += this.chunkCount * ChunkSize == this.size ? 0 : 1;
            jbox = new JboxDownloadSession(path, size);
            tbox = new TboxUploadSession(path, size);
        }

        public void Start()
        {
            Task.Run(internalStart);
        }

        public void internalStart()
        {
            if (ct.IsCancellationRequested) 
                return;

            sha256_list = new List<string>();
            var res1 = tbox.PrepareForUpload();
            if (!res1.success)
            {
                Message = res1.result;
                return;
            }

            if (ct.IsCancellationRequested)
                return;

            var res2 = tbox.GetNextPartNumber();
            if (!res2.Success)
            {
                Message = res2.Message;
                return;
            }
            curChunk = res2.Result;

            if (ct.IsCancellationRequested)
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
                        if (ct.IsCancellationRequested)
                            return;

                        res1 = tbox.EnsureNoExpire(curChunk.PartNumber);
                        if (!res1.success)
                            throw new Exception(res1.result);

                        if (ct.IsCancellationRequested)
                            return;

                        chunkRes = jbox.GetChunk(curChunk.PartNumber);
                        if (!chunkRes.Success)
                            throw new Exception(chunkRes.Message);

                        if (ct.IsCancellationRequested)
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
                if (ct.IsCancellationRequested)
                    return;
                if (t <= 0)
                {
                    tbox.ResetPartNumber(curChunk);
                    State = SyncTaskState.Error;
                    Message = ex.Message;
                    return;
                }
                else
                {
                    chunkRes.Result.Position = 0;
                    sha256_list.Add(HashHelper.SHA256Hash(chunkRes.Result));
                }

                tbox.CompletePart(curChunk);

                if (ct.IsCancellationRequested)
                    return;

                res2 = tbox.GetNextPartNumber();
                if (!res2.Success)
                {
                    Message = res2.Message;
                    return;
                }
                curChunk = res2.Result;

                if (curChunk.PartNumber != 0)
                {
                    succChunk++;
                    jbox.ClearProgress();
                    tbox.ClearProgress();
                }
            }

            var actualHash = HashHelper.MD5Hash(string.Join(',', sha256_list));

            if (hash == actualHash)
                Message = "哈希值校验通过";
            else
                Message = "哈希值校验失败";

            if (succChunk == chunkCount && curChunk.PartNumber == 0)//&& hash == actualHash
            {
                var res4 = tbox.Confirm();
                if (!res4.Success)
                {
                    Message = res4.Message;
                    return;
                }
                Message = "同步完成";
            }
        }

        public string GetProgressStr()
        {
            var down = (succChunk * ChunkSize + jbox.Progress).PrettyPrint();
            var up = (succChunk * ChunkSize + tbox.Progress).PrettyPrint();
            var all = size.PrettyPrint();
            return $"{down} / {up} / {all}";
        }

        //public delegate void OnUpdate(SyncTaskState state);
    }
}
