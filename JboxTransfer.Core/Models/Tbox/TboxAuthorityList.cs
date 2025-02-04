using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public class TboxAuthorityList
    {
        [JsonProperty("canAuthorize")]
        public bool CanAuthorize { get; set; }

        [JsonProperty("canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("canDownload")]
        public bool CanDownload { get; set; }

        [JsonProperty("canDownloadSelf")]
        public bool CanDownloadSelf { get; set; }

        [JsonProperty("canModify")]
        public bool CanModify { get; set; }

        [JsonProperty("canPreview")]
        public bool CanPreview { get; set; }

        [JsonProperty("canPreviewSelf")]
        public bool CanPreviewSelf { get; set; }

        [JsonProperty("canPrint")]
        public bool CanPrint { get; set; }

        [JsonProperty("canShare")]
        public bool CanShare { get; set; }

        [JsonProperty("canUpload")]
        public bool CanUpload { get; set; }

        [JsonProperty("canView")]
        public bool CanView { get; set; }
    }
}
