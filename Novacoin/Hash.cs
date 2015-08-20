using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace Novacoin
{
    public abstract class Hash
    {
        /// <summary>
        /// Computes the SHA256 hash for the input data using the managed library.
        /// </summary>
        protected static SHA256Managed _hasher256 = new SHA256Managed();
        
        /// <summary>
        /// Array of digest bytes.
        /// </summary>
        protected byte[] _hashBytes = null;

        /// <summary>
        /// Hash size, must be overriden
        /// </summary>
        public virtual int hashSize 
        {
            get; 
            private set;
        }

        public byte[] hashBytes
        {
            get { return _hashBytes; }
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

        public bool IsZero()
        {
            return !_hashBytes.Any(b => b != 0);
        }

        public override string ToString()
        {
            return Interop.ToHex(Interop.ReverseBytes(_hashBytes));
        }
    }
}
