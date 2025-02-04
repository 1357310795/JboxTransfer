using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using TboxWebdav.Server.Modules;

namespace JboxTransfer.Core.Modules.Tbox
{
    public class TboxUserTokenProvider
    {
        private string userToken;
        private readonly ILogger<TboxUserTokenProvider> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMemoryCache _mcache;
        private readonly HttpClientFactory _clientFactory;
        private readonly SystemUserInfoProvider _user;

        public TboxUserTokenProvider(ILogger<TboxUserTokenProvider> logger, IHttpContextAccessor contextAccessor, IMemoryCache mcache, HttpClientFactory clientFactory, SystemUserInfoProvider user)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _mcache = mcache;
            _clientFactory = clientFactory;
            _user = user;
            if (_mcache.TryGetValue($"UserTokenByJac_{_user.GetUser().Jaccount}", out var token))
            {
                userToken = token.ToString();
            }
            else
            {
                userToken = TryJaccountLogin();
            }
        }

        private string TryJaccountLogin()
        {
            var client = _clientFactory.CreateClient();
            var res = TboxService.LoginUseJaccount(client);
            if (res.Success)
            {
                _mcache.Set($"UserTokenByJac_{_user.GetUser().Jaccount}", res.Result.UserToken, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(res.Result.ExpiresIn)
                });
                return res.Result.UserToken;
            }
            else
            {
                _logger.LogWarning($"Jaccount login failed: {res.Message}");
                return string.Empty;
            }
        }

        public void SetUserToken(string usertoken)
        {
            userToken = usertoken;
        }

        public string GetUserToken()
        {
            return userToken;
        }
    }
}
