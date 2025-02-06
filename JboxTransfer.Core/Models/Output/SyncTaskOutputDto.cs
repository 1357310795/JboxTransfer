using JboxTransfer.Core.Models.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Output
{
    public class SyncTaskOutputDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
        
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }

        [JsonPropertyName("parentPath")]
        public string ParentPath { get; set; }

        [JsonPropertyName("progress")]
        public double Progress { get; set; }

        [JsonPropertyName("totalBytes")]
        public long TotalBytes { get; set; }

        [JsonPropertyName("downloadedBytes")]
        public long DownloadedBytes { get; set; }

        [JsonPropertyName("uploadedBytes")]
        public long UploadedBytes { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SyncTaskState state { get; set; }
    }
}
