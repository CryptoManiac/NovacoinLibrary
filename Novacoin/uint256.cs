using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class uint256 : base_uint
    {
        new public readonly int nWidth = 8;

        public uint256()
        {
            pn = new uint[nWidth];

            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint256(uint256 b)
        {
            pn = new uint[nWidth];

            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = b.pn[i];
            }
        }


        public uint256(ulong n)
        {
            pn = new uint[nWidth];

            pn[0] = (uint)n;
            pn[1] = (uint)(n >> 32);
            for (int i = 2; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint256(byte[] bytes)
        {
            Contract.Requires<ArgumentException>(bytes.Length == 32, "Incorrect array length");

            pn = Interop.ToUInt32Array(bytes);
        }

        public uint256(string hex)
        {
            Contract.Requires<ArgumentException>(hex.Length == 64, "Incorrect string");

            var bytes = Interop.ReverseBytes(Interop.HexToArray(hex));
            pn = Interop.ToUInt32Array(bytes);
        }


        public static uint256 operator ~(uint256 a)
        {
            var ret = new uint256();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
        }

        public static uint256 operator -(uint256 a)
        {
            var ret = new uint256();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            ret++;
            return ret;
        }


        public static uint256 operator ++(uint256 a)
        {
            int i = 0;
            while (++a.pn[i] == 0 && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static uint256 operator --(uint256 a)
        {
            int i = 0;
            while (--a.pn[i] == uint.MaxValue && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static uint256 operator ^(uint256 a, uint256 b)
        {
            var result = new uint256();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] ^ b.pn[i];
            }
            return result;
        }

        public static uint256 operator +(uint256 a, uint256 b)
        {
            var result = new uint256();
            ulong carry = 0;
            for (int i = 0; i < result.nWidth; i++)
            {
                ulong n = carry + a.pn[i] + b.pn[i];
                result.pn[i] = (uint)(n & 0xffffffff);
                carry = n >> 32;
            }
            return result;
        }

        public static uint256 operator +(uint256 a, ulong b)
        {
            return a + new uint256(b);
        }

        public static uint256 operator -(uint256 a, uint256 b)
        {
            return a + (-b);
        }

        public static uint256 operator -(uint256 a, ulong b)
        {
            return a - new uint256(b);
        }

        public static uint256 operator &(uint256 a, uint256 b)
        {
            var result = new uint256();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] & b.pn[i];
            }
            return result;
        }

        public static uint256 operator |(uint256 a, uint256 b)
        {
            var result = new uint256();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] | b.pn[i];
            }
            return result;
        }

        public static uint256 operator <<(uint256 a, int shift)
        {
            var result = new uint256();
            int k = shift / 32;
            shift = shift % 32;

            for (int i = 0; i < a.nWidth; i++)
            {
                if (i + k + 1 < a.nWidth && shift != 0)
                {
                    result.pn[i + k + 1] |= (a.pn[i] >> (32 - shift));
                }

                if (i + k < a.nWidth)
                {
                    result.pn[i + k] |= (a.pn[i] << shift);
                }
            }

            return result;
        }

        public static uint256 operator >>(uint256 a, int shift)
        {
            var result = new uint256();
            int k = shift / 32;
            shift = shift % 32;

            for (int i = 0; i < a.nWidth; i++)
            {
                if (i - k - 1 >= 0 && shift != 0)
                {
                    result.pn[i - k - 1] |= (a.pn[i] << (32 - shift));
                }

                if (i - k >= 0)
                {
                    result.pn[i - k] |= (a.pn[i] >> shift);
                }
            }

            return result;
        }
    }
}
