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

namespace Novacoin
{
    public class ByteQueueException : Exception
    {
        public ByteQueueException()
        {
        }

        public ByteQueueException(string message)
            : base(message)
        {
        }

        public ByteQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ByteQueue
    {
        private int Index;
        private List<byte> Elements;

        public ByteQueue(IList<byte> List, int Start)
        {
            Elements = new List<byte>(List);
            Index = Start;
        }

        public ByteQueue(IList<byte> List)
        {
            Elements = new List<byte>(List);
            Index = 0;
        }

        public byte Get()
        {
            if (Elements.Count <= Index)
            {
                throw new ByteQueueException("No elements left.");
            }

            return Elements[Index++];
        }

        public byte GetCurrent()
        {
            return Elements[Index];
        }

        public byte[] Get(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new ByteQueueException("Unable to read requested amount of data.");
            }

            var result = Elements.GetRange(Index, Count).ToArray();
            Index += Count;

            return result;
        }

        public byte[] GetCurrent(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new ByteQueueException("Unable to read requested amount of data.");
            }

            var result = Elements.GetRange(Index, Count).ToArray();

            return result;
        }

        /// <summary>
        /// Current index value
        /// </summary>
        public int CurrentIndex
        {
            get { return Index; }
        }

        public ulong GetVarInt()
        {
            byte prefix = Get();

            switch (prefix)
            {
                case 0xfd: // ushort
                    return BitConverter.ToUInt16(Get(2), 0);
                case 0xfe: // uint
                    return BitConverter.ToUInt32(Get(4), 0);
                case 0xff: // ulong
                    return BitConverter.ToUInt64(Get(8), 0);
                default:
                    return prefix;
            }
        }
    }
}
