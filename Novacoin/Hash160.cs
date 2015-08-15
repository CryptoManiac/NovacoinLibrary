using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Novacoin
{
	/// <summary>
	/// Representation of pubkey/script hash.
	/// </summary>
	public class Hash160 : Hash
	{
        // 20 bytes
        public override int hashSize
        {
            get { return 20; }
        }
	}
}

