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
    public class SHA256 : Hash
    {
        // 32 bytes
        public override int hashSize
        {
            get { return 32; }
        }

        public SHA256() : base() { }
        public SHA256(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public SHA256(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public SHA256(SHA256 h) : base(h) { }


        public static SHA256 Compute256(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher256.ComputeHash(dataBytes, 0, dataBytes.Length);

            return new SHA256(digest1);
        }
    }
}

