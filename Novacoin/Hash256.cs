using System;
using System.Text;
using System.Linq;

namespace Novacoin
{
	/// <summary>
	/// Representation of SHA-256 hash
	/// </summary>
	public class Hash256
	{
		/// <summary>
		/// Array of digest bytes.
		/// </summary>
		private byte[] h;

		/// <summary>
		/// Initializes an empty instance of the Hash256 class.
		/// </summary>
		public Hash256 ()
		{
			h = Enumerable.Repeat<byte>(0, 32).ToArray();
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

