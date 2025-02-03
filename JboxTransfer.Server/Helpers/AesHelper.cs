using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Server.Helpers
{
    public static class AesHelper
    {
        public const string DefaultKey = "0123456789abcdef";
        #region 方法
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="sourceText">原文</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(string sourceText, byte[] rgbKey = null, byte[] rgbIV = null)
        {
            if (rgbKey == null)
                rgbKey = Encoding.Default.GetBytes(DefaultKey);
            if (rgbIV == null)
                rgbIV = Encoding.Default.GetBytes(DefaultKey);
            if (sourceText == null)
                throw new ArgumentNullException(nameof(sourceText));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = rgbKey;
                    aes.IV = rgbIV;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform transform = aes.CreateEncryptor())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(sourceText);
                        streamWriter.Flush();
                    }
                }


                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="cipherBuffer">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(byte[] rgbKey, byte[]? rgbIV, byte[] cipherBuffer)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (cipherBuffer == null)
                throw new ArgumentNullException(nameof(cipherBuffer));

            using (MemoryStream stream = new MemoryStream(cipherBuffer))
            {
                return Decrypt(rgbKey, rgbIV, stream);
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="cipherStream">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(byte[] rgbKey, byte[]? rgbIV, Stream cipherStream)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (cipherStream == null)
                throw new ArgumentNullException(nameof(cipherStream));

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform transform = aes.CreateDecryptor(rgbKey, rgbIV))
                using (CryptoStream cryptoStream = new CryptoStream(cipherStream, transform, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
