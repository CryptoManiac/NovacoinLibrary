using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Sec;

namespace Novacoin
{
    /// <summary>
    /// Representation of ECDSA public key
    /// </summary>
    public class CPubKey
    {
        private ECPoint Q;
        private static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
        private static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        /// <summary>
        /// Initializes a new instance of CPubKey class as the copy of another instance
        /// </summary>
        /// <param name="pubKey">Another CPubKey instance</param>
        public CPubKey(CPubKey pubKey)
        {
            Q = pubKey.Q;
        }

        /// <summary>
        /// Initializes a new instance of CPubKey class using supplied sequence of bytes
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        public CPubKey(IEnumerable<byte> bytes)
        {
            Q = ((ECPublicKeyParameters)PublicKeyFactory.CreateKey(bytes.ToArray())).Q;
        }

        public CPubKey(ECPoint pQ)
        {
            Q = pQ;
        }

        /// <summary>
        /// Quick validity test
        /// </summary>
        /// <returns>Validation result</returns>
        public bool IsValid
        {
            get { return !Q.IsInfinity; }
        }

        /// <summary>
        /// Is this a compressed public key?
        /// </summary>
        /// <returns></returns>
        public bool IsCompressed
        {
            get { return Q.IsCompressed; }
        }

        /// <summary>
        /// Calculate Hash160 and create new CKeyID instance.
        /// </summary>
        /// <returns>New key ID</returns>
        public CKeyID GetKeyID()
        {
            return new CKeyID(Hash160.Compute160(Raw));
        }

        public bool Verify(IEnumerable<byte> data, IEnumerable<byte> signature)
        {
            byte[] dataBytes = data.ToArray();

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            ECPublicKeyParameters keyParameters = new ECPublicKeyParameters(Q, domain);
            signer.Init(false, keyParameters);
            signer.BlockUpdate(dataBytes, 0, dataBytes.Length);

            return signer.VerifySignature(signature.ToArray());
        }

        /// <summary>
        /// Accessor for internal representation
        /// </summary>
        public IEnumerable<byte> Raw
        {
            get { return Q.GetEncoded(); }
        }

        public override string ToString()
        {
            return Interop.ToHex(Raw);
        }
    }
}
