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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Novacoin
{
    /// <summary>
    /// Stake modifier calculation. Doesn't work properly, for now.
    /// </summary>
    public static class StakeModifier
    {
        /// <summary>
        /// 30 days as zero time weight
        /// </summary>
        public const uint nStakeMinAge = 60 * 60 * 24 * 30;

        /// <summary>
        /// 90 days as full weight
        /// </summary>
        public const uint nStakeMaxAge = 60 * 60 * 24 * 90;

        /// <summary>
        /// 10-minute stakes spacing
        /// </summary>
        public const uint nStakeTargetSpacing = 10 * 60;

        /// <summary>
        /// Time to elapse before new modifier is computed
        /// </summary>
        public const uint nModifierInterval = 6 * 60 * 60;

        /// <summary>
        /// Ratio of group interval length between the last group and the first group
        /// </summary>
        public const int nModifierIntervalRatio = 3;

        /// <summary>
        /// Protocol switch time for fixed kernel modifier interval
        /// 
        /// Mon, 20 Oct 2014 00:00:00 GMT
        /// </summary>
        public const uint nModifierSwitchTime = 1413763200;

        /// <summary>
        /// Whether the given block is subject to new modifier protocol
        /// </summary>
        /// <param name="nTimeBlock">Block timestamp</param>
        /// <returns>Result</returns>
        private static bool IsFixedModifierInterval(uint nTimeBlock)
        {
            return (nTimeBlock >= nModifierSwitchTime);
        }

        /// <summary>
        /// Get the last stake modifier and its generation time from a given block
        /// </summary>
        /// <param name="cursor">Block cursor</param>
        /// <param name="nStakeModifier">Stake modifier (ref)</param>
        /// <param name="nModifierTime">Stake modifier generation time (ref)</param>
        /// <returns></returns>
        private static bool GetLastStakeModifier(CBlockStoreItem cursor, ref long nStakeModifier, ref uint nModifierTime)
        {
            if (cursor == null)
            {
                return false;
            }

            while (cursor != null && cursor.prev != null && !cursor.GeneratedStakeModifier)
            {
                cursor = cursor.prev;
            }

            if (!cursor.GeneratedStakeModifier)
            {
                return false;  // no generation at genesis block
            }

            nStakeModifier = cursor.nStakeModifier;
            nModifierTime = cursor.nTime;

            return true;
        }

        /// <summary>
        /// Get selection interval section (in seconds)
        /// </summary>
        /// <param name="nSection"></param>
        /// <returns></returns>
        private static long GetStakeModifierSelectionIntervalSection(int nSection)
        {
            Contract.Assert(nSection >= 0 && nSection < 64);
            return (nModifierInterval * 63 / (63 + ((63 - nSection) * (nModifierIntervalRatio - 1))));
        }

        /// <summary>
        /// Get stake modifier selection interval (in seconds)
        /// </summary>
        /// <returns></returns>
        private static long GetStakeModifierSelectionInterval()
        {
            long nSelectionInterval = 0;
            for (int nSection = 0; nSection < 64; nSection++)
                nSelectionInterval += GetStakeModifierSelectionIntervalSection(nSection);
            return nSelectionInterval;
        }


        /// <summary>
        /// Select a block from the candidate blocks in vSortedByTimestamp, excluding 
        /// already selected blocks in vSelectedBlocks, and with timestamp 
        /// up to nSelectionIntervalStop.
        /// </summary>
        /// <param name="sortedByTimestamp"></param>
        /// <param name="mapSelectedBlocks"></param>
        /// <param name="nSelectionIntervalStop">Upper boundary for timestamp value.</param>
        /// <param name="nStakeModifierPrev">Previous value of stake modifier.</param>
        /// <param name="selectedCursor">Selection result.</param>
        /// <returns></returns>
        private static bool SelectBlockFromCandidates(List<Tuple<uint, uint256>> sortedByTimestamp, Dictionary<uint256, CBlockStoreItem> mapSelectedBlocks, long nSelectionIntervalStop, long nStakeModifierPrev, ref CBlockStoreItem selectedCursor)
        {
            bool fSelected = false;
            uint256 hashBest = 0;
            selectedCursor = null;
            foreach (var item in sortedByTimestamp)
            {
                CBlockStoreItem cursor = CBlockStore.Instance.GetMapCursor(item.Item2);

                if (cursor == null)
                {
                    return false; // Failed to find block index for candidate block
                }

                if (fSelected && cursor.nTime > nSelectionIntervalStop)
                {
                    break;
                }

                uint256 selectedBlockHash = cursor.Hash;

                if (mapSelectedBlocks.Count(pair => pair.Key == selectedBlockHash) > 0)
                {
                    continue;
                }

                // compute the selection hash by hashing its proof-hash and the
                // previous proof-of-stake modifier

                var hashProof = cursor.IsProofOfStake ? (uint256)cursor.hashProofOfStake : selectedBlockHash;
                uint256 hashSelection;

                var stream = new MemoryStream();
                var bw = new BinaryWriter(stream);

                bw.Write(hashProof);
                bw.Write(nStakeModifierPrev);
                hashSelection = CryptoUtils.ComputeHash256(stream.ToArray());
                bw.Close();

                // the selection hash is divided by 2**32 so that proof-of-stake block
                // is always favored over proof-of-work block. this is to preserve
                // the energy efficiency property
                if (cursor.IsProofOfStake)
                    hashSelection >>= 32;
                if (fSelected && hashSelection < hashBest)
                {
                    hashBest = hashSelection;
                    selectedCursor = cursor;
                }
                else if (!fSelected)
                {
                    fSelected = true;
                    hashBest = hashSelection;
                    selectedCursor = cursor;
                }
            }

            return fSelected;
        }

        /// <summary>
        /// Stake Modifier (hash modifier of proof-of-stake):
        /// The purpose of stake modifier is to prevent a txout (coin) owner from
        /// computing future proof-of-stake generated by this txout at the time
        /// of transaction confirmation. To meet kernel protocol, the txout
        /// must hash with a future stake modifier to generate the proof.
        /// Stake modifier consists of bits each of which is contributed from a
        /// selected block of a given block group in the past.
        /// The selection of a block is based on a hash of the block's proof-hash and
        /// the previous stake modifier.
        /// Stake modifier is recomputed at a fixed time interval instead of every 
        /// block. This is to make it difficult for an attacker to gain control of
        /// additional bits in the stake modifier, even after generating a chain of
        /// blocks.
        /// </summary>
        public static bool ComputeNextStakeModifier(CBlockStoreItem cursorCurrent, ref long nStakeModifier, ref bool fGeneratedStakeModifier)
        {
            nStakeModifier = 0;
            fGeneratedStakeModifier = false;
            CBlockStoreItem cursorPrev = cursorCurrent.prev;
            if (cursorPrev == null)
            {
                fGeneratedStakeModifier = true;
                return true;  // genesis block's modifier is 0
            }

            // First find current stake modifier and its generation block time
            // if it's not old enough, return the same stake modifier
            uint nModifierTime = 0;
            if (!GetLastStakeModifier(cursorPrev, ref nStakeModifier, ref nModifierTime))
                return false; // Unable to get last modifier
            if (nModifierTime / nModifierInterval >= cursorPrev.nTime / nModifierInterval)
            {
                // no new interval keep current modifier
                return true;
            }
            if (nModifierTime / nModifierInterval >= cursorCurrent.nTime / nModifierInterval)
            {
                // fixed interval protocol requires current block timestamp also be in a different modifier interval
                if (IsFixedModifierInterval(cursorCurrent.nTime))
                {
                    // no new interval keep current modifier
                    return true;
                }
                else
                {
                    // old modifier not meeting fixed modifier interval
                }
            }

            // Sort candidate blocks by timestamp
            List<Tuple<uint, uint256>> vSortedByTimestamp = new List<Tuple<uint, uint256>>();
            // vSortedByTimestamp.reserve(64 * nModifierInterval / nStakeTargetSpacing);

            long nSelectionInterval = GetStakeModifierSelectionInterval();
            long nSelectionIntervalStart = (cursorPrev.nTime / nModifierInterval) * nModifierInterval - nSelectionInterval;

            CBlockStoreItem cursor = cursorPrev;
            while (cursor != null && cursor.nTime >= nSelectionIntervalStart)
            {
                vSortedByTimestamp.Add(new Tuple<uint, uint256>(cursor.nTime, cursor.Hash));
                cursor = cursor.prev;
            }
            uint nHeightFirstCandidate = cursor != null ? (cursor.nHeight + 1) : 0;
            vSortedByTimestamp.Reverse();
            vSortedByTimestamp.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            // Select 64 blocks from candidate blocks to generate stake modifier
            long nStakeModifierNew = 0;
            long nSelectionIntervalStop = nSelectionIntervalStart;
            Dictionary<uint256, CBlockStoreItem> mapSelectedBlocks = new Dictionary<uint256, CBlockStoreItem>();
            for (int nRound = 0; nRound < Math.Min(64, (int)vSortedByTimestamp.Count); nRound++)
            {
                // add an interval section to the current selection round
                nSelectionIntervalStop += GetStakeModifierSelectionIntervalSection(nRound);
                // select a block from the candidates of current round
                if (!SelectBlockFromCandidates(vSortedByTimestamp, mapSelectedBlocks, nSelectionIntervalStop, nStakeModifier, ref cursor))
                {
                    return false; // unable to select block
                }

                // write the entropy bit of the selected block
                nStakeModifierNew |= (((long)cursor.StakeEntropyBit) << nRound);

                // add the selected block from candidates to selected list
                mapSelectedBlocks.Add(cursor.Hash, cursor);
            }

            nStakeModifier = nStakeModifierNew;
            fGeneratedStakeModifier = true;
            return true;
        }

        /// <summary>
        /// The stake modifier used to hash for a stake kernel is chosen as the stake
        /// modifier about a selection interval later than the coin generating the kernel
        /// </summary>
        private static bool GetKernelStakeModifier(ref uint256 hashBlockFrom, out long nStakeModifier, out uint nStakeModifierHeight, out uint nStakeModifierTime)
        {
            nStakeModifier = 0;
            nStakeModifierTime = 0;
            nStakeModifierHeight = 0;

            var cursorFrom = CBlockStore.Instance.GetMapCursor(hashBlockFrom);
            if (cursorFrom == null)
            {
                return false; // Block not indexed
            }

            nStakeModifierHeight = cursorFrom.nHeight;
            nStakeModifierTime = cursorFrom.nTime;

            long nStakeModifierSelectionInterval = GetStakeModifierSelectionInterval();
            CBlockStoreItem cursor = cursorFrom;

            // loop to find the stake modifier later by a selection interval
            while (nStakeModifierTime < cursorFrom.nTime + nStakeModifierSelectionInterval)
            {
                if (cursor.next == null)
                {
                    // reached best block; may happen if node is behind on block chain
                    return false;
                }
                cursor = cursor.next;
                if (cursor.GeneratedStakeModifier)
                {
                    nStakeModifierHeight = cursor.nHeight;
                    nStakeModifierTime = cursor.nTime;
                }
            }
            nStakeModifier = cursor.nStakeModifier;

            return true;
        }

        private static bool GetKernelStakeModifier(ref uint256 hashBlockFrom, out long nStakeModifier)
        {
            uint nStakeModifierHeight = 0;
            uint nStakeModifierTime = 0;

            return GetKernelStakeModifier(ref hashBlockFrom, out nStakeModifier, out nStakeModifierHeight, out nStakeModifierTime);
        }

        public static bool CheckStakeKernelHash(uint nBits, uint256 hashBlockFrom, uint nTimeBlockFrom, uint nTxPrevOffset, CTransaction txPrev, COutPoint prevout, uint nTimeTx, out uint256 hashProofOfStake, out uint256 targetProofOfStake)
        {
            hashProofOfStake = targetProofOfStake = 0;

            if (nTimeTx < txPrev.nTime)
            {
                return false; // Transaction timestamp violation
            }

            if (nTimeBlockFrom + nStakeMinAge > nTimeTx) // Min age requirement
            {
                return false; // Min age violation
            }

            uint256 nTargetPerCoinDay = 0;
            nTargetPerCoinDay.Compact = nBits;

            long nValueIn = txPrev.vout[prevout.n].nValue;
            uint256 nCoinDayWeight = new uint256((ulong)nValueIn) * GetWeight(txPrev.nTime, nTimeTx) / CTransaction.nCoin / (24 * 60 * 60);

            targetProofOfStake = nCoinDayWeight * nTargetPerCoinDay;

            // Calculate hash
            long nStakeModifier;
            if (!GetKernelStakeModifier(ref hashBlockFrom, out nStakeModifier))
            {
                return false;
            }

            var stream = new MemoryStream();
            var bw = new BinaryWriter(stream);

            // Coinstake kernel (input 0) must meet the formula
            //
            //     hash(nStakeModifier + txPrev.block.nTime + txPrev.offset + txPrev.nTime + txPrev.vout.n + nTime) < bnTarget * nCoinDayWeight
            //
            // This ensures that the chance of getting a coinstake is proportional to the
            // amount of coin age one owns.
            // 
            // Note that "+" is not arithmetic operation here, this means concatenation of byte arrays.
            //
            // Check https://github.com/novacoin-project/novacoin/wiki/Kernel for additional information.

            bw.Write(nStakeModifier);
            bw.Write(nTimeBlockFrom);
            bw.Write(nTxPrevOffset);
            bw.Write(txPrev.nTime);
            bw.Write(prevout.n);
            bw.Write(nTimeTx);

            hashProofOfStake = CryptoUtils.ComputeHash256(stream.ToArray());
            bw.Close();

            // Now check if proof-of-stake hash meets target protocol
            return targetProofOfStake >= hashProofOfStake;
        }

        // Get time weight using supplied timestamps
        static ulong GetWeight(ulong nIntervalBeginning, ulong nIntervalEnd)
        {
            // Kernel hash weight starts from 0 at the 30-day min age
            // this change increases active coins participating the hash and helps
            // to secure the network when proof-of-stake difficulty is low
            //
            // Maximum TimeWeight is 90 days.

            return Math.Min(nIntervalEnd - nIntervalBeginning - nStakeMinAge, nStakeMaxAge);
        }

        /// <summary>
        /// Calculate stake modifier checksum.
        /// </summary>
        /// <param name="cursorBlock">Block cursor.</param>
        /// <returns>Checksum value.</returns>
        public static uint GetModifierChecksum(CBlockStoreItem cursorBlock)
        {
            Contract.Assert(cursorBlock.prev != null || (uint256)cursorBlock.Hash == NetInfo.nHashGenesisBlock);

            var stream = new MemoryStream();
            var bw = new BinaryWriter(stream);

            // Kernel hash for proof-of-stake or zero bytes array for proof-of-work
            byte[] proofBytes = cursorBlock.IsProofOfStake ? cursorBlock.hashProofOfStake : new byte[32];

            // Hash previous checksum with flags, hashProofOfStake and nStakeModifier
            if (cursorBlock.prev != null)
            {
                bw.Write(cursorBlock.prev.nStakeModifierChecksum);
            }

            bw.Write((uint)cursorBlock.BlockTypeFlag);
            bw.Write(proofBytes);
            bw.Write(cursorBlock.nStakeModifier);

            uint256 hashChecksum = CryptoUtils.ComputeHash256(stream.ToArray());
            bw.Close();

            hashChecksum >>= (256 - 32);

            return hashChecksum.Low32;
        }

        public static bool CheckProofOfStake(CTransaction tx, uint nBits, out uint256 hashProofOfStake, out uint256 targetProofOfStake)
        {
            hashProofOfStake = targetProofOfStake = 0;

            if (!tx.IsCoinStake)
            {
                return false; // called on non-coinstake
            }

            // Kernel (input 0) must match the stake hash target per coin age (nBits)
            CTxIn txin = tx.vin[0];

            // Read block header

            CBlock block;
            long nBlockPos;
            if (!CBlockStore.Instance.GetBlockByTransactionID(txin.prevout.hash, out block, out nBlockPos))
            {
                return false; // unable to read block of previous transaction
            }

            long nTxPos = 0;
            CTransaction txPrev = null;

            // Iterate through vtx array
            var nTxPrevIndex = Array.FindIndex(block.vtx, txItem => txItem.Hash == txin.prevout.hash);

            if (nTxPrevIndex == -1)
            {
                return false; // No such transaction found in the block
            }

            txPrev = block.vtx[nTxPrevIndex];
            nTxPos = nBlockPos + block.GetTxOffset(nTxPrevIndex);

            if (!ScriptCode.VerifyScript(txin.scriptSig, txPrev.vout[txin.prevout.n].scriptPubKey, tx, 0, (int)scriptflag.SCRIPT_VERIFY_P2SH, 0))
            {
                return false; // vin[0] signature check failed
            }

            if (!CheckStakeKernelHash(nBits, block.header.Hash, block.header.nTime, (uint)(nTxPos - nBlockPos), txPrev, txin.prevout, tx.nTime, out hashProofOfStake, out targetProofOfStake))
            {
                return false; // check kernel failed on coinstake 
            }

            return true;
        }
    }
}
