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
using System.Numerics; // TODO: implement wrapper for BouncyCastle implementation of BigInteger and use it instead. This is necessary due to incompatibility of System.Numerics.BigInteger with OpenSSL.

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
        /// Maximum block size is 1Mb.
        /// </summary>
        public const uint nMaxBlockSize = 1000000;

        /// <summary>
        /// Sanity threshold for amount of sigops.
        /// </summary>
        public const uint nMaxSigOps = 20000;

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
                header = new CBlockHeader(ref reader);               

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
            var uniqueTX = new List<uint256>(); // tx hashes
            uint nSigOps = 0; // total sigops

            // Basic sanity checkings
            if (vtx.Length == 0 || Size > nMaxBlockSize)
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
                if (header.nTime > NetInfo.FutureDrift(NetInfo.GetAdjustedTime()))
                {
                    return false;
                }

                // Check coinbase timestamp
                if (header.nTime < NetInfo.PastDrift(vtx[0].nTime))
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
            if (nSigOps > nMaxSigOps)
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

        private bool CheckProofOfWork(uint256 hash, uint nBits)
        {
            uint256 nTarget = new uint256();
            nTarget.Compact = nBits;

            // Check range
            if (nTarget > NetInfo.nProofOfWorkLimit)
            {
                // nBits below minimum work
                return false; 
            }

            // Check proof of work matches claimed amount
            if (hash > nTarget)
            {
                //  hash doesn't match nBits
                return false;
            }

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
        public uint Size
        {
            get
            {
                uint nSize = 80 + VarInt.GetEncodedSize(vtx.Length); // CBlockHeader + NumTx

                foreach (var tx in vtx)
                {
                    nSize += tx.Size;
                }

                nSize += VarInt.GetEncodedSize(signature.Length) + (uint)signature.Length;

                return nSize;
            }
        }

        /// <summary>
        /// Get transaction offset inside block.
        /// </summary>
        /// <param name="nTx">Transaction index.</param>
        /// <returns>Offset in bytes from the beginning of block header.</returns>
        public uint GetTxOffset(int nTx)
        {
            Contract.Requires<ArgumentException>(nTx >= 0 && nTx < vtx.Length, "Transaction index you've specified is incorrect.");

            uint nOffset = 80 + VarInt.GetEncodedSize(vtx.Length); // CBlockHeader + NumTx

            for (int i = 0; i < nTx; i++)
            {
                nOffset += vtx[i].Size;
            }

            return nOffset;
        }

        /// <summary>
        /// Merkle root
        /// </summary>
        public uint256 hashMerkleRoot
        {
            get {
                
                var merkleTree = new List<byte>();

                foreach (var tx in vtx)
                {
                    merkleTree.AddRange(CryptoUtils.ComputeHash256(tx));
                }

                int levelOffset = 0;
                for (int nLevelSize = vtx.Length; nLevelSize > 1; nLevelSize = (nLevelSize + 1) / 2)
                {
                    for (int nLeft = 0; nLeft < nLevelSize; nLeft += 2)
                    {
                        int nRight = Math.Min(nLeft + 1, nLevelSize - 1);

                        var left = merkleTree.GetRange((levelOffset + nLeft) * 32, 32).ToArray();
                        var right = merkleTree.GetRange((levelOffset + nRight) * 32, 32).ToArray();

                        merkleTree.AddRange(CryptoUtils.ComputeHash256(ref left, ref right));
                    }
                    levelOffset += nLevelSize;
                }

                return (merkleTree.Count == 0) ? 0 : (uint256)merkleTree.GetRange(merkleTree.Count-32, 32).ToArray();
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

        /// <summary>
        /// Calculate proof-of-work reward.
        /// </summary>
        /// <param name="nBits">Packed difficulty representation.</param>
        /// <param name="nFees">Amount of fees.</param>
        /// <returns>Reward value.</returns>
        public static ulong GetProofOfWorkReward(uint nBits, ulong nFees)
        {
            // NovaCoin: subsidy is cut in half every 64x multiply of PoW difficulty
            // A reasonably continuous curve is used to avoid shock to market
            // (nSubsidyLimit / nSubsidy) ** 6 == bnProofOfWorkLimit / bnTarget
            //
            // Human readable form:
            //
            // nSubsidy = 100 / (diff ^ 1/6)
            //
            // Please note that we're using bisection to find an approximate solutuion

            BigInteger bnSubsidyLimit = NetInfo.nMaxMintProofOfWork;

            uint256 nTarget = 0;
            nTarget.Compact = nBits;

            BigInteger bnTarget = new BigInteger(nTarget);
            BigInteger bnTargetLimit = new BigInteger(NetInfo.nProofOfWorkLimit);

            BigInteger bnLowerBound = CTransaction.nCent;
            BigInteger bnUpperBound = bnSubsidyLimit;

            while (bnLowerBound + CTransaction.nCent <= bnUpperBound)
            {
                BigInteger bnMidValue = (bnLowerBound + bnUpperBound) / 2;
                if (bnMidValue * bnMidValue * bnMidValue * bnMidValue * bnMidValue * bnMidValue * bnTargetLimit > bnSubsidyLimit * bnSubsidyLimit * bnSubsidyLimit * bnSubsidyLimit * bnSubsidyLimit * bnSubsidyLimit * bnTarget)
                    bnUpperBound = bnMidValue;
                else
                    bnLowerBound = bnMidValue;
            }

            ulong nSubsidy = (ulong)bnUpperBound;
            nSubsidy = (nSubsidy / CTransaction.nCent) * CTransaction.nCent;


            return Math.Min(nSubsidy, NetInfo.nMaxMintProofOfWork) + nFees;
        }

        public static ulong GetProofOfStakeReward(ulong nCoinAge, uint nBits, uint nTime)
        {
            ulong nRewardCoinYear, nSubsidy, nSubsidyLimit = 10 * CTransaction.nCoin;

            if (nTime > NetInfo.nDynamicStakeRewardTime)
            {
                // Stage 2 of emission process is PoS-based. It will be active on mainNet since 20 Jun 2013.

                BigInteger bnRewardCoinYearLimit = NetInfo.nMaxMintProofOfStake; // Base stake mint rate, 100% year interest

                uint256 nTarget = 0;
                nTarget.Compact = nBits;

                BigInteger bnTarget = new BigInteger(nTarget);
                BigInteger bnTargetLimit = new BigInteger(NetInfo.GetProofOfStakeLimit(0, nTime));

                // NovaCoin: A reasonably continuous curve is used to avoid shock to market

                BigInteger bnLowerBound = CTransaction.nCent, // Lower interest bound is 1% per year
                    bnUpperBound = bnRewardCoinYearLimit, // Upper interest bound is 100% per year
                    bnMidPart, bnRewardPart;

                while (bnLowerBound + CTransaction.nCent <= bnUpperBound)
                {
                    BigInteger bnMidValue = (bnLowerBound + bnUpperBound) / 2;
                    if (nTime < NetInfo.nStakeCurveSwitchTime)
                    {
                        //
                        // Until 20 Oct 2013: reward for coin-year is cut in half every 64x multiply of PoS difficulty
                        //
                        // (nRewardCoinYearLimit / nRewardCoinYear) ** 6 == bnProofOfStakeLimit / bnTarget
                        //
                        // Human readable form: nRewardCoinYear = 1 / (posdiff ^ 1/6)
                        //

                        bnMidPart = bnMidValue * bnMidValue * bnMidValue * bnMidValue * bnMidValue * bnMidValue;
                        bnRewardPart = bnRewardCoinYearLimit * bnRewardCoinYearLimit * bnRewardCoinYearLimit * bnRewardCoinYearLimit * bnRewardCoinYearLimit * bnRewardCoinYearLimit;
                    }
                    else
                    {
                        //
                        // Since 20 Oct 2013: reward for coin-year is cut in half every 8x multiply of PoS difficulty
                        //
                        // (nRewardCoinYearLimit / nRewardCoinYear) ** 3 == bnProofOfStakeLimit / bnTarget
                        //
                        // Human readable form: nRewardCoinYear = 1 / (posdiff ^ 1/3)
                        //

                        bnMidPart = bnMidValue * bnMidValue * bnMidValue;
                        bnRewardPart = bnRewardCoinYearLimit * bnRewardCoinYearLimit * bnRewardCoinYearLimit;
                    }

                    if (bnMidPart * bnTargetLimit > bnRewardPart * bnTarget)
                        bnUpperBound = bnMidValue;
                    else
                        bnLowerBound = bnMidValue;
                }

                nRewardCoinYear = (ulong)bnUpperBound;
                nRewardCoinYear = Math.Min((nRewardCoinYear / CTransaction.nCent) * CTransaction.nCent, NetInfo.nMaxMintProofOfStake);
            }
            else
            {
                // Old creation amount per coin-year, 5% fixed stake mint rate
                nRewardCoinYear = 5 * CTransaction.nCent;
            }

            nSubsidy = nCoinAge * nRewardCoinYear * 33 / (365 * 33 + 8);

            // Set reasonable reward limit for large inputs since 20 Oct 2013
            //
            // This will stimulate large holders to use smaller inputs, that's good for the network protection
            if (NetInfo.nStakeCurveSwitchTime < nTime)
            {
                nSubsidy = Math.Min(nSubsidy, nSubsidyLimit);
            }

            return nSubsidy;
        }
    }
}

