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
using System.IO;
using System.Linq;
using System.Text;

namespace Novacoin
{
    public class COutPoint : IComparable<COutPoint>, IEquatable<COutPoint>, IEqualityComparer<COutPoint>
    {
        /// <summary>
        /// Hash of parent transaction.
        /// </summary>
        public uint256 hash;

        /// <summary>
        /// Parent input number.
        /// </summary>
        public uint n;

        /// <summary>
        /// Out reference is always 36 bytes long.
        /// </summary>
        public const int Size = 36;

        public COutPoint()
        {
            hash = new uint256();
            n = uint.MaxValue;
        }

        public COutPoint(uint256 hashIn, uint nIn)
        {
            hash = hashIn;
            n = nIn;
        }

        public COutPoint(COutPoint o)
        {
            hash = o.hash;
            n = o.n;
        }

        public COutPoint(byte[] bytes)
        {
            Contract.Requires<ArgumentException>(bytes.Length == 36, "Any valid outpoint reference data item is exactly 36 bytes long.");

            hash = bytes.Take(32).ToArray();
            n = BitConverter.ToUInt32(bytes, 32);
        }

        public bool IsNull
        {
            get { return !hash && n == uint.MaxValue; }
        }

        public static implicit operator byte[] (COutPoint o)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(o.hash);
            writer.Write(o.n);

            var outBytes = stream.ToArray();

            writer.Close();

            return outBytes;
        }

        public static implicit operator COutPoint(byte[] b)
        {
            return new COutPoint(b);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("COutPoint({0}, {1})", hash, n);

            return sb.ToString();
        }

        /// <summary>
        /// Compare this outpoint with some other.
        /// </summary>
        /// <param name="o">Other outpoint.</param>
        /// <returns>Result of comparison.</returns>
        public int CompareTo(COutPoint o)
        {
            if (n > o.n)
            {
                return 1;
            }
            else if (n < o.n)
            {
                return -1;
            }

            return 0;

        }

        /// <summary>
        /// Equality comparer for outpoints.
        /// </summary>
        /// <param name="o">Other outpoint.</param>
        /// <returns>Result of comparison.</returns>
        public bool Equals(COutPoint o)
        {
            if (object.ReferenceEquals(o, null))
            {
                return false;
            }

            return (o.n == n) && (o.hash == hash);
        }

        /// <summary>
        /// Equality comparer for outpoints.
        /// </summary>
        /// <param name="x">First outpoint.</param>
        /// <param name="y">Second outpoint.</param>
        /// <returns>Result of comparison.</returns>
        public bool Equals(COutPoint x, COutPoint y)
        {
            return (x.n == y.n) && (x.hash == y.hash);
        }

        public override int GetHashCode()
        {
            return n.GetHashCode() ^ hash.GetHashCode();
        }

        public int GetHashCode(COutPoint obj)
        {
            return obj.GetHashCode();
        }
    }

}
