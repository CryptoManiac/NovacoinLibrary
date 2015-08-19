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
                Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

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

        public static void UInt16ToBE(ushort n, byte[] bs)
        {
            bs[0] = (byte)(n >> 8);
            bs[1] = (byte)(n);
        }

        public static ushort BEToUInt16(byte[] bs)
        {
            ushort n = (ushort)(bs[0] << 8);
            n |= (ushort)bs[1];
            return n;
        }

        public static ushort BEToUInt16(byte[] bs, int off)
        {
            ushort n = (ushort)(bs[off] << 8);
            n |= (ushort)bs[++off];
            return n;
        }

        public static void UInt16ToLE(ushort n, byte[] bs)
        {
            bs[0] = (byte)(n);
            bs[1] = (byte)(n >> 8);
        }

        public static void UInt16ToLE(ushort n, byte[] bs, int off)
        {
            bs[off] = (byte)(n);
            bs[++off] = (byte)(n >> 8);
        }

        public static ushort LEToUInt16(byte[] bs)
        {
            ushort n = (ushort)bs[0];
            n |= (ushort)(bs[1] << 8);
            return n;
        }

        public static ushort LEToUInt16(byte[] bs, int off)
        {
            ushort n = (ushort)bs[off];
            n |= (ushort)(bs[++off] << 8);
            return n;
        }

        public static void UInt32ToBE(uint n, byte[] bs)
        {
            bs[0] = (byte)(n >> 24);
            bs[1] = (byte)(n >> 16);
            bs[2] = (byte)(n >> 8);
            bs[3] = (byte)(n);
        }

        public static void UInt32ToBE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)(n >> 24);
            bs[++off] = (byte)(n >> 16);
            bs[++off] = (byte)(n >> 8);
            bs[++off] = (byte)(n);
        }

        public static uint BEToUInt32(byte[] bs)
        {
            uint n = (uint)bs[0] << 24;
            n |= (uint)bs[1] << 16;
            n |= (uint)bs[2] << 8;
            n |= (uint)bs[3];
            return n;
        }

        public static uint BEToUInt32(byte[] bs, int off)
        {
            uint n = (uint)bs[off] << 24;
            n |= (uint)bs[++off] << 16;
            n |= (uint)bs[++off] << 8;
            n |= (uint)bs[++off];
            return n;
        }

        public static ulong BEToUInt64(byte[] bs)
        {
            uint hi = BEToUInt32(bs);
            uint lo = BEToUInt32(bs, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        public static ulong BEToUInt64(byte[] bs, int off)
        {
            uint hi = BEToUInt32(bs, off);
            uint lo = BEToUInt32(bs, off + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        public static void UInt64ToBE(ulong n, byte[] bs)
        {
            UInt32ToBE((uint)(n >> 32), bs);
            UInt32ToBE((uint)(n), bs, 4);
        }

        public static void UInt64ToBE(ulong n, byte[] bs, int off)
        {
            UInt32ToBE((uint)(n >> 32), bs, off);
            UInt32ToBE((uint)(n), bs, off + 4);
        }

        public static void UInt32ToLE(uint n, byte[] bs)
        {
            bs[0] = (byte)(n);
            bs[1] = (byte)(n >> 8);
            bs[2] = (byte)(n >> 16);
            bs[3] = (byte)(n >> 24);
        }

        public static void UInt32ToLE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)(n);
            bs[++off] = (byte)(n >> 8);
            bs[++off] = (byte)(n >> 16);
            bs[++off] = (byte)(n >> 24);
        }

        public static uint LEToUInt32(byte[] bs)
        {
            uint n = (uint)bs[0];
            n |= (uint)bs[1] << 8;
            n |= (uint)bs[2] << 16;
            n |= (uint)bs[3] << 24;
            return n;
        }

        public static uint LEToUInt32(byte[] bs, int off)
        {
            uint n = (uint)bs[off];
            n |= (uint)bs[++off] << 8;
            n |= (uint)bs[++off] << 16;
            n |= (uint)bs[++off] << 24;
            return n;
        }

        public static ulong LEToUInt64(byte[] bs)
        {
            uint lo = LEToUInt32(bs);
            uint hi = LEToUInt32(bs, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        public static ulong LEToUInt64(byte[] bs, int off)
        {
            uint lo = LEToUInt32(bs, off);
            uint hi = LEToUInt32(bs, off + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        public static void UInt64ToLE(ulong n, byte[] bs)
        {
            UInt32ToLE((uint)(n), bs);
            UInt32ToLE((uint)(n >> 32), bs, 4);
        }

        public static void UInt64ToLE(ulong n, byte[] bs, int off)
        {
            UInt32ToLE((uint)(n), bs, off);
            UInt32ToLE((uint)(n >> 32), bs, off + 4);
        }

    }
}
