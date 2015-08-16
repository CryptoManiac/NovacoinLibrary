using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    /// <summary>
    /// Representation of ECDSA private key
    /// </summary>
    public class CKey
    {
        /// <summary>
        /// Private key bytes
        /// </summary>
        private List<byte> privKeyBytes;

        /// <summary>
        /// Initialize new instance of CKey as copy of another instance.
        /// </summary>
        /// <param name="key">New CKey instance.</param>
        public CKey(CKey key)
        {
            privKeyBytes = key.privKeyBytes;
        }

        /// <summary>
        /// Initialize new instance of CKey using supplied byte sequence.
        /// </summary>
        /// <param name="bytes">New CKey instance.</param>
        public CKey(IEnumerable<byte> bytes)
        {
            privKeyBytes = new List<byte>(bytes);
        }

        /// <summary>
        /// Calculate public key for this private key.
        /// </summary>
        /// <returns>New CPubKey instance.</returns>
        public CPubKey GetPubKey()
        {
            // stub

            return new CPubKey((CPubKey)null);
        }
    }
}
