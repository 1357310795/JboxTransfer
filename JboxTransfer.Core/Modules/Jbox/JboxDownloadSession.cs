using JboxTransfer.Core.Extensions;
using Teru.Code.Models;

namespace JboxTransfer.Core.Modules.Jbox
{
    public class JboxDownloadSession
    {
        public const long ChunkSize = 4 * 1024 * 1024;
        private string path;
        private long size;
        private int chunkCount;
        private Pack<long> chunkProgress;

        public long Progress
        {
            get
            {
                return chunkProgress.Value;
            }
        }

        public JboxDownloadSession(string path, long size)
        {
            this.path = path;
            this.size = size;
            chunkCount = size.GetChunkCount();
            chunkProgress = new Pack<long>(0);
        }

        public CommonResult<MemoryStream> GetChunk(int chunk)
        {
            //Todo : 检查块大小
            var curchunksize = chunk == chunkCount ? size - ChunkSize * (chunk - 1) : ChunkSize;
            var res = JboxService.DownloadChunk(path, (chunk - 1) * ChunkSize, curchunksize, chunkProgress);
            if (!res.Success)
                return res;
            if (res.Result.Length != curchunksize)
                return new CommonResult<MemoryStream>(false, $"块大小错误：got {res.Result.Length}, expected {curchunksize}");
            return res;
        }

        public void ClearProgress()
        {
            chunkProgress.Value = 0;
        }
    }
}
