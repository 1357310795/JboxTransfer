using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public class SyncTaskDbModel
    {
        public SyncTaskDbModel()
        {

        }

        public SyncTaskDbModel(int type, string filePath, long size)
        {
            Type = type;
            FilePath = filePath;
            Size = size;
            FileName = FilePath.Split('/').Last();
            State = 0;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 0:file
        /// 1:folder
        /// </summary>
        public int Type { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Size { get; set; }
        public string ConfirmKey { get; set; }
        /// <summary>
        /// 0:Idle
        /// 1:Busy
        /// 2:Error
        /// 3:Done
        /// </summary>
        public int State { get; set; }
        public string MD5_Part { get; set; }
        public string MD5_Ori { get; set; }
        public long CRC64_Part { get; set; }
        public string RemainParts { get; set; }
    }
}
