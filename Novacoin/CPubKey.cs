using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    /// <summary>
    /// Representation of ECDSA public key
    /// </summary>
    public class CPubKey
    {
        /// <summary>
        /// Public key bytes
        /// </summary>
        private List<byte> pubKeyBytes;

        /// <summary>
        /// Initializes a new instance of CPubKey class as the copy of another instance
        /// </summary>
        /// <param name="pubKey">Another CPubKey instance</param>
        public CPubKey(CPubKey pubKey)
        {
            pubKeyBytes = pubKey.pubKeyBytes;
        }

        /// <summary>
        /// Initializes a new instance of CPubKey class using supplied sequence of bytes
        /// </summary>
        /// <param name="bytesList"></param>
        public CPubKey(IEnumerable<byte> bytesList)
        {
            pubKeyBytes = new List<byte>(bytesList);
        }

        /// <summary>
        /// Quick validity test
        /// </summary>
        /// <returns>Validation result</returns>
        public bool IsValid()
        {
            return pubKeyBytes.Count == 33 || pubKeyBytes.Count == 65;
        }

        /// <summary>
        /// Is this a compressed public key?
        /// </summary>
        /// <returns></returns>
        public bool IsCompressed()
        {
            // Compressed public keys are 33 bytes long
            return pubKeyBytes.Count == 33;
        }

        /// <summary>
        /// Calculate Hash160 and create new CKeyID instance.
        /// </summary>
        /// <returns>New key ID</returns>
        public CKeyID GetKeyID()
        {
            return new CKeyID(Hash160.Compute160(this.Raw));
        }

        /// <summary>
        /// Accessor for internal representation
        /// </summary>
        public IList<byte> Raw
        {
            get { return pubKeyBytes; }
        }
    }
}
