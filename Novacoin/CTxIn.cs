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
        /// Previous input data
        /// </summary>
        public COutPoint prevout;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
        public CScript scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
        public uint nSequence = uint.MaxValue;

        /// <summary>
        /// Initialize new CTxIn instance as copy of another one.
        /// </summary>
        /// <param name="i">CTxIn instance.</param>
        public CTxIn(CTxIn i)
        {
            prevout = new COutPoint(i.prevout);
            scriptSig = i.scriptSig;
            nSequence = i.nSequence;
        }

        /// <summary>
        /// Initialize an empty instance of CTxIn class
        /// </summary>
        public CTxIn()
        {
            prevout = new COutPoint();
            scriptSig = new CScript();
        }

        /// <summary>
        /// Read vin list from byte sequence.
        /// </summary>
        /// <param name="wBytes">Reference to byte sequence</param>
        /// <returns>Inputs array</returns>
        public static CTxIn[] ReadTxInList(ref ByteQueue wBytes)
        {
            CTxIn[] vin;

            // Get amount
            int nInputs = (int)VarInt.ReadVarInt(ref wBytes);
            vin = new CTxIn[nInputs];

            for (int nIndex = 0; nIndex < nInputs; nIndex++)
            {
                // Fill inputs array
                vin[nIndex] = new CTxIn();
                vin[nIndex].prevout = new COutPoint(wBytes.Get(36));
                vin[nIndex].scriptSig = new CScript(wBytes.Get((int)VarInt.ReadVarInt(ref wBytes)));
                vin[nIndex].nSequence = BitConverter.ToUInt32(wBytes.Get(4), 0);
            }

            // Return inputs array
            return vin;
        }

        /// <summary>
        /// Get raw bytes representation of our input.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public IList<byte> Bytes
        {
            get
            {
                List<byte> inputBytes = new List<byte>();

                inputBytes.AddRange(prevout.Bytes); // prevout

                List<byte> s = new List<byte>(scriptSig.Bytes);

                inputBytes.AddRange(VarInt.EncodeVarInt(s.Count)); // scriptSig length
                inputBytes.AddRange(s); // scriptSig
                inputBytes.AddRange(BitConverter.GetBytes(nSequence)); // Sequence

                return inputBytes;
            }
        }

        public bool IsFinal
        {
            get { return (nSequence == uint.MaxValue); }
        }
        public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

            sb.AppendFormat("CTxIn(");
            sb.Append(prevout.ToString());

            if(prevout.IsNull)
            {
                sb.AppendFormat(", coinbase={0}", Interop.ToHex(scriptSig.Bytes));
            }
            else
            {
                sb.AppendFormat(", scriptsig={0}", scriptSig.ToString());
            }

            if (nSequence != uint.MaxValue)
            {
                sb.AppendFormat(", nSequence={0}", nSequence);
            }

            sb.Append(")");


            return sb.ToString ();
		}

	}
}

