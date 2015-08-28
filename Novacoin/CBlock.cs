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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;

namespace Novacoin
{
	/// <summary>
	/// Represents the block. Block consists of header, transaction array and header signature.
	/// </summary>
	public class CBlock
	{
		/// <summary>
		/// Block header.
		/// </summary>
		public CBlockHeader header;

		/// <summary>
		/// Transactions array.
		/// </summary>
		public CTransaction[] vtx;

        /// <summary>
        /// Block header signature.
        /// </summary>
        public byte[] signature = new byte[0];

        public CBlock(CBlock b)
        {
            header = new CBlockHeader(b.header);
            vtx = new CTransaction[b.vtx.Length];

            for (int i = 0; i < b.vtx.Length; i++)
            {
                vtx[i] = new CTransaction(b.vtx[i]);
            }

            b.signature.CopyTo(signature, 0);
        }

        /// <summary>
        /// Parse byte sequence and initialize new block instance
        /// </summary>
        /// <param name="blockBytes"></param>
		public CBlock (byte[] blockBytes)
		{
            ByteQueue wBytes = new ByteQueue(blockBytes);

            // Fill the block header fields
            header = new CBlockHeader(wBytes.Get(80));

            // Parse transactions list
            vtx = CTransaction.ReadTransactionsList(ref wBytes);

            // Read block signature
            signature = wBytes.Get((int)wBytes.GetVarInt());
		}

        public CBlock()
        {
            // Initialize empty array of transactions. Please note that such 
            // configuration is not valid real block since it has to provide 
            // at least one transaction.
            vtx = new CTransaction[0];
        }

        /// <summary>
        /// Is this a Proof-of-Stake block?
        /// </summary>
        public bool IsProofOfStake
        {
            get
            {
                return (vtx.Length > 1 && vtx[1].IsCoinStake);
            }
        }

        /// <summary>
        /// Was this signed correctly?
        /// </summary>
        public bool SignatureOK
        {
            get
            {
                if (IsProofOfStake)
                {
                    if (signature.Length == 0)
                    {
                        return false; // No signature
                    }

                    txnouttype whichType;
                    IList<byte[]> solutions;

                    if (!ScriptCode.Solver(vtx[1].vout[1].scriptPubKey, out whichType, out solutions))
                    {
                        return false; // No solutions found
                    }

                    if (whichType == txnouttype.TX_PUBKEY)
                    {
                        CPubKey pubkey;

                        try
                        {
                            pubkey = new CPubKey(solutions[0]);
                        }
                        catch (Exception)
                        {
                            return false; // Error while loading public key
                        }

                        return pubkey.VerifySignature(header.Hash, signature);
                    }
                }
                else
                {
                    // Proof-of-Work blocks have no signature

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Get instance as sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public static implicit operator byte[] (CBlock b)
        {
            var r = new List<byte>();

            r.AddRange((byte[])b.header);
            r.AddRange(VarInt.EncodeVarInt(b.vtx.LongLength)); // transactions count

            foreach (var tx in b.vtx)
            {
                r.AddRange((byte[])tx);
            }

            r.AddRange(VarInt.EncodeVarInt(b.signature.LongLength));
            r.AddRange(b.signature);

            return r.ToArray();
        }

        /// <summary>
        /// Serialized size
        /// </summary>
        public int Size
        {
            get
            {
                int nSize = 80 + VarInt.GetEncodedSize(vtx.Length); // CBlockHeader + NumTx

                foreach (var tx in vtx)
                {
                    nSize += tx.Size;
                }

                nSize += VarInt.GetEncodedSize(signature.Length) + signature.Length;

                return nSize;
            }
        }

        /// <summary>
        /// Get transaction offset inside block.
        /// </summary>
        /// <param name="nTx">Transaction index.</param>
        /// <returns>Offset in bytes from the beginning of block header.</returns>
        public int GetTxOffset(int nTx)
        {
            Contract.Requires<ArgumentException>(nTx >= 0 && nTx < vtx.Length, "Transaction index you've specified is incorrect.");

            int nOffset = 80 + VarInt.GetEncodedSize(vtx.Length); // CBlockHeader + NumTx

            for (int i = 0; i < nTx; i++)
            {
                nOffset += vtx[nTx].Size;
            }

            return nOffset;
        }

        /// <summary>
        /// Merkle root
        /// </summary>
        public Hash256 hashMerkleRoot
        {
            get {
                
                var merkleTree = new List<byte>();

                foreach (var tx in vtx)
                {
                    merkleTree.AddRange(Hash256.ComputeRaw256(tx));
                }

                int levelOffset = 0;
                for (int nLevelSize = vtx.Length; nLevelSize > 1; nLevelSize = (nLevelSize + 1) / 2)
                {
                    for (int nLeft = 0; nLeft < nLevelSize; nLeft += 2)
                    {
                        int nRight = Math.Min(nLeft + 1, nLevelSize - 1);

                        var left = merkleTree.GetRange((levelOffset + nLeft) * 32, 32).ToArray();
                        var right = merkleTree.GetRange((levelOffset + nRight) * 32, 32).ToArray();

                        merkleTree.AddRange(Hash256.ComputeRaw256(ref left, ref right));
                    }
                    levelOffset += nLevelSize;
                }

                return (merkleTree.Count == 0) ? new Hash256() : new Hash256(merkleTree.GetRange(merkleTree.Count-32, 32).ToArray());
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("CBlock(\n header={0},\n", header.ToString());

            foreach(var tx in vtx)
            {
                sb.AppendFormat("{0}", tx.ToString());
            }

            if (IsProofOfStake)
            {
                sb.AppendFormat(", signature={0}, signatureOK={1}\n", Interop.ToHex(signature), SignatureOK);
            }

            sb.Append(")");
            
            return sb.ToString();
        }
	}
}

