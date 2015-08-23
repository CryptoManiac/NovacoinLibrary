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

using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Novacoin
{
    /// <summary>
    /// Representation of pubkey/script hash.
    /// </summary>
    public class Hash160 : Hash
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

        public Hash160() : base() { }
        public Hash160(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public Hash160(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public Hash160(Hash160 h) : base(h) { }

        public static Hash160 Compute160(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher256.ComputeHash(dataBytes, 0, dataBytes.Length);
            byte[] digest2 = _hasher160.ComputeHash(digest1, 0, digest1.Length);

            return new Hash160(digest2);
        }
	}
}

