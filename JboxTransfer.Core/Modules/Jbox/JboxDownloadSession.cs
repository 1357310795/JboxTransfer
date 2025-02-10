using JboxTransfer.Core.Extensions;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<JboxDownloadSession> _logger;
        private readonly JboxService _jbox;

        public JboxDownloadSession(ILogger<JboxDownloadSession> logger, JboxService jbox)
        {
            _logger = logger;
            _jbox = jbox;
        }

        public void Init(string path, long size)
        {
            this.path = path;
            this.size = size;
            chunkCount = size.GetChunkCount();
            chunkProgress = new Pack<long>(0);
        }

        public CommonResult<MemoryStream> GetChunk(int chunk, CancellationToken ct)
        {
            //Todo : 检查块大小
            var curchunksize = chunk == chunkCount ? size - ChunkSize * (chunk - 1) : ChunkSize;
            var res = _jbox.DownloadChunk(path, (chunk - 1) * ChunkSize, curchunksize, chunkProgress, ct).GetAwaiter().GetResult();
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
