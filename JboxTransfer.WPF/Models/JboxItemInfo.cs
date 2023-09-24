using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Models
{
    public class JboxItemInfo
    {
        #region Json Properties
        [JsonProperty("access_mode")]
        public long AccessMode { get; set; }

        [JsonProperty("approveable")]
        public bool Approveable { get; set; }

        [JsonProperty("authable")]
        public bool Authable { get; set; }

        [JsonProperty("bytes")]
        public long Bytes { get; set; }

        [JsonProperty("content")]
        public JboxItemInfo[] Content { get; set; }

        [JsonProperty("content_size")]
        public long ContentSize { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("creator_uid")]
        public long CreatorUid { get; set; }

        [JsonProperty("delivery_code")]
        public string DeliveryCode { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("dir_type")]
        public long DirType { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("is_bookmark")]
        public bool IsBookmark { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("is_dir")]
        public bool IsDir { get; set; }

        [JsonProperty("is_display")]
        public bool IsDisplay { get; set; }

        [JsonProperty("is_group")]
        public bool IsGroup { get; set; }

        [JsonProperty("is_shared")]
        public bool IsShared { get; set; }

        [JsonProperty("is_team")]
        public bool IsTeam { get; set; }

        [JsonProperty("localModifyTime")]
        public DateTime LocalModifyTime { get; set; }

        [JsonProperty("lock_uid")]
        public long LockUid { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [JsonProperty("neid")]
        public string Neid { get; set; }

        [JsonProperty("nsid")]
        public long Nsid { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("path_type")]
        public string PathType { get; set; }

        [JsonProperty("pid")]
        public string Pid { get; set; }

        [JsonProperty("prefix_neid")]
        public string PrefixNeid { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("rev")]
        public string Rev { get; set; }

        [JsonProperty("rev_index")]
        public long RevIndex { get; set; }

        [JsonProperty("router")]
        public Dictionary<string, object> Router { get; set; }

        [JsonProperty("share_to_personal")]
        public bool ShareToPersonal { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("support_preview")]
        public string SupportPreview { get; set; }

        [JsonProperty("updator")]
        public string Updator { get; set; }

        [JsonProperty("updator_uid")]
        public long UpdatorUid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
        #endregion

    }
}
