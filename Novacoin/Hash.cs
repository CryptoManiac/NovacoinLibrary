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
        public abstract int hashSize 
        {
            get; 
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
        public Hash(IEnumerable<byte> bytes)
        {
            _hashBytes = bytes.Take<byte>(hashSize).ToArray<byte>();
        }

        public Hash(byte[] bytes)
        {
            _hashBytes = bytes;
        }

        public Hash(Hash h)
        {
            _hashBytes = new byte[h.hashSize];
            h._hashBytes.CopyTo(_hashBytes, 0);
        }

        public bool IsZero
        {
            get { return !_hashBytes.Any(b => b != 0); }
        }

        public override string ToString()
        {
            return Interop.ToHex(Interop.ReverseBytes(_hashBytes));
        }
    }
}
