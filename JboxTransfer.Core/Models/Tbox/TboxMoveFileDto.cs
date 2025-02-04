using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxMoveFileDto
    {
        [JsonProperty("path")]
        public List<string> Path { get; set; }
    }
}
