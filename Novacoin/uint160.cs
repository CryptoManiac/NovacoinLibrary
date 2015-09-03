﻿/**
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

namespace Novacoin
{
    public class uint160 : base_uint
    {
        #region Access to internal representation
        new protected int nWidth
        {
            get { return base.nWidth; }
            private set { base.nWidth = value; }
        }
        new protected uint[] pn
        {
            get { return base.pn; }
            private set { base.pn = value; }
        }
        #endregion

        #region Constructors
        public uint160()
        {
            nWidth = 5;
            pn = new uint[nWidth];
        }

        public uint160(uint160 b) : this()
        {
            for (int i = 0; i < nWidth; i++)
            {
                pn[i] = b.pn[i];
            }
        }


        public uint160(ulong n) : this()
        {
            pn[0] = (uint)n;
            pn[1] = (uint)(n >> 32);
            for (int i = 2; i < nWidth; i++)
            {
                pn[i] = 0;
            }
        }

        public uint160(byte[] bytes) : this()
        {
            Contract.Requires<ArgumentException>(bytes.Length == 20, "Incorrect array length");
            pn = Interop.ToUInt32Array(bytes);
        }

        public uint160(string hex) : this()
        {
            Contract.Requires<ArgumentException>(hex.Length == 40, "Incorrect string");
            var bytes = Interop.ReverseBytes(Interop.HexToArray(hex));
            pn = Interop.ToUInt32Array(bytes);
        }
        #endregion

        #region Cast operators
        public static implicit operator uint160(byte[] bytes)
        {
            return new uint160(bytes);
        }

        public static implicit operator uint160(ulong n)
        {
            return new uint160(n);
        }
        #endregion

        #region Bitwise operations
        public static uint160 operator ~(uint160 a)
        {
            var ret = new uint160();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
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
        #endregion

        #region Basic arithmetics
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

        public static uint160 operator /(uint160 a, uint b)
        {
            var result = new uint160();

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

        #endregion

        #region Shift
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
        #endregion
    }
}
