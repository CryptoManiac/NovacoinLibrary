using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class WrappedListException : Exception
    {
        public WrappedListException()
        {
        }

        public WrappedListException(string message)
            : base(message)
        {
        }

        public WrappedListException(string message, Exception inner)
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
                throw new WrappedListException("No elements left.");
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
                throw new WrappedListException("Unable to read requested amount of data.");
            }

            byte[] result = Elements.Skip(Index).Take(Count).ToArray();
            Index += Count;

            return result;
        }

        public byte[] GetCurrent(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new WrappedListException("Unable to read requested amount of data.");
            }

            byte[] result = Elements.Skip(Index).Take(Count).ToArray();

            return result;
        }

        public IEnumerable<byte> GetEnumerable(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new WrappedListException("Unable to read requested amount of data.");
            }

            IEnumerable<byte> result = Elements.Skip(Index).Take(Count);
            Index += Count;

            return result;
        }
    }
}
