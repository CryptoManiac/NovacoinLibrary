/**
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Novacoin
{
    /// <summary>
    /// Script opcodes
    /// </summary>
    public enum instruction
    {
        // push value
        OP_0 = 0x00,
        OP_FALSE = OP_0,
        OP_PUSHDATA1 = 0x4c,
        OP_PUSHDATA2 = 0x4d,
        OP_PUSHDATA4 = 0x4e,
        OP_1NEGATE = 0x4f,
        OP_RESERVED = 0x50,
        OP_1 = 0x51,
        OP_TRUE = OP_1,
        OP_2 = 0x52,
        OP_3 = 0x53,
        OP_4 = 0x54,
        OP_5 = 0x55,
        OP_6 = 0x56,
        OP_7 = 0x57,
        OP_8 = 0x58,
        OP_9 = 0x59,
        OP_10 = 0x5a,
        OP_11 = 0x5b,
        OP_12 = 0x5c,
        OP_13 = 0x5d,
        OP_14 = 0x5e,
        OP_15 = 0x5f,
        OP_16 = 0x60,

        // control
        OP_NOP = 0x61,
        OP_VER = 0x62,
        OP_IF = 0x63,
        OP_NOTIF = 0x64,
        OP_VERIF = 0x65,
        OP_VERNOTIF = 0x66,
        OP_ELSE = 0x67,
        OP_ENDIF = 0x68,
        OP_VERIFY = 0x69,
        OP_RETURN = 0x6a,

        // stack ops
        OP_TOALTSTACK = 0x6b,
        OP_FROMALTSTACK = 0x6c,
        OP_2DROP = 0x6d,
        OP_2DUP = 0x6e,
        OP_3DUP = 0x6f,
        OP_2OVER = 0x70,
        OP_2ROT = 0x71,
        OP_2SWAP = 0x72,
        OP_IFDUP = 0x73,
        OP_DEPTH = 0x74,
        OP_DROP = 0x75,
        OP_DUP = 0x76,
        OP_NIP = 0x77,
        OP_OVER = 0x78,
        OP_PICK = 0x79,
        OP_ROLL = 0x7a,
        OP_ROT = 0x7b,
        OP_SWAP = 0x7c,
        OP_TUCK = 0x7d,

        // splice ops
        OP_CAT = 0x7e,
        OP_SUBSTR = 0x7f,
        OP_LEFT = 0x80,
        OP_RIGHT = 0x81,
        OP_SIZE = 0x82,

        // bit logic
        OP_INVERT = 0x83,
        OP_AND = 0x84,
        OP_OR = 0x85,
        OP_XOR = 0x86,
        OP_EQUAL = 0x87,
        OP_EQUALVERIFY = 0x88,
        OP_RESERVED1 = 0x89,
        OP_RESERVED2 = 0x8a,

        // numeric
        OP_1ADD = 0x8b,
        OP_1SUB = 0x8c,
        OP_2MUL = 0x8d,
        OP_2DIV = 0x8e,
        OP_NEGATE = 0x8f,
        OP_ABS = 0x90,
        OP_NOT = 0x91,
        OP_0NOTEQUAL = 0x92,

        OP_ADD = 0x93,
        OP_SUB = 0x94,
        OP_MUL = 0x95,
        OP_DIV = 0x96,
        OP_MOD = 0x97,
        OP_LSHIFT = 0x98,
        OP_RSHIFT = 0x99,

        OP_BOOLAND = 0x9a,
        OP_BOOLOR = 0x9b,
        OP_NUMEQUAL = 0x9c,
        OP_NUMEQUALVERIFY = 0x9d,
        OP_NUMNOTEQUAL = 0x9e,
        OP_LESSTHAN = 0x9f,
        OP_GREATERTHAN = 0xa0,
        OP_LESSTHANOREQUAL = 0xa1,
        OP_GREATERTHANOREQUAL = 0xa2,
        OP_MIN = 0xa3,
        OP_MAX = 0xa4,

        OP_WITHIN = 0xa5,

        // crypto
        OP_RIPEMD160 = 0xa6,
        OP_SHA1 = 0xa7,
        OP_SHA256 = 0xa8,
        OP_HASH160 = 0xa9,
        OP_HASH256 = 0xaa,
        OP_CODESEPARATOR = 0xab,
        OP_CHECKSIG = 0xac,
        OP_CHECKSIGVERIFY = 0xad,
        OP_CHECKMULTISIG = 0xae,
        OP_CHECKMULTISIGVERIFY = 0xaf,

        // expansion
        OP_NOP1 = 0xb0,
        OP_NOP2 = 0xb1,
        OP_NOP3 = 0xb2,
        OP_NOP4 = 0xb3,
        OP_NOP5 = 0xb4,
        OP_NOP6 = 0xb5,
        OP_NOP7 = 0xb6,
        OP_NOP8 = 0xb7,
        OP_NOP9 = 0xb8,
        OP_NOP10 = 0xb9,

        // template matching params
        OP_SMALLDATA = 0xf9,
        OP_SMALLINTEGER = 0xfa,
        OP_PUBKEYS = 0xfb,
        OP_PUBKEYHASH = 0xfd,
        OP_PUBKEY = 0xfe,

        OP_INVALIDOPCODE = 0xff,
    };

    /// <summary>
    /// Transaction output types.
    /// </summary>
    public enum txnouttype
    {
        TX_NONSTANDARD,

        // 'standard' transaction types:
        TX_PUBKEY,
        TX_PUBKEYHASH,
        TX_SCRIPTHASH,
        TX_MULTISIG,
        TX_NULL_DATA,
    };

    /// <summary>
    /// Signature hash types/flags
    /// </summary>
    public enum sigflag
    {
        SIGHASH_ALL = 1,
        SIGHASH_NONE = 2,
        SIGHASH_SINGLE = 3,
        SIGHASH_ANYONECANPAY = 0x80,
    };

    /** Script verification flags */
    public enum scriptflag
    {
        SCRIPT_VERIFY_NONE = 0,
        SCRIPT_VERIFY_P2SH = (1 << 0), // evaluate P2SH (BIP16) subscripts
        SCRIPT_VERIFY_STRICTENC = (1 << 1), // enforce strict conformance to DER and SEC2 for signatures and pubkeys
        SCRIPT_VERIFY_LOW_S = (1 << 2), // enforce low S values in signatures (depends on STRICTENC)
        SCRIPT_VERIFY_NOCACHE = (1 << 3), // do not store results in signature cache (but do query it)
        SCRIPT_VERIFY_NULLDUMMY = (1 << 4), // verify dummy stack item consumed by CHECKMULTISIG is of zero-length
    };

    public static class ScriptCode
    {
        public static string GetTxnOutputType(txnouttype t)
        {
            switch (t)
            {
                case txnouttype.TX_NONSTANDARD: return "nonstandard";
                case txnouttype.TX_PUBKEY: return "pubkey";
                case txnouttype.TX_PUBKEYHASH: return "pubkeyhash";
                case txnouttype.TX_SCRIPTHASH: return "scripthash";
                case txnouttype.TX_MULTISIG: return "multisig";
                case txnouttype.TX_NULL_DATA: return "nulldata";
            }
            return string.Empty;
        }

        /// <summary>
        /// Get the name of supplied opcode
        /// </summary>
        /// <param name="opcode">Opcode</param>
        /// <returns>Opcode name</returns>
        public static string GetOpName(instruction opcode)
        {
            if (opcode == instruction.OP_0) // OP_0 and OP_FALSE are synonyms
                return "OP_0";
            if (opcode == instruction.OP_1) // OP_1 and OP_TRUE are synonyms
                return "OP_1";

            return Enum.GetName(typeof(instruction), opcode);
        }

        /// <summary>
        /// Get next opcode from passed list of bytes and extract push arguments if there are some.
        /// </summary>
        /// <param name="codeBytes">ByteQueue reference.</param>
        /// <param name="opcodeRet">Found opcode.</param>
        /// <param name="bytesRet">IEnumerable out param which is used to get the push arguments.</param>
        /// <returns>Result of operation</returns>
        public static bool GetOp(ref ByteQueue codeBytes, out instruction opcodeRet, out byte[] bytesRet)
        {
            bytesRet = new byte[0];
            opcodeRet = instruction.OP_INVALIDOPCODE;

            instruction opcode;

            try
            {
                // Read instruction
                opcode = (instruction)codeBytes.Get();
            }
            catch (ByteQueueException)
            {
                // No instruction found there
                return false;
            }

            // Immediate operand
            if (opcode <= instruction.OP_PUSHDATA4)
            {
                byte[] szBytes = new byte[4] { 0, 0, 0, 0 }; // Zero length

                try
                {
                    if (opcode < instruction.OP_PUSHDATA1)
                    {
                        // Zero value opcodes (OP_0, OP_FALSE)
                        szBytes[3] = (byte)opcode;
                    }
                    else if (opcode == instruction.OP_PUSHDATA1)
                    {
                        // The next byte contains the number of bytes to be pushed onto the stack, 
                        //    i.e. you have something like OP_PUSHDATA1 0x01 [0x5a]
                        szBytes[3] = codeBytes.Get();
                    }
                    else if (opcode == instruction.OP_PUSHDATA2)
                    {
                        // The next two bytes contain the number of bytes to be pushed onto the stack,
                        //    i.e. now your operation will seem like this: OP_PUSHDATA2 0x00 0x01 [0x5a]
                        codeBytes.Get(2).CopyTo(szBytes, 2);
                    }
                    else if (opcode == instruction.OP_PUSHDATA4)
                    {
                        // The next four bytes contain the number of bytes to be pushed onto the stack,
                        //   OP_PUSHDATA4 0x00 0x00 0x00 0x01 [0x5a]
                        szBytes = codeBytes.Get(4);
                    }
                }
                catch (ByteQueueException)
                {
                    // Unable to read operand length
                    return false;
                }

                int nSize = (int)Interop.BEBytesToUInt32(szBytes);

                if (nSize > 0)
                {
                    // If nSize is greater than zero then there is some data available
                    try
                    {
                        // Read found number of bytes into list of OP_PUSHDATAn arguments.
                        bytesRet = codeBytes.Get(nSize);
                    }
                    catch (ByteQueueException)
                    {
                        // Unable to read data
                        return false;
                    }
                }
            }

            opcodeRet = opcode;

            return true;
        }

        /// <summary>
        /// Convert value bytes into readable representation.
        /// 
        /// If list lengh is equal or lesser than 4 bytes then bytes are interpreted as integer value. Otherwise you will get hex representation of supplied data.
        /// </summary>
        /// <param name="bytes">Collection of value bytes.</param>
        /// <returns>Formatted value.</returns>
        public static string ValueString(byte[] bytes)
        {
            var sb = new StringBuilder();

            if (bytes.Length <= 4)
            {
                sb.Append(Interop.BEBytesToUInt32(bytes));
            }
            else
            {
                return Interop.ToHex(bytes);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert list of stack items into human readable representation.
        /// </summary>
        /// <param name="stackList">List of stack items.</param>
        /// <returns>Formatted value.</returns>
        public static string StackString(IList<byte[]> stackList)
        {
            var sb = new StringBuilder();
            foreach (var bytes in stackList)
            {
                sb.Append(ValueString(bytes));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decode instruction to integer value
        /// </summary>
        /// <param name="opcode">Small integer opcode (OP_1_NEGATE and OP_0 - OP_16)</param>
        /// <returns>Small integer</returns>
        public static int DecodeOP_N(instruction opcode, bool AllowNegate = false)
        {
            if (AllowNegate && opcode == instruction.OP_1NEGATE)
            {
                return -1;
            }

            if (opcode == instruction.OP_0)
            {
                return 0;
            }

            // Only OP_n opcodes are supported, throw exception otherwise.
            if (opcode < instruction.OP_1 || opcode > instruction.OP_16)
            {
                throw new ArgumentException("Invalid integer instruction.");
            }

            return (int)opcode - (int)(instruction.OP_1 - 1);
        }

        /// <summary>
        /// Converts integer into instruction
        /// </summary>
        /// <param name="n">Small integer from the range of -1 up to 16.</param>
        /// <returns>Corresponding opcode.</returns>
        public static instruction EncodeOP_N(int n, bool allowNegate = false)
        {
            if (allowNegate && n == -1)
            {
                return instruction.OP_1NEGATE;
            }

            if (n == 0)
            {
                return instruction.OP_0;
            }

            // The n value must be in the range of 0 to 16.
            if (n < 0 || n > 16)
                throw new ArgumentException("Invalid integer value.");
            return (instruction.OP_1 + n - 1);
        }

        public static int ScriptSigArgsExpected(txnouttype t, IList<byte[]> solutions)
        {
            switch (t)
            {
                case txnouttype.TX_NONSTANDARD:
                    return -1;
                case txnouttype.TX_NULL_DATA:
                    return 1;
                case txnouttype.TX_PUBKEY:
                    return 1;
                case txnouttype.TX_PUBKEYHASH:
                    return 2;
                case txnouttype.TX_MULTISIG:
                    if (solutions.Count < 1 || solutions.First().Length < 1)
                        return -1;
                    return solutions.First()[0] + 1;
                case txnouttype.TX_SCRIPTHASH:
                    return 1; // doesn't include args needed by the script
            }
            return -1;
        }

        /// <summary>
        /// Is it a standart type of scriptPubKey?
        /// </summary>
        /// <param name="scriptPubKey">CScript instance</param>
        /// <param name="whichType">utut type</param>
        /// <returns>Checking result</returns>
        public static bool IsStandard(CScript scriptPubKey, out txnouttype whichType)
        {
            IList<byte[]> solutions;

            if (!Solver(scriptPubKey, out whichType, out solutions))
            {
                // No solutions found
                return false;
            }

            if (whichType == txnouttype.TX_MULTISIG)
            {
                // Additional verification of OP_CHECKMULTISIG arguments
                var m = solutions.First()[0];
                var n = solutions.Last()[0];

                // Support up to x-of-3 multisig txns as standard
                if (n < 1 || n > 3)
                {
                    return false;
                }
                if (m < 1 || m > n)
                {
                    return false;
                }
            }

            return whichType != txnouttype.TX_NONSTANDARD;
        }

        /// <summary>
        /// Return public keys or hashes from scriptPubKey, for 'standard' transaction types.
        /// </summary>
        /// <param name="scriptPubKey">CScript instance</param>
        /// <param name="typeRet">Output type</param>
        /// <param name="solutions">Set of solutions</param>
        /// <returns>Result</returns>
        public static bool Solver(CScript scriptPubKey, out txnouttype typeRet, out IList<byte[]> solutions)
        {
            solutions = new List<byte[]>();

            // There are shortcuts for pay-to-script-hash and pay-to-pubkey-hash, which are more constrained than the other types.

            // It is always OP_HASH160 20 [20 byte hash] OP_EQUAL
            if (scriptPubKey.IsPayToScriptHash)
            {
                typeRet = txnouttype.TX_SCRIPTHASH;

                // Take 20 bytes with offset of 2 bytes
                var hashBytes = scriptPubKey.Bytes.Skip(2).Take(20);
                solutions.Add(hashBytes.ToArray());

                return true;
            }

            // It is always OP_DUP OP_HASH160 20 [20 byte hash] OP_EQUALVERIFY OP_CHECKSIG
            if (scriptPubKey.IsPayToPubKeyHash)
            {
                typeRet = txnouttype.TX_PUBKEYHASH;

                // Take 20 bytes with offset of 3 bytes
                var hashBytes = scriptPubKey.Bytes.Skip(3).Take(20);
                solutions.Add(hashBytes.ToArray());

                return true;
            }

            var templateTuples = new List<Tuple<txnouttype, byte[]>>();

            // Sender provides pubkey, receiver adds signature
            // [ECDSA public key] OP_CHECKSIG
            templateTuples.Add(
                new Tuple<txnouttype, byte[]>(
                    txnouttype.TX_PUBKEY,
                    new byte[] {
                        (byte)instruction.OP_PUBKEY,
                        (byte)instruction.OP_CHECKSIG
                    })
            );

            // Sender provides N pubkeys, receivers provides M signatures
            // N [pubkey1] [pubkey2] ... [pubkeyN] M OP_CHECKMULTISIG
            // Where N and M are small integer opcodes (OP1 ... OP_16)
            templateTuples.Add(
                new Tuple<txnouttype, byte[]>(
                    txnouttype.TX_MULTISIG,
                    new byte[] {
                        (byte)instruction.OP_SMALLINTEGER,
                        (byte)instruction.OP_PUBKEYS,
                        (byte)instruction.OP_SMALLINTEGER,
                        (byte)instruction.OP_CHECKMULTISIG
                    })
            );

            // Data-carrying output
            // OP_RETURN [up to 80 bytes of data]
            templateTuples.Add(
                new Tuple<txnouttype, byte[]>(
                    txnouttype.TX_NULL_DATA,
                    new byte[] {
                        (byte)instruction.OP_RETURN,
                        (byte)instruction.OP_SMALLDATA
                    })
            );

            // Nonstandard tx output
            typeRet = txnouttype.TX_NONSTANDARD;

            foreach (var templateTuple in templateTuples)
            {
                var script1 = scriptPubKey;
                var script2 = new CScript(templateTuple.Item2);

                instruction opcode1, opcode2;

                // Compare
                var bq1 = script1.GetByteQUeue();
                var bq2 = script2.GetByteQUeue();

                byte[] args1, args2;

                int last1 = script1.Bytes.Count() -1;
                int last2 = script2.Bytes.Count() - 1;

                while (true)
                {
                    if (bq1.CurrentIndex == last1 && bq2.CurrentIndex == last2)
                    {
                        // Found a match
                        typeRet = templateTuple.Item1;
                        if (typeRet == txnouttype.TX_MULTISIG)
                        {
                            // Additional checks for TX_MULTISIG:
                            var m = solutions.First().First();
                            var n = solutions.Last().First();

                            if (m < 1 || n < 1 || m > n || solutions.Count - 2 != n)
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                    if (!GetOp(ref bq1, out opcode1, out args1))
                    {
                        break;
                    }
                    if (!GetOp(ref bq2, out opcode2, out args2))
                    {
                        break;
                    }

                    // Template matching opcodes:
                    if (opcode2 == instruction.OP_PUBKEYS)
                    {
                        while (args1.Count() >= 33 && args1.Count() <= 120)
                        {
                            solutions.Add(args1);
                            if (!GetOp(ref bq1, out opcode1, out args1))
                            {
                                break;
                            }
                        }
                        if (!GetOp(ref bq2, out opcode2, out args2))
                        {
                            break;
                        }
                        // Normal situation is to fall through
                        // to other if/else statements
                    }
                    if (opcode2 == instruction.OP_PUBKEY)
                    {
                        int PubKeyLen = args1.Count();
                        if (PubKeyLen < 33 || PubKeyLen > 120)
                        {
                            break;
                        }
                        solutions.Add(args1);
                    }
                    else if (opcode2 == instruction.OP_PUBKEYHASH)
                    {
                        if (args1.Count() != 20) // hash160 size
                        {
                            break;
                        }
                        solutions.Add(args1);
                    }
                    else if (opcode2 == instruction.OP_SMALLINTEGER)
                    {
                        // Single-byte small integer pushed onto solutions
                        try
                        {
                            var n = (byte)DecodeOP_N(opcode1);
                            solutions.Add(new byte[] { n });
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    else if (opcode2 == instruction.OP_SMALLDATA)
                    {
                        // small pushdata, <= 80 bytes
                        if (args1.Length > 80)
                        {
                            break;
                        }
                    }
                    else if (opcode1 != opcode2 || !args1.SequenceEqual(args2))
                    {
                        // Others must match exactly
                        break;
                    }
                }
            }

            solutions.Clear();
            typeRet = txnouttype.TX_NONSTANDARD;

            return false;
        }

        /// <summary>
        /// Generation of SignatureHash. This method is responsible for removal of transaction metadata. It's necessary signature can't sign itself. 
        /// </summary>
        /// <param name="script">Spending instructions</param>
        /// <param name="txTo">Instance of transaction</param>
        /// <param name="nIn">Input number</param>
        /// <param name="nHashType">Hash type flag</param>
        /// <returns></returns>
        public static Hash256 SignatureHash(CScript script, CTransaction txTo, int nIn, int nHashType)
        {
            if (nIn >= txTo.vin.Length)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("ERROR: SignatureHash() : nIn={0} out of range\n", nIn);
                throw new ArgumentOutOfRangeException("nIn", sb.ToString());
            }

            // Init a copy of transaction
            var txTmp = new CTransaction(txTo);

            // In case concatenating two scripts ends up with two codeseparators,
            // or an extra one at the end, this prevents all those possible incompatibilities.
            script.RemovePattern(new byte[] { (byte)instruction.OP_CODESEPARATOR });

            // Blank out other inputs' signatures
            for (int i = 0; i < txTmp.vin.Length; i++)
            {
                txTmp.vin[i].scriptSig = new CScript();
            }
            txTmp.vin[nIn].scriptSig = script;

            // Blank out some of the outputs
            if ((nHashType & 0x1f) == (int)sigflag.SIGHASH_NONE)
            {
                // Wildcard payee
                txTmp.vout = new CTxOut[0];

                // Let the others update at will
                for (int i = 0; i < txTmp.vin.Length; i++)
                {
                    if (i != nIn)
                    {
                        txTmp.vin[i].nSequence = 0;
                    }
                }
            }
            else if ((nHashType & 0x1f) == (int)sigflag.SIGHASH_SINGLE)
            {
                // Only lock-in the txout payee at same index as txin
                int nOut = nIn;
                if (nOut >= txTmp.vout.Length)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("ERROR: SignatureHash() : nOut={0} out of range\n", nOut);
                    throw new ArgumentOutOfRangeException("nOut", sb.ToString());
                }
                Array.Resize(ref txTmp.vout, nOut + 1);

                for (int i = 0; i < nOut; i++)
                {
                    txTmp.vout[i] = new CTxOut();
                }

                // Let the others update at will
                for (int i = 0; i < txTmp.vin.Length; i++)
                {
                    if (i != nIn)
                    {
                        txTmp.vin[i].nSequence = 0;
                    }
                }
            }

            // Blank out other inputs completely, not recommended for open transactions
            if ((nHashType & (int)sigflag.SIGHASH_ANYONECANPAY) != 0)
            {
                txTmp.vin[0] = txTmp.vin[nIn];
                Array.Resize(ref txTmp.vin, 1);
            }

            // Serialize and hash
            var b = new List<byte>();
            b.AddRange(txTmp.Bytes);
            b.AddRange(BitConverter.GetBytes(nHashType));

            return Hash256.Compute256(b);
        }

        //
        // Script is a stack machine (like Forth) that evaluates a predicate
        // returning a bool indicating valid or not.  There are no loops.
        //

        /// <summary>
        /// Script machine exception
        /// </summary>
        public class StackMachineException : Exception
        {
            public StackMachineException()
            {
            }

            public StackMachineException(string message)
                : base(message)
            {
            }

            public StackMachineException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        /// <summary>
        /// Remove last element from stack
        /// </summary>
        /// <param name="stack">Stack reference</param>
        private static void popstack(ref List<byte[]> stack)
        {
            int nCount = stack.Count;
            if (nCount == 0)
                throw new StackMachineException("popstack() : stack empty");
            stack.RemoveAt(nCount - 1);
        }

        /// <summary>
        /// Get element at specified stack depth
        /// </summary>
        /// <param name="stack">Stack reference</param>
        /// <param name="nDepth">Depth</param>
        /// <returns>Byte sequence</returns>
        private static byte[] stacktop(ref List<byte[]> stack, int nDepth)
        {
            int nStackElement = stack.Count + nDepth;

            if (nDepth >= 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("stacktop() : positive depth ({0}) has no sense.", nDepth);

                throw new StackMachineException(sb.ToString());
            }

            if (nStackElement < 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("stacktop() : nDepth={0} exceeds real stack depth ({1})", nDepth, stack.Count);

                throw new StackMachineException(sb.ToString());
            }

            return stack[nStackElement];
        }

        /// <summary>
        /// Cast argument to boolean value
        /// </summary>
        /// <param name="value">Some byte sequence</param>
        /// <returns></returns>
        private static bool CastToBool(byte[] arg)
        {
            for (var i = 0; i < arg.Length; i++)
            {
                if (arg[i] != 0)
                {
                    // Can be negative zero
                    if (i == arg.Length - 1 && arg[i] == 0x80)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Cast argument to integer value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static BigInteger CastToBigInteger(byte[] value)
        {
            if (value.Length > 4)
            {
                throw new StackMachineException("CastToBigInteger() : overflow");
            }

            return new BigInteger(value);
        }

        /// <summary>
        /// Execution of script
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="script">Script to execute</param>
        /// <param name="txTo">Transaction instance</param>
        /// <param name="nIn">Input number</param>
        /// <param name="flags">Signature checking flags</param>
        /// <param name="nHashType">Hash type flag</param>
        /// <returns></returns>
        public static bool EvalScript(ref List<byte[]> stack, CScript script, CTransaction txTo, int nIn, int flags, int nHashType)
        {
            if (script.Bytes.Count() > 10000)
            {
                return false; // Size limit failed
            }

            var vfExec = new List<bool>();

            int nOpCount = 0;
            int nCodeHashBegin = 0;

            var falseBytes = new byte[0];
            var trueBytes = new byte[] { 0x01 };

            var CodeQueue = script.GetByteQUeue();
            var altStack = new List<byte[]>();

            try
            {
                instruction opcode;
                byte[] pushArg;

                while (GetOp(ref CodeQueue, out opcode, out pushArg)) // Read instructions
                {
                    bool fExec = vfExec.IndexOf(false) == -1;

                    if (pushArg.Length > 520)
                    {
                        return false; // Script element size limit failed
                    }

                    if (opcode > instruction.OP_16 && ++nOpCount > 201)
                    {
                        return false;
                    }

                    if (fExec && 0 <= opcode && opcode <= instruction.OP_PUSHDATA4)
                    {
                        stack.Add(pushArg); // Push argument to stack
                    }
                    else if (fExec || (instruction.OP_IF <= opcode && opcode <= instruction.OP_ENDIF))
                        switch (opcode)
                        {
                            //
                            // Disabled opcodes
                            //
                            case instruction.OP_CAT:
                            case instruction.OP_SUBSTR:
                            case instruction.OP_LEFT:
                            case instruction.OP_RIGHT:
                            case instruction.OP_INVERT:
                            case instruction.OP_AND:
                            case instruction.OP_OR:
                            case instruction.OP_XOR:
                            case instruction.OP_2MUL:
                            case instruction.OP_2DIV:
                            case instruction.OP_MUL:
                            case instruction.OP_DIV:
                            case instruction.OP_MOD:
                            case instruction.OP_LSHIFT:
                            case instruction.OP_RSHIFT:
                                return false;

                            //
                            // Push integer instructions
                            //
                            case instruction.OP_1NEGATE:
                            case instruction.OP_1:
                            case instruction.OP_2:
                            case instruction.OP_3:
                            case instruction.OP_4:
                            case instruction.OP_5:
                            case instruction.OP_6:
                            case instruction.OP_7:
                            case instruction.OP_8:
                            case instruction.OP_9:
                            case instruction.OP_10:
                            case instruction.OP_11:
                            case instruction.OP_12:
                            case instruction.OP_13:
                            case instruction.OP_14:
                            case instruction.OP_15:
                            case instruction.OP_16:
                                {
                                    // ( -- value)
                                    BigInteger bn = DecodeOP_N(opcode, true);
                                    stack.Add(bn.ToByteArray());
                                }
                                break;

                            //
                            // Extension
                            //
                            case instruction.OP_NOP:
                            case instruction.OP_NOP1:
                            case instruction.OP_NOP2:
                            case instruction.OP_NOP3:
                            case instruction.OP_NOP4:
                            case instruction.OP_NOP5:
                            case instruction.OP_NOP6:
                            case instruction.OP_NOP7:
                            case instruction.OP_NOP8:
                            case instruction.OP_NOP9:
                            case instruction.OP_NOP10:
                                {
                                    // Just do nothing
                                }
                                break;

                            //
                            // Control
                            //
                            case instruction.OP_IF:
                            case instruction.OP_NOTIF:
                                {
                                    // <expression> if [statements] [else [statements]] endif
                                    var fValue = false;
                                    if (fExec)
                                    {
                                        if (stack.Count() < 1)
                                        {
                                            return false;
                                        }
                                        var vch = stacktop(ref stack, -1);
                                        fValue = CastToBool(vch);
                                        if (opcode == instruction.OP_NOTIF)
                                        {
                                            fValue = !fValue;
                                        }
                                        popstack(ref stack);
                                    }
                                    vfExec.Add(fValue);
                                }
                                break;

                            case instruction.OP_ELSE:
                                {
                                    int nExecCount = vfExec.Count();
                                    if (nExecCount == 0)
                                    {
                                        return false;
                                    }
                                    vfExec[nExecCount - 1] = !vfExec[nExecCount - 1];
                                }
                                break;

                            case instruction.OP_ENDIF:
                                {
                                    int nExecCount = vfExec.Count();
                                    if (nExecCount == 0)
                                    {
                                        return false;
                                    }
                                    vfExec.RemoveAt(nExecCount - 1);
                                }
                                break;

                            case instruction.OP_VERIFY:
                                {
                                    // (true -- ) or
                                    // (false -- false) and return
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    bool fValue = CastToBool(stacktop(ref stack, -1));
                                    if (fValue)
                                    {
                                        popstack(ref stack);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                break;

                            case instruction.OP_RETURN:
                                {
                                    return false;
                                }

                            //
                            // Stack ops
                            //
                            case instruction.OP_TOALTSTACK:
                                {
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }
                                    altStack.Add(stacktop(ref stack, -1));
                                    popstack(ref stack);
                                }
                                break;

                            case instruction.OP_FROMALTSTACK:
                                {
                                    if (altStack.Count() < 1)
                                    {
                                        return false;
                                    }
                                    stack.Add(stacktop(ref stack, -1));
                                    popstack(ref altStack);
                                }
                                break;

                            case instruction.OP_2DROP:
                                {
                                    // (x1 x2 -- )
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }
                                    popstack(ref stack);
                                    popstack(ref stack);
                                }
                                break;

                            case instruction.OP_2DUP:
                                {
                                    // (x1 x2 -- x1 x2 x1 x2)
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }
                                    var vch1 = stacktop(ref stack, -2);
                                    var vch2 = stacktop(ref stack, -1);
                                    stack.Add(vch1);
                                    stack.Add(vch2);
                                }
                                break;

                            case instruction.OP_3DUP:
                                {
                                    // (x1 x2 x3 -- x1 x2 x3 x1 x2 x3)
                                    if (stack.Count() < 3)
                                    {
                                        return false;
                                    }
                                    var vch1 = stacktop(ref stack, -3);
                                    var vch2 = stacktop(ref stack, -2);
                                    var vch3 = stacktop(ref stack, -1);
                                    stack.Add(vch1);
                                    stack.Add(vch2);
                                    stack.Add(vch3);
                                }
                                break;

                            case instruction.OP_2OVER:
                                {
                                    // (x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2)
                                    if (stack.Count() < 4)
                                    {
                                        return false;
                                    }
                                    var vch1 = stacktop(ref stack, -4);
                                    var vch2 = stacktop(ref stack, -3);
                                    stack.Add(vch1);
                                    stack.Add(vch2);
                                }
                                break;

                            case instruction.OP_2ROT:
                                {
                                    int nStackDepth = stack.Count();
                                    // (x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2)
                                    if (nStackDepth < 6)
                                    {
                                        return false;
                                    }
                                    var vch1 = stacktop(ref stack, -6);
                                    var vch2 = stacktop(ref stack, -5);
                                    stack.RemoveRange(nStackDepth - 6, 2);
                                    stack.Add(vch1);
                                    stack.Add(vch2);
                                }
                                break;

                            case instruction.OP_2SWAP:
                                {
                                    // (x1 x2 x3 x4 -- x3 x4 x1 x2)
                                    int nStackDepth = stack.Count;
                                    if (nStackDepth < 4)
                                    {
                                        return false;
                                    }
                                    stack.Swap(nStackDepth - 4, nStackDepth - 2);
                                    stack.Swap(nStackDepth - 3, nStackDepth - 1);
                                }
                                break;

                            case instruction.OP_IFDUP:
                                {
                                    // (x - 0 | x x)
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    var vch = stacktop(ref stack, -1);

                                    if (CastToBool(vch))
                                    {
                                        stack.Add(vch);
                                    }
                                }
                                break;

                            case instruction.OP_DEPTH:
                                {
                                    // -- stacksize
                                    BigInteger bn = new BigInteger((ushort)stack.Count());
                                    stack.Add(bn.ToByteArray());
                                }
                                break;

                            case instruction.OP_DROP:
                                {
                                    // (x -- )
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    popstack(ref stack);
                                }
                                break;

                            case instruction.OP_DUP:
                                {
                                    // (x -- x x)
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    var vch = stacktop(ref stack, -1);
                                    stack.Add(vch);
                                }
                                break;

                            case instruction.OP_NIP:
                                {
                                    // (x1 x2 -- x2)
                                    int nStackDepth = stack.Count();
                                    if (nStackDepth < 2)
                                    {
                                        return false;
                                    }

                                    stack.RemoveAt(nStackDepth - 2);
                                }
                                break;

                            case instruction.OP_OVER:
                                {
                                    // (x1 x2 -- x1 x2 x1)
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }

                                    var vch = stacktop(ref stack, -2);
                                    stack.Add(vch);
                                }
                                break;

                            case instruction.OP_PICK:
                            case instruction.OP_ROLL:
                                {
                                    // (xn ... x2 x1 x0 n - xn ... x2 x1 x0 xn)
                                    // (xn ... x2 x1 x0 n - ... x2 x1 x0 xn)

                                    int nStackDepth = stack.Count();
                                    if (nStackDepth < 2)
                                    {
                                        return false;
                                    }

                                    int n = (int)CastToBigInteger(stacktop(ref stack, -1));
                                    popstack(ref stack);

                                    if (n < 0 || n >= stack.Count())
                                    {
                                        return false;
                                    }

                                    var vch = stacktop(ref stack, -n - 1);
                                    if (opcode == instruction.OP_ROLL)
                                    {
                                        stack.RemoveAt(nStackDepth - n - 1);
                                    }

                                    stack.Add(vch);
                                }
                                break;

                            case instruction.OP_ROT:
                                {
                                    // (x1 x2 x3 -- x2 x3 x1)
                                    //  x2 x1 x3  after first swap
                                    //  x2 x3 x1  after second swap
                                    int nStackDepth = stack.Count();
                                    if (nStackDepth < 3)
                                    {
                                        return false;
                                    }
                                    stack.Swap(nStackDepth - 3, nStackDepth - 2);
                                    stack.Swap(nStackDepth - 2, nStackDepth - 1);

                                }
                                break;

                            case instruction.OP_SWAP:
                                {
                                    // (x1 x2 -- x2 x1)
                                    int nStackDepth = stack.Count();
                                    if (nStackDepth < 2)
                                    {
                                        return false;
                                    }
                                    stack.Swap(nStackDepth - 2, nStackDepth - 1);
                                }
                                break;

                            case instruction.OP_TUCK:
                                {
                                    // (x1 x2 -- x2 x1 x2)
                                    int nStackDepth = stack.Count();
                                    if (nStackDepth < 2)
                                    {
                                        return false;
                                    }
                                    var vch = stacktop(ref stack, -1);
                                    stack.Insert(nStackDepth - 2, vch);
                                }
                                break;


                            case instruction.OP_SIZE:
                                {
                                    // (in -- in size)
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    var bnSize = new BigInteger((ushort)stacktop(ref stack, -1).Count());
                                    stack.Add(bnSize.ToByteArray());
                                }
                                break;


                            //
                            // Bitwise logic
                            //
                            case instruction.OP_EQUAL:
                            case instruction.OP_EQUALVERIFY:
                                //case instruction.OP_NOTEQUAL: // use OP_NUMNOTEQUAL
                                {
                                    // (x1 x2 - bool)
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }

                                    var vch1 = stacktop(ref stack, -2);
                                    var vch2 = stacktop(ref stack, -1);
                                    bool fEqual = (vch1.SequenceEqual(vch2));
                                    // OP_NOTEQUAL is disabled because it would be too easy to say
                                    // something like n != 1 and have some wiseguy pass in 1 with extra
                                    // zero bytes after it (numerically, 0x01 == 0x0001 == 0x000001)
                                    //if (opcode == instruction.OP_NOTEQUAL)
                                    //    fEqual = !fEqual;
                                    popstack(ref stack);
                                    popstack(ref stack);
                                    stack.Add(fEqual ? trueBytes : falseBytes);

                                    if (opcode == instruction.OP_EQUALVERIFY)
                                    {
                                        if (fEqual)
                                        {
                                            popstack(ref stack);
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;


                            //
                            // Numeric
                            //
                            case instruction.OP_1ADD:
                            case instruction.OP_1SUB:
                            case instruction.OP_NEGATE:
                            case instruction.OP_ABS:
                            case instruction.OP_NOT:
                            case instruction.OP_0NOTEQUAL:
                                {
                                    // (in -- out)
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }

                                    var bn = CastToBigInteger(stacktop(ref stack, -1));
                                    switch (opcode)
                                    {
                                        case instruction.OP_1ADD:
                                            bn = bn + 1;
                                            break;
                                        case instruction.OP_1SUB:
                                            bn = bn - 1;
                                            break;
                                        case instruction.OP_NEGATE:
                                            bn = -bn;
                                            break;
                                        case instruction.OP_ABS:
                                            bn = BigInteger.Abs(bn);
                                            break;
                                        case instruction.OP_NOT:
                                            bn = bn == 0 ? 1 : 0;
                                            break;
                                        case instruction.OP_0NOTEQUAL:
                                            bn = bn != 0 ? 1 : 0;
                                            break;
                                    }

                                    popstack(ref stack);
                                    stack.Add(bn.ToByteArray());
                                }
                                break;

                            case instruction.OP_ADD:
                            case instruction.OP_SUB:
                            case instruction.OP_BOOLAND:
                            case instruction.OP_BOOLOR:
                            case instruction.OP_NUMEQUAL:
                            case instruction.OP_NUMEQUALVERIFY:
                            case instruction.OP_NUMNOTEQUAL:
                            case instruction.OP_LESSTHAN:
                            case instruction.OP_GREATERTHAN:
                            case instruction.OP_LESSTHANOREQUAL:
                            case instruction.OP_GREATERTHANOREQUAL:
                            case instruction.OP_MIN:
                            case instruction.OP_MAX:
                                {
                                    // (x1 x2 -- out)
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }

                                    var bn1 = CastToBigInteger(stacktop(ref stack, -2));
                                    var bn2 = CastToBigInteger(stacktop(ref stack, -1));
                                    BigInteger bn = 0;

                                    switch (opcode)
                                    {
                                        case instruction.OP_ADD:
                                            bn = bn1 + bn2;
                                            break;
                                        case instruction.OP_SUB:
                                            bn = bn1 - bn2;
                                            break;
                                        case instruction.OP_BOOLAND:
                                            bn = (bn1 != 0 && bn2 != 0) ? 1 : 0;
                                            break;
                                        case instruction.OP_BOOLOR:
                                            bn = (bn1 != 0 || bn2 != 0) ? 1 : 0;
                                            break;
                                        case instruction.OP_NUMEQUAL:
                                            bn = (bn1 == bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_NUMEQUALVERIFY:
                                            bn = (bn1 == bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_NUMNOTEQUAL:
                                            bn = (bn1 != bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_LESSTHAN:
                                            bn = (bn1 < bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_GREATERTHAN:
                                            bn = (bn1 > bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_LESSTHANOREQUAL:
                                            bn = (bn1 <= bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_GREATERTHANOREQUAL:
                                            bn = (bn1 >= bn2) ? 1 : 0;
                                            break;
                                        case instruction.OP_MIN:
                                            bn = (bn1 < bn2 ? bn1 : bn2);
                                            break;
                                        case instruction.OP_MAX:
                                            bn = (bn1 > bn2 ? bn1 : bn2);
                                            break;
                                    }

                                    popstack(ref stack);
                                    popstack(ref stack);
                                    stack.Add(bn.ToByteArray());

                                    if (opcode == instruction.OP_NUMEQUALVERIFY)
                                    {
                                        if (CastToBool(stacktop(ref stack, -1)))
                                        {
                                            popstack(ref stack);
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case instruction.OP_WITHIN:
                                {
                                    // (x min max -- out)
                                    if (stack.Count() < 3)
                                    {
                                        return false;
                                    }

                                    var bn1 = CastToBigInteger(stacktop(ref stack, -3));
                                    var bn2 = CastToBigInteger(stacktop(ref stack, -2));
                                    var bn3 = CastToBigInteger(stacktop(ref stack, -1));

                                    bool fValue = (bn2 <= bn1 && bn1 < bn3);

                                    popstack(ref stack);
                                    popstack(ref stack);
                                    popstack(ref stack);

                                    stack.Add(fValue ? trueBytes : falseBytes);
                                }
                                break;

                            //
                            // Crypto
                            //
                            case instruction.OP_RIPEMD160:
                            case instruction.OP_SHA1:
                            case instruction.OP_SHA256:
                            case instruction.OP_HASH160:
                            case instruction.OP_HASH256:
                                {
                                    // (in -- hash)
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }
                                    Hash hash = null;
                                    var data = stacktop(ref stack, -1);

                                    switch (opcode)
                                    {
                                        case instruction.OP_HASH160:
                                            hash = Hash160.Compute160(data);
                                            break;
                                        case instruction.OP_HASH256:
                                            hash = Hash256.Compute256(data);
                                            break;
                                        case instruction.OP_SHA1:
                                            hash = SHA1.Compute1(data);
                                            break;
                                        case instruction.OP_SHA256:
                                            hash = SHA256.Compute256(data);
                                            break;
                                        case instruction.OP_RIPEMD160:
                                            hash = RIPEMD160.Compute160(data);
                                            break;
                                    }
                                    popstack(ref stack);
                                    stack.Add(hash.hashBytes);
                                }
                                break;

                            case instruction.OP_CODESEPARATOR:
                                {
                                    // Hash starts after the code separator
                                    nCodeHashBegin = CodeQueue.CurrentIndex;
                                }
                                break;

                            case instruction.OP_CHECKSIG:
                            case instruction.OP_CHECKSIGVERIFY:
                                {
                                    // (sig pubkey -- bool)
                                    if (stack.Count() < 2)
                                    {
                                        return false;
                                    }

                                    byte[] sigBytes = stacktop(ref stack, -2);
                                    byte[] pubkeyBytes = stacktop(ref stack, -1);

                                    // Subset of script starting at the most recent codeseparator
                                    CScript scriptCode = new CScript(script.Bytes.Skip(nCodeHashBegin));

                                    // There's no way for a signature to sign itself
                                    scriptCode.RemovePattern(sigBytes);

                                    bool fSuccess = IsCanonicalSignature(sigBytes, flags) && IsCanonicalPubKey(pubkeyBytes, flags) && CheckSig(sigBytes, pubkeyBytes, scriptCode, txTo, nIn, nHashType, flags);

                                    popstack(ref stack);
                                    popstack(ref stack);

                                    stack.Add(fSuccess ? trueBytes : falseBytes);

                                    if (opcode == instruction.OP_CHECKSIGVERIFY)
                                    {
                                        if (fSuccess)
                                        {
                                            popstack(ref stack);
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case instruction.OP_CHECKMULTISIG:
                            case instruction.OP_CHECKMULTISIGVERIFY:
                                {
                                    // ([sig ...] num_of_signatures [pubkey ...] num_of_pubkeys -- bool)

                                    int i = 1;
                                    if (stack.Count() < i)
                                    {
                                        return false;
                                    }

                                    int nKeysCount = (int)CastToBigInteger(stacktop(ref stack, -i));
                                    if (nKeysCount < 0 || nKeysCount > 20)
                                    {
                                        return false;
                                    }
                                    nOpCount += nKeysCount;
                                    if (nOpCount > 201)
                                    {
                                        return false;
                                    }
                                    int ikey = ++i;
                                    i += nKeysCount;
                                    if (stack.Count() < i)
                                    {
                                        return false;
                                    }

                                    int nSigsCount = (int)CastToBigInteger(stacktop(ref stack, -i));
                                    if (nSigsCount < 0 || nSigsCount > nKeysCount)
                                    {
                                        return false;
                                    }
                                    int isig = ++i;
                                    i += nSigsCount;
                                    if (stack.Count() < i)
                                    {
                                        return false;
                                    }

                                    // Subset of script starting at the most recent codeseparator
                                    CScript scriptCode = new CScript(script.Bytes.Skip(nCodeHashBegin));

                                    // There is no way for a signature to sign itself, so we need to drop the signatures
                                    for (int k = 0; k < nSigsCount; k++)
                                    {
                                        var vchSig = stacktop(ref stack, -isig - k);
                                        scriptCode.RemovePattern(vchSig);
                                    }

                                    bool fSuccess = true;
                                    while (fSuccess && nSigsCount > 0)
                                    {
                                        var sigBytes = stacktop(ref stack, -isig);
                                        var pubKeyBytes = stacktop(ref stack, -ikey);

                                        // Check signature
                                        bool fOk = IsCanonicalSignature(sigBytes, flags) && IsCanonicalPubKey(pubKeyBytes, flags) && CheckSig(sigBytes, pubKeyBytes, scriptCode, txTo, nIn, nHashType, flags);

                                        if (fOk)
                                        {
                                            isig++;
                                            nSigsCount--;
                                        }
                                        ikey++;
                                        nKeysCount--;

                                        // If there are more signatures left than keys left,
                                        // then too many signatures have failed
                                        if (nSigsCount > nKeysCount)
                                        {
                                            fSuccess = false;
                                        }
                                    }

                                    while (i-- > 1)
                                    {
                                        popstack(ref stack);
                                    }

                                    // A bug causes CHECKMULTISIG to consume one extra argument
                                    // whose contents were not checked in any way.
                                    //
                                    // Unfortunately this is a potential source of mutability,
                                    // so optionally verify it is exactly equal to zero prior
                                    // to removing it from the stack.
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }
                                    if ((flags & (int)scriptflag.SCRIPT_VERIFY_NULLDUMMY) != 0 && stacktop(ref stack, -1).Count() != 0)
                                    {
                                        return false; // CHECKMULTISIG dummy argument not null
                                    }
                                    popstack(ref stack);

                                    stack.Add(fSuccess ? trueBytes : falseBytes);

                                    if (opcode == instruction.OP_CHECKMULTISIGVERIFY)
                                    {
                                        if (fSuccess)
                                        {
                                            popstack(ref stack);
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            default:
                                return false;
                        }

                    // Size limits
                    if (stack.Count() + altStack.Count() > 1000)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // If there are any exceptions then just return false.
                return false;
            }

            if (vfExec.Count() != 0)
            {
                // Something went wrong with conditional instructions.
                return false;
            }

            return true;
        }


        public static bool IsCanonicalPubKey(byte[] pubKeyBytes, int flags)
        {
            if ((flags & (int)scriptflag.SCRIPT_VERIFY_STRICTENC) == 0)
                return true;

            if (pubKeyBytes.Length < 33)
                return false;  // Non-canonical public key: too short
            if (pubKeyBytes[0] == 0x04)
            {
                if (pubKeyBytes.Length != 65)
                    return false; // Non-canonical public key: invalid length for uncompressed key
            }
            else if (pubKeyBytes[0] == 0x02 || pubKeyBytes[0] == 0x03)
            {
                if (pubKeyBytes.Length != 33)
                    return false; // Non-canonical public key: invalid length for compressed key
            }
            else
            {
                return false; // Non-canonical public key: compressed nor uncompressed
            }
            return true;
        }

        public static bool IsCanonicalSignature(byte[] sigBytes, int flags)
        {
            // STUB

            return true;
        }

        /// <summary>
        /// Check signature.
        /// </summary>
        /// <param name="sigBytes">Signature</param>
        /// <param name="pubkeyBytes">Public key</param>
        /// <param name="script">Spending script</param>
        /// <param name="txTo">CTransaction instance</param>
        /// <param name="nIn">Input number</param>
        /// <param name="nHashType">Hashing type flag</param>
        /// <param name="flags">Signature checking flags</param>
        /// <returns>Checking result</returns>
        public static bool CheckSig(byte[] sigBytes, byte[] pubkeyBytes, CScript script, CTransaction txTo, int nIn, int nHashType, int flags)
        {
            CPubKey pubkey;

            try
            {
                // Trying to initialize the public key instance

                pubkey = new CPubKey(pubkeyBytes);
            }
            catch (Exception)
            {
                // Exception occurred while initializing the public key

                return false; 
            }

            if (!pubkey.IsValid)
            {
                return false;
            }

            if (sigBytes.Length == 0)
            {
                return false;
            }

            // Hash type is one byte tacked on to the end of the signature
            if (nHashType == 0)
            {
                nHashType = sigBytes.Last();
            }
            else if (nHashType != sigBytes.Last())
            {
                return false;
            }

            // Remove hash type
            Array.Resize(ref sigBytes, sigBytes.Length - 1);

            var sighash = SignatureHash(script, txTo, nIn, nHashType);

            if (!pubkey.VerifySignature(sighash, sigBytes))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluates the both scriptSig and scriptPubKey.
        /// </summary>
        /// <param name="scriptSig"></param>
        /// <param name="scriptPubKey"></param>
        /// <param name="txTo">Transaction</param>
        /// <param name="nIn">Input number</param>
        /// <param name="flags">Script validation flags</param>
        /// <param name="nHashType">Hash type flag</param>
        /// <returns></returns>
        public static bool VerifyScript(CScript scriptSig, CScript scriptPubKey, CTransaction txTo, int nIn, int flags, int nHashType)
        {
            var stack = new List<byte[]>();
            List<byte[]> stackCopy = null;

            if (!EvalScript(ref stack, scriptSig, txTo, nIn, flags, nHashType))
            {
                return false;
            }

            if ((flags & (int)scriptflag.SCRIPT_VERIFY_P2SH) != 0)
            {
                stackCopy = new List<byte[]>(stack);
            }

            if (!EvalScript(ref stack, scriptPubKey, txTo, nIn, flags, nHashType))
            {
                return false;
            }

            if (stack.Count == 0 || CastToBool(stack.Last()) == false)
            {
                return false;
            }

            // Additional validation for spend-to-script-hash transactions:
            if ((flags & (int)scriptflag.SCRIPT_VERIFY_P2SH) != 0 && scriptPubKey.IsPayToScriptHash)
            {
                if (!scriptSig.IsPushOnly) // scriptSig must be literals-only
                {
                    return false;
                }

                // stackCopy cannot be empty here, because if it was the
                // P2SH  HASH <> EQUAL  scriptPubKey would be evaluated with
                // an empty stack and the EvalScript above would return false.

                if (stackCopy.Count == 0)
                {
                    throw new StackMachineException("Fatal script validation error.");
                }

                var pubKey2 = new CScript(stackCopy.Last());
                popstack(ref stackCopy);

                if (!EvalScript(ref stackCopy, pubKey2, txTo, nIn, flags, nHashType))
                    return false;
                if (stackCopy.Count == 0)
                    return false;

                return CastToBool(stackCopy.Last());
            }

            return true;
        }
    };
}
