using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using System.Security.Cryptography;

namespace Novacoin
{
    /// <summary>
    /// Representation of SHA-256 hash
    /// </summary>
    public class SHA1 : Hash
    {
        /// <summary>
        /// Computes RIPEMD160 hash using managed library
        /// </summary>
        private static readonly SHA1Managed _hasher1 = new SHA1Managed();

        // 32 bytes
        public override int hashSize
        {
            get { return 20; }
        }

        public SHA1() : base() { }
        public SHA1(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public SHA1(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public SHA1(SHA1 h) : base(h) { }


        public static SHA1 Compute1(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher1.ComputeHash(dataBytes, 0, dataBytes.Length);

            return new SHA1(digest1);
        }
    }
}

