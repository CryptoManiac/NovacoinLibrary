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
        public static byte[] ReverseIfLE(byte[] source)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(source);
            }

            return source;
        }

        public static byte[] LEBytes(uint[] values)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] result = new byte[values.Length * sizeof(uint)];
                Buffer.BlockCopy(values, 0, result, 0, result.Length);

                return result;
            }
            else
            {
                List<byte> result = new List<byte>();

                foreach (uint i in values)
                {
                    result.AddRange(LEBytes(i));
                }

                return result.ToArray();
            }
        }

        public static uint[] ToUInt32Array(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                uint[] result = new uint[bytes.Length / sizeof(uint)];
                Buffer.BlockCopy(bytes, 0, result, 0, result.Length);

                return result;
            }
            else
            {
                List<uint> result = new List<uint>();

                for (int i = 0; i < bytes.Length; i += 4)
                {
                    result.Add(LEBytesToUInt32(bytes.Skip(i).Take(4).ToArray()));
                }

                return result.ToArray();
            }

        }

        public static byte[] LEBytes(ushort n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on big-endian machine
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] LEBytes(uint n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on big-endian machine
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] LEBytes(ulong n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (!BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on big-endian machine
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(ushort n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(uint n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
                Array.Reverse(resultBytes);
            }

            return resultBytes;
        }

        public static byte[] BEBytes(ulong n)
        {
            byte[] resultBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
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
                // Reverse array if we are on big-endian machine
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        public static uint LEBytesToUInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(uint))
            {
                throw new InteropException("Array size doesn't match the uint data type.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on big-endian machine
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong LEBytesToUInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(ulong))
            {
                throw new InteropException("Array size doesn't match the ulong data type.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on big-endian machine
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ushort BEBytesToUInt16(byte[] bytes)
        {
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the ushort data type.");
            }

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        public static uint BEBytesToUInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(uint))
            {
                throw new InteropException("Array size doesn't match the uint data type.");
            }

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong BEBytesToUInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(ulong))
            {
                throw new InteropException("Array size doesn't match the ulong data type.");
            }

            if (BitConverter.IsLittleEndian)
            {
                // Reverse array if we are on little-endian machine
                Array.Reverse(bytes);
            }

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
