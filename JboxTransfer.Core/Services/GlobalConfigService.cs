using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Models;
using Newtonsoft.Json;

namespace JboxTransfer.Core.Services
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
