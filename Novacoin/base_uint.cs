using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class base_uint
    {
        protected uint nWidth;
        protected uint[] pn;

        public static bool operator !(base_uint a)
        {
            for (int i = 0; i < a.nWidth; i++)
            {
                if (a.pn[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static base_uint operator ~(base_uint a)
        {
            var ret = new base_uint();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            return ret;
        }

        public static base_uint operator -(base_uint a)
        {
            var ret = new base_uint();
            for (int i = 0; i < a.nWidth; i++)
            {
                ret.pn[i] = ~a.pn[i];
            }
            ret++;
            return ret;
        }

        public static base_uint operator ++(base_uint a)
        {
            // prefix operator
            int i = 0;
            while (++a.pn[i] == 0 && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static base_uint operator --(base_uint a)
        {
            // prefix operator
            int i = 0;
            while (--a.pn[i] == uint.MaxValue && i < a.nWidth - 1)
            {
                i++;
            }
            return a;
        }

        public static base_uint operator ^(base_uint a, base_uint b)
        {
            var c = new base_uint();
            c.pn = new uint[a.nWidth];
            for (int i = 0; i < c.nWidth; i++)
            {
                c.pn[i] = a.pn[i] ^ b.pn[i];
            }
            return c;
        }

        public static base_uint operator +(base_uint a, base_uint b)
        {
            var result = new base_uint();
            ulong carry = 0;
            for (int i = 0; i < result.nWidth; i++)
            {
                ulong n = carry + a.pn[i] + b.pn[i];
                result.pn[i] = (uint)(n & 0xffffffff);
                carry = n >> 32;
            }
            return result;
        }



    }
}
