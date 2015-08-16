using System;
using System.Text;
using System.Collections.Generic;

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
		private Hash256 txID = new Hash256();

		/// <summary>
		/// Parent input number.
		/// </summary>
		private uint nInput = 0;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
		private byte[] scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
		private uint nSequence = 0xffffffff;

		public CTxIn ()
		{
		}

        public IList<byte> ToBytes()
        {
            List<byte> inputBytes = new List<byte>();


            inputBytes.AddRange(txID.hashBytes); // Input transaction id
            inputBytes.AddRange(Interop.LEBytes(nInput)); // Input number
            inputBytes.AddRange(VarInt.EncodeVarInt(scriptSig.LongLength)); // Scriptsig length
            inputBytes.AddRange(scriptSig); // ScriptSig
            inputBytes.AddRange(Interop.LEBytes(nSequence)); // Sequence

            return inputBytes;
        }

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("CTxIn(txId={0},n={1},scriptSig={2}", nInput, nInput, scriptSig.ToString());

			return sb.ToString ();
		}

	}
}

