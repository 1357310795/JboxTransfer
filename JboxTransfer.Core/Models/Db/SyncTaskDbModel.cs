﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Db
{
    public class SyncTaskDbModel
    {
        public SyncTaskDbModel()
        {

        }

        public SyncTaskDbModel(SystemUser user, SyncTaskType type, string filePath, long size, int order)
        {
            Type = type;
            FilePath = filePath;
            Size = size;
            Order = order;
            FileName = FilePath.Split('/').Last();
            if (FileName == "") FileName = "根目录";
            State = SyncTaskDbState.Idle;
            CreationTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            UserId = user.Id;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public SystemUser? User { get; set; }
        public int Order { get; set; }
        /// <summary>
        /// 0:file
        /// 1:folder
        /// </summary>
        public SyncTaskType Type { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Size { get; set; }
        public string? ConfirmKey { get; set; }
        /// <summary>
        /// 0:Idle
        /// 1:Busy
        /// 2:Error
        /// 3:Done
        /// 4:Cancel
        /// </summary>
        public SyncTaskDbState State { get; set; }
        public string? MD5_Part { get; set; }
        public string? MD5_Ori { get; set; }
        public long? CRC64_Part { get; set; }
        public string? RemainParts { get; set; }
        public string? Message { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public enum SyncTaskDbState
    {
        Idle,
        Pending,
        Busy,
        Error,
        Done,
        Cancel,
    }

    public enum SyncTaskType
    {
        File, Folder
    }
}
