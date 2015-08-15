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

    public class WrappedList<T>
    {
        private int Index;
        private List<T> Elements;

        public int ItemsLeft
        {
            get { return Elements.Count - Index; }
        }

        public WrappedList(IList<T> List, int Start)
        {
            Elements = new List<T>(List);
            Index = Start;
        }

        public WrappedList(IList<T> List)
        {
            Elements = new List<T>(List);
            Index = 0;
        }

        public T GetItem()
        {
            if (Elements.Count <= Index)
            {
                throw new WrappedListException("No elements left.");
            }

            return Elements[Index++];
        }

        public T[] GetItems(int Count)
        {
            if (ItemsLeft < Count)
            {
                throw new WrappedListException("Unable to read requested amount of data.");
            }

            T[] result = Elements.Skip<T>(Index).Take<T>(Count).ToArray<T>();
            Index += Count;

            return result;
        }

        public IEnumerable<T> GetEnumerableItems(int Count)
        {
            if (Elements.Count - Index < Count)
            {
                throw new WrappedListException("Unable to read requested amount of data.");
            }

            IEnumerable<T> result = Elements.Skip<T>(Index).Take<T>(Count);
            Index += Count;

            return result;
        }
    }
}
