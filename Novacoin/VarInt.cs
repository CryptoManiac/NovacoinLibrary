﻿using System;
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
                    valueBytes = BitConverter.GetBytes((ushort)n);
                }
                else if (n <= uint.MaxValue)
                {
                    // uint flag
                    prefix = 0xfe;
                    valueBytes = BitConverter.GetBytes((uint)n);
                }
                else
                {
                    // ulong flag
                    prefix = 0xff;
                    valueBytes = BitConverter.GetBytes(n);
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

        /// <summary>
        /// Read and decode variable integer from wrapped list object.
        /// 
        /// Note: Should be used only if there is some variable integer data at current position. Otherwise you will get undefined behavior, so make sure that you know what you are doing.
        /// </summary>
        /// <param name="wBytes"></param>
        /// <returns></returns>
        public static ulong ReadVarInt(ref WrappedList<byte> wBytes)
        {
            byte prefix = wBytes.GetItem();

            switch (prefix)
            {
                case 0xfd: // ushort
                    return BitConverter.ToUInt16(wBytes.GetItems(2), 0);
                case 0xfe: // uint
                    return BitConverter.ToUInt32(wBytes.GetItems(4), 0);
                case 0xff: // ulong
                    return BitConverter.ToUInt64(wBytes.GetItems(8), 0);
                default:
                    return prefix;
            }

        }
    }
}
