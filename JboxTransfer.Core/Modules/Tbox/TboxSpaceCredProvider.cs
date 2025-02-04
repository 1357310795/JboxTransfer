using JboxTransfer.Core.Models.Tbox;
using Microsoft.Extensions.Caching.Memory;

namespace JboxTransfer.Core.Modules.Tbox
{
    public class TboxSpaceCredProvider
    {
        private readonly IMemoryCache _mcache;
        private readonly TboxService _tservice;

        public TboxSpaceCredProvider(IMemoryCache mcache)
        {
            _mcache = mcache;
        }

        public TboxSpaceCred? GetSpaceCred(string userToken)
        {
            TboxSpaceCred? cred = _mcache.Get($"UserSpaceCred_{userToken}") as TboxSpaceCred;
            return cred;
        }

        public void SetSpaceCred(string userToken, TboxSpaceCred cred)
        {
            _mcache.Set($"UserSpaceCred_{userToken}", cred, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cred.ExpiresIn)
            });
        }
    }
}
