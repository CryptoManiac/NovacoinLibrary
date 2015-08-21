using System;
using System.Text;
using System.Collections.Generic;

namespace Novacoin
{
	/// <summary>
	/// Transaction output.
	/// </summary>
	public class CTxOut
	{
		/// <summary>
		/// Input value.
		/// </summary>
        public long nValue = -1;

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
        public CScript scriptPubKey;

        /// <summary>
        /// Initialize new CTxOut instance as a copy of another instance.
        /// </summary>
        /// <param name="o">CTxOut instance.</param>
        public CTxOut(CTxOut o)
        {
            nValue = o.nValue;
            scriptPubKey = o.scriptPubKey;
        }

        /// <summary>
        /// Initialize an empty instance of CTxOut class
        /// </summary>
        public CTxOut()
        {
            SetEmpty();
        }

        /// <summary>
        /// Read vout list from byte sequence.
        /// </summary>
        /// <param name="wBytes">Reference to byte sequence</param>
        /// <returns>Outputs array</returns>
        public static CTxOut[] ReadTxOutList(ref WrappedList<byte> wBytes)
        {
            int nOutputs = (int)VarInt.ReadVarInt(ref wBytes);
            CTxOut[] vout =new CTxOut[nOutputs];

            for (int nIndex = 0; nIndex < nOutputs; nIndex++)
            {
                // Fill outputs array
                vout[nIndex] = new CTxOut();
                vout[nIndex].nValue = BitConverter.ToUInt32(wBytes.GetItems(8), 0);

                int nScriptPKLen = (int)VarInt.ReadVarInt(ref wBytes);
                vout[nIndex].scriptPubKey = new CScript(wBytes.GetItems(nScriptPKLen));
            }

            return vout;
        }

        /// <summary>
        /// Get raw bytes representation of our output.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public IList<byte> Bytes
        {
            get
            {
                List<byte> resultBytes = new List<byte>();

                resultBytes.AddRange(BitConverter.GetBytes(nValue)); // txout value

                List<byte> s = new List<byte>(scriptPubKey.Bytes);

                resultBytes.AddRange(VarInt.EncodeVarInt(s.Count)); // scriptPubKey length
                resultBytes.AddRange(s); // scriptPubKey

                return resultBytes;
            }
        }

        /// <summary>
        /// Null prevouts have -1 value
        /// </summary>
        public void SetNull()
        {
            nValue = -1;
        }

        /// <summary>
        /// Empty outputs have zero value and empty scriptPubKey
        /// </summary>
        public void SetEmpty()
        {
            nValue = 0;
            scriptPubKey.SetNullDestination();
        }

        public bool IsNull
        {
            get { return (nValue == -1); }
        }

        public bool IsEmpty
        {
           get { return nValue == 0 && scriptPubKey.IsNull; }
        }

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("CTxOut(nValue={0}, scriptPubKey={1})", nValue, scriptPubKey.ToString());

			return sb.ToString ();
		}
	}
}

