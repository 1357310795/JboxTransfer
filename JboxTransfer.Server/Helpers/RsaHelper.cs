using System.Security.Cryptography;

namespace JboxTransfer.Server.Helpers
{
    public static class RsaHelper
    {
        public const string DefaultPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArKZOdKQAL+iYzJ4Q5EQzwv/yvVPnfdNVKRgNG19HbCYM4qIzFPEOFv28SVFQh+xqAj8tAfjpMSTihFwt6BQuWfZXWYpAqf4jF4cU7ez/VHJyzsn8Cb7Lf/1KsLpuz+MbqufrA57AysnLAnRXHOwik+QnpsXZYjTcjgxQ0iLMe5iJyo06CKFxH1rmgYMwS4E89kNg1VtYrFKs1MajApfhu9hTEXnm/lP24TPdefRXbf+z84p1GLue2HRhZs3wECH1HJWZOsrdL/M+wigWldY0fHoiaKsjD9rK1NyaPtk4bIYuwPsfQu5RN4hkEPpTvdw1nKzOdo77zNa5ovCY0uNLZwIDAQAB";
        //public const string DefaultPublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDGz5pk36kOqDyz3VkhqIn8FA/p+b/ehvPZxxVqYWUbn8PQNnucr/WIrhKDnKvR6NZuhBc+7KznYxsU5KjFxy/i3ne7JiJPhOtU4MJASgdaD84bbhWaAfAU6885K/UOKjbf+//NmfXKyvO3ZQCOHU070smd3s/xZaFQuyxM5pHXlwIDAQAB";

        public static byte[] RSAEncrypt(byte[] data, string publicKey = DefaultPublicKey)
        {
            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            byte[] encryptData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            return encryptData;
        }
    }
}
