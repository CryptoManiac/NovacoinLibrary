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
    public class CKeyPair
    {
        private BigInteger D;
        private ECPoint Q;

        private static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
        private static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        /// <summary>
        /// Initialize new CKeyPair instance with random secret.
        /// </summary>
        public CKeyPair()
        {
            ECKeyGenerationParameters genParams = new ECKeyGenerationParameters(domain, new SecureRandom());

            ECKeyPairGenerator generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(genParams);
            AsymmetricCipherKeyPair ecKeyPair = generator.GenerateKeyPair();

            Q = ((ECPublicKeyParameters)ecKeyPair.Public).Q;
            D = ((ECPrivateKeyParameters)ecKeyPair.Private).D;
        }

        /// <summary>
        /// Init key pair using secret sequence of bytes
        /// </summary>
        /// <param name="secretBytes">Byte sequence</param>
        public CKeyPair(IEnumerable<byte> secretBytes)
        {
            D = new BigInteger(secretBytes.ToArray());
            Q = curve.G.Multiply(D);
        }

        /// <summary>
        /// Secret part of key pair
        /// </summary>
        public IEnumerable<byte> Secret
        {
            get { return D.ToByteArray(); }
        }

        /// <summary>
        /// Public part of key pair
        /// </summary>
        public IEnumerable<byte> Public
        {
            get { return Q.GetEncoded(); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CKeyPair(Secret={0}, Public={1})", Interop.ToHex(Secret), Interop.ToHex(Public));

            return sb.ToString();
        }
    }
}
