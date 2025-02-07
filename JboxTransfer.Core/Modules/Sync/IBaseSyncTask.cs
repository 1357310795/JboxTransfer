using JboxTransfer.Core.Models.Db;
using JboxTransfer.Core.Models.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Sync
{
    public interface IBaseSyncTask
    {
        public void Init(SyncTaskDbModel dbModel);
        public string GetProgressStr();
        public string GetProgressTextTooltip();
        public void Start();
        public void Pause();
        public void Resume();
        public void Cancel();
        public void Recover(bool keepProgress);

        public string FileName { get; }
        public string FilePath { get; }
        public string ParentPath { get; }

        public double Progress { get; }
        public string Message { get; }
        public SyncTaskState State { get; set; }
        public SyncTaskType Type { get; }
        public bool IsUserPause { get; set; }
        public int SyncTaskId { get; }
        public long TotalBytes { get; }
        public long DownloadedBytes { get; }
        public long UploadedBytes { get; }
    }
}
