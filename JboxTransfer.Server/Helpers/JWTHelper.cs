using JWT.Algorithms;
using JWT.Serializers;
using JWT;

namespace JboxTransfer.Server.Helpers
{
    public class JWTHelper
    {
        public static string Create(object payload, string key)
        {
            //HMACSHA256加密
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            //序列化和反序列
            IJsonSerializer serializer = new SystemTextSerializer();
            //Base64编解码
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            //编码成JWT令牌
            var token = encoder.Encode(payload, key);
            return token;
        }
    }
}
