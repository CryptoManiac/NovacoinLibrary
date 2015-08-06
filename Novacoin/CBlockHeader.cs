using System;

namespace Novacoin
{
	public class CBlockHeader
	{
		/// <summary>
		/// Version of block schema.
		/// </summary>
		public uint nVersion;

		/// <summary>
		/// Previous block hash.
		/// </summary>
		public Hash256 prevHash;

		/// <summary>
		/// Merkle root hash.
		/// </summary>
		public Hash256 merkleRoot;

		/// <summary>
		/// Block timestamp.
		/// </summary>
		public uint nTime;

		/// <summary>
		/// Compressed difficulty representation.
		/// </summary>
		public uint nBits;

		/// <summary>
		/// Nonce counter.
		/// </summary>
		public uint nNonce;

		public CBlockHeader ()
		{
		}
	}
}

