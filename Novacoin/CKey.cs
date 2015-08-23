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

using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

using System.Collections.Generic;
using System.Linq;


namespace Novacoin
{
    /// <summary>
    /// Basic pubkey functionality
    /// </summary>
    public abstract class CKey
    {
        // These fields are inherited by CPubKey and CKeyPair
        protected ECPublicKeyParameters _Public;

        protected static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
        protected static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        /// <summary>
        /// Regenerate public key parameters (ECPoint compression)
        /// </summary>
        /// <param name="pubKeyParams">Non-compressed key parameters</param>
        /// <returns>Parameters for compressed key</returns>
        protected ECPublicKeyParameters Compress(ECPublicKeyParameters pubKeyParams)
        {
            if (pubKeyParams.Q.IsCompressed)
            {
                // Already compressed
                return pubKeyParams;
            }

            ECPoint q = new FpPoint(curve.Curve, pubKeyParams.Q.X, pubKeyParams.Q.Y, true);

            return new ECPublicKeyParameters(q, domain);
        }

        /// <summary>
        /// Regenerate public key parameters (ECPoint decompression)
        /// </summary>
        /// <param name="pubKeyParams">Compressed key parameters</param>
        /// <returns>Parameters for non-compressed key</returns>
        protected ECPublicKeyParameters Decompress(ECPublicKeyParameters pubKeyParams)
        {
            if (!pubKeyParams.Q.IsCompressed)
            {
                // Isn't compressed
                return pubKeyParams;
            }

            ECPoint q = new FpPoint(curve.Curve, pubKeyParams.Q.X, pubKeyParams.Q.Y, false);

            return new ECPublicKeyParameters(q, domain);
        }

        /// <summary>
        /// Does the signature matches our public key?
        /// </summary>
        /// <param name="sigHash">Data hash</param>
        /// <param name="signature">Signature bytes</param>
        /// <returns>Checking result</returns>
        public bool VerifySignature(Hash sigHash, IEnumerable<byte> signature)
        {
            ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(false, _Public);
            signer.BlockUpdate(sigHash.hashBytes, 0, sigHash.hashSize);

            return signer.VerifySignature(signature.ToArray());
        }

        /// <summary>
        /// Calculate Hash160 and create new CKeyID instance.
        /// </summary>
        /// <returns>New key ID</returns>
        public CKeyID KeyID
        {
            get { return new CKeyID(Hash160.Compute160(PublicBytes)); }
        }

        /// <summary>
        /// PublicBytes part of key pair
        /// </summary>
        public IEnumerable<byte> PublicBytes
        {
            get { return _Public.Q.GetEncoded(); }
        }

        /// <summary>
        /// Is this a compressed public key?
        /// </summary>
        /// <returns></returns>
        public bool IsCompressed
        {
            get { return _Public.Q.IsCompressed; }
        }

    }
}
