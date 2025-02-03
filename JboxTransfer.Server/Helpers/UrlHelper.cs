using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Server.Helpers
{
    public static class UrlHelper
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
    }
}
