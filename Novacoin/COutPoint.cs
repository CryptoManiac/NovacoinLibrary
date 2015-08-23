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
using System.Text;

namespace Novacoin
{
    public class COutPoint
    {
        /// <summary>
        /// Hash of parent transaction.
        /// </summary>
        public Hash256 hash;

        /// <summary>
        /// Parent input number.
        /// </summary>
        public uint n;

        public COutPoint()
        {
            hash = new Hash256();
            n = uint.MaxValue;
        }

        public COutPoint(Hash256 hashIn, uint nIn)
        {
            hash = hashIn;
            n = nIn;
        }

        public COutPoint(COutPoint o)
        {
            hash = new Hash256(o.hash);
            n = o.n;
        }

        public COutPoint(byte[] bytes)
        {
            hash = new Hash256(bytes);
            n = BitConverter.ToUInt32(bytes, 32);
        }

        public bool IsNull
        {
            get { return hash.IsZero && n == uint.MaxValue; }
        }

        public IList<byte> Bytes
        {
            get
            {
                var r = new List<byte>();
                r.AddRange(hash.hashBytes);
                r.AddRange(BitConverter.GetBytes(n));

                return r;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("COutPoint({0}, {1})", hash.ToString(), n);

            return sb.ToString();
        }

        
    }

}
