using Newtonsoft.Json;

namespace JboxTransfer.Server.Models
{
    public class ConfigModel
    {
        [JsonProperty("oauth")]
        public OAuthConfigModel OAuthConfig { get; set; }
    }

    public class OAuthConfigModel
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}
