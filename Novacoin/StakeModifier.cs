﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class StakeModifier
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
        internal const uint nModifierSwitchTime = 1413763200;

        /// <summary>
        /// Whether the given block is subject to new modifier protocol
        /// </summary>
        /// <param name="nTimeBlock">Block timestamp</param>
        /// <returns>Result</returns>
        internal static bool IsFixedModifierInterval(uint nTimeBlock)
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
        internal static bool GetLastStakeModifier(CBlockStoreItem cursor, ref long nStakeModifier, ref uint nModifierTime)
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
        internal static long GetStakeModifierSelectionIntervalSection(int nSection)
        {
            Contract.Assert(nSection >= 0 && nSection < 64);
            return (nModifierInterval * 63 / (63 + ((63 - nSection) * (nModifierIntervalRatio - 1))));
        }

        /// <summary>
        /// Get stake modifier selection interval (in seconds)
        /// </summary>
        /// <returns></returns>
        static long GetStakeModifierSelectionInterval()
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
        static bool SelectBlockFromCandidates(List<Tuple<uint, uint256>> sortedByTimestamp, Dictionary<uint256, CBlockStoreItem> mapSelectedBlocks, long nSelectionIntervalStop, long nStakeModifierPrev, ref CBlockStoreItem selectedCursor)
        {
            bool fSelected = false;
            uint256 hashBest = 0;
            selectedCursor = null;
            foreach (var item in sortedByTimestamp)
            {
                CBlockStoreItem cursor = CBlockStore.Instance.GetCursor(item.Item2);

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

                uint256 hashProof = cursor.IsProofOfStake ? (uint256)cursor.hashProofOfStake : selectedBlockHash;
                uint256 hashSelection;

                var s = new MemoryStream();
                var writer = new BinaryWriter(s);

                writer.Write(hashProof);
                writer.Write(nStakeModifierPrev);
                hashSelection = CryptoUtils.ComputeHash256(s.ToArray());
                writer.Close();

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
        bool ComputeNextStakeModifier(CBlockStoreItem cursorCurrent, ref long nStakeModifier, ref bool fGeneratedStakeModifier)
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
                nStakeModifierNew |= ((cursor.StakeEntropyBit) << nRound);

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
        static bool GetKernelStakeModifier(uint256 hashBlockFrom, ref long nStakeModifier, ref uint nStakeModifierHeight, ref uint nStakeModifierTime)
        {
            nStakeModifier = 0;
            var cursorFrom = CBlockStore.Instance.GetCursor(hashBlockFrom);
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

        bool GetKernelStakeModifier(uint256 hashBlockFrom, ref long nStakeModifier)
        {
            uint nStakeModifierHeight = 0;
            uint nStakeModifierTime = 0;

            return GetKernelStakeModifier(hashBlockFrom, ref nStakeModifier, ref nStakeModifierHeight, ref nStakeModifierTime);
        }

        bool CheckStakeKernelHash(uint nBits, CBlock blockFrom, uint nTxPrevOffset, CTransaction txPrev, COutPoint prevout, uint nTimeTx, ref uint256 hashProofOfStake, ref uint256 targetProofOfStake)
        {
            if (nTimeTx < txPrev.nTime)
            {
                return false; // Transaction timestamp violation
            }

            uint nTimeBlockFrom = blockFrom.header.nTime;
            if (nTimeBlockFrom + nStakeMinAge > nTimeTx) // Min age requirement
            {
                return false; // Min age violation
            }

            uint256 nTargetPerCoinDay = new uint256();
            nTargetPerCoinDay.Compact = nBits;

            ulong nValueIn = txPrev.vout[prevout.n].nValue;
            uint256 hashBlockFrom = blockFrom.header.Hash;
            BigInteger bnCoinDayWeight = (long)nValueIn * GetWeight(txPrev.nTime, nTimeTx) / (long)CTransaction.nCoin / (24 * 60 * 60);

            targetProofOfStake = (bnCoinDayWeight * new BigInteger(nTargetPerCoinDay)).ToByteArray();

            // Calculate hash
            long nStakeModifier = 0;
            uint nStakeModifierHeight = 0;
            uint nStakeModifierTime = 0;
            if (!GetKernelStakeModifier(hashBlockFrom, ref nStakeModifier, ref nStakeModifierHeight, ref nStakeModifierTime))
            {
                return false;
            }

            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(nStakeModifier);
            w.Write(nTimeBlockFrom);
            w.Write(nTxPrevOffset);
            w.Write(txPrev.nTime);
            w.Write(prevout.n);
            w.Write(nTimeTx);

            hashProofOfStake = CryptoUtils.ComputeHash256(s.ToArray());
            w.Close();

            // Now check if proof-of-stake hash meets target protocol
            if (hashProofOfStake > targetProofOfStake)
                return false;

            return true;
        }

        // Get time weight using supplied timestamps
        static long GetWeight(long nIntervalBeginning, long nIntervalEnd)
        {
            // Kernel hash weight starts from 0 at the 30-day min age
            // this change increases active coins participating the hash and helps
            // to secure the network when proof-of-stake difficulty is low
            //
            // Maximum TimeWeight is 90 days.

            return Math.Min(nIntervalEnd - nIntervalBeginning - nStakeMinAge, nStakeMaxAge);
        }
    }
}