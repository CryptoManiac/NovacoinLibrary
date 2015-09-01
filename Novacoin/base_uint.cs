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

namespace Novacoin
{
    public class base_uint : IComparable<base_uint>, IEquatable<base_uint>
    {
        protected int nWidth;
        protected uint[] pn;

        public double getDouble()
        {
            double ret = 0.0;
            double fact = 1.0;

            for (int i = 0; i < nWidth; i++)
            {
                ret += fact * pn[i];
                fact *= 4294967296.0;
            }

            return ret;
        }

        public ulong GetLow64()
        {
            return pn[0] | (ulong)pn[1] << 32;
        }

        public uint GetLow32()
        {
            return pn[0];
        }

        public int Size
        {
            get
            {
                return nWidth;
            }
        }


        public static bool operator !(base_uint a)
        {
            for (int i = 0; i < a.nWidth; i++)
            {
                if (a.pn[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static base_uint operator ~(base_uint a)
        {
            var ret = new base_uint();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
        }

        public static base_uint operator -(base_uint a)
        {
            var ret = new base_uint();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            ret++;
            return ret;
        }


        public static base_uint operator ++(base_uint a)
        {
            int i = 0;
            while (++a.pn[i] == 0 && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static base_uint operator --(base_uint a)
        {
            int i = 0;
            while (--a.pn[i] == uint.MaxValue && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static base_uint operator ^(base_uint a, base_uint b)
        {
            var result = new base_uint();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] ^ b.pn[i];
            }
            return result;
        }

        public static base_uint operator +(base_uint a, base_uint b)
        {
            var result = new base_uint();
            ulong carry = 0;
            for (int i = 0; i < result.nWidth; i++)
            {
                ulong n = carry + a.pn[i] + b.pn[i];
                result.pn[i] = (uint)(n & 0xffffffff);
                carry = n >> 32;
            }
            return result;
        }

        public static base_uint operator -(base_uint a, base_uint b)
        {
            return a + (-b);
        }

        public static base_uint operator &(base_uint a, base_uint b)
        {
            var result = new base_uint();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] & b.pn[i];
            }
            return result;
        }

        public static base_uint operator |(base_uint a, base_uint b)
        {
            var result = new base_uint();
            result.pn = new uint[a.nWidth];
            for (int i = 0; i < result.nWidth; i++)
            {
                result.pn[i] = a.pn[i] | b.pn[i];
            }
            return result;
        }

        public static bool operator <(base_uint a, base_uint b)
        {
            for (int i = a.nWidth - 1; i >= 0; i--)
            {
                if (a.pn[i] < b.pn[i])
                {
                    return true;
                }
                else if (a.pn[i] > b.pn[i])
                {
                    return false;
                }
            }
            return false;
        }

        public static bool operator <=(base_uint a, base_uint b)
        {
            for (int i = a.nWidth - 1; i >= 0; i--)
            {
                if (a.pn[i] < b.pn[i])
                {
                    return true;
                }
                else if (a.pn[i] > b.pn[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >(base_uint a, base_uint b)
        {
            for (int i = a.nWidth - 1; i >= 0; i--)
            {
                if (a.pn[i] > b.pn[i])
                {
                    return true;
                }
                else if (a.pn[i] < b.pn[i])
                {
                    return false;
                }
            }
            return false;
        }

        public static bool operator >=(base_uint a, base_uint b)
        {
            for (int i = a.nWidth - 1; i >= 0; i--)
            {
                if (a.pn[i] > b.pn[i])
                {
                    return true;
                }
                else if (a.pn[i] < b.pn[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(base_uint a, base_uint b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            for (int i = 0; i < a.nWidth; i++)
            {
                if (a.pn[i] != b.pn[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(base_uint a, ulong b)
        {
            if (a.pn[0] != (uint)b)
            {
                return false;
            }
            if (a.pn[1] != (uint)(b >> 32))
            {
                return false;
            }
            for (int i = 2; i < a.nWidth; i++)
            {
                if (a.pn[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(base_uint a, base_uint b)
        {
            return (!(a == b));
        }

        public static bool operator !=(base_uint a, ulong b)
        {
            return (!(a == b));
        }

        public static bool operator true(base_uint a)
        {
            return (a != 0);
        }

        public static bool operator false(base_uint a)
        {
            return (a == 0);
        }

        public static implicit operator byte[] (base_uint a)
        {
            var result = new byte[a.nWidth];
            for (int i = 0; i < a.nWidth; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(a.pn[i]), 0, result, 4 * i, 4);
            }
            return result;
        }

        private static bool ArraysEqual(uint[] a, uint[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                foreach (var element in pn)
                {
                    hash = hash * 31 + element.GetHashCode();
                }
            }
            return hash;
        }

        public int CompareTo(base_uint item)
        {
            if (this > item)
            {
                return 1;
            }
            else if (this < item)
            {
                return -1;
            }

            return 0;
        }

        public bool Equals(base_uint a)
        {
            if (a == null)
            {
                return false;
            }

            return ArraysEqual(pn, a.pn);
        }
    }
}
