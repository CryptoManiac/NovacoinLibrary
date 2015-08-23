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


using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Crypto.Digests;

namespace Novacoin
{
    /// <summary>
    /// Representation of SHA-256 hash
    /// </summary>
    public class SHA1 : Hash
    {
        /// <summary>
        /// Computes RIPEMD160 hash using managed library
        /// </summary>
        private static Sha1Digest _hasher1 = new Sha1Digest();

        // 32 bytes
        public override int hashSize
        {
            get { return _hasher1.GetDigestSize(); }
        }

        public SHA1() : base() { }
        public SHA1(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public SHA1(SHA1 h) : base(h) { }


        public static SHA1 Compute1(byte[] inputBytes)
        {
            var digest1 = new byte[_hasher1.GetDigestSize()];

            _hasher1.BlockUpdate(inputBytes, 0, inputBytes.Length);
            _hasher1.DoFinal(digest1, 0);

            return new SHA1(digest1);
        }
    }
}

