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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novacoin
{
    /// <summary>
    /// Base class for uint256 and uint160.
    /// </summary>
    public class base_uint : IComparable<base_uint>, IEquatable<base_uint>, IEqualityComparer<base_uint>
    {
        #region Internal representation
        /// <summary>
        /// Length of internal representation
        /// </summary>
        protected int nWidth;
        /// <summary>
        /// Big numbers are stored as array of unsigned 32-bit integers.
        /// </summary>
        protected uint[] pn;
        #endregion

        #region Helper properties
        public double Double
        {
            get
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
        }

        public ulong Low64
        {
            get { return pn[0] | (ulong)pn[1] << 32; }
        }

        public uint Low32
        {
            get { return pn[0]; }
        }

        /// <summary>
        /// Total size in bytes.
        /// </summary>
        public int Size
        {
            get
            {
                return nWidth * sizeof(uint);
            }
        }

        /// <summary>
        /// Zero or the position of highest non-zero bit plus one.
        /// </summary>
        public int bits
        {
            get
            {
                for (int pos = nWidth - 1; pos >= 0; pos--)
                {
                    if (pn[pos] != 0)
                    {
                        for (int bits = 31; bits > 0; bits--)
                        {
                            if ((pn[pos] & 1 << bits) != 0)
                                return 32 * pos + bits + 1;
                        }
                        return 32 * pos + 1;
                    }
                }
                return 0;
            }
        }
        #endregion


        /// <summary>
        /// Negation operator
        /// </summary>
        /// <param name="a">Value</param>
        /// <returns>True if value is zero, false otherwise.</returns>
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


        #region Comparison operations
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
        #endregion

        #region Equality operators
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
        #endregion

        #region Cast operations
        /// <summary>
        /// True cast operator
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static bool operator true(base_uint a)
        {
            return (a != 0);
        }

        /// <summary>
        /// False cast operator.
        /// </summary>
        /// <param name="a">Value</param>
        /// <returns>Boolean result</returns>
        public static bool operator false(base_uint a)
        {
            return (a == 0);
        }

        /// <summary>
        /// Imlicit byte[] cast operator.
        /// </summary>
        /// <param name="a">Value</param>
        public static implicit operator byte[] (base_uint a)
        {
            return Interop.LEBytes(a.pn);
        }
        #endregion

        /// <summary>
        /// Arrays equality checking helper method.
        /// </summary>
        /// <param name="a">Array 1</param>
        /// <param name="b">Array 2</param>
        /// <returns>Result.</returns>
        private static bool ArraysEqual(uint[] a, uint[] b)
        {
            Contract.Requires<ArgumentException>(a.Length == b.Length, "Array length mismatch.");

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }


        #region IEqualityComparer
        public bool Equals(base_uint a, base_uint b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            return ArraysEqual(a.pn, b.pn);
        }

        public int GetHashCode(base_uint a)
        {
            return a.GetHashCode();
        }

        #endregion

        #region IComparable
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
        #endregion

        #region IEquatable
        public bool Equals(base_uint a)
        {
            if (object.ReferenceEquals(a, null))
            {
                return false;
            }

            return ArraysEqual(pn, a.pn);
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

        public override bool Equals(object o)
        {
            return Equals(o as base_uint);
        }
        #endregion

        /// <summary>
        /// Serialize to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Interop.ToHex(Interop.ReverseBytes(this));
        }
    }
}
