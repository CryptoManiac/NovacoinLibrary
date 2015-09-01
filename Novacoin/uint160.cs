using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class uint160 : base_uint
    {
        new protected readonly int nWidth = 5;

        public uint160()
        {
            pn = new uint[nWidth];

            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint160(uint160 b)
        {
            pn = new uint[nWidth];

            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = b.pn[i];
            }
        }


        public uint160(ulong n)
        {
            pn = new uint[nWidth];

            pn[0] = (uint)n;
            pn[1] = (uint)(n >> 32);
            for (int i = 2; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint160(byte[] bytes)
        {
            Contract.Requires<ArgumentException>(bytes.Length == 20, "Incorrect array length");

            pn = Interop.ToUInt32Array(bytes);
        }

        public uint160(string hex)
        {
            Contract.Requires<ArgumentException>(hex.Length == 40, "Incorrect string");

            var bytes = Interop.ReverseBytes(Interop.HexToArray(hex));
            pn = Interop.ToUInt32Array(bytes);
        }

        public static uint160 operator ~(uint160 a)
        {
            var ret = new uint160();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
        }

        public static uint160 operator -(uint160 a)
        {
            var ret = new uint160();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            ret++;
            return ret;
        }


        public static uint160 operator ++(uint160 a)
        {
            int i = 0;
            while (++a.pn[i] == 0 && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static uint160 operator --(uint160 a)
        {
            int i = 0;
            while (--a.pn[i] == uint.MaxValue && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static uint160 operator ^(uint160 a, uint160 b)
        {
            var result = new uint160();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] ^ b.pn[i];
            }
            return result;
        }

        public static uint160 operator +(uint160 a, uint160 b)
        {
            var result = new uint160();
            ulong carry = 0;
            for (int i = 0; i < result.nWidth; i++)
            {
                ulong n = carry + a.pn[i] + b.pn[i];
                result.pn[i] = (uint)(n & 0xffffffff);
                carry = n >> 32;
            }
            return result;
        }

        public static uint160 operator +(uint160 a, ulong b)
        {
            return a + new uint160(b);
        }

        public static uint160 operator -(uint160 a, uint160 b)
        {
            return a + (-b);
        }

        public static uint160 operator -(uint160 a, ulong b)
        {
            return a - new uint160(b);
        }

        public static uint160 operator &(uint160 a, uint160 b)
        {
            var result = new uint160();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] & b.pn[i];
            }
            return result;
        }

        public static uint160 operator |(uint160 a, uint160 b)
        {
            var result = new uint160();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] | b.pn[i];
            }
            return result;
        }

        public static uint160 operator <<(uint160 a, int shift)
        {
            var result = new uint160();
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

        public static uint160 operator >>(uint160 a, int shift)
        {
            var result = new uint160();
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
