using JboxTransfer.Core.Modules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TboxWebdav.Server.Modules
{
    public class HttpClientFactory
    {
        private readonly ILogger<HttpClientFactory> _logger;
        private readonly SystemUserInfoProvider _user;
        private readonly CookieContainerProvider _cc;

        public HttpClientFactory(ILogger<HttpClientFactory> logger, SystemUserInfoProvider user, CookieContainerProvider cc)
        {
            _logger = logger;
            _user = user;
            _cc = cc;
        }

        public HttpClient CreateClient()
        {
            var cc = _cc.GetCookieContainer(_user.GetUser());
            var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, AutomaticDecompression = System.Net.DecompressionMethods.Deflate | DecompressionMethods.GZip, UseCookies = true, CookieContainer = cc });
            
            return client;
        }
    }
}
