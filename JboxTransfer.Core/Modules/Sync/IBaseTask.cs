using JboxTransfer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Sync
{
    public interface IBaseTask
    {
        public string GetProgressStr();
        public string GetProgressTextTooltip();
        public void Start();
        public void Pause();
        public void Resume();
        public void Cancel();
        public void Recover(bool keepProgress);

        public string GetName();
        public string GetPath();
        public string GetParentPath();

        public double Progress { get; }
        public string Message { get; }
        public SyncTaskState State { get; set; }
    }
}
