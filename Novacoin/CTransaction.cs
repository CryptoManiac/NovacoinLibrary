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
using System.Text;
using System.Collections.Generic;

namespace Novacoin
{
    /// <summary>
    /// Represents the transaction. Any transaction must provide one input and one output at least.
    /// </summary>
    public class CTransaction
    {
        /// <summary>
        /// Version of transaction schema.
        /// </summary>
        public uint nVersion = 1;

        /// <summary>
        /// Transaction timestamp.
        /// </summary>
        public uint nTime = 0;

        /// <summary>
        /// Array of transaction inputs
        /// </summary>
        public CTxIn[] vin;

        /// <summary>
        /// Array of transaction outputs
        /// </summary>
        public CTxOut[] vout;

        /// <summary>
        /// Block height or timestamp when transaction is final
        /// </summary>
        public uint nLockTime = 0;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
        public CTransaction()
        {
            // Initialize empty input and output arrays. Please note that such 
            // configuration is not valid for real transaction, you have to supply 
            // at least one input and one output.
            vin = new CTxIn[0];
            vout = new CTxOut[0];
        }

        /// <summary>
        /// Initialize new instance as a copy of another transaction
        /// </summary>
        /// <param name="tx">Transaction to copy from</param>
        public CTransaction(CTransaction tx)
        {
            nVersion = tx.nVersion;
            nTime = tx.nTime;

            vin = new CTxIn[tx.vin.Length];

            for (int i = 0; i < vin.Length; i++)
            {
                vin[i] = new CTxIn(tx.vin[i]);
            }

            vout = new CTxOut[tx.vout.Length];

            for (int i = 0; i < vout.Length; i++)
            {
                vout[i] = new CTxOut(tx.vout[i]);
            }

            nLockTime = tx.nLockTime;
        }


        /// <summary>
        /// Parse byte sequence and initialize new instance of CTransaction
        /// </summary>
        /// <param name="txBytes">Byte sequence</param>
		public CTransaction(byte[] txBytes)
        {
            var wBytes = new ByteQueue(txBytes);

            nVersion = BitConverter.ToUInt32(wBytes.Get(4), 0);
            nTime = BitConverter.ToUInt32(wBytes.Get(4), 0);

            int nInputs = (int)(int)wBytes.GetVarInt();
            vin = new CTxIn[nInputs];

            for (int nCurrentInput = 0; nCurrentInput < nInputs; nCurrentInput++)
            {
                // Fill inputs array
                vin[nCurrentInput] = new CTxIn();
                
                vin[nCurrentInput].prevout = new COutPoint(wBytes.Get(36));

                int nScriptSigLen = (int)wBytes.GetVarInt();
                vin[nCurrentInput].scriptSig = new CScript(wBytes.Get(nScriptSigLen));

                vin[nCurrentInput].nSequence = BitConverter.ToUInt32(wBytes.Get(4), 0);
            }

            int nOutputs = (int)wBytes.GetVarInt();
            vout = new CTxOut[nOutputs];

            for (int nCurrentOutput = 0; nCurrentOutput < nOutputs; nCurrentOutput++)
            {
                // Fill outputs array
                vout[nCurrentOutput] = new CTxOut();
                vout[nCurrentOutput].nValue = BitConverter.ToInt64(wBytes.Get(8), 0);

                int nScriptPKLen = (int)wBytes.GetVarInt();
                vout[nCurrentOutput].scriptPubKey = new CScript(wBytes.Get(nScriptPKLen));
            }

            nLockTime = BitConverter.ToUInt32(wBytes.Get(4), 0);
        }

        /// <summary>
        /// Read transactions array which is encoded in the block body.
        /// </summary>
        /// <param name="wTxBytes">Bytes sequence</param>
        /// <returns>Transactions array</returns>
        public static CTransaction[] ReadTransactionsList(ref ByteQueue wTxBytes)
        {
            // Read amount of transactions
            int nTransactions = (int)wTxBytes.GetVarInt();
            var tx = new CTransaction[nTransactions];

            for (int nTx = 0; nTx < nTransactions; nTx++)
            {
                // Fill the transactions array
                tx[nTx] = new CTransaction();

                tx[nTx].nVersion = BitConverter.ToUInt32(wTxBytes.Get(4), 0);
                tx[nTx].nTime = BitConverter.ToUInt32(wTxBytes.Get(4), 0);

                // Inputs array
                tx[nTx].vin = CTxIn.ReadTxInList(ref wTxBytes);

                // outputs array
                tx[nTx].vout = CTxOut.ReadTxOutList(ref wTxBytes);

                tx[nTx].nLockTime = BitConverter.ToUInt32(wTxBytes.Get(4), 0);
            }

            return tx;
        }

        public bool IsCoinBase
        {
            get { return (vin.Length == 1 && vin[0].prevout.IsNull && vout.Length >= 1); }
        }

        public bool IsCoinStake
        {
            get
            {
                return (vin.Length > 0 && (!vin[0].prevout.IsNull) && vout.Length >= 2 && vout[0].IsEmpty);
            }
        }

        /// <summary>
        /// Transaction hash
        /// </summary>
        public Hash256 Hash
        {
            get { return Hash256.Compute256(Bytes); }
        }

        /// <summary>
        /// A sequence of bytes, which corresponds to the current state of CTransaction.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                var resultBytes = new List<byte>();

                // Typical transaction example:
                //
                // 01000000 -- version
                // 78b4c953 -- timestamp
                // 06       -- amount of txins
                // 340d96b77ec4ee9d42b31cadc2fab911e48d48c36274d516f226d5e85bbc512c -- txin hash
                // 01000000 -- txin outnumber
                // 6b       -- txin scriptSig length
                // 483045022100c8df1fc17b6ea1355a39b92146ec67b3b53565e636e028010d3a8a87f6f805f202203888b9b74df03c3960773f2a81b2dfd1efb08bb036a8f3600bd24d5ed694cd5a0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // 364c640420de8fa77313475970bf09ce4d0b1f8eabb8f1d6ea49d90c85b202ee -- txin hash
                // 01000000 -- txin outnumber
                // 6b       -- txin scriptSig length
                // 483045022100b651bf3a6835d714d2c990c742136d769258d0170c9aac24803b986050a8655b0220623651077ff14b0a9d61e30e30f2c15352f70491096f0ec655ae1c79a44e53aa0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // 7adbd5f2e521f567bfea2cb63e65d55e66c83563fe253464b75184a5e462043d -- txin hash
                // 00000000 -- txin outnumber
                // 6a       -- txin scriptSig length
                // 4730440220183609f2b995993acc9df241aff722d48b9a731b0cd376212934565723ed81f00220737e7ce75ef39bdc061d0dcdba3ee24e43b899696a7c96803cee0a79e1f78ecb0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // 999eb03e00a41c2f9fde8865a554ceebbc48d30f4c8ba22dd88da8c9b46fa920 -- txin hash
                // 03000000 -- txin outnumber
                // 6b       -- txin scriptSig length
                // 483045022100ec1ab104ef086ba79b0f2611ebf1bfdd22a7a1020f6630fa1c6707546626e0db022056093d4048a999392185ccc735ef736a5497bd68f60b42e6c0c93ba770b54d010121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // c0543b86be257ddd85b014a76718a70fab9eaa3c477460e4ca187094d86f369c -- txin hash
                // 05000000 -- txin outnumber
                // 69       -- txin scriptSig length
                // 463043021f24275c72f952043174daf01d7f713f878625f0522124a3cab48a0a2e12604202201b47742e6697b0ebdd1e4ba49c74baf142a0228ad0e0ee847488994c9dce78470121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // e1793d4519147782293dd1db6d90e461265d91db2cc6889c37209394d42ad10d -- txin hash
                // 05000000 -- txin outnumber
                // 6a       -- txin scriptSig length
                // 473044022018a0c3d73b2765d75380614ab36ee8e3c937080894a19166128b1e3357b208fb0220233c9609985f535547381431526867ad0255ec4969afe5c360544992ed6b3ed60121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4c -- txin scriptSig
                // ffffffff -- txin nSequence
                // 02 -- amount of txouts
                // e542000000000000 -- txout value
                // 19 -- scriptPubKey length
                // 76a91457d84c814b14bd86bf32f106b733baa693db7dc788ac -- scriptPubKey
                // 409c000000000000 -- txout value
                // 19 -- scriptPubKey length
                // 76a91408c8768d5d6bf7c1d9609da4e766c3f1752247b188ac -- scriptPubKey
                // 00000000 -- lock time

                resultBytes.AddRange(BitConverter.GetBytes(nVersion));
                resultBytes.AddRange(BitConverter.GetBytes(nTime));
                resultBytes.AddRange(VarInt.EncodeVarInt(vin.LongLength));

                foreach (var input in vin)
                {
                    resultBytes.AddRange(input.Bytes);
                }

                resultBytes.AddRange(VarInt.EncodeVarInt(vout.LongLength));

                foreach (var output in vout)
                {
                    resultBytes.AddRange(output.Bytes);
                }

                resultBytes.AddRange(BitConverter.GetBytes(nLockTime));

                return resultBytes.ToArray();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("CTransaction(\n nVersion={0},\n nTime={1},\n", nVersion, nTime);

            foreach (var txin in vin)
            {
                sb.AppendFormat(" {0},\n", txin.ToString());
            }

            foreach (var txout in vout)
            {
                sb.AppendFormat(" {0},\n", txout.ToString());
            }

            sb.AppendFormat("\nnLockTime={0}\n)", nLockTime);

            return sb.ToString();
        }
	}
}
