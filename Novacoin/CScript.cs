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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Novacoin
{
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
        public CScript(byte[] bytes)
        {
            codeBytes = new List<byte>(bytes);
        }

        /// <summary>
        /// Return a new instance of ByteQueue object for current code bytes
        /// </summary>
        /// <returns></returns>
        public InstructionQueue GetInstructionQueue()
        {
             return new InstructionQueue(ref codeBytes);
        }

        /// <summary>
        /// Add serialized number to instructions list.
        /// </summary>
        /// <param name="n">Number to add.</param>
        public void AddNumber(int n)
        {
            if (n == -1 || (n >= 1 && n <= 16))
            {
                codeBytes.Add((byte)ScriptCode.EncodeOP_N(n, true));
            }
            else
            {
                BigInteger bn = n;
                PushData(bn.ToByteArray());
            }
        }

        /// <summary>
        /// Adds specified operation to instruction list
        /// </summary>
        /// <param name="opcode">Instruction to add.</param>
        public void AddInstruction(instruction opcode)
        {
            Contract.Requires<ArgumentException>(opcode >= instruction.OP_0 && opcode <= instruction.OP_INVALIDOPCODE, "Invalid instruction.");

            codeBytes.Add((byte)opcode);
        }

        /// <summary>
        /// Adds hash to instruction list.
        ///    New items are added in this format:
        ///    hash_length_byte hash_bytes
        /// </summary>
        /// <param name="hash">uint160 instance</param>
        public void AddHash(uint160 hash)
        {
            codeBytes.Add((byte)hash.Size);
            codeBytes.AddRange((byte[])hash);
        }

        /// <summary>
        /// Adds hash to instruction list.
        ///    New items are added in this format:
        ///    hash_length_byte hash_bytes
        /// </summary>
        /// <param name="hash">uint256 instance</param>
        public void AddHash(uint256 hash)
        {
            codeBytes.Add((byte)hash.Size);
            codeBytes.AddRange((byte[])hash);
        }

        /// <summary>
        /// Create new OP_PUSHDATAn operator and add it to instruction list
        /// </summary>
        /// <param name="dataBytes">Set of data bytes</param>
        public void PushData(byte[] dataBytes)
        {
            var nCount = dataBytes.LongLength;

            if (nCount < (int)instruction.OP_PUSHDATA1)
            {
                // OP_0 and OP_FALSE
                codeBytes.Add((byte)nCount);
            }
            else if (nCount < 0xff)
            {
                // OP_PUSHDATA1 0x01 [0x5a]
                codeBytes.Add((byte)instruction.OP_PUSHDATA1);
                codeBytes.Add((byte)nCount);
            }
            else if (nCount < 0xffff)
            {
                // OP_PUSHDATA1 0x01 0x00 [0x5a]
                codeBytes.Add((byte)instruction.OP_PUSHDATA2);

                var szBytes = BitConverter.GetBytes((ushort)nCount);
                codeBytes.AddRange(szBytes);
            }
            else if (nCount < 0xffffffff)
            {
                // OP_PUSHDATA1 0x01 0x00 0x00 0x00 [0x5a]
                codeBytes.Add((byte)instruction.OP_PUSHDATA4);

                var szBytes = BitConverter.GetBytes((uint)nCount);
                codeBytes.AddRange(szBytes);
            }

            // Add data bytes
            codeBytes.AddRange(dataBytes);
        }

        /// <summary>
        /// Just insert data array without including any prefixes. Please make sure that you know what you're doing, 
        ///    it is recommended to use AddInstruction, AddHash or PushData instead.
        /// </summary>
        /// <param name="dataBytes">Data bytes</param>
        public void AddRawData(byte[] dataBytes)
        {
            // Add data bytes
            codeBytes.AddRange(dataBytes);
        }

        /// <summary>
        /// Scan pushed data bytes for pattern and, in case of exact match, remove it.
        /// </summary>
        /// <param name="pattern">Pattern sequence</param>
        /// <returns>Matches count</returns>
        public int RemovePattern(byte[] pattern)
        {
            // There is no sense to continue if pattern is empty or longer than script itself
            if (pattern.Length == 0 || pattern.Length > codeBytes.Count)
            {
                return 0;
            }

            var count = 0;
            var bq1 = new InstructionQueue(ref codeBytes);

            byte[] pushData;
            instruction opcode;

            var newScript = new CScript();

            while (ScriptCode.GetOp(ref bq1, out opcode, out pushData))
            {
                if (pushData.Length == 0)
                {
                    // No data, put instruction on its place
                    newScript.AddInstruction(opcode);
                }
                else if (!pushData.SequenceEqual(pattern))
                {
                    // No match, create push operator
                    newScript.PushData(pushData);
                }
                else
                {
                    count++; // match
                }
            }

            if (count > 0)
            {
                // Replace current script if any matches were found
                codeBytes = newScript.codeBytes;
            }

            return count;
        }

        /// <summary>
        /// Scan script for specific instruction and remove it if there are some matches.
        /// </summary>
        /// <param name="op">Instruction</param>
        /// <returns>Matches count</returns>
        public int RemoveInstruction(instruction op)
        {
            byte[] pushData;
            instruction opcode;

            var count = 0;
            var newScript = new CScript();
            var bq1 = new InstructionQueue(ref codeBytes);

            while (ScriptCode.GetOp(ref bq1, out opcode, out pushData))
            {
                if (pushData.Length != 0 && op != opcode)
                {
                    // If instruction didn't match then push its data again
                    newScript.PushData(pushData);
                }
                else if (Enum.IsDefined(typeof(instruction), op) && op != opcode)
                {
                    // Instruction didn't match
                    newScript.AddInstruction(opcode);
                }
                else
                {
                    count++; // match
                }
            }

            if (count > 0)
            {
                // Replace current script if any matches were found
                codeBytes = newScript.codeBytes;
            }

            return count;
        }

        /// <summary>
        /// Is it true that script doesn't contain anything except push value operations?
        /// </summary>
        public bool IsPushOnly
        {
            get
            {
                var wCodeBytes = new InstructionQueue(ref codeBytes);

                instruction opcode; // Current instruction
                byte[] pushArgs; // OP_PUSHDATAn argument

                // Scan instructions sequence
                while (ScriptCode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
                {
                    if (opcode > instruction.OP_16)
                    {
                        // We don't allow control instructions here
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Is it true that script doesn't contain non-canonical push operations?
        /// </summary>
        public bool HasOnlyCanonicalPushes
        {
            get
            {
                var wCodeBytes = new InstructionQueue(ref codeBytes);

                byte[] pushArgs; // OP_PUSHDATAn argument
                instruction opcode; // Current instruction

                // Scan instructions sequence
                while (ScriptCode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
                {
                    var data = pushArgs;

                    if (opcode < instruction.OP_PUSHDATA1 && opcode > instruction.OP_0 && (data.Length == 1 && data[0] <= 16))
                    {
                        // Could have used an OP_n code, rather than a 1-byte push.
                        return false;
                    }
                    if (opcode == instruction.OP_PUSHDATA1 && data.Length < (int)instruction.OP_PUSHDATA1)
                    {
                        // Could have used a normal n-byte push, rather than OP_PUSHDATA1.
                        return false;
                    }
                    if (opcode == instruction.OP_PUSHDATA2 && data.Length <= 0xFF)
                    {
                        // Could have used an OP_PUSHDATA1.
                        return false;
                    }
                    if (opcode == instruction.OP_PUSHDATA4 && data.LongLength <= 0xFFFF)
                    {
                        // Could have used an OP_PUSHDATA2.
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Quick test for pay-to-script-hash CScripts
        /// </summary>
        public bool IsPayToScriptHash
        {
            get
            {
                // Sender provides redeem script hash, receiver provides signature list and redeem script
                // OP_HASH160 20 [20 byte hash] OP_EQUAL
                return (codeBytes.Count() == 23 &&
                        codeBytes[0] == (byte)instruction.OP_HASH160 &&
                        codeBytes[1] == 0x14 && // 20 bytes hash length prefix
                        codeBytes[22] == (byte)instruction.OP_EQUAL);
            }
        }

        /// <summary>
        /// Quick test for pay-to-pubkeyhash CScripts
        /// </summary>
        public bool IsPayToPubKeyHash
        {
            get
            {
                // Sender provides hash of pubkey, receiver provides signature and pubkey
                // OP_DUP OP_HASH160 20 [20 byte hash] OP_EQUALVERIFY OP_CHECKSIG
                return (codeBytes.Count == 25 &&
                        codeBytes[0] == (byte)instruction.OP_DUP &&
                        codeBytes[1] == (byte)instruction.OP_HASH160 &&
                        codeBytes[2] == 0x14 && // 20 bytes hash length prefix
                        codeBytes[23] == (byte)instruction.OP_EQUALVERIFY &&
                        codeBytes[24] == (byte)instruction.OP_CHECKSIG);
            }
        }

        /// <summary>
        /// Quick test for Null destination
        /// </summary>
        public bool IsNull
        {
            get { return codeBytes.Count == 0; }
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
        public uint GetSigOpCount(bool fAccurate)
        {
            var wCodeBytes = new InstructionQueue(ref codeBytes);

            instruction opcode; // Current instruction
            byte[] pushArgs; // OP_PUSHDATAn argument

            uint nCount = 0;
            var lastOpcode = instruction.OP_INVALIDOPCODE;

            // Scan instructions sequence
            while (ScriptCode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                if (opcode == instruction.OP_CHECKSIG || opcode == instruction.OP_CHECKSIGVERIFY)
                {
                    nCount++;
                }
                else if (opcode == instruction.OP_CHECKMULTISIG || opcode == instruction.OP_CHECKMULTISIGVERIFY)
                {
                    if (fAccurate && lastOpcode >= instruction.OP_1 && lastOpcode <= instruction.OP_16)
                    {
                        nCount += (uint)ScriptCode.DecodeOP_N(lastOpcode);
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
        public uint GetSigOpCount(CScript scriptSig)
        {
            if (!IsPayToScriptHash)
            {
                return GetSigOpCount(true);
            }

            // This is a pay-to-script-hash scriptPubKey;
            // get the last item that the scriptSig
            // pushes onto the stack:
            InstructionQueue wScriptSig = scriptSig.GetInstructionQueue();
            uint nScriptSigSize = scriptSig.Size;

            instruction opcode; // Current instruction
            byte[] pushArgs = new byte[0]; // OP_PUSHDATAn argument


            while (wScriptSig.Index < nScriptSigSize)
            {
                if (!ScriptCode.GetOp(ref wScriptSig, out opcode, out pushArgs))
                {
                    return 0;
                }

                if (opcode > instruction.OP_16)
                {
                    return 0;
                }
            }

            /// ... and return its opcount:
            var subScript = new CScript(pushArgs);

            return subScript.GetSigOpCount(true);

        }

        /// <summary>
        /// Set pay-to-pubkey destination.
        /// </summary>
        /// <param name="pubKey">Instance of CPubKey.</param>
        public void SetDestination(CPubKey pubKey)
        {
            codeBytes.Clear();
            PushData(pubKey);
            AddInstruction(instruction.OP_CHECKSIG);
        }

        /// <summary>
        /// Set pay-to-pubkeyhash destination
        /// </summary>
        /// <param name="ID">Public key hash</param>
        public void SetDestination(CKeyID ID)
        {
            codeBytes.Clear();
            AddInstruction(instruction.OP_DUP);
            AddInstruction(instruction.OP_HASH160);
            AddHash(ID);
            AddInstruction(instruction.OP_EQUALVERIFY);
            AddInstruction(instruction.OP_CHECKSIG);
        }

        /// <summary>
        /// Set pay-to-scripthash destination
        /// </summary>
        /// <param name="ID">Script hash</param>
        public void SetDestination(CScriptID ID)
        {
            codeBytes.Clear();
            AddInstruction(instruction.OP_HASH160);
            AddHash(ID);
            AddInstruction(instruction.OP_EQUAL);
        }

        /// <summary>
        /// Reset script code buffer.
        /// </summary>
        public void SetNullDestination()
        {
            codeBytes.Clear();
        }

        /// <summary>
        /// Set multisig destination.
        /// </summary>
        /// <param name="nRequired">Amount of required signatures.</param>
        /// <param name="keys">Set of public keys.</param>
        public void SetMultiSig(int nRequired, CPubKey[] keys)
        {
            codeBytes.Clear();
            AddInstruction(ScriptCode.EncodeOP_N(nRequired));

            foreach (var key in keys)
            {
                PushData(key);
            }

            AddInstruction(ScriptCode.EncodeOP_N(keys.Length));
            AddInstruction(instruction.OP_CHECKMULTISIG);
        }

        /// <summary>
        /// Access to script code.
        /// </summary>
        public static implicit operator byte[] (CScript script)
        {
            return script.codeBytes.ToArray();
        }

        /// <summary>
        /// Implicit cast of byte array to CScript.
        /// </summary>
        /// <param name="scriptBytes"></param>
        public static implicit operator CScript(byte[] scriptBytes)
        {
            return new CScript(scriptBytes);
        }

        /// <summary>
        /// Script size
        /// </summary>
        public uint Size
        {
            get { return (uint) codeBytes.Count; }
        }

        public CScriptID ScriptID
        {
            get { return new CScriptID(CryptoUtils.ComputeHash160(codeBytes.ToArray())); }
        }

        /// <summary>
        /// Disassemble current script code
        /// </summary>
        /// <returns>Code listing</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
            var wCodeBytes = new InstructionQueue(ref codeBytes);

            instruction opcode; // Current instruction
            byte[] pushArgs; // OP_PUSHDATAn argument
            while (ScriptCode.GetOp(ref wCodeBytes, out opcode, out pushArgs))
            {
                if (sb.Length != 0)
                {
                    sb.Append(" ");
                }

                if (0 <= opcode && opcode <= instruction.OP_PUSHDATA4)
                {
                    sb.Append(ScriptCode.ValueString(pushArgs));
                }
                else
                {
                    sb.Append(ScriptCode.GetOpName(opcode));
                }
            }

            return sb.ToString();
		}
	}
}
