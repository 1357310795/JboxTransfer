using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Helpers
{
    public static class HashHelper
    {
        public static string SHA1Hash(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            StringBuilder sub = new StringBuilder();
            foreach (var t in data)
            {
                sub.Append(t.ToString("x2"));
            }

            return sub.ToString();
        }

        public static string SHA256Hash(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA256.Create().ComputeHash(buffer);

            StringBuilder sub = new StringBuilder();
            foreach (var t in data)
            {
                sub.Append(t.ToString("x2"));
            }

            return sub.ToString();
        }

        public static SHA256 SHA256Hash_Start()
        {
            return SHA256.Create();
        }

        public static int SHA256Hash_Proc(this SHA256 sha256, byte[] input)
        {
            return sha256.TransformBlock(input, 0, input.Length, null, 0);
        }

        public static byte[] SHA256Hash_Finish(this SHA256 sha256)
        {
            sha256.TransformFinalBlock(new byte[] { }, 0, 0);
            return sha256.Hash;
        }

        public static string MD5Hash(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = MD5.Create().ComputeHash(buffer);

            StringBuilder sub = new StringBuilder();
            foreach (var t in data)
            {
                sub.Append(t.ToString("x2"));
            }

            return sub.ToString();
        }

        public static MD5 MD5Hash_Start()
        {
            return MD5.Create();
        }

        public static int MD5Hash_Proc(this MD5 md5, byte[] input)
        {
            return md5.TransformBlock(input, 0, input.Length, null, 0);
        }

        public static byte[] MD5Hash_Finish(this MD5 md5)
        {
            md5.TransformFinalBlock(new byte[] { }, 0, 0);
            return md5.Hash;
        }
    }
}
