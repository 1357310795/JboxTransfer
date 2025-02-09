using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Output
{
    public class FileSystemItemInfoOutputDto
    {
        /// <summary>
        /// 初始化文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullPath"></param>
        /// <param name="size"></param>
        /// <param name="creationTime"></param>
        /// <param name="updateTime"></param>
        public FileSystemItemInfoOutputDto(string name, string fullPath, long size, DateTime? creationTime, DateTime? updateTime, string syncState = "None")
        {
            Type = FileSystemItemType.File;
            Name = name;
            FullPath = fullPath;
            Size = size;
            CreationTime = creationTime;
            UpdateTime = updateTime;
            SyncState = syncState;
        }

        /// <summary>
        /// 初始化文件夹
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullPath"></param>
        /// <param name="creationTime"></param>
        /// <param name="updateTime"></param>
        /// <param name="contents"></param>
        public FileSystemItemInfoOutputDto(string name, string fullPath, DateTime? creationTime, DateTime? updateTime, List<FileSystemItemInfoOutputDto>? contents, int totalCount, string syncState = "None")
        {
            Type = FileSystemItemType.Folder;
            Name = name;
            FullPath = fullPath;
            CreationTime = creationTime;
            UpdateTime = updateTime;
            Contents = contents;
            TotalCount = totalCount;
            SyncState = syncState;
        }

        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileSystemItemType Type { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public List<FileSystemItemInfoOutputDto>? Contents { get; set; }

        public int TotalCount { get; set; }
        public string SyncState { get; set; }
    }

    public enum FileSystemItemType
    {
        File, Folder
    }
}
