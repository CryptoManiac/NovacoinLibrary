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
		public Hash256 txID = new Hash256();

		/// <summary>
		/// Parent input number.
		/// </summary>
        public uint n = 0;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
        public byte[] scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
        public uint nSequence = 0xffffffff;

        /// <summary>
        /// Initialize new CTxIn instance as copy of another one.
        /// </summary>
        /// <param name="i">CTxIn instance.</param>
        public CTxIn(CTxIn i)
        {
            txID = i.txID;
            n = i.n;
            scriptSig = i.scriptSig;
            nSequence = i.nSequence;
        }

        /// <summary>
        /// Initialize an empty instance of CTxIn class
        /// </summary>
        public CTxIn()
        {
        }

        /// <summary>
        /// Get raw bytes representation of our input.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public IList<byte> ToBytes()
        {
            List<byte> inputBytes = new List<byte>();

            inputBytes.AddRange(txID.hashBytes); // Input transaction id
            inputBytes.AddRange(Interop.LEBytes(n)); // Output number
            inputBytes.AddRange(VarInt.EncodeVarInt(scriptSig.LongLength)); // scriptSig length
            inputBytes.AddRange(scriptSig); // scriptSig
            inputBytes.AddRange(Interop.LEBytes(nSequence)); // Sequence

            return inputBytes;
        }

        public bool IsCoinBase()
        {
            return txID.IsZero();
        }

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

            if (IsCoinBase())
            {
                sb.AppendFormat("CTxIn(txId={0},coinbase={2},nSequence={3})", txID.ToString(), n, Interop.ToHex(scriptSig), nSequence);
            }
            else
            {
                sb.AppendFormat("CTxIn(txId={0},n={1},scriptSig={2},nSequence={3})", txID.ToString(), n, (new CScript(scriptSig)).ToString(), nSequence);
            }

			return sb.ToString ();
		}

	}
}

