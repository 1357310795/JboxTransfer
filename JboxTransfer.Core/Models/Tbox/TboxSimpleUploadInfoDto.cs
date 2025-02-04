using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxSimpleUploadInfoDto
    {
        [JsonProperty("confirmKey")]
        public string ConfirmKey { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }
        //public TboxUploadHeaders Headers { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public partial class TboxUploadHeaders
    {
        [JsonProperty("authorization")]
        public string Authorization { get; set; }

        [JsonProperty("cache-control")]
        public string CacheControl { get; set; }

        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("x-amz-acl")]
        public string XAmzAcl { get; set; }

        [JsonProperty("x-amz-content-sha256")]
        public string XAmzContentSha256 { get; set; }

        [JsonProperty("x-amz-date")]
        public string XAmzDate { get; set; }
    }
}
