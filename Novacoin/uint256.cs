/**
 *  Novacoin classes library
 *  Copyright (C) 2015 Alex D. (balthazar.ad@gmail.com)

 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.

 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Novacoin
{
    public class uint256 : base_uint
    {
        #region Access to internal representation
        new protected int nWidth {
            get { return base.nWidth; }
            private set { base.nWidth = value; }
        }
        new protected uint[] pn {
            get { return base.pn; }
            private set { base.pn = value; }
        }
        #endregion

        #region Constructors
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
        #endregion

        #region Cast operators
        public static implicit operator uint256(byte[] bytes)
        {
            return new uint256(bytes);
        }

        public static implicit operator uint256(ulong n)
        {
            return new uint256(n);
        }
        #endregion

        #region Compact representation
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
                    nCompact = ((uint)Low64) << 8 * (3 - nSize);
                else
                {
                    uint256 bn = this >> 8 * (nSize - 3);
                    nCompact = (uint)bn.Low64;
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
        #endregion

        #region Bitwise operations
        public static uint256 operator ~(uint256 a)
        {
            var ret = new uint256();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
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
        #endregion

        #region Basic arithmetics
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

        public static uint256 operator /(uint256 a, uint b)
        {
            var result = new uint256();

            ulong r = 0;
            int i = a.nWidth;

            while (i-- > 0)
            {
                r <<= 32;
                r |= a.pn[i];
                result.pn[i] = (uint)(r / b);
                r %= b;
            }

            return result;
        }

        public static uint256 operator *(uint256 a, ulong b)
        {
            var result = new uint256();

            ulong c = 0;
            uint i = 0;

            do
            {
                c += a.pn[i] * b;
                result.pn[i] = (uint)c;
                c >>= 32;
            } while (++i < result.nWidth);

            return result;
        }

        public static uint256 operator *(uint256 a, uint256 b)
        {
            if (!a || !b)
            {
                // Multiplication by zero results with zero.
                return 0;
            }
            else if (b.bits <= 32)
            {
                if (b.pn[0] == 1)
                {
                    // If right is 1 then return left operand value
                    return a;
                }

                return a * b.pn[0];
            }
            else if (a.bits <= 32)
            {
                if (a.pn[0] == 1)
                {
                    // If left is 1 then return right operand value
                    return b;
                }

                return a * b.pn[0];
            }

            int m = a.bits / 32 + (a.bits % 32 != 0 ? 1 : 0);
            int n = b.bits / 32 + (b.bits % 32 != 0 ? 1 : 0);

            uint256 result = new uint256();

            uint[] left = a.pn.Take(m).ToArray();
            uint[] right = b.pn.Take(n).ToArray();

            for (int i = 0; i < m; ++i)
            {
                uint ai = left[i];
                int k = i;

                ulong temp = 0;
                for (int j = 0; j < n; ++j)
                {
                    temp = temp + ((ulong)ai) * right[j] + result.pn[k];
                    result.pn[k++] = (uint)temp;
                    temp >>= 32;
                }

                while (temp != 0)
                {
                    temp += result.pn[k];
                    result.pn[k++] = (uint)temp;
                    temp >>= 32;
                }
            }

            return result;
        }

        public static uint operator %(uint256 a, uint b)
        {
            ulong r = 0;
            int i = a.nWidth;

            while (i-- > 0)
            {
                r <<= 32;
                r |= a.pn[i];
                r %= b;
            }

            return (uint)r;
        }

        public static uint256 operator /(uint256 a, uint256 b)
        {
            if (b.bits <= 32)
            {
                return a / b.Low32;
            }

            uint256 result = new uint256();

            uint[] quotient;
            uint[] remainder_value;

            int m = a.bits / 32 + (a.bits % 32 != 0 ? 1 : 0);
            int n = b.bits / 32 + (b.bits % 32 != 0 ? 1 : 0);

            BignumHelper.DivModUnsigned(a.pn.Take(m).ToArray(), b.pn.Take(n).ToArray(), out quotient, out remainder_value);

            quotient.CopyTo(result.pn, 0);

            return result;
        }

        public static uint256 operator %(uint256 a, uint256 b)
        {
            if (b.bits <= 32)
            {
                return a % b.Low32;
            }

            uint256 result = new uint256();

            uint[] quotient;
            uint[] remainder_value;

            int m = a.bits / 32 + (a.bits % 32 != 0 ? 1 : 0);
            int n = b.bits / 32 + (b.bits % 32 != 0 ? 1 : 0);

            BignumHelper.DivModUnsigned(a.pn.Take(m).ToArray(), b.pn.Take(n).ToArray(), out quotient, out remainder_value);

            remainder_value.CopyTo(result.pn, 0);

            return result;

        }
        #endregion

        #region Shift
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
        #endregion
    }
}
