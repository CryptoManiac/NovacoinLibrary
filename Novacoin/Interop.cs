using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class InteropException : Exception
    {
        public InteropException()
        {
        }

        public InteropException(string message)
            : base(message)
        {
        }

        public InteropException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

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

        public static ushort LEBytesToUInt16(byte[] bytes)
        {
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the ushort data type.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        public static uint LEBytesToUInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the uint data type.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong LEBytesToUInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the ulong data type.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt64(bytes, 0);
        }


    }
}
