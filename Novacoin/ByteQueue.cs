using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            byte[] result = Elements.Skip(Index).Take(Count).ToArray();
            Index += Count;

            return result;
        }

        public byte[] GetCurrent(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new ByteQueueException("Unable to read requested amount of data.");
            }

            byte[] result = Elements.Skip(Index).Take(Count).ToArray();

            return result;
        }

        /// <summary>
        /// Current index value
        /// </summary>
        public int CurrentIndex
        {
            get { return Index; }
        }

        public IEnumerable<byte> GetEnumerable(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new ByteQueueException("Unable to read requested amount of data.");
            }

            IEnumerable<byte> result = Elements.Skip(Index).Take(Count);
            Index += Count;

            return result;
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
