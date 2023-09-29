using JboxTransfer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Modules.Sync
{
    public interface IBaseTask
    {
        public string GetProgressStr();
        public void Start();
        public void Parse();
        public void Resume();

        public string Message { get; set; }
    }
}
