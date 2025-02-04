using JboxTransfer.Core.Models.Tbox;
using Microsoft.Extensions.Caching.Memory;

namespace JboxTransfer.Core.Modules.Tbox
{
    public class TboxSpaceInfoProvider
    {
        private readonly TboxUserTokenProvider _tokenProvider;
        private readonly IMemoryCache _mcache;
        private readonly TboxService _service;

        public TboxSpaceInfoProvider(TboxUserTokenProvider tokenProvider, IMemoryCache mcache, TboxService service)
        {
            _tokenProvider = tokenProvider;
            _mcache = mcache;
            _service = service;
        }

        public TboxSpaceQuotaInfo GetSpaceInfo()
        {
            var token = _tokenProvider.GetUserToken();
            if (string.IsNullOrEmpty(token))
                return null;
            if (_mcache.TryGetValue($"UserSpaceInfo_{token}", out var spaceInfo))
            {
                return spaceInfo as TboxSpaceQuotaInfo;
            }
            var res = _service.GetSpaceQuotaInfo();
            if (!res.Success)
                return null;
            _mcache.Set($"UserSpaceInfo_{token}", res.Result, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
            });
            return res.Result;
        }
    }
}
