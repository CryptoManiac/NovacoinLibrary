using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Math.EC;
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
        public CPubKey(IEnumerable<byte> bytes)
        {
            ECPoint pQ = curve.Curve.DecodePoint(bytes.ToArray());
            _Public = new ECPublicKeyParameters(pQ, domain);
        }

        /// <summary>
        /// Init with base58 encoded sequence of bytes
        /// </summary>
        /// <param name="strBase58"></param>
        public CPubKey(string strBase58)
        {
            ECPoint pQ = curve.Curve.DecodePoint(AddressTools.Base58DecodeCheck(strBase58).ToArray());
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
            return Interop.ToHex(PublicBytes);
        }

        public override string ToString()
        {
            List<byte> r = new List<byte>();

            r.Add((byte)(AddrType.PUBKEY_ADDRESS));
            r.AddRange(PublicBytes);

            return AddressTools.Base58EncodeCheck(r);
        }
    }
}
