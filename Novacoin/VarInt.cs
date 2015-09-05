/**
 *  Novacoin classes library
 *  Copyright (C) 2015 Alex D. (balthazar.ad@gmail.com)

 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.

 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;

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
        public static byte[] EncodeVarInt(ulong n)
        {
            var resultBytes = new List<byte>();

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

            return resultBytes.ToArray();
        }

        /// <summary>
        /// Encodes integer value into compact representation.
        /// 
        /// See https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers for additional information.
        /// </summary>
        /// <param name="n">Integer value</param>
        /// <returns>Byte sequence</returns>
        public static byte[] EncodeVarInt(long n)
        {
            return EncodeVarInt((ulong)n);
        }

        public static int GetEncodedSize(long n)
        {
            if (n <= 0xfc)
            {
                return 1;
            }
            else if (n <= ushort.MaxValue)
            {
                return 3;
            }
            else if (n <= uint.MaxValue)
            {
                return 5;
            }
            else
            {
                return 9;
            }
        }

        /// <summary>
        /// Decodes integer value from compact representation
        /// 
        /// See https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers for additional information.
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Integer value</returns>
        public static ulong DecodeVarInt(byte[] bytes)
        {
            var prefix = bytes[0];

            if (bytes.Length > 1)
            {
                var bytesArray = new byte[bytes.Length - 1];
                Array.Copy(bytes, 1, bytesArray, 0, bytesArray.Length);

                switch (prefix)
                {
                    case 0xfd: // ushort flag
                        return BitConverter.ToUInt16(bytesArray, 0);
                    case 0xfe: // uint flag
                        return BitConverter.ToUInt32(bytesArray, 0);
                    case 0xff: // ulong flag
                        return BitConverter.ToUInt64(bytesArray, 0);
                }
            }
            
            return prefix; // Values lower than 0xfd are stored directly
        }

        public static ulong ReadVarInt(ref BinaryReader reader)
        {
            byte prefix = reader.ReadByte();

            switch (prefix)
            {
                case 0xfd: // ushort
                    return reader.ReadUInt16();
                case 0xfe: // uint
                    return reader.ReadUInt32();
                case 0xff: // ulong
                    return reader.ReadUInt64();
                default:
                    return prefix;
            }
        }

    }
}
