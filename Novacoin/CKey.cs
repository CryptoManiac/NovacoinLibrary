using System.Collections.Generic;
using System.Linq;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Sec;

using Org.BouncyCastle.Math.EC;


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
