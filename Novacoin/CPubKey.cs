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
        /// Quick validity test
        /// </summary>
        /// <returns>Validation result</returns>
        public bool IsValid
        {
            get { return !_Public.Q.IsInfinity; }
        }

        /// <summary>
        /// Is this a compressed public key?
        /// </summary>
        /// <returns></returns>
        public bool IsCompressed
        {
            get { return _Public.Q.IsCompressed; }
        }

        public override string ToString()
        {
            return Interop.ToHex(Public);
        }
    }
}
