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
    public class Hash256 : Hash
    {
        // 32 bytes
        public override int hashSize
        {
            get { return 32; }
        }

        public Hash256() : base() { }
        public Hash256(byte[] bytesArray) : base(bytesArray) { }
        public Hash256(IList<byte> bytesList) : base(bytesList) { }

        public static Hash256 Compute256(IEnumerable<byte> inputBytes)
        {
            byte[] dataBytes = inputBytes.ToArray();
            byte[] digest1 = _hasher256.ComputeHash(dataBytes, 0, dataBytes.Length);
            byte[] digest2 = _hasher256.ComputeHash(digest1, 0, digest1.Length);

            return new Hash256(digest2);
        }
    }
}

