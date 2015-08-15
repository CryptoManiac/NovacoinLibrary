using System;
using System.Text;

using System.Collections;
using System.Collections.Generic;

namespace Novacoin
{
    public class CScriptException : Exception
    {
        public CScriptException()
        {
        }

        public CScriptException(string message)
            : base(message)
        {
        }

        public CScriptException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Representation of script code
    /// </summary>
	public class CScript
	{
        private List<byte> codeBytes;

        /// <summary>
        /// Initializes an empty instance of CScript
        /// </summary>
		public CScript ()
		{
            codeBytes = new List<byte>();
		}

        /// <summary>
        /// Initializes new instance of CScript and fills it with supplied bytes
        /// </summary>
        /// <param name="bytes">List of bytes</param>
        public CScript(IList<byte> bytes)
        {
            codeBytes = new List<byte>(bytes);
        }

        /// <summary>
        /// Initializes new instance of CScript and fills it with supplied bytes
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        public CScript(byte[] bytes)
        {
            codeBytes = new List<byte>(bytes);
        }

        /// <summary>
        /// Adds specified operation to opcode bytes list
        /// </summary>
        /// <param name="opcode"></param>
        public void AddOp(opcodetype opcode)
        {
            if (opcode < opcodetype.OP_0 || opcode > opcodetype.OP_INVALIDOPCODE)
            {
                throw new CScriptException("CScript::AddOp() : invalid opcode");
            }

            codeBytes.Add((byte)opcode);
        }

        /// <summary>
        /// Adds hash to opcode bytes list.
        ///    New items are added in this format:
        ///    hash_length_byte hash_bytes
        /// </summary>
        /// <param name="hash">Hash160 instance</param>
        public void AddHash(Hash160 hash)
        {
            codeBytes.Add((byte)hash.hashSize);
            codeBytes.AddRange(hash.hashBytes);
        }

        /// <summary>
        /// Adds hash to opcode bytes list.
        ///    New items are added in this format:
        ///    hash_length_byte hash_bytes
        /// </summary>
        /// <param name="hash">Hash256 instance</param>
        public void AddHash(Hash256 hash)
        {
            codeBytes.Add((byte)hash.hashSize);
            codeBytes.AddRange(hash.hashBytes);
        }

        public void PushData(IList<byte> dataBytes)
        {
            if (dataBytes.Count < (int)opcodetype.OP_PUSHDATA1)
            {
                codeBytes.Add((byte)dataBytes.Count);
            }
            else if (dataBytes.Count < 0xff)
            {
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA1);
                codeBytes.Add((byte)dataBytes.Count);
            }
            else if (dataBytes.Count < 0xffff)
            {
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA2);

                byte[] szBytes = BitConverter.GetBytes((short)dataBytes.Count);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(szBytes);

                codeBytes.AddRange(szBytes);
            }
            else if ((uint)dataBytes.Count < 0xffffffff)
            {
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA2);

                byte[] szBytes = BitConverter.GetBytes((uint)dataBytes.Count);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(szBytes);

                codeBytes.AddRange(szBytes);
            }
            codeBytes.AddRange(dataBytes);
        }

		public override string ToString()
		{
			// TODO: disassembly 

			StringBuilder sb = new StringBuilder();

            //
            
            return sb.ToString();
		}
	}
}

