using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public abstract class Hash
    {
        /// <summary>
        /// Array of digest bytes.
        /// </summary>
        private byte[] _hashBytes = null;

        /// <summary>
        /// Hash size, must be overriden
        /// </summary>
        public virtual int hashSize 
        {
            get; 
            private set;
        }

        public IList<byte> hashBytes
        {
            get { return new List<byte>(_hashBytes); }
        }

        /// <summary>
        /// Initializes an empty instance of the Hash class.
        /// </summary>
        public Hash()
        {
            _hashBytes = Enumerable.Repeat<byte>(0, hashSize).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of Hash class with first 20 bytes from supplied list
        /// </summary>
        /// <param name="bytesList">List of bytes</param>
        public Hash(IList<byte> bytesList)
        {
            _hashBytes = bytesList.Take<byte>(hashSize).ToArray<byte>();
        }

        public Hash(byte[] bytesArray)
        {
            _hashBytes = bytesArray;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(hashSize * 2);
            foreach (byte b in _hashBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
