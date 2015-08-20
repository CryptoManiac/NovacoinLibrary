using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Novacoin
{
	/// <summary>
	/// Representation of pubkey/script hash.
	/// </summary>
	public class Hash160 : Hash
	{
        /// <summary>
        /// Computes RIPEMD160 hash using managed library
        /// </summary>
        private static readonly RIPEMD160Managed _hasher160 = new RIPEMD160Managed();
        
        // 20 bytes
        public override int hashSize
        {
            get { return 20; }
        }

        public Hash160() : base() { }
        public Hash160(byte[] bytes) : base(bytes) { }
        public Hash160(IEnumerable<byte> bytes) : base(bytes) { }

        public static Hash160 Compute160(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher256.ComputeHash(dataBytes, 0, dataBytes.Length);
            byte[] digest2 = _hasher160.ComputeHash(digest1, 0, digest1.Length);

            return new Hash160(digest2);
        }
	}
}

