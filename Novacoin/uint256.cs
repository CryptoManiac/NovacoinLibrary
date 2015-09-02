using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Novacoin
{
    public class uint256 : base_uint
    {
        new protected int nWidth {
            get { return base.nWidth; }
            private set { base.nWidth = value; }
        }
        new protected uint[] pn {
            get { return base.pn; }
            private set { base.pn = value; }
        }

        public uint256()
        {
            nWidth = 8;
            pn = new uint[nWidth];
        }

        public uint256(uint256 b) : this()
        {
            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = b.pn[i];
            }
        }

        public uint256(ulong n) : this()
        {
            pn[0] = (uint)n;
            pn[1] = (uint)(n >> 32);
            for (int i = 2; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint256(byte[] bytes) : this()
        {
            Contract.Requires<ArgumentException>(bytes.Length == 32, "Incorrect array length");

            pn = Interop.ToUInt32Array(bytes);
        }

        public uint256(string hex) : this()
        {
            Contract.Requires<ArgumentException>(hex.Length == 64, "Incorrect string");

            var bytes = Interop.ReverseBytes(Interop.HexToArray(hex));
            pn = Interop.ToUInt32Array(bytes);
        }

        /// <summary>
        /// Compact representation of unsigned 256bit numbers.
        /// 
        /// N = (-1^sign) * m * 256^(exp-3)
        /// 
        /// http://bitcoin.stackexchange.com/questions/30467/what-are-the-equations-to-convert-between-bits-and-difficulty
        /// </summary>
        public uint Compact
        {
            get
            {
                int nSize = (bits + 7) / 8;
                uint nCompact = 0;
                if (nSize <= 3)
                    nCompact = ((uint)GetLow64()) << 8 * (3 - nSize);
                else
                {
                    uint256 bn = this >> 8 * (nSize - 3);
                    nCompact = (uint)bn.GetLow64();
                }

                if ((nCompact & 0x00800000) != 0)
                {
                    nCompact >>= 8;
                    nSize++;
                }

                Contract.Assert((nCompact & ~0x007fffff) == 0);
                Contract.Assert(nSize < 256);

                nCompact |= (uint)nSize << 24;
                nCompact |= 0;

                return nCompact;
            }
            set {
                int nSize = (int)value >> 24;
                uint nWord = value & 0x007fffff;

                uint256 i;

                if (nSize <= 3)
                {
                    nWord >>= 8 * (3 - nSize);
                    i = new uint256(nWord);
                }
                else
                {
                    i = new uint256(nWord);
                    i <<= 8 * (nSize - 3);
                }

                pn = i.pn;
            }
        }

        private void SetBytes(byte[] bytes)
        {
            pn = Interop.ToUInt32Array(Interop.ReverseBytes(bytes));
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
