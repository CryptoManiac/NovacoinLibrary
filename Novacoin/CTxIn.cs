using System;
using System.Text;

namespace Novacoin
{
	/// <summary>
	/// Transaction input.
	/// </summary>
	public class CTxIn
	{
		/// <summary>
		/// Hash of parent transaction.
		/// </summary>
		public Hash256 txID = new Hash256();

		/// <summary>
		/// Parent input number.
		/// </summary>
		public uint nInput = 0;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
		public byte[] scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
		public uint nSequence = 0xffffffff;

		public CTxIn ()
		{
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("CTxIn(txId={0},n={1},scriptSig={2}", nInput, nInput, scriptSig.ToString());

			return sb.ToString ();
		}

	}
}

