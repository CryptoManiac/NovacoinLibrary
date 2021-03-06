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
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Novacoin
{
    [Serializable]
    public class TransactionConstructorException : Exception
    {
        public TransactionConstructorException()
        {
        }

        public TransactionConstructorException(string message)
                : base(message)
        {
        }

        public TransactionConstructorException(string message, Exception inner)
                : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Represents the transaction.
    /// </summary>
    public class CTransaction
    {
        /// <summary>
        /// One cent = 10000 satoshis.
        /// </summary>
        public const long nCent = 10000;

        /// <summary>
        /// One coin = 1000000 satoshis.
        /// </summary>
        public const long nCoin = 1000000;
        
        /// <summary>
        /// Sanity checking threshold.
        /// </summary>
        public const long nMaxMoney = 2000000000 * nCoin;

        public const long nMinTxFee = nCent / 10;
        public const long nMinRelayTxFee = nCent / 50;
        public const long nMinTxoutAmount = nCent / 100;        

        /// <summary>
        /// Maximum transaction size is 250Kb
        /// </summary>
        public const uint nMaxTxSize = 250000;

        public enum MinFeeMode
        {
            GMF_BLOCK,
            GMF_RELAY,
            GMF_SEND,
        }

        /// <summary>
        /// Version of transaction schema.
        /// </summary>
        public uint nVersion;

        /// <summary>
        /// Transaction timestamp.
        /// </summary>
        public uint nTime;

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
        public uint nLockTime;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
        public CTransaction()
        {
            // Initialize empty input and output arrays. Please note that such 
            // configuration is not valid for real transaction, you have to supply 
            // at least one input and one output.
            nVersion = 1;
            nTime = 0;
            vin = new CTxIn[0];
            vout = new CTxOut[0];
            nLockTime = 0;
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
        /// Attempts to execute all transaction scripts and validate the results.
        /// </summary>
        /// <returns>Checking result.</returns>
        public bool VerifyScripts()
        {
            if (IsCoinBase)
            {
                return true;
            }

            for (int i = 0; i < vin.Length; i++)
            {
                var outpoint = vin[i].prevout;

                TxOutItem txOutCursor;
                if (!CBlockStore.Instance.GetTxOutCursor(outpoint, out txOutCursor))
                    return false;

                if (!ScriptCode.VerifyScript(vin[i].scriptSig, txOutCursor.scriptPubKey, this, i, (int)scriptflag.SCRIPT_VERIFY_P2SH, 0))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate amount of signature operations without trying to properly evaluate P2SH scripts.
        /// </summary>
        public uint LegacySigOpCount
        {
            get
            {
                uint nSigOps = 0;

                if (!IsCoinBase)
                {
                    // http://lists.linuxfoundation.org/pipermail/bitcoin-dev/2012-July/001718.html

                    foreach (var txin in vin)
                    {
                        nSigOps += txin.scriptSig.GetSigOpCount(false);
                    }
                }

                foreach (var txout in vout)
                {
                    nSigOps += txout.scriptPubKey.GetSigOpCount(false);
                }

                return nSigOps;
            }
        }

        /// <summary>
        /// Basic sanity checkings
        /// </summary>
        /// <returns>Checking result</returns>
        public bool CheckTransaction()
        {
            if (Size > nMaxTxSize || vin.Length == 0 || vout.Length == 0)
            {
                return false;
            }

            // Check for empty or overflow output values
            long nValueOut = 0;
            for (int i = 0; i < vout.Length; i++)
            {
                CTxOut txout = vout[i];
                if (txout.IsEmpty && !IsCoinBase && !IsCoinStake)
                {
                    // Empty outputs aren't allowed for user transactions.
                    return false;
                }

                nValueOut += txout.nValue;
                if (!MoneyRange(nValueOut))
                {
                    return false;
                }
            }

            // Check for duplicate inputs
            var InOutPoints = new List<COutPoint>();
            foreach (var txin in vin)
            {
                if (InOutPoints.IndexOf(txin.prevout) != -1)
                {
                    // Duplicate input.
                    return false;
                }
                InOutPoints.Add(txin.prevout);
            }

            if (IsCoinBase)
            {
                if (vin[0].scriptSig.Size < 2 || vin[0].scriptSig.Size > 100)
                {
                    // Script size is invalid
                    return false;
                }
            }
            else
            {
                foreach (var txin in vin)
                {
                    if (txin.prevout.IsNull)
                    {
                        // Null input in non-coinbase transaction.
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsFinal(uint nBlockHeight = 0, uint nBlockTime = 0)
        {
            // Time based nLockTime
            if (nLockTime == 0)
            {
                return true;
            }
            if (nBlockHeight == 0)
            {
                nBlockHeight = uint.MaxValue; // TODO: stupid stub here, should be best height instead.
            }
            if (nBlockTime == 0)
            {
                nBlockTime = NetInfo.GetAdjustedTime();
            }
            if (nLockTime < (nLockTime < NetInfo.nLockTimeThreshold ? nBlockHeight : nBlockTime))
            {
                return true;
            }
            foreach (var txin in vin)
            {
                if (!txin.IsFinal)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Parse byte sequence and initialize new instance of CTransaction
        /// </summary>
        /// <param name="txBytes">Byte sequence</param>
        public CTransaction(byte[] txBytes)
        {
            try
            {
                var stream = new MemoryStream(txBytes);
                var reader = new BinaryReader(stream);

                nVersion = reader.ReadUInt32();
                nTime = reader.ReadUInt32();

                int nInputs = (int)VarInt.ReadVarInt(ref reader);
                vin = new CTxIn[nInputs];

                for (int nCurrentInput = 0; nCurrentInput < nInputs; nCurrentInput++)
                {
                    // Fill inputs array
                    vin[nCurrentInput] = new CTxIn();

                    vin[nCurrentInput].prevout = new COutPoint(reader.ReadBytes(36));

                    int nScriptSigLen = (int)VarInt.ReadVarInt(ref reader);
                    vin[nCurrentInput].scriptSig = new CScript(reader.ReadBytes(nScriptSigLen));

                    vin[nCurrentInput].nSequence = reader.ReadUInt32();
                }

                int nOutputs = (int)VarInt.ReadVarInt(ref reader);
                vout = new CTxOut[nOutputs];

                for (int nCurrentOutput = 0; nCurrentOutput < nOutputs; nCurrentOutput++)
                {
                    // Fill outputs array
                    vout[nCurrentOutput] = new CTxOut();
                    vout[nCurrentOutput].nValue = reader.ReadInt64();

                    int nScriptPKLen = (int)VarInt.ReadVarInt(ref reader);
                    vout[nCurrentOutput].scriptPubKey = new CScript(reader.ReadBytes(nScriptPKLen));
                }

                nLockTime = reader.ReadUInt32();
            }
            catch (Exception e)
            {
                throw new TransactionConstructorException("Deserialization failed", e);
            }
        }

        /// <summary>
        /// Serialized size
        /// </summary>
        public uint Size
        {
            get
            {
                uint nSize = 12; // nVersion, nTime, nLockLime

                nSize += VarInt.GetEncodedSize(vin.Length);
                nSize += VarInt.GetEncodedSize(vout.Length);

                foreach (var input in vin)
                {
                    nSize += input.Size;
                }

                foreach (var output in vout)
                {
                    nSize += output.Size;
                }

                return nSize;
            }
        }

        /// <summary>
        /// Read transactions array which is encoded in the block body.
        /// </summary>
        /// <param name="wTxBytes">Bytes sequence</param>
        /// <returns>Transactions array</returns>
        internal static CTransaction[] ReadTransactionsList(ref BinaryReader reader)
        {
            try
            {
                // Read amount of transactions
                int nTransactions = (int)VarInt.ReadVarInt(ref reader);
                var tx = new CTransaction[nTransactions];

                for (int nTx = 0; nTx < nTransactions; nTx++)
                {
                    // Fill the transactions array
                    tx[nTx] = new CTransaction();

                    tx[nTx].nVersion = reader.ReadUInt32();
                    tx[nTx].nTime = reader.ReadUInt32();

                    // Inputs array
                    tx[nTx].vin = CTxIn.ReadTxInList(ref reader);

                    // outputs array
                    tx[nTx].vout = CTxOut.ReadTxOutList(ref reader);

                    tx[nTx].nLockTime = reader.ReadUInt32();
                }

                return tx;

            }
            catch (Exception e)
            {
                throw new TransactionConstructorException("Deserialization failed", e);
            }
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
        public uint256 Hash
        {
            get { return CryptoUtils.ComputeHash256(this); }
        }

        /// <summary>
        /// Amount of novacoins spent by this transaction.
        /// </summary>
        public long nValueOut
        {
            get
            {
                long nValueOut = 0;
                foreach (var txout in vout)
                {
                    nValueOut += txout.nValue;
                    Contract.Assert(MoneyRange(txout.nValue) && MoneyRange(nValueOut));
                }
                return nValueOut;
            }
        }

        /// <summary>
        /// A sequence of bytes, which corresponds to the current state of CTransaction.
        /// </summary>
        public static implicit operator byte[] (CTransaction tx)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(tx.nVersion);
            writer.Write(tx.nTime);
            writer.Write(VarInt.EncodeVarInt(tx.vin.LongLength));

            foreach (var input in tx.vin)
            {
                writer.Write(input);
            }

            writer.Write(VarInt.EncodeVarInt(tx.vout.LongLength));

            foreach (var output in tx.vout)
            {
                writer.Write(output);
            }

            writer.Write(tx.nLockTime);
            var resultBytes = stream.ToArray();
            writer.Close();

            return resultBytes;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("CTransaction(\n nVersion={0},\n nTime={1},\n", nVersion, nTime);

            foreach (var txin in vin)
            {
                sb.AppendFormat(" {0},\n", txin);
            }

            foreach (var txout in vout)
            {
                sb.AppendFormat(" {0},\n", txout);
            }

            sb.AppendFormat("\nnLockTime={0}\n)", nLockTime);

            return sb.ToString();
        }

        public static bool MoneyRange(long nValue) { return (nValue <= nMaxMoney); }

        /// <summary>
        /// Get total sigops.
        /// </summary>
        /// <param name="inputs">Inputs map.</param>
        /// <returns>Amount of sigops.</returns>
        public uint GetP2SHSigOpCount(ref Dictionary<COutPoint, TxOutItem> inputs)
        {
            if (IsCoinBase)
            {
                return 0;
            }

            uint nSigOps = 0;
            for (var i = 0; i < vin.Length; i++)
            {
                var prevout = GetOutputFor(vin[i], ref inputs);
                if (prevout.scriptPubKey.IsPayToScriptHash)
                {
                    nSigOps += prevout.scriptPubKey.GetSigOpCount(vin[i].scriptSig);
                }
            }

            return nSigOps;
        }

        /// <summary>
        /// Get sum of inputs spent by this transaction.
        /// </summary>
        /// <param name="inputs">Reference to innputs map.</param>
        /// <returns>Sum of inputs.</returns>
        public long GetValueIn(ref Dictionary<COutPoint, TxOutItem> inputs)
        {
            if (IsCoinBase)
            {
                return 0;
            }

            long nResult = 0;
            for (int i = 0; i < vin.Length; i++)
            {
                nResult += GetOutputFor(vin[i], ref inputs).nValue;
            }

            return nResult;
        }

        /// <summary>
        /// Helper method to find output in the map.
        /// </summary>
        /// <param name="input">Transaction input.</param>
        /// <param name="inputs">eference to inuts map.</param>
        /// <returns>Parent output.</returns>
        private CTxOut GetOutputFor(CTxIn input, ref Dictionary<COutPoint, TxOutItem> inputs)
        {
            if (!inputs.ContainsKey(input.prevout))
            {
                throw new Exception("No such input");
            }

            var outItem = inputs[input.prevout];

            return new CTxOut(outItem.nValue, outItem.scriptPubKey);
        }

        /// <summary>
        /// Calculate coin*age. 
        /// 
        /// Note, only those coins meeting minimum age requirement counts.
        /// </summary>
        /// <param name="inputs">Inputs set.</param>
        /// <param name="nCoinAge">Coin age calculation result.</param>
        /// <returns>Result</returns>
        public bool GetCoinAge(ref Dictionary<COutPoint, TxOutItem> inputs, out long nCoinAge)
        {
            BigInteger bnCentSecond = 0;  // coin age in the unit of cent-seconds
            nCoinAge = 0;

            if (IsCoinBase)
            {
                // Nothing spent by coinbase, coinage is always zero.
                return true;
            }

            for( var i = 0; i<vin.Length; i++)
            {
                var prevout = vin[i].prevout;
                Contract.Assert(inputs.ContainsKey(prevout));
                var input = inputs[prevout];

                CBlockStoreItem parentBlockCursor;
                var merkleItem = CBlockStore.Instance.GetMerkleCursor(input, out parentBlockCursor);

                if (merkleItem == null)
                {
                    return false; // Unable to find merkle node
                }

                if (nTime < merkleItem.nTime)
                {
                    return false;  // Transaction timestamp violation
                }

                if (parentBlockCursor.nTime + StakeModifier.nStakeMinAge > nTime)
                {
                    continue; // only count coins meeting min age requirement
                }

                long nValueIn = input.nValue;
                bnCentSecond += new BigInteger(nValueIn) * (nTime - merkleItem.nTime) / nCent;
            }

            BigInteger bnCoinDay = bnCentSecond * nCent / nCoin / (24 * 60 * 60);
            nCoinAge = (long)bnCoinDay;

            return true;
        }

        public long GetMinFee(uint nBlockSize, bool fAllowFree, MinFeeMode mode)
        {
            long nMinTxFee = CTransaction.nMinTxFee, nMinRelayTxFee = CTransaction.nMinRelayTxFee;
            uint nBytes = Size;

            if (IsCoinStake)
            {
                // Enforce 0.01 as minimum fee for old approach or coinstake
                nMinTxFee = nCent;
                nMinRelayTxFee = nCent;
            }

            // Base fee is either nMinTxFee or nMinRelayTxFee
            long nBaseFee = (mode == MinFeeMode.GMF_RELAY) ? nMinRelayTxFee : nMinTxFee;

            uint nNewBlockSize = nBlockSize + nBytes;
            long nMinFee = (1 + (long)nBytes / 1000) * nBaseFee;

            if (fAllowFree)
            {
                if (nBlockSize == 1)
                {
                    // Transactions under 1K are free
                    if (nBytes < 1000)
                        nMinFee = 0;
                }
                else
                {
                    // Free transaction area
                    if (nNewBlockSize < 27000)
                        nMinFee = 0;
                }
            }

            // To limit dust spam, require additional MIN_TX_FEE/MIN_RELAY_TX_FEE for
            //    each non empty output which is less than 0.01
            //
            // It's safe to ignore empty outputs here, because these inputs are allowed
            //     only for coinbase and coinstake transactions.
            foreach (var txout in vout)
            {
                if (txout.nValue < nCent && !txout.IsEmpty)
                {
                    nMinFee += nBaseFee;
                }
            }

            var nMaxBlockSizeGen = CBlock.nMaxBlockSize / 2;

            // Raise the price as the block approaches full
            if (nBlockSize != 1 && nNewBlockSize >= nMaxBlockSizeGen / 2)
            {
                if (nNewBlockSize >= nMaxBlockSizeGen)
                {
                    return nMaxMoney;
                }

                nMinFee *= nMaxBlockSizeGen / (nMaxBlockSizeGen - nNewBlockSize);
            }

            if (!MoneyRange(nMinFee))
            {
                nMinFee = nMaxMoney;
            }

            return nMinFee;
        }
    }
}
