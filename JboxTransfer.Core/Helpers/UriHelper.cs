using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Helpers
{
    public static class UriHelper
    {
        public static string BuildQuery(Dictionary<string, string> query)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            foreach (var kvp in query)
            {
                sb.Append(kvp.Key);
                if (kvp.Value != null)
                {
                    sb.Append("=");
                    sb.Append(kvp.Value);
                }
                sb.Append("&");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static string PathToName(this string path)
        {
            var name = path.Split('/').Last();
            return name == "" ? "根目录" : name;
        }

        public static string GetParentPath(this string path)
        {
            var s = path.Split('/');
            var p = string.Join("/", s.Take(s.Length - 1));
            return p == "" ? "/" : p;
        }

        public static string UrlEncode(this string url)
        {
            return Uri.EscapeDataString(url);
        }

        public static string Connect(this IEnumerable<string> iterator, string separator)
        {
            StringBuilder s = new StringBuilder();
            var it = iterator.GetEnumerator();
            bool next = it.MoveNext();
            while (next)
            {
                s.Append(it.Current);
                next = it.MoveNext();
                if (next)
                    s.Append(separator);

            }
            return s.ToString();
        }

        public static string UrlEncodeByParts(this string url)
        {
            return url.Split('/')
                       .Select(s => s.UrlEncode())
                       .Connect("/");
        }
    }
}
