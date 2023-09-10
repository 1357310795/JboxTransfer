using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Services
{
    public class NetService
    {
        public static HttpClient Client;
        public static void Init()
        {
            Client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = GlobalCookie.CookieContainer, UseCookies = true, MaxAutomaticRedirections = 10 });
        }
    }
}
