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
        private long length;
        private int curChunk;
        private Exception ex;
        private CommonResult<MemoryStream> chunkRes;
        public SyncTaskState State { get; set; }
        public string Message { get; set; }
        private Pack<long> chunkProgress;

        private int chunks;

        private List<string> sha256_list;

        public double Progress { get; set; }

        public FileSyncTask(string path, string hash, long length) {
            this.path = path;
            this.hash = hash;
            this.length = length;
            chunkProgress = new Pack<long>(0);
        }

        public void Start()
        {
            Task.Run(internalStart);
        }

        public void internalStart()
        {
            sha256_list = new List<string>();
            chunks = (int)(length / ChunkSize);
            if (length > chunks * ChunkSize)
                chunks++;
            for (curChunk = 0; curChunk < chunks; curChunk++)
            {
                int t = RetryTimes;
                chunkProgress = new Pack<long>(0);
                while (t-- > 0)
                {
                    try
                    {
                        var size = curChunk == chunks - 1 ? (length % ChunkSize) : (ChunkSize);
                        chunkRes = JboxService.DownloadChunk(path, curChunk * ChunkSize, size, chunkProgress);
                        
                        if (chunkRes.Success)
                            break;
                    }
                    catch (Exception ex)
                    {
                        this.ex = ex;
                    }
                }
                if (t <= 0)
                {
                    State = SyncTaskState.Error;
                    Message = ex.Message;
                    return;
                }
                else
                {
                    chunkRes.Result.Position = 0;
                    sha256_list.Add(HashHelper.SHA256Hash(chunkRes.Result));
                }
            }
            curChunk--;

            var actualHash = HashHelper.MD5Hash(string.Join(',', sha256_list));

            if (hash == actualHash)
                Message = "哈希值校验通过";
            else
                Message = "哈希值校验失败";
        }

        public string GetProgressStr()
        {
            var down = (curChunk * ChunkSize + chunkProgress.Value).PrettyPrint();
            var up = ((long)0).PrettyPrint();
            var all = length.PrettyPrint();
            return $"{down} / {up} / {all}";
        }

        //public delegate void OnUpdate(SyncTaskState state);
    }
}
