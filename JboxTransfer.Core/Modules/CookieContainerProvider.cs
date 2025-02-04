using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TboxWebdav.Server.Modules;

namespace JboxTransfer.Core.Modules
{
    public class CookieContainerProvider
    {
        private readonly ILogger<HttpClientFactory> _logger;
        private readonly SystemUserInfoProvider _user;
        private readonly Dictionary<string, CookieContainer> _dic;

        public CookieContainerProvider(ILogger<HttpClientFactory> logger, SystemUserInfoProvider user)
        {
            _logger = logger;
            _user = user;
            _dic = new Dictionary<string, CookieContainer>();
        }

        public CookieContainer GetCookieContainer()
        {
            var key = _user.GetUser().Jaccount;
            if (!_dic.TryGetValue(key, out CookieContainer? cc))
            {
                cc = new CookieContainer();
                _dic.Add(key, cc);
                cc.Add(new Cookie("JAAuthCookie", _user.GetUser().Cookie, "/jaccount", "jaccount.sjtu.edu.cn"));
            }
            return cc;
        }
    }
}
