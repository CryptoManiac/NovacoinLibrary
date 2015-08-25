using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class CKeyPool
    {
        public CKeyPool(int nSize = 100)
        {
            // TODO
        }

        public CKeyID ReserveKey(out int nKeyIndex)
        {
            // TODO

            nKeyIndex = -1;

            return (new CKeyPair()).KeyID;
        }

        public void RemoveKey(int nIndex)
        {
            // TODO
        }

        public void ReturnKey(int nIndex)
        {
            // TODO
        }

        public void ResetPool()
        {
            // TODO
        }

        public long OldestTime
        {
            get {
                // TODO
                return 0;
            }
        }
    }
}
