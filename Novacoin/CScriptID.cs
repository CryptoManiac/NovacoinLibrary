using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class CScriptID : Hash160
    {
        public CScriptID(Hash160 scriptHash)
        {
            _hashBytes = scriptHash.hashBytes;
        }

        public override string ToString()
        {
            return (new CNovacoinAddress(this)).ToString();
        }

    }
}
