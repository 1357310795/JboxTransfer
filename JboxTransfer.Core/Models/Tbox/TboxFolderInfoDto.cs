using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public class TboxFolderInfoDto
    {
        [JsonProperty("authorityList")]
        public TboxAuthorityList AuthorityList { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("isAuthorized")]
        public bool IsAuthorized { get; set; }

        //[JsonProperty("localSync")]
        //public object LocalSync { get; set; }

        [JsonProperty("modificationTime")]
        public DateTime ModificationTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }

        [JsonProperty("sensitiveWordAuditStatus")]
        public long SensitiveWordAuditStatus { get; set; }

        [JsonProperty("tagList")]
        public List<string> TagList { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userOrgId")]
        public string UserOrgId { get; set; }

        [JsonProperty("virusAuditStatus")]
        public long VirusAuditStatus { get; set; }
    }
}
