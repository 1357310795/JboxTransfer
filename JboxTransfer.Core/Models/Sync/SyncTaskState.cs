﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Sync
{
    public enum SyncTaskState
    {
        Wait, Running, Error, Complete, Pause
    }
}
