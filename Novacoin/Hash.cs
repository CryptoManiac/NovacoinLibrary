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
using System.Linq;

namespace Novacoin
{
    public abstract class Hash
    {
        /// <summary>
        /// Array of digest bytes.
        /// </summary>
        protected byte[] _hashBytes = null;

        /// <summary>
        /// Hash size, must be overriden
        /// </summary>
        public abstract int hashSize 
        {
            get; 
        }

        /// <summary>
        /// Initializes an empty instance of the Hash class.
        /// </summary>
        public Hash()
        {
            _hashBytes = new byte[hashSize];
        }

        /// <summary>
        /// Initializes a new instance of Hash class
        /// </summary>
        /// <param name="bytesList">Array of bytes</param>
        public Hash(byte[] bytes, int offset = 0)
        {
            _hashBytes = new byte[hashSize];
            Array.Copy(bytes, offset, _hashBytes, 0, hashSize);
        }

        /// <summary>
        /// Initializes a new instance of Hash class as a copy of another one
        /// </summary>
        /// <param name="bytesList">Instance of hash class</param>
        public Hash(Hash h)
        {
            _hashBytes = new byte[h.hashSize];
            h._hashBytes.CopyTo(_hashBytes, 0);
        }

        public bool IsZero
        {
            get { return !_hashBytes.Any(b => b != 0); }
        }

        public static implicit operator byte[](Hash h)
        {
            return h._hashBytes;
        }

        public override string ToString()
        {
            return Interop.ToHex(Interop.ReverseBytes(_hashBytes));
        }
    }
}
