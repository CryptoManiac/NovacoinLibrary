using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;


namespace Novacoin
{
	/// <summary>
	/// Representation of SHA-256 hash
	/// </summary>
    public class Hash256 : Hash
    {
        // 32 bytes
        public override int hashSize
        {
            get { return 32; }
        }
    }
}

