using System.Text.Json.Serialization;

namespace JboxTransfer.Server.Models.User
{
    public class JaccountQrCodeDataDto
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("qrcode")]
        public string Qrcode { get; set; }
    }
}
