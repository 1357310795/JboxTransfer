using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Extensions
{

    public static class CookieExtension
    {

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

        public static Cookie? FirstOrDefault(this CookieCollection cc, Func<Cookie, bool> predicate)
        {
            if (cc == null)
            {
                throw new ArgumentNullException(nameof(cc));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var enumerator = cc.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var cookie = enumerator.Current as Cookie;
                if (cookie != null && predicate(cookie))
                {
                    return cookie;
                }
            }

            return null;
        }
#endif
    }

}
