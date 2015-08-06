using System;

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
		public ulong nValue;

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
		public byte[] scriptPubKey;

		public CTxOut ()
		{
		}
	}
}

