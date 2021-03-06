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
using System.IO;
using System.Collections.Concurrent;

using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Novacoin
{
    public class CBlockStore : IDisposable
    {
        public const uint nMagicNumber = 0xe5e9e8e4;

        private bool disposed = false;
        private object LockObj = new object();

        /// <summary>
        /// SQLite connection object.
        /// </summary>
        private SQLiteConnection dbConn;

        /// <summary>
        /// Current SQLite platform
        /// </summary>
        private ISQLitePlatform dbPlatform;

        /// <summary>
        /// Block file.
        /// </summary>
        private string strBlockFile;

        /// <summary>
        /// Index database file.
        /// </summary>
        private string strDbFile;

        /// <summary>
        /// Map of block tree nodes.
        /// 
        /// blockHash => CBlockStoreItem
        /// </summary>
        private ConcurrentDictionary<uint256, CBlockStoreItem> blockMap = new ConcurrentDictionary<uint256, CBlockStoreItem>();

        /// <summary>
        /// Orphaned blocks map.
        /// </summary>
        private ConcurrentDictionary<uint256, CBlock> orphanMap = new ConcurrentDictionary<uint256, CBlock>();
        private ConcurrentDictionary<uint256, CBlock> orphanMapByPrev = new ConcurrentDictionary<uint256, CBlock>();

        /// <summary>
        /// Unconfirmed transactions.
        /// 
        /// TxID => Transaction
        /// </summary>
        private ConcurrentDictionary<uint256, CTransaction> mapUnconfirmedTx = new ConcurrentDictionary<uint256, CTransaction>();

        /// <summary>
        /// Map of the proof-of-stake hashes. This is necessary for stake duplication checks.
        /// </summary>
        private ConcurrentDictionary<uint256, uint256> mapProofOfStake = new ConcurrentDictionary<uint256, uint256>();


        private ConcurrentDictionary<COutPoint, uint> mapStakeSeen = new ConcurrentDictionary<COutPoint, uint>();
        private ConcurrentDictionary<Tuple<COutPoint, uint>, uint256> mapStakeSeenOrphan = new ConcurrentDictionary<Tuple<COutPoint, uint>, uint256>();


        /// <summary>
        /// Copy of chain state object.
        /// </summary>
        private ChainState ChainParams;

        /// <summary>
        /// Cursor which is pointing us to the end of best chain.
        /// </summary>
        private CBlockStoreItem bestBlockCursor = null;

        /// <summary>
        /// Cursor which is always pointing us to genesis block.
        /// </summary>
        private CBlockStoreItem genesisBlockCursor = null;

        /// <summary>
        /// Current and the only instance of block storage manager. Should be a property with private setter though it's enough for the beginning.
        /// </summary>
        public static CBlockStore Instance = null;

        /// <summary>
        /// Block file stream with read/write access
        /// </summary>
        private Stream fStreamReadWrite;
        private uint nTimeBestReceived;
        private int nTransactionsUpdated;

        /// <summary>
        /// Init the block storage manager.
        /// </summary>
        /// <param name="IndexDB">Path to index database</param>
        /// <param name="BlockFile">Path to block file</param>
        public CBlockStore(string IndexDB = "blockstore.dat", string BlockFile = "blk0001.dat")
        {
            strDbFile = IndexDB;
            strBlockFile = BlockFile;

            bool firstInit = !File.Exists(strDbFile);
            dbPlatform = new SQLitePlatformGeneric();
            dbConn = new SQLiteConnection(dbPlatform, strDbFile);

            fStreamReadWrite = File.Open(strBlockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Instance = this;

            if (firstInit)
            {
                lock (LockObj)
                {
                    // Create tables
                    dbConn.CreateTable<CBlockStoreItem>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<CMerkleNode>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<TxOutItem>(CreateFlags.ImplicitPK);
                    dbConn.CreateTable<ChainState>(CreateFlags.AutoIncPK);

                    ChainParams = new ChainState()
                    {
                        nBestChainTrust = 0,
                        nBestHeight = 0,
                        nHashBestChain = 0
                    };

                    dbConn.Insert(ChainParams);

                    var genesisBlock = new CBlock(
                        Interop.HexToArray(
                            "01000000" + // nVersion=1
                            "0000000000000000000000000000000000000000000000000000000000000000" + // prevhash is zero
                            "7b0502ad2f9f675528183f83d6385794fbcaa914e6d385c6cb1d866a3b3bb34c" + // merkle root
                            "398e1151" + // nTime=1360105017
                            "ffff0f1e" + // nBits=0x1e0fffff
                            "d3091800" + // nNonce=1575379
                            "01" +       // nTxCount=1
                            "01000000" + // nVersion=1
                            "398e1151" + // nTime=1360105017
                            "01" +       // nInputs=1
                            "0000000000000000000000000000000000000000000000000000000000000000" + // input txid is zero
                            "ffffffff" + // n=uint.maxValue
                            "4d" +       // scriptSigLen=77
                            "04ffff001d020f274468747470733a2f2f626974636f696e74616c6b2e6f72672f696e6465782e7068703f746f7069633d3133343137392e6d736731353032313936236d736731353032313936" + // scriptSig
                            "ffffffff" + // nSequence=uint.maxValue
                            "01" +       // nOutputs=1
                            "0000000000000000" + // nValue=0
                            "00" +       // scriptPubkeyLen=0
                            "00000000" + // nLockTime=0
                            "00"         // sigLen=0
                    ));

                    // Write block to file.
                    var rootCursor = new CBlockStoreItem()
                    {
                        nHeight = 0
                    };

                    rootCursor.FillHeader(genesisBlock.header);

                    if (!AddItemToIndex(ref rootCursor, ref genesisBlock))
                    {
                        throw new Exception("Unable to write genesis block");
                    }
                }
            }
            else
            {
                var blockTreeItems = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] order by [ItemId] asc");

                // Init list of block items
                foreach (var item in blockTreeItems)
                {
                    item.nStakeModifierChecksum = StakeModifier.GetModifierChecksum(item);

                    blockMap.TryAdd(item.Hash, item);

                    if (item.IsProofOfStake)
                    {
                        // build mapStakeSeen
                        mapStakeSeen.TryAdd(item.prevoutStake, item.nStakeTime);
                    }
                }

                // Load data about the top node.
                ChainParams = dbConn.Table<ChainState>().First();

                genesisBlockCursor = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])NetInfo.nHashGenesisBlock).First();
                bestBlockCursor = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", ChainParams.HashBestChain).First();
            }
        }

        public bool GetTxOutCursor(COutPoint outpoint, out TxOutItem txOutCursor)
        {
            var queryResults = dbConn.Query<TxOutItem>("select o.* from [Outputs] o left join [MerkleNodes] m on (m.nMerkleNodeID = o.nMerkleNodeID) where m.[TransactionHash] = ?", (byte[])outpoint.hash);

            if (queryResults.Count == 1)
            {
                txOutCursor = queryResults[0];

                return true;
            }

            // Tx not found

            txOutCursor = null;

            return false;
        }
        
        public bool FetchInputs(ref CTransaction tx, ref Dictionary<COutPoint, TxOutItem> queued, ref Dictionary<COutPoint, TxOutItem> inputs, bool IsBlock, out bool Invalid)
        {
            Invalid = false;

            if (tx.IsCoinBase)
            {
                // Coinbase transactions have no inputs to fetch.
                return true;
            }

            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append("select o.*, m.[TransactionHash] from [Outputs] o left join [MerkleNodes] m on (m.[nMerkleNodeID] = o.[nMerkleNodeID]) where ");

            for (var i = 0; i < tx.vin.Length; i++)
            {
                queryBuilder.AppendFormat(" {0} (m.[TransactionHash] = x'{1}' and o.[OutputNumber] = x'{2}')",
                    (i > 0 ? "or" : string.Empty), Interop.ToHex(tx.vin[i].prevout.hash),
                    Interop.ToHex(VarInt.EncodeVarInt(tx.vin[i].prevout.n)
                ));
            }

            var queryResults = dbConn.Query<InputsJoin>(queryBuilder.ToString());

            foreach (var item in queryResults)
            {
                if (item.IsSpent)
                {
                    return false; // Already spent
                }

                var inputsKey = new COutPoint(item.TransactionHash, item.nOut);

                // Add output data to dictionary
                inputs.Add(inputsKey, item.getTxOutItem());
            }

            if (queryResults.Count < tx.vin.Length)
            {
                if (IsBlock)
                {
                    // It seems that some transactions are being spent in the same block.

                    foreach (var txin in tx.vin)
                    {
                        var outPoint = txin.prevout;

                        if (inputs.ContainsKey(outPoint))
                        {
                            continue; // We have already seen this input.
                        }

                        if (!queued.ContainsKey(outPoint))
                        {
                            return false; // No such transaction
                        }

                        // Add output data to dictionary
                        inputs.Add(outPoint, queued[outPoint]);

                        // Mark output as spent
                        // queued[outPoint].IsSpent = true;
                    }
                }
                else
                {
                    // Unconfirmed transaction

                    foreach (var txin in tx.vin)
                    {
                        var outPoint = txin.prevout;
                        CTransaction txPrev;

                        if (!mapUnconfirmedTx.TryGetValue(outPoint.hash, out txPrev))
                        {
                            return false; // No such transaction
                        }

                        if (outPoint.n > txPrev.vout.Length)
                        {
                            Invalid = true;

                            return false; // nOut is out of range
                        }

                        // TODO: return inputs from map 
                        throw new NotImplementedException();

                    }

                    return false;
                }
            }

            return true;
        }

        private bool AddItemToIndex(ref CBlockStoreItem newCursor, ref CBlock block)
        {
            uint256 blockHash = newCursor.Hash;

            if (blockMap.ContainsKey(blockHash))
            {
                // Already have this block.
                return false;
            }

            // Begin transaction
            dbConn.BeginTransaction();

            // Compute chain trust score
            newCursor.nChainTrust = (newCursor.prev != null ? newCursor.prev.nChainTrust : 0) + newCursor.nBlockTrust;

            if (!newCursor.SetStakeEntropyBit(Entropy.GetStakeEntropyBit(newCursor.nHeight, blockHash)))
            {
                return false; // SetStakeEntropyBit() failed
            }

            // compute stake modifier
            long nStakeModifier = 0;
            bool fGeneratedStakeModifier = false;
            if (!StakeModifier.ComputeNextStakeModifier(ref newCursor, ref nStakeModifier, ref fGeneratedStakeModifier))
            {
                return false;  // ComputeNextStakeModifier() failed
            }

            newCursor.SetStakeModifier(nStakeModifier, fGeneratedStakeModifier);
            newCursor.nStakeModifierChecksum = StakeModifier.GetModifierChecksum(newCursor);

            if (!ModifierCheckpoints.Verify(newCursor.nHeight, newCursor.nStakeModifierChecksum))
            {
                return false; // Stake modifier checkpoints mismatch
            }

            // Add to index
            if (block.IsProofOfStake)
            {
                newCursor.SetProofOfStake();

                newCursor.prevoutStake = block.vtx[1].vin[0].prevout;
                newCursor.nStakeTime = block.vtx[1].nTime;

                // Save proof-of-stake hash value
                uint256 hashProofOfStake;
                if (!GetProofOfStakeHash(ref blockHash, out hashProofOfStake))
                {
                    return false;  // hashProofOfStake not found 
                }
                newCursor.hashProofOfStake = hashProofOfStake;
            }

            if (!newCursor.WriteToFile(ref fStreamReadWrite, ref block))
            {
                return false;
            }

            if (dbConn.Insert(newCursor) == 0)
            {
                return false; // Insert failed
            }

            // Get last RowID.
            newCursor.ItemID = dbPlatform.SQLiteApi.LastInsertRowid(dbConn.Handle);

            if (!blockMap.TryAdd(blockHash, newCursor))
            {
                return false; // blockMap add failed
            }

            if (newCursor.nChainTrust > ChainParams.nBestChainTrust)
            {
                // New best chain

                if (!SetBestChain(ref newCursor))
                {
                    return false; // SetBestChain failed.
                }
            }

            // Commit transaction
            dbConn.Commit();

            return true;
        }

        private bool SetBestChain(ref CBlockStoreItem cursor)
        {
            uint256 hashBlock = cursor.Hash;

            if (genesisBlockCursor == null && hashBlock == NetInfo.nHashGenesisBlock)
            {
                genesisBlockCursor = cursor;
            }
            else if (ChainParams.nHashBestChain == (uint256)cursor.prevHash)
            {
                if (!SetBestChainInner(cursor))
                {
                    return false;
                }
            }
            else
            {
                // the first block in the new chain that will cause it to become the new best chain
                var cursorIntermediate = cursor;

                // list of blocks that need to be connected afterwards
                var secondary = new List<CBlockStoreItem>();

                // Reorganize is costly in terms of db load, as it works in a single db transaction.
                // Try to limit how much needs to be done inside
                while (cursorIntermediate.prev != null && cursorIntermediate.prev.nChainTrust > bestBlockCursor.nChainTrust)
                {
                    secondary.Add(cursorIntermediate);
                    cursorIntermediate = cursorIntermediate.prev;
                }

                // Switch to new best branch
                if (!Reorganize(cursorIntermediate))
                {
                    InvalidChainFound(cursor);
                    return false; // reorganize failed
                }

                // Connect further blocks
                foreach (var currentCursor in secondary)
                {
                    CBlock block;
                    if (!currentCursor.ReadFromFile(ref fStreamReadWrite, out block))
                    {
                        // ReadFromDisk failed
                        break;
                    }

                    // errors now are not fatal, we still did a reorganisation to a new chain in a valid way
                    if (!SetBestChainInner(currentCursor))
                    {
                        break;
                    }
                }
            }

            bestBlockCursor = cursor;
            nTimeBestReceived = Interop.GetTime();
            nTransactionsUpdated++;

            if (!UpdateTopChain(cursor))
            {
                return false; // unable to set top chain node.
            }

            return true;
        }

        private void InvalidChainFound(CBlockStoreItem cursor)
        {
            throw new NotImplementedException();
        }

        private bool Reorganize(CBlockStoreItem cursorIntermediate)
        {
            // Find the fork
            var fork = bestBlockCursor;
            var longer = cursorIntermediate;

            while (fork.ItemID != longer.ItemID)
            {
                while (longer.nHeight > fork.nHeight)
                {
                    if ((longer = longer.prev) == null)
                    {
                        return false; // longer.prev is null
                    }
                }

                if (fork.ItemID == longer.ItemID)
                {
                    break;
                }

                if ((fork = fork.prev) == null)
                {
                    return false; // fork.prev is null
                }
            }

            // List of what to disconnect
            var disconnect = new List<CBlockStoreItem>();
            for (var cursor = bestBlockCursor; cursor.ItemID != fork.ItemID; cursor = cursor.prev)
            {
                disconnect.Add(cursor);
            }

            // List of what to connect
            var connect = new List<CBlockStoreItem>();
            for (var cursor = cursorIntermediate; cursor.ItemID != fork.ItemID; cursor = cursor.prev)
            {
                connect.Add(cursor);
            }
            connect.Reverse();

            // Disconnect shorter branch
            var txResurrect = new List<CTransaction>();
            foreach (var blockCursor in disconnect)
            {
                CBlock block;
                if (!blockCursor.ReadFromFile(ref fStreamReadWrite, out block))
                {
                    return false; // ReadFromFile for disconnect failed.
                }
                if (!DisconnectBlock(blockCursor, ref block))
                {
                    return false; // DisconnectBlock failed.
                }

                // Queue memory transactions to resurrect
                foreach (var tx in block.vtx)
                {
                    if (!tx.IsCoinBase && !tx.IsCoinStake)
                    {
                        txResurrect.Add(tx);
                    }
                }
            }

            // Connect longer branch
            var txDelete = new List<CTransaction>();
            foreach (var cursor in connect)
            {
                CBlock block;
                if (!cursor.ReadFromFile(ref fStreamReadWrite, out block))
                {
                    return false; // ReadFromDisk for connect failed
                }

                if (!ConnectBlock(cursor, ref block))
                {
                    // Invalid block
                    return false; // ConnectBlock failed
                }

                // Queue memory transactions to delete
                foreach (var tx in block.vtx)
                {
                    txDelete.Add(tx);
                }
            }

            if (!UpdateTopChain(cursorIntermediate))
            {
                return false; // UpdateTopChain failed
            }

            // Resurrect memory transactions that were in the disconnected branch
            foreach (var tx in txResurrect)
            {
                mapUnconfirmedTx.TryAdd(tx.Hash, tx);
            }

            // Delete redundant memory transactions that are in the connected branch
            foreach (var tx in txDelete)
            {
                CTransaction dummy;
                mapUnconfirmedTx.TryRemove(tx.Hash, out dummy);
            }

            return true; // Done
        }

        private bool DisconnectBlock(CBlockStoreItem blockCursor, ref CBlock block)
        {
            throw new NotImplementedException();
        }

        private bool SetBestChainInner(CBlockStoreItem cursor)
        {
            uint256 hash = cursor.Hash;
            CBlock block;
            if (!cursor.ReadFromFile(ref fStreamReadWrite, out block))
            {
                return false; // Unable to read block from file.
            }

            // Adding to current best branch
            if (!ConnectBlock(cursor, ref block) || !UpdateTopChain(cursor))
            {
                InvalidChainFound(cursor);
                return false;
            }

            // Add to current best branch
            var prevCursor = cursor.prev;
            prevCursor.next = cursor;

            if (!UpdateDBCursor(ref prevCursor))
            {
                return false; // unable to update
            }

            // Delete redundant memory transactions
            foreach (var tx in block.vtx)
            {
                CTransaction dummy;
                mapUnconfirmedTx.TryRemove(tx.Hash, out dummy);
            }

            return true;
        }

        private bool ConnectBlock(CBlockStoreItem cursor, ref CBlock block, bool fJustCheck = false)
        {
            // Check it again in case a previous version let a bad block in, but skip BlockSig checking
            if (!block.CheckBlock(!fJustCheck, !fJustCheck, false))
            {
                return false; // Invalid block found.
            }

            bool fScriptChecks = cursor.nHeight >= HashCheckpoints.TotalBlocksEstimate;
            var scriptFlags = scriptflag.SCRIPT_VERIFY_NOCACHE | scriptflag.SCRIPT_VERIFY_P2SH;

            long nFees = 0;
            long nValueIn = 0;
            long nValueOut = 0;
            uint nSigOps = 0;

            var queuedMerkleNodes = new Dictionary<uint256, CMerkleNode>();
            var queuedOutputs = new Dictionary<COutPoint, TxOutItem>();

            for (var nTx = 0; nTx < block.vtx.Length; nTx++)
            {
                var tx = block.vtx[nTx];
                var hashTx = tx.Hash;

                if (!queuedMerkleNodes.ContainsKey(hashTx))
                {
                    var nTxPos = cursor.nBlockPos + block.GetTxOffset(nTx);
                    var mNode = new CMerkleNode(cursor.ItemID, nTxPos, tx);

                    queuedMerkleNodes.Add(hashTx, mNode);
                }

                Dictionary<COutPoint, TxOutItem> txouts;
                if (GetOutputs(hashTx, out txouts))
                {
                    // Do not allow blocks that contain transactions which 'overwrite' older transactions,
                    // unless those are already completely spent.
                    return false;
                }

                nSigOps += tx.LegacySigOpCount;
                if (nSigOps > CBlock.nMaxSigOps)
                {
                    return false; // too many sigops
                }

                var inputs = new Dictionary<COutPoint, TxOutItem>();

                if (tx.IsCoinBase)
                {
                    nValueOut += tx.nValueOut;
                }
                else
                {
                    bool Invalid;
                    if (!FetchInputs(ref tx, ref queuedOutputs, ref inputs, true, out Invalid))
                    {
                        return false; // Unable to fetch some inputs.
                    }

                    // Add in sigops done by pay-to-script-hash inputs;
                    // this is to prevent a "rogue miner" from creating
                    // an incredibly-expensive-to-validate block.
                    nSigOps += tx.GetP2SHSigOpCount(ref inputs);
                    if (nSigOps > CBlock.nMaxSigOps)
                    {
                        return false; // too many sigops
                    }

                    long nTxValueIn = tx.GetValueIn(ref inputs);
                    long nTxValueOut = tx.nValueOut;

                    nValueIn += nTxValueIn;
                    nValueOut += nTxValueOut;

                    if (!tx.IsCoinStake)
                    {
                        nFees += nTxValueIn - nTxValueOut;
                    }

                    if (!ConnectInputs(ref tx, ref inputs, ref queuedOutputs, ref cursor, true, fScriptChecks, scriptFlags))
                    {
                        return false;
                    }
                }

                for (var i = 0u; i < tx.vout.Length; i++)
                {
                    var outKey = new COutPoint(hashTx, i);
                    var outData = new TxOutItem()
                    {
                        nMerkleNodeID = -1,
                        nValue = tx.vout[i].nValue,
                        scriptPubKey = tx.vout[i].scriptPubKey,
                        IsSpent = false,
                        nOut = i
                    };

                    queuedOutputs.Add(outKey, outData);
                }
            }

            if (!block.IsProofOfStake)
            {
                long nBlockReward = CBlock.GetProofOfWorkReward(cursor.nBits, nFees);

                // Check coinbase reward
                if (block.vtx[0].nValueOut > nBlockReward)
                {
                    return false; // coinbase reward exceeded
                }
            }

            cursor.nMint = nValueOut - nValueIn + nFees;
            cursor.nMoneySupply = (cursor.prev != null ? cursor.prev.nMoneySupply : 0) + nValueOut - nValueIn;

            if (!UpdateDBCursor(ref cursor))
            {
                return false; // Unable to commit changes
            }

            if (fJustCheck)
            {
                return true;
            }

            // Flush merkle nodes.
            var savedMerkleNodes = new Dictionary<uint256, CMerkleNode>();
            foreach (var merklePair in queuedMerkleNodes)
            {
                var merkleNode = merklePair.Value;

                if (!SaveMerkleNode(ref merkleNode))
                {
                    // Unable to save merkle tree cursor.
                    return false;
                }

                savedMerkleNodes.Add(merklePair.Key, merkleNode);
            }

            // Write queued transaction changes
            var newOutpointItems = new List<TxOutItem>();
            var updatedOutpointItems = new List<TxOutItem>();
            foreach (var outPair in queuedOutputs)
            {
                var outItem = outPair.Value;

                if (outItem.nMerkleNodeID == -1)
                {
                    // This outpoint doesn't exist yet, adding to insert list.

                    outItem.nMerkleNodeID = savedMerkleNodes[outPair.Key.hash].nMerkleNodeID;
                    newOutpointItems.Add(outItem);
                }
                else
                {
                    // This outpount already exists, adding to update list.

                    updatedOutpointItems.Add(outItem);
                }
            }

            if (updatedOutpointItems.Count != 0 && !UpdateOutpoints(ref updatedOutpointItems))
            {
                return false; // Unable to update outpoints
            }

            if (newOutpointItems.Count != 0 && !InsertOutpoints(ref newOutpointItems))
            {
                return false; // Unable to insert outpoints
            }

            return true;
        }

        /// <summary>
        /// Insert set of new outpoints
        /// </summary>
        /// <param name="newOutpointItems">List of TxOutItem objects.</param>
        /// <returns>Result</returns>
        private bool InsertOutpoints(ref List<TxOutItem> newOutpointItems)
        {
            return (dbConn.InsertAll(newOutpointItems, false) != 0);
        }


        /// <summary>
        /// Update set of outpoints
        /// </summary>
        /// <param name="queuedOutpointItems">List of TxOutItem objects.</param>
        /// <returns>Result</returns>
        private bool UpdateOutpoints(ref List<TxOutItem> updatedOutpointItems)
        {
            return (dbConn.UpdateAll(updatedOutpointItems, false) != 0);
        }

        /// <summary>
        /// Insert merkle node into db and set actual record id value.
        /// </summary>
        /// <param name="merkleNode">Merkle node object reference.</param>
        /// <returns>Result</returns>
        private bool SaveMerkleNode(ref CMerkleNode merkleNode)
        {
            if (dbConn.Insert(merkleNode) == 0)
            {
                return false;
            }

            merkleNode.nMerkleNodeID = dbPlatform.SQLiteApi.LastInsertRowid(dbConn.Handle);

            return true;
        }

        private bool ConnectInputs(ref CTransaction tx, ref Dictionary<COutPoint, TxOutItem> inputs, ref Dictionary<COutPoint, TxOutItem> queued, ref CBlockStoreItem cursorBlock, bool fBlock, bool fScriptChecks, scriptflag scriptFlags)
        {
            // Take over previous transactions' spent items
            // fBlock is true when this is called from AcceptBlock when a new best-block is added to the blockchain

            if (!tx.IsCoinBase)
            {
                long nValueIn = 0;
                long nFees = 0;
                for (uint i = 0; i < tx.vin.Length; i++)
                {
                    var prevout = tx.vin[i].prevout;
                    Contract.Assert(inputs.ContainsKey(prevout));
                    var input = inputs[prevout];

                    CBlockStoreItem parentBlockCursor;

                    if (input.nMerkleNodeID == -1)
                    {
                        // This input seems as is confirmed by the same block.

                        if (!queued.ContainsKey(prevout))
                        {
                            return false; // No such output has been queued by this block.
                        }

                        // TODO: Ensure that neither coinbase nor coinstake outputs are 
                        //    available for spending in the generation block.
                    }
                    else
                    {
                        // This input has been confirmed by one of the earlier accepted blocks.

                        var merkleItem = GetMerkleCursor(input, out parentBlockCursor);

                        if (merkleItem == null)
                        {
                            return false; // Unable to find merkle node
                        }

                        // If prev is coinbase or coinstake, check that it's matured
                        if (merkleItem.IsCoinBase || merkleItem.IsCoinStake)
                        {
                            if (cursorBlock.nHeight - parentBlockCursor.nHeight < NetInfo.nGeneratedMaturity)
                            {
                                return false; // tried to spend non-matured generation input.
                            }
                        }

                        // check transaction timestamp
                        if (merkleItem.nTime > tx.nTime)
                        {
                            return false; // transaction timestamp earlier than input transaction
                        }
                    }

                    // Check for negative or overflow input values
                    nValueIn += input.nValue;
                    if (!CTransaction.MoneyRange(input.nValue) || !CTransaction.MoneyRange(nValueIn))
                    {
                        return false; // txin values out of range
                    }

                }

                // The first loop above does all the inexpensive checks.
                // Only if ALL inputs pass do we perform expensive ECDSA signature checks.
                // Helps prevent CPU exhaustion attacks.
                for (int i = 0; i < tx.vin.Length; i++)
                {
                    var prevout = tx.vin[i].prevout;
                    Contract.Assert(inputs.ContainsKey(prevout));
                    var input = inputs[prevout];

                    // Check for conflicts (double-spend)
                    if (input.IsSpent)
                    {
                        return false;
                    }

                    // Skip ECDSA signature verification when connecting blocks (fBlock=true)
                    // before the last blockchain checkpoint. This is safe because block merkle hashes are
                    // still computed and checked, and any change will be caught at the next checkpoint.
                    if (fScriptChecks)
                    {
                        // Verify signature
                        if (!ScriptCode.VerifyScript(tx.vin[i].scriptSig, input.scriptPubKey, tx, i, (int)scriptflag.SCRIPT_VERIFY_P2SH, 0))
                        {
                            return false; // VerifyScript failed.
                        }
                    }

                    // Mark outpoint as spent
                    input.IsSpent = true;
                    inputs[prevout] = input;

                    // Write back
                    if (fBlock)
                    {
                        if (input.nMerkleNodeID != -1)
                        {
                            // Input has been confirmed earlier.
                            queued.Add(prevout, input);
                        }
                        else
                        {
                            // Input has been confirmed by current block.
                            queued[prevout] = input;
                        }
                    }
                }

                if (tx.IsCoinStake)
                {
                    if (HashCheckpoints.LastCheckpointTime < tx.nTime)
                    {
                        // Coin stake tx earns reward instead of paying fee
                        long nCoinAge;
                        if (!tx.GetCoinAge(ref inputs, out nCoinAge))
                        {
                            return false; // unable to get coin age for coinstake
                        }

                        long nReward = tx.nValueOut - nValueIn;
                        long nCalculatedReward = CBlock.GetProofOfStakeReward(nCoinAge, cursorBlock.nBits, tx.nTime) - tx.GetMinFee(1, false, CTransaction.MinFeeMode.GMF_BLOCK) + CTransaction.nCent;

                        if (nReward > nCalculatedReward)
                        {
                            return false; // coinstake pays too much
                        }
                    }
                }
                else
                {
                    if (nValueIn < tx.nValueOut)
                    {
                        return false; // value in < value out
                    }

                    // Tally transaction fees
                    long nTxFee = nValueIn - tx.nValueOut;
                    if (nTxFee < 0)
                    {
                        return false; // nTxFee < 0
                    }

                    nFees += nTxFee;

                    if (!CTransaction.MoneyRange(nFees))
                    {
                        return false; // nFees out of range
                    }
                }
                
            }

            return true;
        }


        /// <summary>
        /// Set new top node or current best chain.
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        private bool UpdateTopChain(CBlockStoreItem cursor)
        {
            ChainParams.HashBestChain = cursor.Hash;
            ChainParams.nBestChainTrust = cursor.nChainTrust;
            ChainParams.nBestHeight = cursor.nHeight;

            return dbConn.Update(ChainParams) != 0;
        }

        /// <summary>
        /// Try to find proof-of-stake hash in the map.
        /// </summary>
        /// <param name="blockHash">Block hash</param>
        /// <param name="hashProofOfStake">Proof-of-stake hash</param>
        /// <returns>Proof-of-Stake hash value</returns>
        private bool GetProofOfStakeHash(ref uint256 blockHash, out uint256 hashProofOfStake)
        {
            return mapProofOfStake.TryGetValue(blockHash, out hashProofOfStake);
        }

        public bool AcceptBlock(ref CBlock block)
        {
            uint256 nHash = block.header.Hash;

            if (blockMap.ContainsKey(nHash))
            {
                // Already have this block.
                return false;
            }

            CBlockStoreItem prevBlockCursor;
            if (!blockMap.TryGetValue(block.header.prevHash, out prevBlockCursor))
            {
                // Unable to get the cursor.
                return false;
            }

            var prevBlockHeader = prevBlockCursor.BlockHeader;

            uint nHeight = prevBlockCursor.nHeight + 1;

            // Check timestamp against prev
            if (NetInfo.FutureDrift(block.header.nTime) < prevBlockHeader.nTime)
            {
                // block's timestamp is too early
                return false;
            }

            // Check that all transactions are finalized
            foreach (var tx in block.vtx)
            {
                if (!tx.IsFinal(nHeight, block.header.nTime))
                {
                    return false;
                }
            }

            // Check that the block chain matches the known block chain up to a checkpoint
            if (!HashCheckpoints.Verify(nHeight, nHash))
            {
                return false;  // rejected by checkpoint lock-in
            }

            // Enforce rule that the coinbase starts with serialized block height
            var expect = new CScript();
            expect.AddNumber((int)nHeight);

            byte[] expectBytes = expect;
            byte[] scriptSig = block.vtx[0].vin[0].scriptSig;

            if (!expectBytes.SequenceEqual(scriptSig.Take(expectBytes.Length)))
            {
                return false; // coinbase doesn't start with serialized height.
            }

            // Write block to file.
            var newCursor = new CBlockStoreItem()
            {
                nHeight = nHeight,
            };

            newCursor.FillHeader(block.header);

            if (!AddItemToIndex(ref newCursor, ref block))
            {
                dbConn.Rollback();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Get block by hash.
        /// </summary>
        /// <param name="blockHash">Block hash</param>
        /// <param name="block">Block object reference</param>
        /// <param name="nBlockPos">Block position reference</param>
        /// <returns>Result</returns>
        public bool GetBlock(uint256 blockHash, ref CBlock block, ref long nBlockPos)
        {
            CBlockStoreItem cursor;

            if (!blockMap.TryGetValue(blockHash, out cursor))
            {
                return false; // Unable to fetch block cursor
            }

            nBlockPos = cursor.nBlockPos;

            return cursor.ReadFromFile(ref fStreamReadWrite, out block);
        }

        /// <summary>
        /// Get block and transaction by transaction hash.
        /// </summary>
        /// <param name="TxID">Transaction hash</param>
        /// <param name="block">Block reference</param>
        /// <param name="nBlockPos">Block position reference</param>
        /// <returns>Result of operation</returns>
        public bool GetBlockByTransactionID(uint256 TxID, out CBlock block, out long nBlockPos)
        {
            block = null;
            nBlockPos = -1;

            var queryResult = dbConn.Query<CBlockStoreItem>("select b.* from [BlockStorage] b left join [MerkleNodes] m on (b.[ItemID] = m.[nParentBlockID]) where m.[TransactionHash] = ?", (byte[])TxID);

            if (queryResult.Count == 1)
            {
                CBlockStoreItem blockCursor = queryResult[0];

                nBlockPos = blockCursor.nBlockPos;

                return blockCursor.ReadFromFile(ref fStreamReadWrite, out block);
            }

            // Tx not found

            return false;
        }

        public bool GetOutputs(uint256 transactionHash, out Dictionary<COutPoint, TxOutItem> txouts, bool fUnspentOnly=true)
        {
            txouts = null;

            var queryParams = new object[] { (byte[])transactionHash, fUnspentOnly ? OutputFlags.AVAILABLE : (OutputFlags.AVAILABLE | OutputFlags.SPENT) };
            var queryResult = dbConn.Query<TxOutItem>("select o.* from [Outputs] o left join [MerkleNodes] m on m.[nMerkleNodeID] = o.[nMerkleNodeID] where m.[TransactionHash] = ? and outputFlags = ?", queryParams);

            if (queryResult.Count != 0)
            {
                txouts = new Dictionary<COutPoint, TxOutItem>();

                foreach (var o in queryResult)
                {
                    var outpointKey = new COutPoint(transactionHash, o.nOut);
                    var outpointData = o;

                    txouts.Add(outpointKey, outpointData);
                }

                // There are some unspent inputs.
                return true;
            }

            // This transaction has been spent completely.
            return false;
        }

        /// <summary>
        /// Get block cursor from map.
        /// </summary>
        /// <param name="blockHash">block hash</param>
        /// <returns>Cursor or null</returns>
        public CBlockStoreItem GetMapCursor(uint256 blockHash)
        {
            if (blockHash == 0)
            {
                // Genesis block has zero prevHash and no parent.
                return null;
            }

            CBlockStoreItem cursor = null;
            blockMap.TryGetValue(blockHash, out cursor);

            return cursor;
        }

        /// <summary>
        /// Get merkle node cursor by output metadata.
        /// </summary>
        /// <param name="item">Output metadata object</param>
        /// <returns>Merkle node cursor or null</returns>
        public CMerkleNode GetMerkleCursor(TxOutItem item, out CBlockStoreItem blockCursor)
        {
            blockCursor = null;

            // Trying to get cursor from the database.
            var QueryMerkleCursor = dbConn.Query<CMerkleNode>("select * from [MerkleNodes] where [nMerkleNodeID] = ?", item.nMerkleNodeID);

            if (QueryMerkleCursor.Count == 1)
            {
                var merkleNode = QueryMerkleCursor[0];

                // Search for block
                var results = blockMap.Where(x => x.Value.ItemID == merkleNode.nParentBlockID).Select(x => x.Value).ToArray();

                blockCursor = results[0];

                return merkleNode;
            }

            // Nothing found.
            return null;
        }

        /// <summary>
        /// Load cursor from database.
        /// </summary>
        /// <param name="blockHash">Block hash</param>
        /// <returns>Block cursor object</returns>
        public CBlockStoreItem GetDBCursor(uint256 blockHash)
        {
            // Trying to get cursor from the database.
            var QueryBlockCursor = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])blockHash);

            if (QueryBlockCursor.Count == 1)
            {
                return QueryBlockCursor[0];
            }

            // Nothing found.
            return null;
        }

        /// <summary>
        /// Update cursor in memory and on disk.
        /// </summary>
        /// <param name="cursor">Block cursor</param>
        /// <returns>Result</returns>
        public bool UpdateMapCursor(CBlockStoreItem cursor)
        {
            var original = blockMap[cursor.Hash];
            return blockMap.TryUpdate(cursor.Hash, cursor, original);
        }

        /// <summary>
        /// Update cursor record in database.
        /// </summary>
        /// <param name="cursor">Block cursor object</param>
        /// <returns>Result</returns>
        public bool UpdateDBCursor(ref CBlockStoreItem cursor)
        {
            return dbConn.Update(cursor) != 0;
        }

        public bool ProcessBlock(ref CBlock block)
        {
            var blockHash = block.header.Hash;

            if (blockMap.ContainsKey(blockHash))
            {
                // We already have this block.
                return false;
            }

            if (orphanMap.ContainsKey(blockHash))
            {
                // We already have block in the list of orphans.
                return false;
            }

            // TODO: Limited duplicity on stake and reserialization of block signature

            if (!block.CheckBlock(true, true, true))
            {
                // Preliminary checks failure.
                return false;
            }

            if (block.IsProofOfStake)
            {
                uint256 hashProofOfStake = 0, targetProofOfStake = 0;
                if (!StakeModifier.CheckProofOfStake(block.vtx[1], block.header.nBits, out hashProofOfStake, out targetProofOfStake))
                {
                    return false; // do not error here as we expect this during initial block download
                }
                if (!mapProofOfStake.ContainsKey(blockHash)) 
                {
                    // add to mapProofOfStake
                    mapProofOfStake.TryAdd(blockHash, hashProofOfStake);
                }

            }

            // TODO: difficulty verification

            // If don't already have its previous block, shunt it off to holding area until we get it
            if (!blockMap.ContainsKey(block.header.prevHash))
            {
                if (block.IsProofOfStake)
                {
                    var proof = block.ProofOfStake;

                    // Limited duplicity on stake: prevents block flood attack
                    // Duplicate stake allowed only when there is orphan child block
                    if (mapStakeSeenOrphan.ContainsKey(proof) && !orphanMapByPrev.ContainsKey(blockHash))
                    {
                        return false; // duplicate proof-of-stake
                    }
                    else
                    {
                        mapStakeSeenOrphan.TryAdd(proof, blockHash);
                    }
                }

                orphanMap.TryAdd(blockHash, block);
                orphanMapByPrev.TryAdd(blockHash, block);

                return true;
            }

            // Store block to disk
            if (!AcceptBlock(ref block))
            {
                // Accept failed
                return false;
            }

            if (orphanMapByPrev.Count > 0)
            {
                // Recursively process any orphan blocks that depended on this one

                var orphansQueue = new List<uint256>();
                orphansQueue.Add(blockHash);

                for (int i = 0; i < orphansQueue.Count; i++)
                {
                    var hashPrev = orphansQueue[i];

                    foreach (var pair in orphanMapByPrev)
                    {
                        var orphanBlock = pair.Value;

                        if (orphanBlock.header.prevHash == blockHash)
                        {
                            if (AcceptBlock(ref orphanBlock))
                            {
                                orphansQueue.Add(pair.Key);
                            }

                            CBlock dummy1;
                            orphanMap.TryRemove(pair.Key, out dummy1);

                            uint256 dummyHash;
                            mapStakeSeenOrphan.TryRemove(orphanBlock.ProofOfStake, out dummyHash);
                        }
                    }

                    CBlock dummy2;
                    orphanMapByPrev.TryRemove(hashPrev, out dummy2);
                }
            }

            return true;
        }

        public bool ParseBlockFile(string BlockFile = "bootstrap.dat")
        {
            // TODO: Rewrite completely.

            var nOffset = 0L;

            var buffer = new byte[CBlock.nMaxBlockSize]; // Max block size is 1Mb
            var intBuffer = new byte[4];

            var fStream2 = File.OpenRead(BlockFile);

            fStream2.Seek(nOffset, SeekOrigin.Begin); // Seek to previous offset + previous block length

            while (fStream2.Read(buffer, 0, 4) == 4) // Read magic number
            {
                var nMagic = BitConverter.ToUInt32(buffer, 0);
                if (nMagic != 0xe5e9e8e4)
                {
                    throw new Exception("Incorrect magic number.");
                }

                var nBytesRead = fStream2.Read(buffer, 0, 4);
                if (nBytesRead != 4)
                {
                    throw new Exception("BLKSZ EOF");
                }

                var nBlockSize = BitConverter.ToInt32(buffer, 0);

                nOffset = fStream2.Position;

                nBytesRead = fStream2.Read(buffer, 0, nBlockSize);

                if (nBytesRead == 0 || nBytesRead != nBlockSize)
                {
                    throw new Exception("BLK EOF");
                }

                var block = new CBlock(buffer);
                var hash = block.header.Hash;

                if (blockMap.ContainsKey(hash))
                {
                    continue;
                }

                if (!ProcessBlock(ref block))
                {
                    throw new Exception("Invalid block: " + block.header.Hash);
                }

                int nCount = blockMap.Count;
                Console.WriteLine("nCount={0}, Hash={1}, NumTx={2}, Time={3}", nCount, block.header.Hash, block.vtx.Length, DateTime.Now);
            }

            return true;
        }

        ~CBlockStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).

                    fStreamReadWrite.Dispose();
                }

                if (dbConn != null)
                {
                    dbConn.Close();
                    dbConn = null;
                }

                disposed = true;
            }
        }

    }
}
