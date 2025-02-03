using Newtonsoft.Json;

namespace JboxTransfer.Server.Models.Output
{
    public class UserInfoDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("jaccount")]
        public string Jaccount { get; set; }
    }
}
