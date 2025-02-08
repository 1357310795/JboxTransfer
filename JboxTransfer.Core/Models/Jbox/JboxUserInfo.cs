using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Jbox
{
    public class JboxUserInfo
    {
        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("cloud_allow_scan")]
        public long CloudAllowScan { get; set; }

        [JsonProperty("cloud_allow_share")]
        public long CloudAllowShare { get; set; }

        [JsonProperty("cloud_quota")]
        public long CloudQuota { get; set; }

        [JsonProperty("cloud_used")]
        public long CloudUsed { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("ctime")]
        public string Ctime { get; set; }

        [JsonProperty("delivery_support")]
        public bool DeliverySupport { get; set; }

        [JsonProperty("docs_limit_enable")]
        public long DocsLimitEnable { get; set; }

        [JsonProperty("download_threshold_speed")]
        public long DownloadThresholdSpeed { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_chk")]
        public bool EmailChk { get; set; }

        [JsonProperty("from_domain_account")]
        public bool FromDomainAccount { get; set; }

        [JsonProperty("is_beyond_docsLimit")]
        public bool IsBeyondDocsLimit { get; set; }

        [JsonProperty("link_sharing_enable")]
        public long LinkSharingEnable { get; set; }

        [JsonProperty("local_edit_switch")]
        public bool LocalEditSwitch { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("mobile_chk")]
        public bool MobileChk { get; set; }

        [JsonProperty("ne_manage")]
        public long NeManage { get; set; }

        [JsonProperty("netzone_enable")]
        public long NetzoneEnable { get; set; }

        [JsonProperty("netzone_list")]
        public string NetzoneList { get; set; }

        [JsonProperty("password_changeable")]
        public bool PasswordChangeable { get; set; }

        [JsonProperty("personal_sharing_enable")]
        public long PersonalSharingEnable { get; set; }

        [JsonProperty("photo")]
        public string[] Photo { get; set; }

        [JsonProperty("preview_support")]
        public bool PreviewSupport { get; set; }

        [JsonProperty("quota")]
        public long Quota { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_desc")]
        public string RegionDesc { get; set; }

        [JsonProperty("region_id")]
        public long RegionId { get; set; }

        [JsonProperty("role")]
        public long Role { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }        
        
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("upload_threshold_speed")]
        public long UploadThresholdSpeed { get; set; }

        [JsonProperty("use_cloud_quota")]
        public long UseCloudQuota { get; set; }

        [JsonProperty("use_local_quota")]
        public long UseLocalQuota { get; set; }

        [JsonProperty("use_threshold")]
        public long UseThreshold { get; set; }

        [JsonProperty("used")]
        public long Used { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_slug")]
        public string UserSlug { get; set; }

        [JsonProperty("valid_enable")]
        public long ValidEnable { get; set; }

        [JsonProperty("valid_end_time")]
        public string ValidEndTime { get; set; }

        [JsonProperty("valid_start_time")]
        public string ValidStartTime { get; set; }
    }
}
