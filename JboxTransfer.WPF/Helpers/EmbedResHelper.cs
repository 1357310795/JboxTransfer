using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Helpers
{
    public partial class EmbedResHelper
    {
        [EmbedResourceCSharp.FileEmbed("Resources/EULA.rtf")]
        public static partial System.ReadOnlySpan<byte> GetELUA();

        [EmbedResourceCSharp.FileEmbed("Resources/Privacy.rtf")]
        public static partial System.ReadOnlySpan<byte> GetPrivacy();

        [EmbedResourceCSharp.FileEmbed("Resources/OpenSource.rtf")]
        public static partial System.ReadOnlySpan<byte> GetOpenSource();
    }
}
