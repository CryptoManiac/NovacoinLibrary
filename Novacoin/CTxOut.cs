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
		private ulong nValue;

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
		private byte[] scriptPubKey;

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
        /// Parse input byte sequence and initialize new CTxOut instance.
        /// </summary>
        /// <param name="bytes">Byte sequence.</param>
        public CTxOut(IList<byte> bytes)
        {
            WrappedList<byte> wBytes = new WrappedList<byte>(bytes);
            
            nValue = Interop.LEBytesToUInt64(wBytes.GetItems(8));
            int spkLength = (int)VarInt.ReadVarInt(wBytes);

            scriptPubKey = wBytes.GetItems(spkLength);
        }

        /// <summary>
        /// Get raw bytes representation of our output.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public IList<byte> ToBytes()
        {
            List<byte> resultBytes = new List<byte>();

            resultBytes.AddRange(Interop.LEBytes(nValue)); // txout value
            resultBytes.AddRange(VarInt.EncodeVarInt(scriptPubKey.LongLength)); // scriptPubKey length
            resultBytes.AddRange(scriptPubKey); // scriptPubKey

            return resultBytes;
        }

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("CTxOut(nValue={0},scriptPubKey={1}", nValue, scriptPubKey.ToString());

			return sb.ToString ();
		}
	}
}

