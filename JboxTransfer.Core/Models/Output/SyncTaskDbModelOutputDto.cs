using JboxTransfer.Core.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Output
{
    public class SyncTaskDbModelOutputDto
    {
        public int Id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SyncTaskType Type { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ParentPath { get; set; }
        public long Size { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SyncTaskDbState State { get; set; }
        public string? Message { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
