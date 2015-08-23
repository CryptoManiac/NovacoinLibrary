﻿/**
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using System.Linq;


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
            var genParams = new ECKeyGenerationParameters(domain, new SecureRandom());
            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(genParams);
            var ecKeyPair = generator.GenerateKeyPair();

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
        public CKeyPair(byte[] secretBytes, bool Compressed=true)
        {
            // Deserialize secret value
            var D = new BigInteger(secretBytes.Take(32).ToArray());

            if (D.SignValue == -1)
            {
                var fixedKeyBytes = secretBytes.Take(32).ToList();
                fixedKeyBytes.Insert(0, 0x00); // prepend with sign byte

                D = new BigInteger(fixedKeyBytes.ToArray());
            }

            // Calculate public key
            var Q = curve.G.Multiply(D);

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
        public CKeyPair(byte[] secretBytes) : 
            this (secretBytes.Take(32).ToArray(), (secretBytes.Count() == 33 && secretBytes.Last() == 0x01))
        {
        }

        public CKeyPair(string strBase58)
        {
            var rawBytes = AddressTools.Base58DecodeCheck(strBase58).ToList();
            rawBytes.RemoveAt(0); // Remove key version byte

            // Deserialize secret value
            var D = new BigInteger(rawBytes.Take(32).ToArray());

            if (D.SignValue == -1)
            {
                var secretbytes = rawBytes.Take(32).ToList(); // Copy secret
                secretbytes.Insert(0, 0x00); // Prepend with sign byte

                D = new BigInteger(secretbytes.ToArray()); // Try decoding again
            }

            // Calculate public key
            var Q = curve.G.Multiply(D);

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
        /// <param name="data">Hash to sigh</param>
        /// <returns>Signature bytes sequence</returns>
        public byte[] Sign(Hash sigHash)
        {
            var signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(true, _Private);
            signer.BlockUpdate(sigHash.hashBytes, 0, sigHash.hashSize);

            return signer.GenerateSignature();
        }

        public CPubKey PubKey
        {
            get { return new CPubKey(PublicBytes); }
        }

        /// <summary>
        /// SecretBytes part of key pair
        /// </summary>
        public byte[] SecretBytes
        {
            get
            {
                var secretBytes = new List<byte>(_Private.D.ToByteArray());

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

                return secretBytes.ToArray();
            }
        }

        public string ToHex()
        {
            return Interop.ToHex(SecretBytes);
        }

        public override string ToString()
        {
            var r = new List<byte>();

            r.Add((byte)(128 + AddrType.PUBKEY_ADDRESS)); // Key version
            r.AddRange(SecretBytes); // Key data

            return AddressTools.Base58EncodeCheck(r.ToArray());
        }
    }
}
