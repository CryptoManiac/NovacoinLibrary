using System;

namespace Novacoin
{
	/// <summary>
	/// Represents the transaction. Any transaction must provide one input and one output at least.
	/// </summary>
	public class CTransaction
	{
		/// <summary>
		/// Version of transaction schema.
		/// </summary>
		public uint nVersion;

		/// <summary>
		/// Transaction timestamp.
		/// </summary>
		public uint nTime;

		/// <summary>
		/// Array of transaction inputs
		/// </summary>
		public CTxIn[] inputs;

		/// <summary>
		/// Array of transaction outputs
		/// </summary>
		public CTxOut[] outputs;

		/// <summary>
		/// Block height or timestamp when transaction is final
		/// </summary>
		public uint nLockTime;

		public CTransaction ()
		{

		}
	}
}

