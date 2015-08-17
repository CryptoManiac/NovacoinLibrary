﻿using System;
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
		public CTxIn[] inputs;

		/// <summary>
		/// Array of transaction outputs
		/// </summary>
		public CTxOut[] outputs;

		/// <summary>
		/// Block height or timestamp when transaction is final
		/// </summary>
		public uint nLockTime = 0;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
        public CTransaction()
        {
        }

        /// <summary>
        /// Parse byte sequence and initialize new instance of CTransaction
        /// </summary>
        /// <param name="txBytes"></param>
		public CTransaction (IList<byte> txBytes)
		{
            WrappedList<byte> wBytes = new WrappedList<byte>(txBytes);

            nVersion = Interop.LEBytesToUInt32(wBytes.GetItems(4));
            nTime = Interop.LEBytesToUInt32(wBytes.GetItems(4));

            int nInputs = (int)VarInt.ReadVarInt(ref wBytes);
            inputs = new CTxIn[nInputs];

            for (int nCurrentInput = 0; nCurrentInput < nInputs; nCurrentInput++)
            {
                // Fill inputs array
                inputs[nCurrentInput] = new CTxIn();

                inputs[nCurrentInput].txID = new Hash256(wBytes.GetItems(32));
                inputs[nCurrentInput].n = Interop.LEBytesToUInt32(wBytes.GetItems(4));
                inputs[nCurrentInput].scriptSig = wBytes.GetItems((int)VarInt.ReadVarInt(ref wBytes));
                inputs[nCurrentInput].nSequence = Interop.LEBytesToUInt32(wBytes.GetItems(4));
            }

            int nOutputs = (int)VarInt.ReadVarInt(ref wBytes);
            outputs = new CTxOut[nOutputs];

            for (int nCurrentOutput = 0; nCurrentOutput < nOutputs; nCurrentOutput++)
            {
                // Fill outputs array
                outputs[nCurrentOutput] = new CTxOut();
                outputs[nCurrentOutput].nValue = Interop.LEBytesToUInt64(wBytes.GetItems(8));
                outputs[nCurrentOutput].scriptPubKey = wBytes.GetItems((int)VarInt.ReadVarInt(ref wBytes));
            }

            nLockTime = Interop.LEBytesToUInt32(wBytes.GetItems(4));
		}

        /// <summary>
        /// Read transactions array which is encoded in the block body.
        /// </summary>
        /// <param name="wTxBytes">Bytes sequence</param>
        /// <returns>Transactions array</returns>
        public static CTransaction[] ReadTransactionsList(ref WrappedList<byte> wTxBytes)
        {
            CTransaction[] tx;
            
            // Read amount of transactions
            int nTransactions = (int) VarInt.ReadVarInt(ref wTxBytes);
            tx = new CTransaction[nTransactions];

            for (int nTx = 0; nTx < nTransactions; nTx++)
            {
                // Fill the transactions array
                tx[nTx] = new CTransaction();

                tx[nTx].nVersion = Interop.LEBytesToUInt32(wTxBytes.GetItems(4));
                tx[nTx].nTime = Interop.LEBytesToUInt32(wTxBytes.GetItems(4));

                // Inputs array
                tx[nTx].inputs = CTxIn.ReadTxInList(ref wTxBytes);

                // outputs array
                tx[nTx].outputs = CTxOut.ReadTxOutList(ref wTxBytes);

                tx[nTx].nLockTime = Interop.LEBytesToUInt32(wTxBytes.GetItems(4));
            }

            return tx;
        }

        IList<byte> ToBytes()
        {
            List<byte> resultBytes = new List<byte>();

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

            resultBytes.AddRange(Interop.LEBytes(nVersion));
            resultBytes.AddRange(Interop.LEBytes(nTime));
            resultBytes.AddRange(VarInt.EncodeVarInt(inputs.LongLength));

            foreach(CTxIn input in inputs)
            {
                resultBytes.AddRange(input.ToBytes());
            }

            resultBytes.AddRange(VarInt.EncodeVarInt(outputs.LongLength));
            
            foreach(CTxOut output in outputs)
            {
                resultBytes.AddRange(output.ToBytes());
            }

            resultBytes.AddRange(Interop.LEBytes(nLockTime));

            return resultBytes;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CTransaction(\n nVersion={0},\n nTime={1},\n", nVersion, nTime);

            foreach (CTxIn txin in inputs)
            {
                sb.AppendFormat(" {0},\n", txin.ToString());
            }

            foreach (CTxOut txout in outputs)
            {
                sb.AppendFormat(" {0},\n", txout.ToString());
            }

            sb.AppendFormat("nLockTime={0})\n", nLockTime);

            return sb.ToString();
        }
	}
}

