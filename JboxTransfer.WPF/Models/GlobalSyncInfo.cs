﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public class GlobalSyncInfo
    {
        public int Mode { get; set; }
        public long SuccBytes { get; set; }
        public long AllBytes { get; set; }
    }
}
