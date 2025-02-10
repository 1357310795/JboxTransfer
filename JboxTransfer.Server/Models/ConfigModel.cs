using Newtonsoft.Json;

namespace JboxTransfer.Server.Models
{
    public class ConfigModel
    {
        [JsonProperty("server")]
        public ServerConfigModel ServerConfig { get; set; }
    }

    public class ServerConfigModel
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
