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
using Org.BouncyCastle.Crypto.Parameters;

namespace Novacoin
{
    /// <summary>
    /// Representation of ECDSA public key
    /// </summary>
    public class CPubKey : CKey
    {
        /// <summary>
        /// Initializes a new instance of CPubKey class as the copy of another instance
        /// </summary>
        /// <param name="pubKey">Another CPubKey instance</param>
        public CPubKey(CPubKey pubKey)
        {
            _Public = pubKey._Public;
        }

        /// <summary>
        /// Initializes a new instance of CPubKey class using supplied sequence of bytes
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        public CPubKey(byte[] bytes)
        {
            var pQ = curve.Curve.DecodePoint(bytes);
            _Public = new ECPublicKeyParameters(pQ, domain);
        }

        /// <summary>
        /// Init with base58 encoded sequence of bytes
        /// </summary>
        /// <param name="strBase58"></param>
        public CPubKey(string strBase58)
        {
            var pQ = curve.Curve.DecodePoint(AddressTools.Base58DecodeCheck(strBase58));
            _Public = new ECPublicKeyParameters(pQ, domain);
        }

        /// <summary>
        /// Quick validity test
        /// </summary>
        /// <returns>Validation result</returns>
        public bool IsValid
        {
            get { return !_Public.Q.IsInfinity; }
        }

        public string ToHex()
        {
            return Interop.ToHex(this);
        }

        /// <summary>
        /// Public part of key pair
        /// </summary>
        public static implicit operator byte[] (CPubKey p)
        {
            return p._Public.Q.GetEncoded();
        }

        public override string ToString()
        {
            var r = new List<byte>();

            r.Add((byte)(AddrType.PUBKEY_ADDRESS));
            r.AddRange((byte[])this);

            return AddressTools.Base58EncodeCheck(r.ToArray());
        }
    }
}
