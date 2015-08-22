using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Numerics;

// using Org.BouncyCastle.Math;

namespace Novacoin
{
    /// <summary>
    /// Script opcodes
    /// </summary>
    public enum opcodetype
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
        public static string GetOpName(opcodetype opcode)
        {
            switch (opcode)
            {
                // push value
                case opcodetype.OP_0:
                    return "0";
                case opcodetype.OP_PUSHDATA1:
                    return "OP_PUSHDATA1";
                case opcodetype.OP_PUSHDATA2:
                    return "OP_PUSHDATA2";
                case opcodetype.OP_PUSHDATA4:
                    return "OP_PUSHDATA4";
                case opcodetype.OP_1NEGATE:
                    return "-1";
                case opcodetype.OP_RESERVED:
                    return "OP_RESERVED";
                case opcodetype.OP_1:
                    return "1";
                case opcodetype.OP_2:
                    return "2";
                case opcodetype.OP_3:
                    return "3";
                case opcodetype.OP_4:
                    return "4";
                case opcodetype.OP_5:
                    return "5";
                case opcodetype.OP_6:
                    return "6";
                case opcodetype.OP_7:
                    return "7";
                case opcodetype.OP_8:
                    return "8";
                case opcodetype.OP_9:
                    return "9";
                case opcodetype.OP_10:
                    return "10";
                case opcodetype.OP_11:
                    return "11";
                case opcodetype.OP_12:
                    return "12";
                case opcodetype.OP_13:
                    return "13";
                case opcodetype.OP_14:
                    return "14";
                case opcodetype.OP_15:
                    return "15";
                case opcodetype.OP_16:
                    return "16";

                // control
                case opcodetype.OP_NOP:
                    return "OP_NOP";
                case opcodetype.OP_VER:
                    return "OP_VER";
                case opcodetype.OP_IF:
                    return "OP_IF";
                case opcodetype.OP_NOTIF:
                    return "OP_NOTIF";
                case opcodetype.OP_VERIF:
                    return "OP_VERIF";
                case opcodetype.OP_VERNOTIF:
                    return "OP_VERNOTIF";
                case opcodetype.OP_ELSE:
                    return "OP_ELSE";
                case opcodetype.OP_ENDIF:
                    return "OP_ENDIF";
                case opcodetype.OP_VERIFY:
                    return "OP_VERIFY";
                case opcodetype.OP_RETURN:
                    return "OP_RETURN";

                // stack ops
                case opcodetype.OP_TOALTSTACK:
                    return "OP_TOALTSTACK";
                case opcodetype.OP_FROMALTSTACK:
                    return "OP_FROMALTSTACK";
                case opcodetype.OP_2DROP:
                    return "OP_2DROP";
                case opcodetype.OP_2DUP:
                    return "OP_2DUP";
                case opcodetype.OP_3DUP:
                    return "OP_3DUP";
                case opcodetype.OP_2OVER:
                    return "OP_2OVER";
                case opcodetype.OP_2ROT:
                    return "OP_2ROT";
                case opcodetype.OP_2SWAP:
                    return "OP_2SWAP";
                case opcodetype.OP_IFDUP:
                    return "OP_IFDUP";
                case opcodetype.OP_DEPTH:
                    return "OP_DEPTH";
                case opcodetype.OP_DROP:
                    return "OP_DROP";
                case opcodetype.OP_DUP:
                    return "OP_DUP";
                case opcodetype.OP_NIP:
                    return "OP_NIP";
                case opcodetype.OP_OVER:
                    return "OP_OVER";
                case opcodetype.OP_PICK:
                    return "OP_PICK";
                case opcodetype.OP_ROLL:
                    return "OP_ROLL";
                case opcodetype.OP_ROT:
                    return "OP_ROT";
                case opcodetype.OP_SWAP:
                    return "OP_SWAP";
                case opcodetype.OP_TUCK:
                    return "OP_TUCK";

                // splice ops
                case opcodetype.OP_CAT:
                    return "OP_CAT";
                case opcodetype.OP_SUBSTR:
                    return "OP_SUBSTR";
                case opcodetype.OP_LEFT:
                    return "OP_LEFT";
                case opcodetype.OP_RIGHT:
                    return "OP_RIGHT";
                case opcodetype.OP_SIZE:
                    return "OP_SIZE";

                // bit logic
                case opcodetype.OP_INVERT:
                    return "OP_INVERT";
                case opcodetype.OP_AND:
                    return "OP_AND";
                case opcodetype.OP_OR:
                    return "OP_OR";
                case opcodetype.OP_XOR:
                    return "OP_XOR";
                case opcodetype.OP_EQUAL:
                    return "OP_EQUAL";
                case opcodetype.OP_EQUALVERIFY:
                    return "OP_EQUALVERIFY";
                case opcodetype.OP_RESERVED1:
                    return "OP_RESERVED1";
                case opcodetype.OP_RESERVED2:
                    return "OP_RESERVED2";

                // numeric
                case opcodetype.OP_1ADD:
                    return "OP_1ADD";
                case opcodetype.OP_1SUB:
                    return "OP_1SUB";
                case opcodetype.OP_2MUL:
                    return "OP_2MUL";
                case opcodetype.OP_2DIV:
                    return "OP_2DIV";
                case opcodetype.OP_NEGATE:
                    return "OP_NEGATE";
                case opcodetype.OP_ABS:
                    return "OP_ABS";
                case opcodetype.OP_NOT:
                    return "OP_NOT";
                case opcodetype.OP_0NOTEQUAL:
                    return "OP_0NOTEQUAL";
                case opcodetype.OP_ADD:
                    return "OP_ADD";
                case opcodetype.OP_SUB:
                    return "OP_SUB";
                case opcodetype.OP_MUL:
                    return "OP_MUL";
                case opcodetype.OP_DIV:
                    return "OP_DIV";
                case opcodetype.OP_MOD:
                    return "OP_MOD";
                case opcodetype.OP_LSHIFT:
                    return "OP_LSHIFT";
                case opcodetype.OP_RSHIFT:
                    return "OP_RSHIFT";
                case opcodetype.OP_BOOLAND:
                    return "OP_BOOLAND";
                case opcodetype.OP_BOOLOR:
                    return "OP_BOOLOR";
                case opcodetype.OP_NUMEQUAL:
                    return "OP_NUMEQUAL";
                case opcodetype.OP_NUMEQUALVERIFY:
                    return "OP_NUMEQUALVERIFY";
                case opcodetype.OP_NUMNOTEQUAL:
                    return "OP_NUMNOTEQUAL";
                case opcodetype.OP_LESSTHAN:
                    return "OP_LESSTHAN";
                case opcodetype.OP_GREATERTHAN:
                    return "OP_GREATERTHAN";
                case opcodetype.OP_LESSTHANOREQUAL:
                    return "OP_LESSTHANOREQUAL";
                case opcodetype.OP_GREATERTHANOREQUAL:
                    return "OP_GREATERTHANOREQUAL";
                case opcodetype.OP_MIN:
                    return "OP_MIN";
                case opcodetype.OP_MAX:
                    return "OP_MAX";
                case opcodetype.OP_WITHIN:
                    return "OP_WITHIN";

                // crypto
                case opcodetype.OP_RIPEMD160:
                    return "OP_RIPEMD160";
                case opcodetype.OP_SHA1:
                    return "OP_SHA1";
                case opcodetype.OP_SHA256:
                    return "OP_SHA256";
                case opcodetype.OP_HASH160:
                    return "OP_HASH160";
                case opcodetype.OP_HASH256:
                    return "OP_HASH256";
                case opcodetype.OP_CODESEPARATOR:
                    return "OP_CODESEPARATOR";
                case opcodetype.OP_CHECKSIG:
                    return "OP_CHECKSIG";
                case opcodetype.OP_CHECKSIGVERIFY:
                    return "OP_CHECKSIGVERIFY";
                case opcodetype.OP_CHECKMULTISIG:
                    return "OP_CHECKMULTISIG";
                case opcodetype.OP_CHECKMULTISIGVERIFY:
                    return "OP_CHECKMULTISIGVERIFY";

                // expansion
                case opcodetype.OP_NOP1:
                    return "OP_NOP1";
                case opcodetype.OP_NOP2:
                    return "OP_NOP2";
                case opcodetype.OP_NOP3:
                    return "OP_NOP3";
                case opcodetype.OP_NOP4:
                    return "OP_NOP4";
                case opcodetype.OP_NOP5:
                    return "OP_NOP5";
                case opcodetype.OP_NOP6:
                    return "OP_NOP6";
                case opcodetype.OP_NOP7:
                    return "OP_NOP7";
                case opcodetype.OP_NOP8:
                    return "OP_NOP8";
                case opcodetype.OP_NOP9:
                    return "OP_NOP9";
                case opcodetype.OP_NOP10:
                    return "OP_NOP10";

                // template matching params
                case opcodetype.OP_PUBKEYHASH:
                    return "OP_PUBKEYHASH";
                case opcodetype.OP_PUBKEY:
                    return "OP_PUBKEY";
                case opcodetype.OP_SMALLDATA:
                    return "OP_SMALLDATA";

                case opcodetype.OP_INVALIDOPCODE:
                    return "OP_INVALIDOPCODE";
                default:
                    return "OP_UNKNOWN";
            }
        }

        /// <summary>
        /// Get next opcode from passed list of bytes and extract push arguments if there are some.
        /// </summary>
        /// <param name="codeBytes">ByteQueue reference.</param>
        /// <param name="opcodeRet">Found opcode.</param>
        /// <param name="bytesRet">IEnumerable out param which is used to get the push arguments.</param>
        /// <returns>Result of operation</returns>
        public static bool GetOp(ref ByteQueue codeBytes, out opcodetype opcodeRet, out IEnumerable<byte> bytesRet)
        {
            bytesRet = new List<byte>();
            opcodeRet = opcodetype.OP_INVALIDOPCODE;

            opcodetype opcode;

            try
            {
                // Read instruction
                opcode = (opcodetype)codeBytes.Get();
            }
            catch (ByteQueueException)
            {
                // No instruction found there
                return false;
            }

            // Immediate operand
            if (opcode <= opcodetype.OP_PUSHDATA4)
            {
                byte[] szBytes = new byte[4] { 0, 0, 0, 0 }; // Zero length

                try
                {
                    if (opcode < opcodetype.OP_PUSHDATA1)
                    {
                        // Zero value opcodes (OP_0, OP_FALSE)
                        szBytes[3] = (byte)opcode;
                    }
                    else if (opcode == opcodetype.OP_PUSHDATA1)
                    {
                        // The next byte contains the number of bytes to be pushed onto the stack, 
                        //    i.e. you have something like OP_PUSHDATA1 0x01 [0x5a]
                        szBytes[3] = (byte)codeBytes.Get();
                    }
                    else if (opcode == opcodetype.OP_PUSHDATA2)
                    {
                        // The next two bytes contain the number of bytes to be pushed onto the stack,
                        //    i.e. now your operation will seem like this: OP_PUSHDATA2 0x00 0x01 [0x5a]
                        codeBytes.Get(2).CopyTo(szBytes, 2);
                    }
                    else if (opcode == opcodetype.OP_PUSHDATA4)
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
                        bytesRet = codeBytes.GetEnumerable(nSize);
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
        public static string ValueString(IEnumerable<byte> bytes)
        {
            StringBuilder sb = new StringBuilder();

            if (bytes.Count() <= 4)
            {
                byte[] valueBytes = new byte[4] { 0, 0, 0, 0 };
                bytes.ToArray().CopyTo(valueBytes, valueBytes.Length - bytes.Count());

                sb.Append(Interop.BEBytesToUInt32(valueBytes));
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
        public static string StackString(IList<IList<byte>> stackList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IList<byte> bytesList in stackList)
            {
                sb.Append(ValueString(bytesList));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decode small integer
        /// </summary>
        /// <param name="opcode">Small integer opcode (OP_0 - OP_16)</param>
        /// <returns>Small integer</returns>
        public static int DecodeOP_N(opcodetype opcode)
        {
            if (opcode == opcodetype.OP_0)
                return 0;

            // Only OP_n opcodes are supported, throw exception otherwise.
            if (opcode < opcodetype.OP_1 || opcode > opcodetype.OP_16)
                throw new Exception("Invalid small integer opcode.");
            return (int)opcode - (int)(opcodetype.OP_1 - 1);
        }

        /// <summary>
        /// Converts small integer into opcode
        /// </summary>
        /// <param name="n">Small integer from the range of 0 up to 16.</param>
        /// <returns>Corresponding opcode.</returns>
        public static opcodetype EncodeOP_N(int n)
        {
            // The n value must be in the range of 0 to 16.
            if (n < 0 || n > 16)
                throw new Exception("Invalid small integer value.");
            if (n == 0)
                return opcodetype.OP_0;
            return (opcodetype)(opcodetype.OP_1 + n - 1);
        }

        public static int ScriptSigArgsExpected(txnouttype t, IList<IEnumerable<byte>> solutions)
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
                    if (solutions.Count() < 1 || solutions.First().Count() < 1)
                        return -1;
                    return solutions.First().First() + 1;
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
            IList<IEnumerable<byte>> solutions = new List<IEnumerable<byte>>();

            if (!Solver(scriptPubKey, out whichType, out solutions))
            {
                // No solutions found
                return false;
            }

            if (whichType == txnouttype.TX_MULTISIG)
            {
                // Additional verification of OP_CHECKMULTISIG arguments
                byte m = solutions.First().First();
                byte n = solutions.Last().First();

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
        public static bool Solver(CScript scriptPubKey, out txnouttype typeRet, out IList<IEnumerable<byte>> solutions)
        {
            solutions = new List<IEnumerable<byte>>();

            // There are shortcuts for pay-to-script-hash and pay-to-pubkey-hash, which are more constrained than the other types:

            // It is always OP_HASH160 20 [20 byte hash] OP_EQUAL
            if (scriptPubKey.IsPayToScriptHash)
            {
                typeRet = txnouttype.TX_SCRIPTHASH;

                // Take 20 bytes with offset of 2 bytes
                IEnumerable<byte> hashBytes = scriptPubKey.Bytes.Skip(2).Take(20);
                solutions.Add(hashBytes);

                return true;
            }

            // It is always OP_DUP OP_HASH160 20 [20 byte hash] OP_EQUALVERIFY OP_CHECKSIG
            if (scriptPubKey.IsPayToPubKeyHash)
            {
                typeRet = txnouttype.TX_PUBKEYHASH;

                // Take 20 bytes with offset of 3 bytes
                IEnumerable<byte> hashBytes = scriptPubKey.Bytes.Skip(3).Take(20);
                solutions.Add(hashBytes);

                return true;
            }

            List<Tuple<txnouttype, IEnumerable<byte>>> templateTuples = new List<Tuple<txnouttype, IEnumerable<byte>>>();

            // Sender provides pubkey, receiver adds signature
            // [ECDSA public key] OP_CHECKSIG
            templateTuples.Add(
                new Tuple<txnouttype, IEnumerable<byte>>(
                    txnouttype.TX_PUBKEY,
                    new byte[] { (byte)opcodetype.OP_PUBKEY, (byte)opcodetype.OP_CHECKSIG })
            );

            // Sender provides N pubkeys, receivers provides M signatures
            // N [pubkey1] [pubkey2] ... [pubkeyN] M OP_CHECKMULTISIG
            // Where N and M are small integer opcodes (OP1 ... OP_16)
            templateTuples.Add(
                new Tuple<txnouttype, IEnumerable<byte>>(
                    txnouttype.TX_MULTISIG,
                    new byte[] { (byte)opcodetype.OP_SMALLINTEGER, (byte)opcodetype.OP_PUBKEYS, (byte)opcodetype.OP_SMALLINTEGER, (byte)opcodetype.OP_CHECKMULTISIG })
            );

            // Data-carrying output
            // OP_RETURN [up to 80 bytes of data]
            templateTuples.Add(
                new Tuple<txnouttype, IEnumerable<byte>>(
                    txnouttype.TX_NULL_DATA,
                    new byte[] { (byte)opcodetype.OP_RETURN, (byte)opcodetype.OP_SMALLDATA })
            );

            // Nonstandard tx output
            typeRet = txnouttype.TX_NONSTANDARD;

            foreach (Tuple<txnouttype, IEnumerable<byte>> templateTuple in templateTuples)
            {
                CScript script1 = scriptPubKey;
                CScript script2 = new CScript(templateTuple.Item2);

                opcodetype opcode1, opcode2;

                // Compare
                ByteQueue wl1 = script1.GetByteQUeue();
                ByteQueue wl2 = script2.GetByteQUeue();

                IEnumerable<byte> args1, args2;

                byte last1 = script1.Bytes.Last();
                byte last2 = script2.Bytes.Last();

                while (true)
                {
                    if (wl1.GetCurrent() == last1 && wl2.GetCurrent() == last2)
                    {
                        // Found a match
                        typeRet = templateTuple.Item1;
                        if (typeRet == txnouttype.TX_MULTISIG)
                        {
                            // Additional checks for TX_MULTISIG:
                            byte m = solutions.First().First();
                            byte n = solutions.Last().First();

                            if (m < 1 || n < 1 || m > n || solutions.Count - 2 != n)
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                    if (!GetOp(ref wl1, out opcode1, out args1))
                    {
                        break;
                    }
                    if (!GetOp(ref wl2, out opcode2, out args2))
                    {
                        break;
                    }

                    // Template matching opcodes:
                    if (opcode2 == opcodetype.OP_PUBKEYS)
                    {
                        while (args1.Count() >= 33 && args1.Count() <= 120)
                        {
                            solutions.Add(args1);
                            if (!GetOp(ref wl1, out opcode1, out args1))
                            {
                                break;
                            }
                        }
                        if (!GetOp(ref wl2, out opcode2, out args2))
                            break;
                        // Normal situation is to fall through
                        // to other if/else statements
                    }
                    if (opcode2 == opcodetype.OP_PUBKEY)
                    {
                        if (args1.Count() < 33 || args1.Count() > 120)
                        {
                            break;
                        }
                        solutions.Add(args1);
                    }
                    else if (opcode2 == opcodetype.OP_PUBKEYHASH)
                    {
                        if (args1.Count() != 20) // hash160 size
                        {
                            break;
                        }
                        solutions.Add(args1);
                    }
                    else if (opcode2 == opcodetype.OP_SMALLINTEGER)
                    {
                        // Single-byte small integer pushed onto solutions
                        if (opcode1 == opcodetype.OP_0 || (opcode1 >= opcodetype.OP_1 && opcode1 <= opcodetype.OP_16))
                        {
                            byte n = (byte)DecodeOP_N(opcode1);
                            solutions.Add(new byte[] { n });
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (opcode2 == opcodetype.OP_SMALLDATA)
                    {
                        // small pushdata, <= 80 bytes
                        if (args1.Count() > 80)
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

        public static Hash256 SignatureHash(CScript scriptCode, CTransaction txTo, int nIn, int nHashType)
        {
            if (nIn >= txTo.vin.Length)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("ERROR: SignatureHash() : nIn={0} out of range\n", nIn);
                throw new ArgumentOutOfRangeException("nIn", sb.ToString());
            }

            CTransaction txTmp = new CTransaction(txTo);

            // In case concatenating two scripts ends up with two codeseparators,
            // or an extra one at the end, this prevents all those possible incompatibilities.
            scriptCode.RemovePattern(new byte[] { (byte)opcodetype.OP_CODESEPARATOR });

            // Blank out other inputs' signatures
            for (int i = 0; i < txTmp.vin.Length; i++)
            {
                txTmp.vin[i].scriptSig = new CScript();
            }
            txTmp.vin[nIn].scriptSig = scriptCode;

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
            List<byte> b = new List<byte>();
            b.AddRange(txTmp.Bytes);
            b.AddRange(BitConverter.GetBytes(nHashType));

            return Hash256.Compute256(b);
        }

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


        //
        // Script is a stack machine (like Forth) that evaluates a predicate
        // returning a bool indicating valid or not.  There are no loops.
        //

        /// <summary>
        /// Remove last element from stack
        /// </summary>
        /// <param name="stack">Stack reference</param>
        static void popstack(ref List<IEnumerable<byte>> stack)
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
        static IEnumerable<byte> stacktop(ref List<IEnumerable<byte>> stack, int nDepth)
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
        private static bool CastToBool(IEnumerable<byte> arg)
        {
            byte[] value = arg.ToArray();

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0)
                {
                    // Can be negative zero
                    if (i == value.Length - 1 && value[i] == 0x80)
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
        private static BigInteger CastToBigInteger(IEnumerable<byte> value)
        {
            if (value.Count() > 4)
            {
                throw new StackMachineException("CastToBigInteger() : overflow");
            }

            return new BigInteger(value.ToArray());
        }

        static bool EvalScript(ref List<IEnumerable<byte>> stack, CScript script, CTransaction txTo, int nIn, int flags, int nHashType)
        {
            ByteQueue pc = script.GetByteQUeue();

            ByteQueue pbegincodehash = script.GetByteQUeue();

            opcodetype opcode;

            List<bool> vfExec = new List<bool>();
            List<IEnumerable<byte>> altstack = new List<IEnumerable<byte>>();

            byte[] vchFalse = new byte[0];
            byte[] vchTrue = new byte[] { 0x01 };

            if (script.Bytes.Count() > 10000)
            {
                return false;
            }

            int nOpCount = 0;

            while (true)
            {
                bool fExec = false;
                foreach (bool fValue in vfExec)
                {
                    if (!fValue)
                    {
                        fExec = true;
                        break;
                    }
                }

                //
                // Read instruction
                //
                IEnumerable<byte> pushArg;
                if (!GetOp(ref pc, out opcode, out pushArg))
                {
                    return false;
                }

                if (pushArg.Count() > 520) // Check against MAX_SCRIPT_ELEMENT_SIZE
                {
                    return false;
                }

                if (opcode > opcodetype.OP_16 && ++nOpCount > 201)
                {
                    return false;
                }

                if (fExec && 0 <= opcode && opcode <= opcodetype.OP_PUSHDATA4)
                {
                    // Push argument to stack
                    stack.Add(pushArg);
                }
                else if (fExec || (opcodetype.OP_IF <= opcode && opcode <= opcodetype.OP_ENDIF))
                    switch (opcode)
                    {
                        //
                        // Disabled opcodes
                        //
                        case opcodetype.OP_CAT:
                        case opcodetype.OP_SUBSTR:
                        case opcodetype.OP_LEFT:
                        case opcodetype.OP_RIGHT:
                        case opcodetype.OP_INVERT:
                        case opcodetype.OP_AND:
                        case opcodetype.OP_OR:
                        case opcodetype.OP_XOR:
                        case opcodetype.OP_2MUL:
                        case opcodetype.OP_2DIV:
                        case opcodetype.OP_MUL:
                        case opcodetype.OP_DIV:
                        case opcodetype.OP_MOD:
                        case opcodetype.OP_LSHIFT:
                        case opcodetype.OP_RSHIFT:
                            return false;

                        //
                        // Push value
                        //
                        case opcodetype.OP_1NEGATE:
                        case opcodetype.OP_1:
                        case opcodetype.OP_2:
                        case opcodetype.OP_3:
                        case opcodetype.OP_4:
                        case opcodetype.OP_5:
                        case opcodetype.OP_6:
                        case opcodetype.OP_7:
                        case opcodetype.OP_8:
                        case opcodetype.OP_9:
                        case opcodetype.OP_10:
                        case opcodetype.OP_11:
                        case opcodetype.OP_12:
                        case opcodetype.OP_13:
                        case opcodetype.OP_14:
                        case opcodetype.OP_15:
                        case opcodetype.OP_16:
                            {
                                // ( -- value)
                                BigInteger bn = new BigInteger((int)opcode - (int)(opcodetype.OP_1 - 1));
                                stack.Add(bn.ToByteArray());
                            }
                            break;


                        //
                        // Control
                        //
                        case opcodetype.OP_NOP:
                        case opcodetype.OP_NOP1:
                        case opcodetype.OP_NOP2:
                        case opcodetype.OP_NOP3:
                        case opcodetype.OP_NOP4:
                        case opcodetype.OP_NOP5:
                        case opcodetype.OP_NOP6:
                        case opcodetype.OP_NOP7:
                        case opcodetype.OP_NOP8:
                        case opcodetype.OP_NOP9:
                        case opcodetype.OP_NOP10:
                            break;

                        case opcodetype.OP_IF:
                        case opcodetype.OP_NOTIF:
                            {
                                // <expression> if [statements] [else [statements]] endif
                                bool fValue = false;
                                if (fExec)
                                {
                                    if (stack.Count() < 1)
                                    {
                                        return false;
                                    }
                                    IEnumerable<byte> vch = stacktop(ref stack, -1);
                                    fValue = CastToBool(vch);
                                    if (opcode == opcodetype.OP_NOTIF)
                                    {
                                        fValue = !fValue;
                                    }
                                    popstack(ref stack);
                                }
                                vfExec.Add(fValue);
                            }
                            break;

                        case opcodetype.OP_ELSE:
                            {
                                int nExecCount = vfExec.Count();
                                if (nExecCount == 0)
                                {
                                    return false;
                                }
                                vfExec[nExecCount - 1] = !vfExec[nExecCount - 1];
                            }
                            break;

                        case opcodetype.OP_ENDIF:
                            {
                                int nExecCount = vfExec.Count();
                                if (nExecCount == 0)
                                {
                                    return false;
                                }
                                vfExec.RemoveAt(nExecCount - 1);
                            }
                            break;

                        case opcodetype.OP_VERIFY:
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

                        case opcodetype.OP_RETURN:
                            return false;
                        //
                        // Stack ops
                        //
                        case opcodetype.OP_TOALTSTACK:
                            {
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }
                                altstack.Add(stacktop(ref stack, -1));
                                popstack(ref stack);
                            }
                            break;

                        case opcodetype.OP_FROMALTSTACK:
                            {
                                if (altstack.Count() < 1)
                                {
                                    return false;
                                }
                                stack.Add(stacktop(ref stack, -1));
                                popstack(ref altstack);
                            }
                            break;

                        case opcodetype.OP_2DROP:
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

                        case opcodetype.OP_2DUP:
                            {
                                // (x1 x2 -- x1 x2 x1 x2)
                                if (stack.Count() < 2)
                                {
                                    return false;
                                }
                                IEnumerable<byte> vch1 = stacktop(ref stack, -2);
                                IEnumerable<byte> vch2 = stacktop(ref stack, -1);
                                stack.Add(vch1);
                                stack.Add(vch2);
                            }
                            break;

                        case opcodetype.OP_3DUP:
                            {
                                // (x1 x2 x3 -- x1 x2 x3 x1 x2 x3)
                                if (stack.Count() < 3)
                                {
                                    return false;
                                }
                                IEnumerable<byte> vch1 = stacktop(ref stack, -3);
                                IEnumerable<byte> vch2 = stacktop(ref stack, -2);
                                IEnumerable<byte> vch3 = stacktop(ref stack, -1);
                                stack.Add(vch1);
                                stack.Add(vch2);
                                stack.Add(vch3);
                            }
                            break;

                        case opcodetype.OP_2OVER:
                            {
                                // (x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2)
                                if (stack.Count() < 4)
                                {
                                    return false;
                                }
                                IEnumerable<byte> vch1 = stacktop(ref stack, -4);
                                IEnumerable<byte> vch2 = stacktop(ref stack, -3);
                                stack.Add(vch1);
                                stack.Add(vch2);
                            }
                            break;

                        case opcodetype.OP_2ROT:
                            {
                                int nStackDepth = stack.Count();
                                // (x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2)
                                if (nStackDepth < 6)
                                {
                                    return false;
                                }
                                IEnumerable<byte> vch1 = stacktop(ref stack, -6);
                                IEnumerable<byte> vch2 = stacktop(ref stack, -5);
                                stack.RemoveRange(nStackDepth - 6, 2);
                                stack.Add(vch1);
                                stack.Add(vch2);
                            }
                            break;

                        case opcodetype.OP_2SWAP:
                            {
                                // (x1 x2 x3 x4 -- x3 x4 x1 x2)
                                int nStackDepth = stack.Count();
                                if (nStackDepth < 4)
                                {
                                    return false;
                                }
                                stack.Swap(nStackDepth - 4, nStackDepth - 2);
                                stack.Swap(nStackDepth - 3, nStackDepth - 1);
                            }
                            break;

                        case opcodetype.OP_IFDUP:
                            {
                                // (x - 0 | x x)
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }

                                IEnumerable<byte> vch = stacktop(ref stack, -1);

                                if (CastToBool(vch))
                                {
                                    stack.Add(vch);
                                }
                            }
                            break;

                        case opcodetype.OP_DEPTH:
                            {
                                // -- stacksize
                                BigInteger bn = new BigInteger((ushort)stack.Count());
                                stack.Add(bn.ToByteArray());
                            }
                            break;

                        case opcodetype.OP_DROP:
                            {
                                // (x -- )
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }

                                popstack(ref stack);
                            }
                            break;

                        case opcodetype.OP_DUP:
                            {
                                // (x -- x x)
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }

                                IEnumerable<byte> vch = stacktop(ref stack, -1);
                                stack.Add(vch);
                            }
                            break;

                        case opcodetype.OP_NIP:
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

                        case opcodetype.OP_OVER:
                            {
                                // (x1 x2 -- x1 x2 x1)
                                if (stack.Count() < 2)
                                {
                                    return false;
                                }

                                IEnumerable<byte> vch = stacktop(ref stack, -2);
                                stack.Add(vch);
                            }
                            break;

                        case opcodetype.OP_PICK:
                        case opcodetype.OP_ROLL:
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

                                IEnumerable<byte> vch = stacktop(ref stack, -n - 1);
                                if (opcode == opcodetype.OP_ROLL)
                                {
                                    stack.RemoveAt(nStackDepth - n - 1);
                                }

                                stack.Add(vch);
                            }
                            break;

                        case opcodetype.OP_ROT:
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

                        case opcodetype.OP_SWAP:
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

                        case opcodetype.OP_TUCK:
                            {
                                // (x1 x2 -- x2 x1 x2)
                                int nStackDepth = stack.Count();
                                if (nStackDepth < 2)
                                {
                                    return false;
                                }
                                IEnumerable<byte> vch = stacktop(ref stack, -1);
                                stack.Insert(nStackDepth - 2, vch);
                            }
                            break;


                        case opcodetype.OP_SIZE:
                            {
                                // (in -- in size)
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }

                                BigInteger bnSize = new BigInteger((ushort)stacktop(ref stack, -1).Count());
                                stack.Add(bnSize.ToByteArray());
                            }
                            break;


                        //
                        // Bitwise logic
                        //
                        case opcodetype.OP_EQUAL:
                        case opcodetype.OP_EQUALVERIFY:
                            //case opcodetype.OP_NOTEQUAL: // use OP_NUMNOTEQUAL
                            {
                                // (x1 x2 - bool)
                                if (stack.Count() < 2)
                                {
                                    return false;
                                }

                                IEnumerable<byte> vch1 = stacktop(ref stack, -2);
                                IEnumerable<byte> vch2 = stacktop(ref stack, -1);
                                bool fEqual = (vch1 == vch2);
                                // OP_NOTEQUAL is disabled because it would be too easy to say
                                // something like n != 1 and have some wiseguy pass in 1 with extra
                                // zero bytes after it (numerically, 0x01 == 0x0001 == 0x000001)
                                //if (opcode == opcodetype.OP_NOTEQUAL)
                                //    fEqual = !fEqual;
                                popstack(ref stack);
                                popstack(ref stack);
                                stack.Add(fEqual ? vchTrue : vchFalse);

                                if (opcode == opcodetype.OP_EQUALVERIFY)
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
                        case opcodetype.OP_1ADD:
                        case opcodetype.OP_1SUB:
                        case opcodetype.OP_NEGATE:
                        case opcodetype.OP_ABS:
                        case opcodetype.OP_NOT:
                        case opcodetype.OP_0NOTEQUAL:
                            {
                                // (in -- out)
                                if (stack.Count() < 1)
                                {
                                    return false;
                                }

                                BigInteger bn = CastToBigInteger(stacktop(ref stack, -1));
                                switch (opcode)
                                {
                                    case opcodetype.OP_1ADD:
                                        bn = bn + 1;
                                        break;
                                    case opcodetype.OP_1SUB:
                                        bn = bn - 1;
                                        break;
                                    case opcodetype.OP_NEGATE:
                                        bn = -bn;
                                        break;
                                    case opcodetype.OP_ABS:
                                        bn = BigInteger.Abs(bn);
                                        break;
                                    case opcodetype.OP_NOT:
                                        bn = bn == 0 ? 1 : 0;
                                        break;
                                    case opcodetype.OP_0NOTEQUAL:
                                        bn = bn != 0 ? 1 : 0;
                                        break;

                                    default:
                                        throw new StackMachineException("invalid opcode");
                                        break;
                                }

                                popstack(ref stack);
                                stack.Add(bn.ToByteArray());
                            }
                            break;

                        case opcodetype.OP_ADD:
                        case opcodetype.OP_SUB:
                        case opcodetype.OP_BOOLAND:
                        case opcodetype.OP_BOOLOR:
                        case opcodetype.OP_NUMEQUAL:
                        case opcodetype.OP_NUMEQUALVERIFY:
                        case opcodetype.OP_NUMNOTEQUAL:
                        case opcodetype.OP_LESSTHAN:
                        case opcodetype.OP_GREATERTHAN:
                        case opcodetype.OP_LESSTHANOREQUAL:
                        case opcodetype.OP_GREATERTHANOREQUAL:
                        case opcodetype.OP_MIN:
                        case opcodetype.OP_MAX:
                            {
                                // (x1 x2 -- out)
                                if (stack.Count() < 2)
                                {
                                    return false;
                                }

                                BigInteger bn1 = CastToBigInteger(stacktop(ref stack, -2));
                                BigInteger bn2 = CastToBigInteger(stacktop(ref stack, -1));
                                BigInteger bn = 0;

                                switch (opcode)
                                {
                                    case opcodetype.OP_ADD:
                                        bn = bn1 + bn2;
                                        break;
                                    case opcodetype.OP_SUB:
                                        bn = bn1 - bn2;
                                        break;
                                    case opcodetype.OP_BOOLAND:
                                        bn = (bn1 != 0 && bn2 != 0) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_BOOLOR:
                                        bn = (bn1 != 0 || bn2 != 0) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_NUMEQUAL:
                                        bn = (bn1 == bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_NUMEQUALVERIFY:
                                        bn = (bn1 == bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_NUMNOTEQUAL:
                                        bn = (bn1 != bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_LESSTHAN:
                                        bn = (bn1 < bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_GREATERTHAN:
                                        bn = (bn1 > bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_LESSTHANOREQUAL:
                                        bn = (bn1 <= bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_GREATERTHANOREQUAL:
                                        bn = (bn1 >= bn2) ? 1 : 0;
                                        break;
                                    case opcodetype.OP_MIN:
                                        bn = (bn1 < bn2 ? bn1 : bn2);
                                        break;
                                    case opcodetype.OP_MAX:
                                        bn = (bn1 > bn2 ? bn1 : bn2);
                                        break;

                                    default:
                                        throw new StackMachineException("invalid opcode");
                                        break;
                                }

                                popstack(ref stack);
                                popstack(ref stack);
                                stack.Add(bn.ToByteArray());

                                if (opcode == opcodetype.OP_NUMEQUALVERIFY)
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

                        case opcodetype.OP_WITHIN:
                            {
                                // (x min max -- out)
                                if (stack.Count() < 3)
                                    return false;
                                BigInteger bn1 = CastToBigInteger(stacktop(ref stack, -3));
                                BigInteger bn2 = CastToBigInteger(stacktop(ref stack, -2));
                                BigInteger bn3 = CastToBigInteger(stacktop(ref stack, -1));
                                bool fValue = (bn2 <= bn1 && bn1 < bn3);
                                popstack(ref stack);
                                popstack(ref stack);
                                popstack(ref stack);
                                stack.Add(fValue ? vchTrue : vchFalse);
                            }
                            break;


                        //
                        // Crypto
                        //
                        case opcodetype.OP_RIPEMD160:
                        case opcodetype.OP_SHA1:
                        case opcodetype.OP_SHA256:
                        case opcodetype.OP_HASH160:
                        case opcodetype.OP_HASH256:
                            {
                                // (in -- hash)
                                if (stack.Count() < 1)
                                    return false;
                                IEnumerable<byte> vch = stacktop(ref stack, -1);
                                IEnumerable<byte> vchHash = null;
                                if (opcode == opcodetype.OP_RIPEMD160)
                                {
                                    RIPEMD160 hash = RIPEMD160.Compute160(vch);
                                    vchHash = hash.hashBytes;
                                }
                                else if (opcode == opcodetype.OP_SHA1)
                                {
                                    SHA1 hash = SHA1.Compute1(vch);
                                    vchHash = hash.hashBytes;
                                }
                                else if (opcode == opcodetype.OP_SHA256)
                                {
                                    SHA256 hash = SHA256.Compute256(vch);
                                    vchHash = hash.hashBytes;
                                }
                                else if (opcode == opcodetype.OP_HASH160)
                                {
                                    Hash160 hash = Hash160.Compute160(vch);
                                    vchHash = hash.hashBytes;
                                }
                                else if (opcode == opcodetype.OP_HASH256)
                                {
                                    Hash256 hash = Hash256.Compute256(vch);
                                    vchHash = hash.hashBytes;
                                }
                                popstack(ref stack);
                                stack.Add(vchHash);
                            }
                            break;

                        case opcodetype.OP_CODESEPARATOR:
                            {
                                // Hash starts after the code separator
                                pbegincodehash = pc;
                            }
                            break;

                        case opcodetype.OP_CHECKSIG:
                        case opcodetype.OP_CHECKSIGVERIFY:
                            {
                                // (sig pubkey -- bool)
                                if (stack.Count() < 2)
                                {
                                    return false;
                                }

                                IList<byte> sigBytes = stacktop(ref stack, -2).ToList();
                                IList<byte> pubkeyBytes = stacktop(ref stack, -1).ToList();

                                // Subset of script starting at the most recent codeseparator
                                CScript scriptCode = new CScript(script.Bytes.Skip(pbegincodehash.CurrentIndex));

                                // There's no way for a signature to sign itself
                                scriptCode.RemovePattern(sigBytes);

                                bool fSuccess = IsCanonicalSignature(sigBytes, flags) && IsCanonicalPubKey(pubkeyBytes.ToList(), flags) && CheckSig(sigBytes, pubkeyBytes, scriptCode, txTo, nIn, nHashType, flags);

                                popstack(ref stack);
                                popstack(ref stack);
                                stack.Add(fSuccess ? vchTrue : vchFalse);
                                if (opcode == opcodetype.OP_CHECKSIGVERIFY)
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

                        case opcodetype.OP_CHECKMULTISIG:
                        case opcodetype.OP_CHECKMULTISIGVERIFY:
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
                                CScript scriptCode = new CScript(script.Bytes.Skip(pbegincodehash.CurrentIndex));

                                // There is no way for a signature to sign itself, so we need to drop the signatures
                                for (int k = 0; k < nSigsCount; k++)
                                {
                                    IEnumerable<byte> vchSig = stacktop(ref stack, -isig - k);
                                    scriptCode.RemovePattern(vchSig.ToList());
                                }

                                bool fSuccess = true;
                                while (fSuccess && nSigsCount > 0)
                                {
                                    IList<byte> sigBytes = stacktop(ref stack, -isig).ToList();
                                    IList<byte> pubKeyBytes = stacktop(ref stack, -ikey).ToList();

                                    // Check signature
                                    bool fOk = IsCanonicalSignature(sigBytes, flags) && IsCanonicalPubKey(pubKeyBytes.ToList(), flags) && CheckSig(sigBytes, pubKeyBytes, scriptCode, txTo, nIn, nHashType, flags);

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

                                stack.Add(fSuccess ? vchTrue : vchFalse);

                                if (opcode == opcodetype.OP_CHECKMULTISIGVERIFY)
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
                if (stack.Count() + altstack.Count() > 1000)
                {
                    return false;
                }
            }


            if (vfExec.Count() == 0)
            {
                return false;
            }

            return true;
        }


        static bool IsCanonicalPubKey(IList<byte> pubKeyBytes, int flags)
        {
            if ((flags & (int)scriptflag.SCRIPT_VERIFY_STRICTENC) == 0)
                return true;

            if (pubKeyBytes.Count() < 33)
                return false;  // Non-canonical public key: too short
            if (pubKeyBytes[0] == 0x04)
            {
                if (pubKeyBytes.Count() != 65)
                    return false; // Non-canonical public key: invalid length for uncompressed key
            }
            else if (pubKeyBytes[0] == 0x02 || pubKeyBytes[0] == 0x03)
            {
                if (pubKeyBytes.Count() != 33)
                    return false; // Non-canonical public key: invalid length for compressed key
            }
            else
            {
                return false; // Non-canonical public key: compressed nor uncompressed
            }
            return true;
        }

        static bool IsCanonicalSignature(IList<byte> sigBytes, int flags)
        {
            // STUB

            return true;
        }


        static bool CheckSig(IList<byte> sigBytes, IList<byte> pubKeyBytes, CScript scriptCode, CTransaction txTo, int nIn, int nHashType, int flags)
        {
            // STUB
            return true;
        }

};
}
