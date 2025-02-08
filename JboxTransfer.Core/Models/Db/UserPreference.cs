using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Db
{
    public class UserPreference
    {
        [JsonProperty("concurrencyCount")]
        public int ConcurrencyCount { get; set; } = 4;

        [JsonProperty("conflictResolutionStrategy")]
        public string ConflictResolutionStrategy { get; set; } = "overwrite";
    }
}
