using JboxTransfer.Core.Models.Jbox;
using JboxTransfer.Core.Models.Tbox;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace JboxTransfer.Core.Modules.Jbox
{
    public class JboxCredProvider
    {
        private readonly ILogger<JboxCredProvider> _logger;
        private readonly IMemoryCache _mcache;
        private readonly SystemUserInfoProvider _user;

        public JboxCredProvider(ILogger<JboxCredProvider> logger, IMemoryCache mcache, SystemUserInfoProvider user)
        {
            _logger = logger;
            _mcache = mcache;
            _user = user;
        }

        public JboxCredInfo? GetCred()
        {
            JboxCredInfo? cred = _mcache.Get($"JboxCredByJac_{_user.GetUser().Jaccount}") as JboxCredInfo;
            return cred;
        }

        public void SetCred(JboxCredInfo cred)
        {
            _mcache.Set($"JboxCredByJac_{_user.GetUser().Jaccount}", cred, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }
}
