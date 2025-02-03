using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Tbox
{
    public partial class TboxLoginResDto
    {
        [JsonProperty("expiresIn")]
        public long ExpiresIn { get; set; }

        [JsonProperty("isNewUser")]
        public bool IsNewUser { get; set; }

        [JsonProperty("organizations")]
        public List<Organization> Organizations { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("userToken")]
        public string UserToken { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class Organization
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
    }

    public partial class ExtensionData
    {
        [JsonProperty("allowChangeNickname")]
        public bool AllowChangeNickname { get; set; }

        [JsonProperty("allowProduct")]
        public string AllowProduct { get; set; }

        [JsonProperty("cacheDocPreviewTypes")]
        public string CacheDocPreviewTypes { get; set; }

        [JsonProperty("defaultTeamOptions")]
        public DefaultTeamOptions DefaultTeamOptions { get; set; }

        [JsonProperty("defaultUserOptions")]
        public DefaultUserOptions DefaultUserOptions { get; set; }

        [JsonProperty("editionConfig")]
        public EditionConfig EditionConfig { get; set; }

        [JsonProperty("enableDocEdit")]
        public bool EnableDocEdit { get; set; }

        [JsonProperty("enableDocPreview")]
        public bool EnableDocPreview { get; set; }

        [JsonProperty("enableMediaProcessing")]
        public bool EnableMediaProcessing { get; set; }

        [JsonProperty("enableOpenLdapLogin")]
        public bool EnableOpenLdapLogin { get; set; }

        [JsonProperty("enableShare")]
        public bool EnableShare { get; set; }

        [JsonProperty("enableViewAllOrgUser")]
        public bool EnableViewAllOrgUser { get; set; }

        [JsonProperty("enableWeworkLogin")]
        public bool EnableWeworkLogin { get; set; }

        [JsonProperty("enableWindowsAdLogin")]
        public bool EnableWindowsAdLogin { get; set; }

        [JsonProperty("enableYufuLogin")]
        public bool EnableYufuLogin { get; set; }

        [JsonProperty("expireTime")]
        public string ExpireTime { get; set; }

        [JsonProperty("ipLimit")]
        public IpLimit IpLimit { get; set; }

        [JsonProperty("isAccountNotDependentOnPhone")]
        public bool IsAccountNotDependentOnPhone { get; set; }

        [JsonProperty("libraryFlag")]
        public long LibraryFlag { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("officialDocPreviewTypes")]
        public string OfficialDocPreviewTypes { get; set; }

        [JsonProperty("showOrgNameInUI")]
        public bool ShowOrgNameInUi { get; set; }

        [JsonProperty("ssoWay")]
        public string SsoWay { get; set; }

        [JsonProperty("syncWay")]
        public string SyncWay { get; set; }

        [JsonProperty("userLimit")]
        public long UserLimit { get; set; }

        [JsonProperty("watermarkOptions")]
        public WatermarkOptions WatermarkOptions { get; set; }
    }

    public partial class DefaultTeamOptions
    {
        [JsonProperty("defaultRoleId")]
        public long DefaultRoleId { get; set; }

        [JsonProperty("spaceQuotaSize")]
        public object SpaceQuotaSize { get; set; }
    }

    public partial class DefaultUserOptions
    {
        [JsonProperty("allowPersonalSpace")]
        public bool AllowPersonalSpace { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("personalSpaceQuotaSize")]
        public string PersonalSpaceQuotaSize { get; set; }
    }

    public partial class EditionConfig
    {
        [JsonProperty("editionFlag")]
        public string EditionFlag { get; set; }

        [JsonProperty("enableOnlineEdit")]
        public bool EnableOnlineEdit { get; set; }

        [JsonProperty("enableOverseasPhoneNumber")]
        public bool EnableOverseasPhoneNumber { get; set; }
    }

    public partial class IpLimit
    {
        [JsonProperty("limitAdmin")]
        public bool LimitAdmin { get; set; }
    }

    public partial class WatermarkOptions
    {
        [JsonProperty("downloadWatermarkType")]
        public long DownloadWatermarkType { get; set; }

        [JsonProperty("enableDownloadWatermark")]
        public bool EnableDownloadWatermark { get; set; }

        [JsonProperty("enablePreviewWatermark")]
        public bool EnablePreviewWatermark { get; set; }

        [JsonProperty("enableShareWatermark")]
        public bool EnableShareWatermark { get; set; }

        [JsonProperty("previewWatermarkType")]
        public long PreviewWatermarkType { get; set; }

        [JsonProperty("shareWatermarkType")]
        public long ShareWatermarkType { get; set; }
    }

    public partial class OrgUser
    {
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("deregister")]
        public bool Deregister { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("needChangePassword")]
        public bool NeedChangePassword { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
