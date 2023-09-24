using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public partial class TboxChunkUploadInfoResDto
    {
        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("conflictResolutionStrategy")]
        public string ConflictResolutionStrategy { get; set; }

        [JsonProperty("creationTime")]
        public string CreationTime { get; set; }

        [JsonProperty("force")]
        public bool Force { get; set; }

        [JsonProperty("parts")]
        public System.Collections.Generic.List<TboxChunkUploadPartInfo> Parts { get; set; }

        [JsonProperty("path")]
        public System.Collections.Generic.List<string> Path { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uploadId")]
        public string UploadId { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class TboxChunkUploadPartInfo
    {
        [JsonProperty("ETag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        [JsonProperty("LastModified", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModified { get; set; }

        [JsonProperty("PartNumber", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartNumber { get; set; }

        [JsonProperty("Size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }
    }
}
