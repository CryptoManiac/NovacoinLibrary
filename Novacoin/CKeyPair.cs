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
        /// <param name="Compressed">Compression flag</param>
        public CKeyPair(IEnumerable<byte> secretBytes, bool Compressed=true)
        {
            // Deserialize secret value
            BigInteger D = new BigInteger(secretBytes.Take(32).ToArray());

            if (D.SignValue == -1)
            {
                List<byte> fixedKeyBytes = secretBytes.Take(32).ToList();
                fixedKeyBytes.Insert(0, 0x00); // prepend with sign byte

                D = new BigInteger(fixedKeyBytes.ToArray());
            }

            // Calculate public key
            ECPoint Q = curve.G.Multiply(D);

            _Private = new ECPrivateKeyParameters(D, domain);
            _Public = new ECPublicKeyParameters(Q, domain);

            if (Compressed)
            {
                _Public = Compress(_Public);
            }
        }

        /// <summary>
        /// Init key pair using secret sequence of bytes
        /// </summary>
        /// <param name="secretBytes">Byte sequence</param>
        public CKeyPair(IEnumerable<byte> secretBytes) : 
            this (secretBytes.Take(32), (secretBytes.Count() == 33 && secretBytes.Last() == 0x01))
        {
        }

        public CKeyPair(string strBase58)
        {
            List<byte> rawBytes = AddressTools.Base58DecodeCheck(strBase58).ToList();
            rawBytes.RemoveAt(0); // Remove key version byte

            // Deserialize secret value
            BigInteger D = new BigInteger(rawBytes.Take(32).ToArray());

            if (D.SignValue == -1)
            {
                List<byte> secretbytes = rawBytes.Take(32).ToList(); // Copy secret
                secretbytes.Insert(0, 0x00); // Prepend with sign byte

                D = new BigInteger(secretbytes.ToArray()); // Try decoding again
            }

            // Calculate public key
            ECPoint Q = curve.G.Multiply(D);

            _Private = new ECPrivateKeyParameters(D, domain);
            _Public = new ECPublicKeyParameters(Q, domain);

            if (rawBytes.Count == 33 && rawBytes.Last() == 0x01) // Check compression tag
            {
                _Public = Compress(_Public);
            }
        }

        /// <summary>
        /// Initialize a copy of CKeyPair instance
        /// </summary>
        /// <param name="pair">CKyPair instance</param>
        public CKeyPair(CKeyPair pair)
        {
            _Public = pair._Public;
            _Private = pair._Private;
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

        public CPubKey PubKey
        {
            get { return new CPubKey(PublicBytes); }
        }

        /// <summary>
        /// SecretBytes part of key pair
        /// </summary>
        public IEnumerable<byte> SecretBytes
        {
            get
            {
                List<byte> secretBytes = new List<byte>(_Private.D.ToByteArray());

                if (secretBytes[0] == 0x00)
                {
                    // Remove sign
                    secretBytes.RemoveAt(0);
                }

                if (IsCompressed)
                {
                    // Set compression flag
                    secretBytes.Add(0x01);
                }

                return secretBytes;
            }
        }

        public string ToHex()
        {
            return Interop.ToHex(SecretBytes);
        }

        public override string ToString()
        {
            List<byte> r = new List<byte>();

            r.Add((byte)(128 + AddrType.PUBKEY_ADDRESS)); // Key version
            r.AddRange(SecretBytes); // Key data

            return AddressTools.Base58EncodeCheck(r);
        }
    }
}
