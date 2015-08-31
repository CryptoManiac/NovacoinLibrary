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
using System.IO;

namespace Novacoin
{
    [Serializable]
    public class InstructionQueueException : Exception
    {
        public InstructionQueueException()
        {
        }

        public InstructionQueueException(string message)
            : base(message)
        {
        }

        public InstructionQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Stream of instructions.
    /// </summary>
    public class InstructionQueue : IDisposable
    {
        private bool disposed = false;

        private MemoryStream _Stream;
        private BinaryReader _Reader;

        public InstructionQueue(ref List<byte> List, int Start)
        {
            _Stream = new MemoryStream(List.ToArray());
            _Stream.Seek(Start, SeekOrigin.Begin);
            _Reader = new BinaryReader(_Stream);
        }

        public InstructionQueue(ref List<byte> List)
        {
            _Stream = new MemoryStream(List.ToArray());
            _Reader = new BinaryReader(_Stream);
        }

        ~InstructionQueue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _Reader.Dispose();
                    _Stream.Dispose();
                }

                disposed = true;
            }
        }

        public byte Get()
        {
            if (_Stream.Position == _Stream.Length)
            {
                throw new InstructionQueueException("No instructions left.");
            }

            return _Reader.ReadByte();
        }

        public bool TryGet(ref byte Element)
        {
            if (_Stream.Position == _Stream.Length)
            {
                return false;
            }

            Element = _Reader.ReadByte();

            return true;
        }

        public byte[] Get(int nCount)
        {
            Contract.Requires<ArgumentException>(Count - Index >= nCount, "nCount is greater than amount of instructions.");

            return _Reader.ReadBytes(nCount);
        }

        public bool TryGet(int nCount, ref byte[] Elements)
        {
            Elements = _Reader.ReadBytes(nCount);
            return (Elements.Length == nCount);
        }

        /// <summary>
        /// Current index value
        /// </summary>
        public int Index
        {
            get { return (int)_Stream.Position; }
        }

        /// <summary>
        /// Stream length
        /// </summary>
        public int Count
        {
            get { return (int)_Stream.Length; }
        }
    }
}


