using System;

namespace Novacoin
{
	public class CTxIn
	{
		/// <summary>
		/// Hash of parent transaction.
		/// </summary>
		public Hash256 txID;

		/// <summary>
		/// Parent input number.
		/// </summary>
		public uint nInput;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
		public byte[] scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
		public uint nSequence;

		public CTxIn ()
		{
		}
	}
}

