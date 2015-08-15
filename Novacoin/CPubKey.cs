using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class CPubKey
    {
        private List<byte> pubKeyBytes;

        public IList<byte> Raw
        {
            get { return pubKeyBytes; }
        }
    }
}
