using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public partial class TboxStartChunkUploadResDto
    {
        [JsonProperty("confirmKey")]
        public string ConfirmKey { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("parts")]
        public Dictionary<string, TboxChunkUploadPart> Parts { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("uploadId")]
        public string UploadId { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class TboxChunkUploadPart
    {
        [JsonProperty("headers")]
        public TboxChunkUploadHeaders Headers { get; set; }
    }

    public partial class TboxChunkUploadHeaders
    {
        [JsonProperty("authorization")]
        public string Authorization { get; set; }

        [JsonProperty("x-amz-content-sha256")]
        public string XAmzContentSha256 { get; set; }

        [JsonProperty("x-amz-date")]
        public string XAmzDate { get; set; }
    }
}
