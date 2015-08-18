using System;
using System.Linq;
using System.Text;
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
        /// <param name="bytes">Enumerator interface for byte sequence</param>
        public CScript(IEnumerable<byte> bytes)
        {
            codeBytes = new List<byte>(bytes);
        }

        /// <summary>
        /// Return a new instance of WrappedList object for current code bytes
        /// </summary>
        /// <returns></returns>
        public WrappedList<byte> GetWrappedList()
        {
             return new WrappedList<byte>(codeBytes);
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

        /// <summary>
        /// Create new OP_PUSHDATAn operator and add it to opcode bytes list
        /// </summary>
        /// <param name="dataBytes">List of data bytes</param>
        public void PushData(IList<byte> dataBytes)
        {
            long nCount = dataBytes.LongCount();

            if (nCount < (int)opcodetype.OP_PUSHDATA1)
            {
                // OP_0 and OP_FALSE
                codeBytes.Add((byte)nCount);
            }
            else if (nCount < 0xff)
            {
                // OP_PUSHDATA1 0x01 [0x5a]
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA1);
                codeBytes.Add((byte)nCount);
            }
            else if (nCount < 0xffff)
            {
                // OP_PUSHDATA1 0x00 0x01 [0x5a]
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA2);

                byte[] szBytes = Interop.BEBytes((ushort)nCount);
                codeBytes.AddRange(szBytes);
            }
            else if (nCount < 0xffffffff)
            {
                // OP_PUSHDATA1 0x00 0x00 0x00 0x01 [0x5a]
                codeBytes.Add((byte)opcodetype.OP_PUSHDATA4);

                byte[] szBytes = Interop.BEBytes((uint)nCount);
                codeBytes.AddRange(szBytes);
            }

            // Add data bytes
            codeBytes.AddRange(dataBytes);
        }

        /// <summary>
        /// Scan code bytes for pattern
        /// </summary>
        /// <param name="pattern">Pattern sequence</param>
        /// <returns>Matches enumerator</returns>
        private IEnumerable<int> FindPattern(IList<byte> pattern)
        {
            for (int i = 0; i < codeBytes.Count; i++)
            {
                if (codeBytes.Skip(i).Take(pattern.Count).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Scan code bytes for pattern and remove it
        /// </summary>
        /// <param name="pattern">Pattern sequence</param>
        /// <returns>Matches number</returns>
        public int RemovePattern(IList<byte> pattern)
        {
            List<byte> resultBytes = new List<byte>(codeBytes);
            int count = 0;
            int patternLen = pattern.Count;
                        
            foreach (int i in FindPattern(pattern))
            {
                resultBytes.RemoveRange(i - count * patternLen, patternLen);
                count++;
            }

            codeBytes = resultBytes;
            
            return count;
        }

        /// <summary>
        /// Is it true that script doesn't contain anything except push value operations?
        /// </summary>
        /// <returns>Checking result</returns>
        public bool IsPushonly()
        {
            WrappedList<byte> wCodeBytes = new WrappedList<byte>(codeBytes);

            opcodetype opcode; // Current opcode
            IEnumerable<byte> pushArgs; // OP_PUSHDATAn argument
            
            // Scan opcodes sequence
            while (ScriptOpcode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                if (opcode > opcodetype.OP_16)
                {
                    // We don't allow control opcodes here
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Is it true that script doesn't contain non-canonical push operations?
        /// </summary>
        /// <returns>Checking result</returns>
        public bool HashOnlyCanonicalPushes()
        {
            WrappedList<byte> wCodeBytes = new WrappedList<byte>(codeBytes);

            opcodetype opcode; // Current opcode
            IEnumerable<byte> pushArgs; // OP_PUSHDATAn argument

            // Scan opcodes sequence
            while (ScriptOpcode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                byte[] data = pushArgs.ToArray();

                if (opcode < opcodetype.OP_PUSHDATA1 && opcode > opcodetype.OP_0 && (data.Length == 1 && data[0] <= 16))
                {
                    // Could have used an OP_n code, rather than a 1-byte push.
                    return false;
                }
                if (opcode == opcodetype.OP_PUSHDATA1 && data.Length < (int)opcodetype.OP_PUSHDATA1)
                {
                    // Could have used a normal n-byte push, rather than OP_PUSHDATA1.
                    return false;
                }
                if (opcode == opcodetype.OP_PUSHDATA2 && data.Length <= 0xFF)
                {
                    // Could have used an OP_PUSHDATA1.
                    return false;
                }
                if (opcode == opcodetype.OP_PUSHDATA4 && data.LongLength <= 0xFFFF)
                {
                    // Could have used an OP_PUSHDATA2.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Quick test for pay-to-script-hash CScripts
        /// </summary>
        /// <returns>Checking result</returns>
        public bool IsPayToScriptHash()
        {
            return (codeBytes.Count() == 23 &&
                    codeBytes[0] == (byte)opcodetype.OP_HASH160 &&
                    codeBytes[1] == 0x14 &&
                    codeBytes[22] == (byte)opcodetype.OP_EQUAL);
        }

        /// <summary>
        /// Pre-version-0.6, Bitcoin always counted CHECKMULTISIGs
        /// as 20 sigops. With pay-to-script-hash, that changed:
        /// CHECKMULTISIGs serialized in scriptSigs are
        /// counted more accurately, assuming they are of the form
        ///  ... OP_N CHECKMULTISIG ...
        /// </summary>
        /// <param name="fAccurate">Legacy mode flag</param>
        /// <returns>Amount of sigops</returns>
        public int GetSigOpCount(bool fAccurate)
        {
            WrappedList<byte> wCodeBytes = new WrappedList<byte>(codeBytes);

            opcodetype opcode; // Current opcode
            IEnumerable<byte> pushArgs; // OP_PUSHDATAn argument

            int nCount = 0;
            opcodetype lastOpcode = opcodetype.OP_INVALIDOPCODE;

            // Scan opcodes sequence
            while (ScriptOpcode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                if (opcode == opcodetype.OP_CHECKSIG || opcode == opcodetype.OP_CHECKSIGVERIFY)
                {
                    nCount++;
                }
                else if (opcode == opcodetype.OP_CHECKMULTISIG || opcode == opcodetype.OP_CHECKMULTISIGVERIFY)
                {
                    if (fAccurate && lastOpcode >= opcodetype.OP_1 && lastOpcode <= opcodetype.OP_16)
                    {
                        nCount += ScriptOpcode.DecodeOP_N(lastOpcode);
                    }
                    else
                    {
                        nCount += 20;
                    }
                }
            }

            return nCount;
        }

        /// <summary>
        /// Accurately count sigOps, including sigOps in
        /// pay-to-script-hash transactions
        /// </summary>
        /// <param name="scriptSig">pay-to-script-hash scriptPubKey</param>
        /// <returns>SigOps count</returns>
        public int GetSigOpCount(CScript scriptSig)
        {
            if (!IsPayToScriptHash())
            {
                return GetSigOpCount(true);
            }

            // This is a pay-to-script-hash scriptPubKey;
            // get the last item that the scriptSig
            // pushes onto the stack:
            WrappedList<byte> wScriptSig = scriptSig.GetWrappedList();

            opcodetype opcode; // Current opcode
            IEnumerable<byte> pushArgs; // OP_PUSHDATAn argument

            while (ScriptOpcode.GetOp(ref wScriptSig, out opcode, out pushArgs))
            {
                if (opcode > opcodetype.OP_16)
                {
                    return 0;
                }
            }

            /// ... and return its opcount:
            CScript subScript = new CScript(pushArgs);

            return subScript.GetSigOpCount(true);

        }

        public void SetDestination(CKeyID ID)
        {
            codeBytes.Clear();
            AddOp(opcodetype.OP_DUP);
            AddOp(opcodetype.OP_HASH160);
            AddHash(ID);
            AddOp(opcodetype.OP_EQUAL);
        }

        public void SetDestination(CScriptID ID)
        {
            codeBytes.Clear();
            AddOp(opcodetype.OP_HASH160);
            AddHash(ID);
            AddOp(opcodetype.OP_EQUAL);
        }

        public void SetMultiSig(int nRequired, IEnumerable<CPubKey> keys)
        {
            codeBytes.Clear();
            AddOp(ScriptOpcode.EncodeOP_N(nRequired));

            foreach (CPubKey key in keys)
            {
                PushData(key.Public.ToList());
            }
            AddOp(ScriptOpcode.EncodeOP_N(keys.Count()));
            AddOp(opcodetype.OP_CHECKMULTISIG);
        }

        /// <summary>
        /// Disassemble current script code
        /// </summary>
        /// <returns>Code listing</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
            WrappedList<byte> wCodeBytes = new WrappedList<byte>(codeBytes);

            opcodetype opcode; // Current opcode
            IEnumerable<byte> pushArgs; // OP_PUSHDATAn argument
            while (ScriptOpcode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                if (sb.Length != 0)
                {
                    sb.Append(" ");
                }

                if (0 <= opcode && opcode <= opcodetype.OP_PUSHDATA4)
                {
                    sb.Append(ScriptOpcode.ValueString(pushArgs));
                }
                else
                {
                    sb.Append(ScriptOpcode.GetOpName(opcode));
                }
            }

            return sb.ToString();
		}
	}
}

