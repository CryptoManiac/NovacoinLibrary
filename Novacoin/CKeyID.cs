using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class CKeyID : Hash160
    {
        public CKeyID(Hash160 pubKeyHash)
        {
            _hashBytes = pubKeyHash.hashBytes;
        }

        public override string ToString()
        {
            return (new CNovacoinAddress(this)).ToString();
        }
    }
}
