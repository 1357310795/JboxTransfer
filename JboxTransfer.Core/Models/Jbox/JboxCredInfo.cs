using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Models.Jbox
{
    public class JboxCredInfo
    {
        public JboxCredInfo(string s)
        {
            S = s;
        }

        public string S { get; set; }
    }
}
