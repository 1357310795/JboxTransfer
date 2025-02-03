using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Server.Helpers
{
    public partial class EmbedResHelper
    {
        [EmbedResourceCSharp.FileEmbed("Resources/top100_cn.txt")]
        public static partial ReadOnlySpan<byte> GetWeakPassword();
    }
}
