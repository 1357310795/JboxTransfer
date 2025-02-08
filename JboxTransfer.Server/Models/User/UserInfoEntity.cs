using Newtonsoft.Json;

namespace JboxTransfer.Server.Models.User
{
    public partial class UserInfoDto
    {
        [JsonProperty("entities")]
        public UserInfoEntity Entities { get; set; }

        [JsonProperty("errno")]
        public long Errno { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public partial class UserInfoEntity
    {
        [JsonProperty("accountNo")]
        public string AccountNo { get; set; }

        //[JsonProperty("authAccounts")]
        //public System.Collections.Generic.List<string> AuthAccounts { get; set; }

        [JsonProperty("avatars")]
        public string? Avatars { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("expireDate")]
        public string ExpireDate { get; set; }

        //[JsonProperty("identities")]
        //public System.Collections.Generic.List<Identity> Identities { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organizeId")]
        public string OrganizeId { get; set; }

        [JsonProperty("organizeName")]
        public string OrganizeName { get; set; }

        [JsonProperty("responseName")]
        public object ResponseName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusEN")]
        public string StatusEn { get; set; }

        [JsonProperty("userStyleName")]
        public string UserStyleName { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("createDate")]
        public long CreateDate { get; set; }

        [JsonProperty("defaultOptional")]
        public bool DefaultOptional { get; set; }

        [JsonProperty("expireDate")]
        public string ExpireDate { get; set; }

        [JsonProperty("facultyType")]
        public object FacultyType { get; set; }

        [JsonProperty("gjm")]
        public string Gjm { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("organize")]
        public Organize Organize { get; set; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("topOrganize")]
        public TopOrganize TopOrganize { get; set; }

        [JsonProperty("type")]
        public TypeClass Type { get; set; }

        [JsonProperty("updateDate")]
        public long UpdateDate { get; set; }

        [JsonProperty("userStyleName")]
        public string UserStyleName { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }
    }

    public partial class Organize
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class TopOrganize
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class TypeClass
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
