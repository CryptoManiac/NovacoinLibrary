using System;
using System.Text;
using System.Linq;

namespace Novacoin
{
	/// <summary>
	/// Representation of pubkey/script hash.
	/// </summary>
	public class Hash160
	{
		/// <summary>
		/// Array of digest bytes.
		/// </summary>
		private byte[] h;

		/// <summary>
		/// Initializes an empty instance of the Hash160 class.
		/// </summary>
		public Hash160 ()
		{
			// Fill with 20 zero bytes
			h = Enumerable.Repeat<byte> (0, 20).ToArray ();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(h.Length * 2);
			foreach (byte b in h)
			{
				sb.AppendFormat ("{0:x2}", b);
			}
			return sb.ToString();
		}

	}
}

