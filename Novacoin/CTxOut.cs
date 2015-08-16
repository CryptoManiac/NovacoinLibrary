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
		private ulong nValue = 0;

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
		private byte[] scriptPubKey;

		public CTxOut ()
		{
		}

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

