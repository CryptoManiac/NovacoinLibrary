using System;

namespace Novacoin
{
	public class CBlock
	{
		/// <summary>
		/// Block header.
		/// </summary>
		public CBlockHeader header;

		/// <summary>
		/// Transactions array.
		/// </summary>
		public CTransaction[] tx;

		/// <summary>
		/// Block header signature.
		/// </summary>
		public byte[] signature;

		public CBlock ()
		{
		}
	}
}

