using Newtonsoft.Json;

namespace JboxTransfer.Core.Models
{
    public class ConfigModel
    {
        [JsonProperty("oauth")]
        public OAuthConfigModel OAuthConfig { get; set; }

        [JsonProperty("task")]
        public TaskConfigModel TaskConfig { get; set; }
    }

    public class OAuthConfigModel
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }

    public class TaskConfigModel
    {
        [JsonProperty("thread_count")]
        public int ThreadCount { get; set; }
    }
}
