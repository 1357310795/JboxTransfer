using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Extensions
{
    public static class StringExtension
    {
#if NETFRAMEWORK
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (toCheck == null)
            {
                throw new ArgumentNullException(nameof(toCheck));
            }

            return source.IndexOf(toCheck, comp) >= 0;
        }
#endif
    }
}
