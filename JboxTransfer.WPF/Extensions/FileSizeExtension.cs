using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Extensions
{
    public static class FileSizeExtension
    {
        public static string PrettyPrint(this long value)
        {
            if (value < 0) throw new Exception("WTF???");
            double v = value;
            if (v < 1024)
            {
                return $"{value} Bytes";
            }
            else v = v / 1024;
            if (v < 1024)
            {
                return $"{v.ToString("f2")} KB";
            }
            else v = v / 1024;
            if (v < 1024)
            {
                return $"{v.ToString("f2")} MB";
            }
            else v = v / 1024;
            if (v < 1024)
            {
                return $"{v.ToString("f2")} GB";
            }
            else v = v / 1024;
            if (v < 1024)
            {
                return $"{v.ToString("f2")} TB";
            }
            else v = v / 1024;
            throw new Exception("WTF???");
        }
    }
}
