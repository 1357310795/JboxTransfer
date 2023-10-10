using JboxTransfer.Core.Helpers;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Net;
using Teru.Code.Models;

namespace JboxTransfer.Services
{
    public static class GlobalCookie
    {
        public static CookieContainer CookieContainer { get; set; }

        static GlobalCookie()
        {
            _fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie.json");
        }

        private static string _fileName;

        public static void Save()
        {
            Directory.CreateDirectory(PathHelper.AppDataPath);
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(CookieContainer.GetAllCookies().Select(i => new CookieInfo(i))));
        }

        public static CookieContainer Read()
        {
            CookieContainer = new CookieContainer();
            if (File.Exists(_fileName))
            {
                var json = File.ReadAllText(_fileName);
                var result = JsonConvert.DeserializeObject<List<CookieInfo>>(json);
                foreach (var cookie in result)
                {
                    CookieContainer.Add(cookie.ToCookie());
                }
            }
            return CookieContainer;
        }

        public static CommonResult Clear()
        {
            try
            {
                File.Delete(_fileName);
                CookieContainer = new CookieContainer();
                return new CommonResult(true, "");
            }
            catch(Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        public static bool HasJacCookie()
        {
            return CookieContainer.GetAllCookies().Any(x => x.Name == "JAAuthCookie");
        }

#if NETFRAMEWORK
        public static List<Cookie> GetAllCookies(this CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) lstCookies.Add(c);
            }
            return lstCookies;
        }
#endif
    }

    public class CookieInfo
    {
        public CookieInfo()
        {

        }
        public CookieInfo(Cookie cookie)
        {
            Name = cookie.Name;
            Value = cookie.Value;
            Path = cookie.Path;
            Domain = cookie.Domain;
            Secure = cookie.Secure;
            HttpOnly = cookie.HttpOnly;
            Expired = cookie.Expired;
            Expires = cookie.Expires;
            Discard = cookie.Discard;
            Version = cookie.Version;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public string Domain { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public bool Expired { get; set; }
        public DateTime Expires { get; set; }
        public bool Discard { get; set; }
        public string Port { get; set; }
        public int Version { get; set; }
        public Cookie ToCookie()
        {
            return new Cookie(Name, Value, Path, Domain) { Secure = Secure, HttpOnly = HttpOnly, Expired = Expired, Expires = Expires, Discard = Discard, Version = Version };
        }
    }
}
