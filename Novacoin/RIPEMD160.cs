﻿/**
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Novacoin
{
    /// <summary>
    /// Representation of RIPEMD-160 hash.
    /// </summary>
    public class RIPEMD160 : Hash
    {
        /// <summary>
        /// Computes RIPEMD160 hash using managed library
        /// </summary>
        private static readonly RIPEMD160Managed _hasher160 = new RIPEMD160Managed();

        // 20 bytes
        public override int hashSize
        {
            get { return 20; }
        }

        public RIPEMD160() : base() { }
        public RIPEMD160(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public RIPEMD160(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public RIPEMD160(RIPEMD160 h) : base(h) { }

        public static RIPEMD160 Compute160(IEnumerable<byte> inputBytes)
        {
            var dataBytes = inputBytes.ToArray();
            var digest1 = _hasher160.ComputeHash(dataBytes, 0, dataBytes.Length);

            return new RIPEMD160(digest1);
        }
    }
}

