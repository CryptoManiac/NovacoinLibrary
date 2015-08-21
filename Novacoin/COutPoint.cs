using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class COutPoint
    {
        /// <summary>
        /// Hash of parent transaction.
        /// </summary>
        public Hash256 hash;

        /// <summary>
        /// Parent input number.
        /// </summary>
        public uint n;

        public COutPoint()
        {
            hash = new Hash256();
            n = uint.MaxValue;
        }

        public COutPoint(Hash256 hashIn, uint nIn)
        {
            hash = hashIn;
            n = nIn;
        }

        public COutPoint(COutPoint o)
        {
            hash = new Hash256(o.hash);
            n = o.n;
        }

        public COutPoint(IEnumerable<byte> bytes)
        {
            hash = new Hash256(bytes.Take(32));
            n = BitConverter.ToUInt32(bytes.Skip(32).Take(4).ToArray(), 0);
        }

        public bool IsNull
        {
            get { return hash.IsZero && n == uint.MaxValue; }
        }

        public IList<byte> Bytes
        {
            get
            {
                List<byte> r = new List<byte>();
                r.AddRange(hash.hashBytes);
                r.AddRange(BitConverter.GetBytes(n));

                return r;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("COutPoint({0}, {1})", hash.ToString(), n);

            return sb.ToString();
        }

        
    }

}
