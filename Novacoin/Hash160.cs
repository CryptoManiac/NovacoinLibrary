using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Novacoin
{
	/// <summary>
	/// Representation of pubkey/script hash.
	/// </summary>
	public class Hash160
	{
        // 20 bytes
        const int hashSize = 20;

		/// <summary>
		/// Array of digest bytes.
		/// </summary>
        private byte[] hashBytes = new byte[hashSize];

		/// <summary>
		/// Initializes an empty instance of the Hash160 class.
		/// </summary>
		public Hash160 ()
		{
            hashBytes = Enumerable.Repeat<byte>(0, hashSize).ToArray();
		}

        /// <summary>
        /// Initializes a new instance of Hash160 class with first 20 bytes from supplied list
        /// </summary>
        /// <param name="bytesList">List of bytes</param>
        public Hash160(IList<byte> bytesList)
        {
            hashBytes = bytesList.Take<byte>(hashSize).ToArray<byte>();
        }

        public Hash160(byte[] bytesArray)
        {
            hashBytes = bytesArray;
        }

		public override string ToString()
		{
            StringBuilder sb = new StringBuilder(hashSize * 2);
            foreach (byte b in hashBytes)
			{
				sb.AppendFormat ("{0:x2}", b);
			}
			return sb.ToString();
		}
	}
}

