﻿/**
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
            var b = new byte[source.Length];

            source.CopyTo(b, 0);

            Array.Reverse(b);

            return b;
        }

        public static byte[] LEBytes(uint[] values)
        {
            var result = new byte[values.Length * sizeof(uint)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);

            return result;
        }

        public static uint[] ToUInt32Array(byte[] bytes)
        {
            var result = new uint[bytes.Length / sizeof(uint)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

            return result;
        }

        public static byte[] BEBytes(ushort n)
        {
            var resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }

        public static byte[] BEBytes(uint n)
        {
            var resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }

        public static byte[] BEBytes(ulong n)
        {
            var resultBytes = BitConverter.GetBytes(n);

            Array.Reverse(resultBytes);

            return resultBytes;
        }


        public static ushort BEBytesToUInt16(byte[] bytes)
        {
            if (bytes.Length < sizeof(ushort))
            {
                Array.Resize(ref bytes, sizeof(ushort));
            }
            if (bytes.Length != sizeof(ushort))
            {
                throw new InteropException("Array size doesn't match the ushort data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt16(bytes, 0);
        }

        public static uint BEBytesToUInt32(byte[] bytes)
        {
            if (bytes.Length < sizeof(uint))
            {
                Array.Resize(ref bytes, sizeof(uint));
            }
            else if (bytes.Length != sizeof(uint))
            {
                throw new InteropException("Array size doesn't match the uint data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong BEBytesToUInt64(byte[] bytes)
        {
            if (bytes.Length < sizeof(ulong))
            {
                Array.Resize(ref bytes, sizeof(ulong));
            }
            else if (bytes.Length != sizeof(ulong))
            {
                throw new InteropException("Array size doesn't match the ulong data type.");
            }

            Array.Reverse(bytes);

            return BitConverter.ToUInt64(bytes, 0);
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

        public static string ToHex(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
