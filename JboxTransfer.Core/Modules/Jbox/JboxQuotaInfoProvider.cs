using JboxTransfer.Core.Models.Jbox;
using JboxTransfer.Core.Models.Tbox;
using JboxTransfer.Core.Modules;
using JboxTransfer.Core.Modules.Jbox;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TboxWebdav.Server.Modules.Tbox
{
    public class JboxQuotaInfoProvider
    {
        private readonly ILogger<JboxQuotaInfoProvider> _logger;
        private readonly JboxCredProvider _credProvider;
        private readonly IMemoryCache _mcache;
        private readonly JboxService _service;
        private readonly SystemUserInfoProvider _user;

        public JboxQuotaInfoProvider(ILogger<JboxQuotaInfoProvider> logger, JboxCredProvider credProvider, IMemoryCache mcache, JboxService service, SystemUserInfoProvider user)
        {
            _logger = logger;
            _credProvider = credProvider;
            _mcache = mcache;
            _service = service;
            _user = user;
        }

        public JboxUserInfo? GetSpaceInfo()
        {
            var user = _user.GetUser();
            if (user == null)
                return null;
            if (_mcache.TryGetValue($"JboxUserInfoByJac_{user.Jaccount}", out var userInfo))
            {
                return userInfo as JboxUserInfo;
            }
            var res = _service.GetUserInfo();
            if (!res.Success)
            {
                _logger.LogWarning($"Get Jbox user info failed: {res.Message}");
                return null;
            }
            _mcache.Set($"JboxUserInfoByJac_{user.Jaccount}", res.Result, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
            });
            return res.Result;
        }
    }
}
