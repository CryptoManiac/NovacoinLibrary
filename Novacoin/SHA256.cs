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

using Org.BouncyCastle.Crypto.Digests;

namespace Novacoin
{
    /// <summary>
    /// Representation of SHA-256 hash
    /// </summary>
    public class SHA256 : Hash
    {
        private static Sha256Digest _hasher256 = new Sha256Digest();

        // 32 bytes
        public override int hashSize
        {
            get { return _hasher256.GetDigestSize(); }
        }

        public SHA256() : base() { }
        public SHA256(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public SHA256(SHA256 h) : base(h) { }


        public static SHA256 Compute256(byte[] inputBytes)
        {
            var digest1 = new byte[_hasher256.GetDigestSize()];

            _hasher256.BlockUpdate(inputBytes, 0, inputBytes.Length);
            _hasher256.DoFinal(digest1, 0);

            return new SHA256(digest1);
        }
    }
}

