using System.Collections.Generic;
using System.Linq;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Sec;

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
        /// Does the signature matches our public key?
        /// </summary>
        /// <param name="data">Data bytes</param>
        /// <param name="signature">Signature bytes</param>
        /// <returns>Checking result</returns>
        public bool VerifySignature(IEnumerable<byte> data, IEnumerable<byte> signature)
        {
            byte[] dataBytes = data.ToArray();

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, _Public);
            signer.BlockUpdate(dataBytes, 0, dataBytes.Length);

            return signer.VerifySignature(signature.ToArray());
        }

        /// <summary>
        /// Calculate Hash160 and create new CKeyID instance.
        /// </summary>
        /// <returns>New key ID</returns>
        public CKeyID GetKeyID()
        {
            return new CKeyID(Hash160.Compute160(Public));
        }

        /// <summary>
        /// Public part of key pair
        /// </summary>
        public IEnumerable<byte> Public
        {
            get { return _Public.Q.GetEncoded(); }
        }
    }
}
