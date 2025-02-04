using Newtonsoft.Json;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxItemListDto
    {
        [JsonProperty("authorityList")]
        public TboxAuthorityList AuthorityList { get; set; }

        [JsonProperty("contents")]
        public List<TboxMergedItemDto> Contents { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("fileCount")]
        public long FileCount { get; set; }

        //[JsonProperty("localSync")]
        //public object LocalSync { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }

        [JsonProperty("subDirCount")]
        public long SubDirCount { get; set; }

        [JsonProperty("totalNum")]
        public long TotalNum { get; set; }
    }

    public partial class TboxMergedItemDto
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

        [JsonProperty("hasSync")]
        public bool HasSync { get; set; }

        //[JsonProperty("localSync")]
        //public object LocalSync { get; set; }

        [JsonProperty("metaData")]
        public Dictionary<string, object> MetaData { get; set; }

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

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("versionId")]
        public long? VersionId { get; set; }

        [JsonProperty("virusAuditStatus")]
        public long VirusAuditStatus { get; set; }
    }
}
