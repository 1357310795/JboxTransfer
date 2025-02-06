using JboxTransfer.Core.Models.Db;
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
        private readonly ILogger<CookieContainerProvider> _logger;
        private readonly Dictionary<string, CookieContainer> _dic;

        public CookieContainerProvider(ILogger<CookieContainerProvider> logger)
        {
            _logger = logger;
            _dic = new Dictionary<string, CookieContainer>();
        }

        public CookieContainer GetCookieContainer(SystemUser user)
        {
            if (!_dic.TryGetValue(user.Jaccount, out CookieContainer? cc))
            {
                _logger.LogTrace(user.Jaccount + " cookie not found, create new one.");
                cc = new CookieContainer();
                _dic.Add(user.Jaccount, cc);
                cc.Add(new Cookie("JAAuthCookie", user.Cookie, "/jaccount", "jaccount.sjtu.edu.cn"));
            }
            return cc;
        }
    }
}
