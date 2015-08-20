using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class Interop
    {
        public static byte[] ReverseBytes(byte[] source)
        {
            byte[] b = new byte[source.Length];

            source.CopyTo(b, 0);

            Array.Reverse(b);

            return b;
        }

        public static byte[] LEBytes(uint[] values)
        {
            byte[] result = new byte[values.Length * sizeof(uint)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);

            return result;
        }

        public static uint[] ToUInt32Array(byte[] bytes)
        {
            uint[] result = new uint[bytes.Length / sizeof(uint)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

            return result;
        }

        public static byte[] BEBytes(ushort n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }

        public static byte[] BEBytes(uint n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }

        public static byte[] BEBytes(ulong n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }


        public static ushort BEBytesToUInt16(byte[] bytes)
        {
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the ushort data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt16(bytes, 0);
        }

        public static uint BEBytesToUInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(uint))
            {
                throw new InteropException("Array size doesn't match the uint data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong BEBytesToUInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(ulong))
            {
                throw new InteropException("Array size doesn't match the ulong data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt64(bytes, 0);
        }

        public static IEnumerable<byte> ParseHex(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16));
        }

        public static string ToHex(IEnumerable<byte> bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
