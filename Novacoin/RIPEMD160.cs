using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Novacoin
{
    /// <summary>
    /// Representation of RIPEMD-160 hash.
    /// </summary>
    public class RIPEMD160 : Hash
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

        public RIPEMD160() : base() { }
        public RIPEMD160(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public RIPEMD160(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public RIPEMD160(RIPEMD160 h) : base(h) { }

        public static RIPEMD160 Compute160(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher160.ComputeHash(dataBytes, 0, dataBytes.Length);

            return new RIPEMD160(digest1);
        }
    }
}

