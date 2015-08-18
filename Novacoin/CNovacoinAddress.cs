using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public enum AddrType
    {
        PUBKEY_ADDRESS = 8,
        SCRIPT_ADDRESS = 20,
        PUBKEY_ADDRESS_TEST = 111,
        SCRIPT_ADDRESS_TEST = 196
    };

    public class CNovacoinAddress
    {
        private byte nVersion;
        private List<byte> addrData;

        public CNovacoinAddress(byte nVersionIn, IEnumerable<byte> addrDataIn)
        {
            nVersion = nVersionIn;
            addrData = addrDataIn.ToList();
        }

        public CNovacoinAddress(CKeyID keyID)
        {
            nVersion = (byte)AddrType.PUBKEY_ADDRESS;
            addrData = new List<byte>(keyID.hashBytes);
        }

        public CNovacoinAddress(CScriptID scriptID)
        {
            nVersion = (byte)AddrType.SCRIPT_ADDRESS;
            addrData = new List<byte>(scriptID.hashBytes);
        }

        public bool IsValid()
        {
            int nExpectedSize = 20;

            switch ((AddrType) nVersion)
            {
                case AddrType.PUBKEY_ADDRESS:
                    nExpectedSize = 20; // Hash of public key
                    break;
                case AddrType.SCRIPT_ADDRESS:
                    nExpectedSize = 20; // Hash of CScript
                    break;
                case AddrType.PUBKEY_ADDRESS_TEST:
                    nExpectedSize = 20;
                    break;
                case AddrType.SCRIPT_ADDRESS_TEST:
                    nExpectedSize = 20;
                    break;
                default:
                    return false;
            }

            return addrData.Count == nExpectedSize;
        }

        public override string ToString()
        {
            List<byte> r = new List<byte>();

            r.Add(nVersion);
            r.AddRange(addrData);

            return AddressTools.Base58EncodeCheck(r.ToArray());
        }
    }
}
