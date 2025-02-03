using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxUserInfoDto
    {
        [JsonProperty("expired", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Expired { get; set; }

        [JsonProperty("extensionData", NullValueHandling = NullValueHandling.Ignore)]
        public ExtensionData ExtensionData { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("ipLimitEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IpLimitEnabled { get; set; }

        [JsonProperty("isLastSignedIn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsLastSignedIn { get; set; }

        [JsonProperty("isTemporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTemporary { get; set; }

        [JsonProperty("libraryCert", NullValueHandling = NullValueHandling.Ignore)]
        public string LibraryCert { get; set; }

        [JsonProperty("libraryId", NullValueHandling = NullValueHandling.Ignore)]
        public string LibraryId { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("orgUser", NullValueHandling = NullValueHandling.Ignore)]
        public OrgUser OrgUser { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
