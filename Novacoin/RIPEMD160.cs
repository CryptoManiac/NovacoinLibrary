/**
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

using Org.BouncyCastle.Crypto.Digests;
using System.Collections.Generic;
using System.Linq;

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
        // private static readonly RIPEMD160Managed _hasher160 = new RIPEMD160Managed();
        private static RipeMD160Digest _hasher160 = new RipeMD160Digest();
        
        // 20 bytes
        public override int hashSize
        {
            get { return _hasher160.GetDigestSize(); }
        }

        public RIPEMD160() : base() { }
        public RIPEMD160(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public RIPEMD160(RIPEMD160 h) : base(h) { }

        public static RIPEMD160 Compute160(byte[] inputBytes)
        {
            var digest1 = new byte[_hasher160.GetDigestSize()];
            _hasher160.BlockUpdate(inputBytes, 0, inputBytes.Length);
            _hasher160.DoFinal(digest1, 0);

            return new RIPEMD160(digest1);
        }
    }
}

