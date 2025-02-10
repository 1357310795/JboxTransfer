﻿namespace JboxTransfer.Core.Modules
{
    public class MD5Base
    {
        protected uint[] m_state = new uint[4];
        protected uint[] m_buffer = new uint[16];

        static readonly byte[,] ShiftsTable = {
            { 7, 12, 17, 22 }, { 5, 9, 14, 20 }, { 4, 11, 16, 23 }, { 6, 10, 15, 21 },
        };

        static readonly uint[] SineTable = {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
        };

        protected MD5Base()
        {
        }

        public static uint RotL(uint v, int count)
        {
            count &= 0x1F;
            return v << count | v >> (32 - count);
        }

        protected void Transform()
        {
            uint a = m_state[0];
            uint b = m_state[1];
            uint c = m_state[2];
            uint d = m_state[3];

            for (int i = 0; i < 64; ++i)
            {
                uint f;
                int g;
                if (i < 16)
                {
                    f = d ^ b & (c ^ d);
                    g = i;
                }
                else if (i < 32)
                {
                    f = c ^ d & (b ^ c);
                    g = 5 * i + 1 & 0xF;
                }
                else if (i < 48)
                {
                    f = b ^ c ^ d;
                    g = 3 * i + 5 & 0xF;
                }
                else
                {
                    f = c ^ (b | ~d);
                    g = 7 * i & 0xF;
                }
                uint t = d;
                d = c;
                c = b;
                b += RotL(a + f + m_buffer[g] + SineTable[i], ShiftsTable[i >> 4, i & 3]);
                a = t;
            }

            m_state[0] += a;
            m_state[1] += b;
            m_state[2] += c;
            m_state[3] += d;
        }
    }

    //Todo: 用了Buffer.BlockCopy，疑似只能在小端机上用
    public class MD5 : MD5Base
    {
        long m_bit_count;
        int m_buf_pos;

        public uint[] State { get { return m_state; } }

        public MD5()
        {
            Initialize();
        }

        public MD5(MD5StateStorage stateStorage)
        {
            m_state = stateStorage.md_buffer;
            m_bit_count  = stateStorage.bit_count;
            m_buf_pos = stateStorage.buf_count;
            m_buffer = stateStorage.input_buffer;
        }

        public void Initialize()
        {
            m_state[0] = 0x67452301;
            m_state[1] = 0xEFCDAB89;
            m_state[2] = 0x98BADCFE;
            m_state[3] = 0x10325476;
            m_bit_count = 0;
            m_buf_pos = 0;
        }

        public static MD5 Create()
        {
            return new MD5();
        }

        public static MD5 Create(MD5StateStorage stateStorage)
        {
            return new MD5(stateStorage);
        }

        public byte[] ComputeHash(byte[] data)
        {
            return ComputeHash(data, 0, data.Length);
        }

        public byte[] ComputeHash(byte[] data, int pos, int count)
        {
            Initialize();
            TransformBlock(data, pos, count);
            TransformFinalBlock();
            var hash = new byte[16];
            Buffer.BlockCopy(m_state, 0, hash, 0, 16);
            return hash;
        }

        public void TransformBlock(byte[] data, int pos, int count)
        {
            m_bit_count += (long)count << 3;

            if (m_buf_pos != 0)
            {
                int buf_count = 64 - m_buf_pos;
                if (count < buf_count)
                {
                    Buffer.BlockCopy(data, pos, m_buffer, m_buf_pos, count);
                    m_buf_pos += count;
                    return;
                }
                Buffer.BlockCopy(data, pos, m_buffer, m_buf_pos, buf_count);
                Transform();
                pos += buf_count;
                count -= buf_count;
                m_buf_pos = 0;
            }
            // data is processed in 64-byte chunks
            while (count >= 64)
            {
                Buffer.BlockCopy(data, pos, m_buffer, 0, 64);
                Transform();
                pos += 64;
                count -= 64;
            }
            if (count > 0)
            {
                Buffer.BlockCopy(data, pos, m_buffer, 0, count);
                m_buf_pos += count;
            }
        }

        public byte[] TransformFinalBlock()
        {
            Buffer.BlockCopy(Terminator, 0, m_buffer, m_buf_pos++, 1);
            int buf_count = 64 - m_buf_pos;

            if (buf_count < 8)
            {
                Buffer.BlockCopy(ZeroBytes, 0, m_buffer, m_buf_pos, buf_count);
                Transform();
                m_buf_pos = 0;
                buf_count = 64;
            }
            Buffer.BlockCopy(ZeroBytes, 0, m_buffer, m_buf_pos, buf_count - 8);
            m_buffer[14] = (uint)m_bit_count;
            m_buffer[15] = (uint)(m_bit_count >> 32);
            Transform();
            m_buf_pos = 0;

            var hash = new byte[16];
            Buffer.BlockCopy(m_state, 0, hash, 0, 16);
            return hash;
        }

        public MD5StateStorage GetValue()
        {
            return new MD5StateStorage()
            {
                md_buffer = m_state,
                bit_count = m_bit_count,
                buf_count = m_buf_pos,
                input_buffer = m_buffer,
            };
        }

        static readonly byte[] Terminator = new byte[1] { 0x80 };
        static readonly byte[] ZeroBytes = new byte[56];
    }

    public class MD5StateStorage
    {
        public uint[] md_buffer { get; set; }
        public long bit_count { get; set; }
        public int buf_count { get; set; }
        public uint[] input_buffer { get; set; }
    }
}
