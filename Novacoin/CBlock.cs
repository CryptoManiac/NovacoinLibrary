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
using System.Diagnostics.Contracts;
using System.IO;

namespace Novacoin
{
    [Serializable]
    public class BlockException : Exception
    {
        public BlockException()
        {
        }

        public BlockException(string message)
                : base(message)
        {
        }

        public BlockException(string message, Exception inner)
                : base(message, inner)
        {
        }
    }

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

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="b">CBlock instance.</param>
        public CBlock(CBlock b)
        {
            header = new CBlockHeader(b.header);
            vtx = new CTransaction[b.vtx.Length];

            for (int i = 0; i < b.vtx.Length; i++)
            {
                vtx[i] = new CTransaction(b.vtx[i]);
            }

            signature = new byte[b.signature.Length];
            b.signature.CopyTo(signature, 0);
        }

        /// <summary>
        /// Parse byte sequence and initialize new block instance
        /// </summary>
        /// <param name="blockBytes">Bytes sequence.</param>
		public CBlock (byte[] blockBytes)
		{
            try
            {
                var stream = new MemoryStream(blockBytes);
                var reader = new BinaryReader(stream);

                // Fill the block header fields
                header = new CBlockHeader();
                header.nVersion = reader.ReadUInt32();
                header.prevHash = new ScryptHash256(reader.ReadBytes(32));
                header.merkleRoot = new Hash256(reader.ReadBytes(32));
                header.nTime = reader.ReadUInt32();
                header.nBits = reader.ReadUInt32();
                header.nNonce = reader.ReadUInt32();                

                // Parse transactions list
                vtx = CTransaction.ReadTransactionsList(ref reader);

                // Read block signature
                signature = reader.ReadBytes((int)VarInt.ReadVarInt(ref reader));

                reader.Close();
            }
            catch (Exception e)
            {
                throw new BlockException("Deserialization failed", e);
            }
		}

        public CBlock()
        {
            // Initialize empty array of transactions. Please note that such 
            // configuration is not valid real block since it has to provide 
            // at least one transaction.
            vtx = new CTransaction[0];
        }

        public bool CheckBlock(bool fCheckPOW = true, bool fCheckMerkleRoot = true, bool fCheckSig = true)
        {
            var uniqueTX = new List<Hash256>(); // tx hashes
            uint nSigOps = 0; // total sigops

            // Basic sanity checkings
            if (vtx.Length == 0 || Size > 1000000)
            {
                return false;
            }

            bool fProofOfStake = IsProofOfStake;

            // First transaction must be coinbase, the rest must not be
            if (!vtx[0].IsCoinBase)
            {
                return false;
            }

            if (!vtx[0].CheckTransaction())
            {
                return false;
            }

            uniqueTX.Add(vtx[0].Hash);
            nSigOps += vtx[0].LegacySigOpCount;

            if (fProofOfStake)
            {
                // Proof-of-STake related checkings. Note that we know here that 1st transactions is coinstake. We don't need 
                //   check the type of 1st transaction because it's performed earlier by IsProofOfStake()

                // nNonce must be zero for proof-of-stake blocks
                if (header.nNonce != 0)
                {
                    return false;
                }

                // Coinbase output should be empty if proof-of-stake block
                if (vtx[0].vout.Length != 1 || !vtx[0].vout[0].IsEmpty)
                {
                    return false;
                }

                // Check coinstake timestamp
                if (header.nTime != vtx[1].nTime)
                {
                    return false;
                }

                // Check proof-of-stake block signature
                if (fCheckSig && !SignatureOK)
                {
                    return false;
                }

                if (!vtx[1].CheckTransaction())
                {
                    return false;
                }

                uniqueTX.Add(vtx[1].Hash);
                nSigOps += vtx[1].LegacySigOpCount;
            }
            else
            {
                // Check proof of work matches claimed amount
                if (fCheckPOW && !CheckProofOfWork(header.Hash, header.nBits))
                {
                    return false;
                }

                // Check timestamp
                if (header.nTime > NetUtils.FutureDrift(NetUtils.GetAdjustedTime()))
                {
                    return false;
                }

                // Check coinbase timestamp
                if (header.nTime < NetUtils.PastDrift(vtx[0].nTime))
                {
                    return false;
                }
            }

            // Iterate all transactions starting from second for proof-of-stake block 
            //    or first for proof-of-work block
            for (int i = fProofOfStake ? 2 : 1; i < vtx.Length; i++)
            {
                var tx = vtx[i];

                // Reject coinbase transactions at non-zero index
                if (tx.IsCoinBase)
                {
                    return false;
                }

                // Reject coinstake transactions at index != 1
                if (tx.IsCoinStake)
                {
                    return false;
                }

                // Check transaction timestamp
                if (header.nTime < tx.nTime)
                {
                    return false;
                }

                // Check transaction consistency
                if (!tx.CheckTransaction())
                {
                    return false;
                }

                // Add transaction hash into list of unique transaction IDs
                uniqueTX.Add(tx.Hash);

                // Calculate sigops count
                nSigOps += tx.LegacySigOpCount;
            }

            // Check for duplicate txids. 
            if (uniqueTX.Count != vtx.Length)
            {
                return false;
            }

            // Reject block if validation would consume too much resources.
            if (nSigOps > 50000)
            {
                return false;
            }

            // Check merkle root
            if (fCheckMerkleRoot && hashMerkleRoot != header.merkleRoot)
            {
                return false;
            }

            return true;
        }

        private bool CheckProofOfWork(ScryptHash256 hash, uint nBits)
        {
            // TODO: stub!

            return true;
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
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(b.header);
            writer.Write(VarInt.EncodeVarInt(b.vtx.LongLength));

            foreach (var tx in b.vtx)
            {
                writer.Write(tx);
            }

            writer.Write(VarInt.EncodeVarInt(b.signature.LongLength));
            writer.Write(b.signature);

            var resultBytes = stream.ToArray();

            writer.Close();

            return resultBytes;
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
                nOffset += vtx[i].Size;
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

