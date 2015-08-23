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

namespace Novacoin
{
	/// <summary>
	/// Representation of Double SHA-256 hash
	/// </summary>
    public class Hash256 : Hash
    {
        // 32 bytes
        public override int hashSize
        {
            get { return 32; }
        }

        public Hash256() : base() { }
        public Hash256(byte[] bytes, int offset=0) : base(bytes, offset) { }
        public Hash256(IEnumerable<byte> bytes, int skip=0) : base(bytes, skip) { }
        public Hash256(Hash256 h) : base(h) { }


        public static Hash256 Compute256(IEnumerable<byte> inputBytes)
        {
            var dataBytes = inputBytes.ToArray();
            var digest1 = _hasher256.ComputeHash(dataBytes, 0, dataBytes.Length);
            var digest2 = _hasher256.ComputeHash(digest1, 0, digest1.Length);

            return new Hash256(digest2);
        }
    }
}

