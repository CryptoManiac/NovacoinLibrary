using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class VarInt
    {
        /// <summary>
        /// Encodes unsigned integer value into compact representation.
        /// 
        /// See https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers for additional information.
        /// </summary>
        /// <param name="n">Unsigned integer value</param>
        /// <returns>Byte sequence</returns>
        public static IList<byte> EncodeVarInt(ulong n)
        {
            List<byte> resultBytes = new List<byte>();

            if (n <= 0xfc)
            {
                // Values up to 0xfc are stored directly without any prefix
                resultBytes.Add((byte)n);
            }
            else
            {
                byte prefix;
                byte[] valueBytes;

                if (n <= ushort.MaxValue)
                {
                    // ushort flag
                    prefix = 0xfd;
                    valueBytes = Interop.LEBytes((ushort)n);
                }
                else if (n <= uint.MaxValue)
                {
                    // uint flag
                    prefix = 0xfe;
                    valueBytes = Interop.LEBytes((uint)n);
                }
                else
                {
                    // ulong flag
                    prefix = 0xff;
                    valueBytes = Interop.LEBytes(n);
                }

                resultBytes.Add(prefix);
                resultBytes.AddRange(valueBytes);
            }

            return resultBytes;
        }

        /// <summary>
        /// Encodes integer value into compact representation.
        /// 
        /// See https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers for additional information.
        /// </summary>
        /// <param name="n">Integer value</param>
        /// <returns>Byte sequence</returns>
        public static IList<byte> EncodeVarInt(long n)
        {
            return EncodeVarInt((ulong)n);
        }

        /// <summary>
        /// Decodes integer value from compact representation
        /// 
        /// See https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers for additional information.
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Integer value</returns>
        public static ulong DecodeVarInt(IList<byte> bytes)
        {
            byte prefix = bytes[0];

            bytes.RemoveAt(0); // Remove prefix
            byte[] bytesArray = bytes.ToArray();

            if (!BitConverter.IsLittleEndian)
            {
                // Values are stored in little-endian order
                Array.Reverse(bytesArray);
            }

            switch (prefix)
            {
                case 0xfd: // ushort flag
                    return BitConverter.ToUInt16(bytesArray, 0);
                case 0xfe: // uint flag
                    return BitConverter.ToUInt32(bytesArray, 0);
                case 0xff: // ulong flag
                    return BitConverter.ToUInt64(bytesArray, 0);
                default:
                    return prefix;
            }
        }
    }
}
