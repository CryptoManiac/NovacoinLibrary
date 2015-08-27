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
using System.Diagnostics.Contracts;

namespace Novacoin
{
    [Serializable]
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
        private int _Index;
        private List<byte> _Elements;

        public ByteQueue(byte[] List, int Start)
        {
            _Elements = new List<byte>(List);
            _Index = Start;
        }

        public ByteQueue(byte[] List)
        {
            _Elements = new List<byte>(List);
            _Index = 0;
        }

        public ByteQueue(List<byte> List, int Start)
        {
            _Elements = new List<byte>(List);
            _Index = Start;
        }

        public ByteQueue(List<byte> List)
        {
            _Elements = new List<byte>(List);
            _Index = 0;
        }

        public byte Get()
        {
            if (_Elements.Count <= _Index)
            {
                throw new ByteQueueException("No elements left.");
            }

            return _Elements[_Index++];
        }

        public byte GetCurrent()
        {
            return _Elements[_Index];
        }

        public byte[] Get(int nCount)
        {
            Contract.Requires<ArgumentException>(Count - Index >= nCount, "nCount is greater than amount of elements.");

            var result = _Elements.GetRange(_Index, nCount).ToArray();
            _Index += nCount;

            return result;
        }

        public byte[] GetCurrent(int nCount)
        {
            Contract.Requires<ArgumentException>(Count - Index >= nCount, "nCount is greater than amount of elements.");

            var result = _Elements.GetRange(_Index, nCount).ToArray();

            return result;
        }

        /// <summary>
        /// Current index value
        /// </summary>
        public int Index
        {
            get { return _Index; }
        }

        public int Count
        {
            get { return _Elements.Count; }
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
