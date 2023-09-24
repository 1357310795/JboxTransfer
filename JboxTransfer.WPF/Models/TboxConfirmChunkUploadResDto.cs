using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public partial class TboxConfirmChunkUploadResDto
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("crc64")]
        public string Crc64 { get; set; }

        [JsonProperty("creationTime")]
        public string CreationTime { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("isOverwrittened")]
        public bool IsOverwrittened { get; set; }

        [JsonProperty("metaData")]
        public System.Collections.Generic.Dictionary<string, object> MetaData { get; set; }

        [JsonProperty("modificationTime")]
        public string ModificationTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public System.Collections.Generic.List<string> Path { get; set; }

        [JsonProperty("previewAsIcon")]
        public bool PreviewAsIcon { get; set; }

        [JsonProperty("previewByCI")]
        public bool PreviewByCi { get; set; }

        [JsonProperty("previewByDoc")]
        public bool PreviewByDoc { get; set; }

        [JsonProperty("sensitiveWordAuditStatus")]
        public long SensitiveWordAuditStatus { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("virusAuditStatus")]
        public long VirusAuditStatus { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
