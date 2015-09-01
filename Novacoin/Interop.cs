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
using System.Text;

namespace Novacoin
{
    /// <summary>
    /// Miscellaneous functions
    /// </summary>
    public class Interop
    {
        /// <summary>
        /// Convert array of unsigned integers to array of bytes.
        /// </summary>
        /// <param name="values">Array of unsigned integer values.</param>
        /// <returns>Byte array</returns>
        public static byte[] LEBytes(uint[] values)
        {
            var result = new byte[values.Length * sizeof(uint)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);

            return result;
        }

        /// <summary>
        /// Convert byte array to array of unsigned integers.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Array of integers</returns>
        public static uint[] ToUInt32Array(byte[] bytes)
        {
            var result = new uint[bytes.Length / sizeof(uint)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

            return result;
        }

        /// <summary>
        /// Reverse byte array
        /// </summary>
        /// <param name="source">Source array</param>
        /// <returns>Result array</returns>
        public static byte[] ReverseBytes(byte[] source)
        {
            var b = new byte[source.Length];

            source.CopyTo(b, 0);

            Array.Reverse(b);

            return b;
        }

        public static byte[] HexToArray(string hex)
        {
            int nChars = hex.Length;
            var bytes = new byte[nChars / 2];

            for (int i = 0; i < nChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public static uint GetTime()
        {
            return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
