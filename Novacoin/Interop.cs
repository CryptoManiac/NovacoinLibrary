using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    class Interop
    {
        public static byte[] LEBytes(ushort n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] LEBytes(uint n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] LEBytes(ulong n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(ushort n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(uint n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(ulong n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

    }
}
