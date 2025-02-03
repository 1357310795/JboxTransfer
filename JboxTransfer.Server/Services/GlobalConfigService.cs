using JboxTransfer.Server.Helpers;
using JboxTransfer.Server.Models;
using Newtonsoft.Json;

namespace JboxTransfer.Server.Services
{
    public static class GlobalConfigService
    {
        public static ConfigModel Config { get; set; }
        static GlobalConfigService()
        {
            var file = File.ReadAllText(Path.Combine(PathHelper.AppPath, "config.json"));
            Config = JsonConvert.DeserializeObject<ConfigModel>(file);
        }
    }
}
