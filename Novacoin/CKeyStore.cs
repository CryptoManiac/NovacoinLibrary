using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class CKeyStore
    {
        public CKeyStore(string strDatabasePath="Wallet.db")
        {
        }

        ~CKeyStore()
        {
        }

        public bool AddKey(CKeyPair keyPair)
        {
            // TODO

            return true;
        }

        public bool HaveKey(CKeyID keyID)
        {
            // TODO

            return true;
        }

        public bool GetKey(CKeyID keyID, out CKeyPair keyPair)
        {
            keyPair = new CKeyPair();

            return false;
        }

        public bool AddScript(CScript script)
        {
            // TODO

            return true;
        }

        public bool HaveScript(CScriptID scriptID)
        {
            // TODO

            return true;
        }

        public bool GetScript(CScriptID scriptID, out CScript script)
        {
            // TODO

            script = new CScript();

            return true;
        }


    }
}
