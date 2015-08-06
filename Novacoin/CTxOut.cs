using System;
using System.Text;

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
		public ulong nValue = 0;

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
		public byte[] scriptPubKey;

		public CTxOut ()
		{
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("CTxOut(nValue={0},scriptPubKey={1}", nValue, scriptPubKey.ToString());

			return sb.ToString ();
		}
	}
}

