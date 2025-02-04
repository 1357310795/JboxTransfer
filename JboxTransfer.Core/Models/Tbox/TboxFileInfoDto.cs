using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxFileInfoDto
    {
        [JsonProperty("authorityList")]
        public TboxAuthorityList AuthorityList { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("crc64")]
        public string Crc64 { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("historySize")]
        public string HistorySize { get; set; }

        [JsonProperty("isAuthorized")]
        public bool IsAuthorized { get; set; }

        [JsonProperty("modificationTime")]
        public DateTime ModificationTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }

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

        [JsonProperty("tagList")]
        public List<string> TagList { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userOrgId")]
        public string UserOrgId { get; set; }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("virusAuditStatus")]
        public long VirusAuditStatus { get; set; }
    }
}
