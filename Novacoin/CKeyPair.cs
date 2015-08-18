using System.Collections.Generic;
using System.Linq;
using System.Text;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;


namespace Novacoin
{
    public class CKeyPair : CKey
    {
        private ECPrivateKeyParameters _Private;

        /// <summary>
        /// Initialize new CKeyPair instance with random secret.
        /// </summary>
        public CKeyPair()
        {
            ECKeyGenerationParameters genParams = new ECKeyGenerationParameters(domain, new SecureRandom());
            ECKeyPairGenerator generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(genParams);
            AsymmetricCipherKeyPair ecKeyPair = generator.GenerateKeyPair();

            _Public = (ECPublicKeyParameters)ecKeyPair.Public;
            _Private = (ECPrivateKeyParameters)ecKeyPair.Private;
        }

        /// <summary>
        /// Init key pair using secret sequence of bytes
        /// </summary>
        /// <param name="secretBytes">Byte sequence</param>
        public CKeyPair(IEnumerable<byte> secretBytes)
        {
            // Deserialize secret value
            BigInteger D = new BigInteger(secretBytes.ToArray());

            // Calculate public key
            ECPoint Q = curve.G.Multiply(D);

            _Public = new ECPublicKeyParameters(Q, domain);
            _Private = new ECPrivateKeyParameters(D, domain);
        }

        /// <summary>
        /// Create signature for supplied data
        /// </summary>
        /// <param name="data">Data bytes sequence</param>
        /// <returns>Signature bytes sequence</returns>
        public IEnumerable<byte> Sign(IEnumerable<byte> data)
        {
            byte[] dataBytes = data.ToArray();

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true, _Private);
            signer.BlockUpdate(dataBytes, 0, dataBytes.Length);

            return signer.GenerateSignature();
        }

        public CPubKey GetPubKey()
        {
            return new CPubKey(Public);
        }

        /// <summary>
        /// Secret part of key pair
        /// </summary>
        public IEnumerable<byte> Secret
        {
            get { return _Private.D.ToByteArray(); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CKeyPair(Secret={0}, Public={1})", Interop.ToHex(Secret), Interop.ToHex(Public));

            return sb.ToString();
        }
    }
}
