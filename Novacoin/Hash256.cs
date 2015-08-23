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
    /// Representation of Double SHA-256 hash
    /// </summary>
    public class Hash256 : Hash
    {
        private static Sha256Digest _hasher256 = new Sha256Digest();

        // 32 bytes
        public override int hashSize
        {
            get { return _hasher256.GetDigestSize(); }
        }

        public Hash256() : base() { }
        public Hash256(byte[] bytes, int offset=0) : base(bytes, offset) { }
        public Hash256(Hash256 h) : base(h) { }

        public static Hash256 Compute256(byte[] dataBytes)
        {
            var digest1 = new byte[32];
            var digest2 = new byte[32];

            _hasher256.BlockUpdate(dataBytes, 0, dataBytes.Length);            
            _hasher256.DoFinal(digest1, 0);
            _hasher256.BlockUpdate(digest1, 0, digest1.Length);            
            _hasher256.DoFinal(digest2, 0);

            return new Hash256(digest2);
        }

        public static Hash256 Compute256(ref byte[] input1, ref byte[] input2)
        {
            var digest1 = new byte[_hasher256.GetDigestSize()];
            var digest2 = new byte[_hasher256.GetDigestSize()];

            _hasher256.BlockUpdate(input1, 0, input1.Length);
            _hasher256.BlockUpdate(input2, 0, input2.Length);
            _hasher256.DoFinal(digest1, 0);

            _hasher256.BlockUpdate(digest1, 0, digest1.Length);
            _hasher256.DoFinal(digest2, 0);

            return new Hash256(digest2);
        }

        public static byte[] ComputeRaw256(byte[] dataBytes)
        {
            var digest1 = new byte[32];
            var digest2 = new byte[32];

            _hasher256.BlockUpdate(dataBytes, 0, dataBytes.Length);
            _hasher256.DoFinal(digest1, 0);
            _hasher256.BlockUpdate(digest1, 0, digest1.Length);
            _hasher256.DoFinal(digest2, 0);

            return digest2;
        }

        public static byte[] ComputeRaw256(ref byte[] input1, ref byte[] input2)
        {
            var digest1 = new byte[_hasher256.GetDigestSize()];
            var digest2 = new byte[_hasher256.GetDigestSize()];

            _hasher256.BlockUpdate(input1, 0, input1.Length);
            _hasher256.BlockUpdate(input2, 0, input2.Length);
            _hasher256.DoFinal(digest1, 0);

            _hasher256.BlockUpdate(digest1, 0, digest1.Length);
            _hasher256.DoFinal(digest2, 0);

            return digest2;
        }
    }
}
