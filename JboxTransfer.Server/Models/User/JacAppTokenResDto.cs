using Newtonsoft.Json;

namespace JboxTransfer.Server.Models.User
{
    public partial class JacAppTokenResDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    public partial class JacOAuthIdToken
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }
    }
}
