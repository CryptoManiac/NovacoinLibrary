using System.Collections.Generic;
using System.Linq;
using System;

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
        public CKeyPair(bool Compressed=true)
        {
            ECKeyGenerationParameters genParams = new ECKeyGenerationParameters(domain, new SecureRandom());
            ECKeyPairGenerator generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(genParams);
            AsymmetricCipherKeyPair ecKeyPair = generator.GenerateKeyPair();

            _Private = (ECPrivateKeyParameters)ecKeyPair.Private;
            _Public = (ECPublicKeyParameters)ecKeyPair.Public;

            if (Compressed)
            {
                _Public = Compress(_Public);
            }
        }

        /// <summary>
        /// Init key pair using secret sequence of bytes
        /// </summary>
        /// <param name="secretBytes">Byte sequence</param>
        public CKeyPair(IEnumerable<byte> secretBytes, bool Compressed=true)
        {
            // Deserialize secret value
            BigInteger D = new BigInteger(secretBytes.ToArray());

            // Calculate public key
            ECPoint Q = curve.G.Multiply(D);

            _Private = new ECPrivateKeyParameters(D, domain);
            _Public = new ECPublicKeyParameters(Q, domain);

            if (Compressed)
            {
                _Public = Compress(_Public);
            }
        }

        public CKeyPair(string strBase58)
        {
            List<byte> rawBytes = AddressTools.Base58DecodeCheck(strBase58).ToList();
            rawBytes.RemoveAt(0); // Remove key version byte

            int nSecretLen = rawBytes[0] == 0x00 ? 33 : 32;
            int nTaggedSecretLen = nSecretLen + 1;

            if (rawBytes.Count > nTaggedSecretLen || rawBytes.Count < nSecretLen)
            {
                throw new FormatException("Invalid private key");
            }

            // Deserialize secret value
            BigInteger D = new BigInteger(rawBytes.Take(nSecretLen).ToArray());

            // Calculate public key
            ECPoint Q = curve.G.Multiply(D);

            _Private = new ECPrivateKeyParameters(D, domain);
            _Public = new ECPublicKeyParameters(Q, domain);

            if (rawBytes.Count == nTaggedSecretLen && rawBytes.Last() == 0x01) // Check compression tag
            {
                _Public = Compress(_Public);
            }
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

        public string ToHex()
        {
            List<byte> r = new List<byte>(Secret);
            
            if (IsCompressed)
            {
                r.Add(0x01);
            }

            return Interop.ToHex(r);
        }

        public override string ToString()
        {
            List<byte> r = new List<byte>();

            r.Add((byte)(128 + AddrType.PUBKEY_ADDRESS));

            r.AddRange(Secret);

            if (IsCompressed)
            {
                r.Add(0x01);
            }

            return AddressTools.Base58EncodeCheck(r);
        }
    }
}
