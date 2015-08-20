using System;
using System.Text;
using System.Collections.Generic;

namespace Novacoin
{
	/// <summary>
	/// Block header
	/// </summary>
	public class CBlockHeader
	{
		/// <summary>
		/// Version of block schema.
		/// </summary>
		public uint nVersion = 6;

		/// <summary>
		/// Previous block hash.
		/// </summary>
		public Hash256 prevHash = new Hash256();

		/// <summary>
		/// Merkle root hash.
		/// </summary>
		public Hash256 merkleRoot = new Hash256();

		/// <summary>
		/// Block timestamp.
		/// </summary>
		public uint nTime = 0;

		/// <summary>
		/// Compressed difficulty representation.
		/// </summary>
		public uint nBits = 0;

		/// <summary>
		/// Nonce counter.
		/// </summary>
		public uint nNonce = 0;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
		public CBlockHeader ()
		{
		}

        /// <summary>
        /// Convert current block header instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public IList<byte> ToBytes()
        {
            List<byte> r = new List<byte>();

            r.AddRange(BitConverter.GetBytes(nVersion));
            r.AddRange(prevHash.hashBytes);
            r.AddRange(merkleRoot.hashBytes);
            r.AddRange(BitConverter.GetBytes(nTime));
            r.AddRange(BitConverter.GetBytes(nBits));
            r.AddRange(BitConverter.GetBytes(nNonce));

            return r;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CBlockHeader(nVersion={0}, prevHash={1}, merkleRoot={2}, nTime={3}, nBits={4}, nNonce={5})", nVersion, prevHash.ToString(), merkleRoot.ToString(), nTime, nBits, nNonce);
            return sb.ToString();
        }
	}
}
