using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;


namespace Novacoin
{
	/// <summary>
	/// Representation of SHA-256 hash
	/// </summary>
    public class Hash256
    {
        // 32 bytes
        const int hashSize = 32;

        /// <summary>
        /// Array of digest bytes.
        /// </summary>
        private byte[] hashBytes = new byte[hashSize];

        /// <summary>
        /// Initializes an empty instance of the Hash256 class.
        /// </summary>
        public Hash256()
        {
            hashBytes = Enumerable.Repeat<byte>(0, hashSize).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of Hash256 class with first 32 bytes from supplied list
        /// </summary>
        /// <param name="bytesList">List of bytes</param>
        public Hash256(IList<byte> bytesList)
        {
            hashBytes = bytesList.Take<byte>(hashSize).ToArray<byte>();
        }

        public Hash256(byte[] bytesArray)
        {
            hashBytes = bytesArray;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(hashSize * 2);
            foreach (byte b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

    }
}

