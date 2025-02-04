using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxSpaceQuotaInfo
    {
        [JsonProperty("availableSpace")]
        public string AvailableSpace { get; set; }

        [JsonProperty("capacity")]
        public string Capacity { get; set; }

        [JsonProperty("hasPersonalSpace")]
        public bool HasPersonalSpace { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
