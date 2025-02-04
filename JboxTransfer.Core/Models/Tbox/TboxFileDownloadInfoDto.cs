using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxFileDownloadInfoDto
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("cosUrl")]
        public string CosUrl { get; set; }

        [JsonProperty("cosUrlExpiration")]
        public string CosUrlExpiration { get; set; }

        [JsonProperty("crc64")]
        public string Crc64 { get; set; }

        [JsonProperty("creationTime")]
        public string CreationTime { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("modificationTime")]
        public string ModificationTime { get; set; }

        [JsonProperty("previewAsIcon")]
        public bool PreviewAsIcon { get; set; }

        [JsonProperty("previewByCI")]
        public bool PreviewByCi { get; set; }

        [JsonProperty("previewByDoc")]
        public bool PreviewByDoc { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
