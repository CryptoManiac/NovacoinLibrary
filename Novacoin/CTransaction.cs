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
        /// Parse byte sequence and initialize new instance of CTransaction
        /// </summary>
        /// <param name="txBytes">Byte sequence</param>
		public CTransaction(byte[] txBytes)
        {
            var wBytes = new ByteQueue(txBytes);

            nVersion = BitConverter.ToUInt32(wBytes.Get(4), 0);
            nTime = BitConverter.ToUInt32(wBytes.Get(4), 0);

            int nInputs = (int)wBytes.GetVarInt();
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
                vout[nCurrentOutput].nValue = BitConverter.ToUInt64(wBytes.Get(8), 0);

                int nScriptPKLen = (int)wBytes.GetVarInt();
                vout[nCurrentOutput].scriptPubKey = new CScript(wBytes.Get(nScriptPKLen));
            }

            nLockTime = BitConverter.ToUInt32(wBytes.Get(4), 0);
        }

        /// <summary>
        /// Serialized size
        /// </summary>
        public int Size
        {
            get
            {
                int nSize = 12; // nVersion, nTime, nLockLime

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
            get { return Hash256.Compute256(this); }
        }

        /// <summary>
        /// A sequence of bytes, which corresponds to the current state of CTransaction.
        /// </summary>
        public static implicit operator byte[] (CTransaction tx)
        {
            var resultBytes = new List<byte>();

            resultBytes.AddRange(BitConverter.GetBytes(tx.nVersion));
            resultBytes.AddRange(BitConverter.GetBytes(tx.nTime));
            resultBytes.AddRange(VarInt.EncodeVarInt(tx.vin.LongLength));

            foreach (var input in tx.vin)
            {
                resultBytes.AddRange((byte[])input);
            }

            resultBytes.AddRange(VarInt.EncodeVarInt(tx.vout.LongLength));

            foreach (var output in tx.vout)
            {
                resultBytes.AddRange((byte[])output);
            }

            resultBytes.AddRange(BitConverter.GetBytes(tx.nLockTime));

            return resultBytes.ToArray();
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
