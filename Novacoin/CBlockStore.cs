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
        private ConcurrentDictionary<COutPoint, uint> mapStakeSeenOrphan = new ConcurrentDictionary<COutPoint, uint>();


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
                    var itemTemplate = new CBlockStoreItem()
                    {
                        nHeight = 0
                    };

                    itemTemplate.FillHeader(genesisBlock.header);

                    if (!AddItemToIndex(ref itemTemplate, ref genesisBlock))
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
                    blockMap.TryAdd(item.Hash, item);

                    if (item.IsProofOfStake)
                    {
                        // build mapStakeSeen
                        mapStakeSeen.TryAdd(item.prevoutStake, item.nStakeTime);
                    }
                }

                // Load data about the top node.
                ChainParams = dbConn.Table<ChainState>().First();
            }
        }

        public bool GetTxOutCursor(COutPoint outpoint, ref TxOutItem txOutCursor)
        {
            var queryResults = dbConn.Query<TxOutItem>("select o.* from [Outputs] o left join [MerkleNodes] m on (m.nMerkleNodeID = o.nMerkleNodeID) where m.[TransactionHash] = ?", (byte[])outpoint.hash);

            if (queryResults.Count == 1)
            {
                txOutCursor = queryResults[0];

                return true;
            }

            // Tx not found

            return false;
        }
        
        public bool FetchInputs(CTransaction tx, ref Dictionary<COutPoint, TxOutItem> queued, ref Dictionary<COutPoint, TxOutItem> inputs, bool IsBlock, out bool Invalid)
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

                item.IsSpent = true;

                // Add output data to dictionary
                inputs.Add(inputsKey, (TxOutItem)item);
            }

            if (queryResults.Count < tx.vin.Length)
            {
                if (IsBlock)
                {
                    // It seems that some transactions are being spent in the same block.

                    foreach (var txin in tx.vin)
                    {
                        var outPoint = txin.prevout;

                        if (!queued.ContainsKey(outPoint))
                        {
                            return false; // No such transaction
                        }

                        // Add output data to dictionary
                        inputs.Add(outPoint, queued[outPoint]);

                        // Mark output as spent
                        queued[outPoint].IsSpent = true;
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

        private bool AddItemToIndex(ref CBlockStoreItem itemTemplate, ref CBlock block)
        {
            uint256 blockHash = itemTemplate.Hash;

            if (blockMap.ContainsKey(blockHash))
            {
                // Already have this block.
                return false;
            }

            // Begin transaction
            dbConn.BeginTransaction();

            // Compute chain trust score
            itemTemplate.nChainTrust = (itemTemplate.prev != null ? itemTemplate.prev.nChainTrust : 0) + itemTemplate.nBlockTrust;

            if (!itemTemplate.SetStakeEntropyBit(Entropy.GetStakeEntropyBit(itemTemplate.nHeight, blockHash)))
            {
                return false; // SetStakeEntropyBit() failed
            }

            // Save proof-of-stake hash value
            if (itemTemplate.IsProofOfStake)
            {
                uint256 hashProofOfStake;
                if (!GetProofOfStakeHash(blockHash, out hashProofOfStake))
                {
                    return false;  // hashProofOfStake not found 
                }
                itemTemplate.hashProofOfStake = hashProofOfStake;
            }

            // compute stake modifier
            long nStakeModifier = 0;
            bool fGeneratedStakeModifier = false;
            if (!StakeModifier.ComputeNextStakeModifier(itemTemplate, ref nStakeModifier, ref fGeneratedStakeModifier))
            {
                return false;  // ComputeNextStakeModifier() failed
            }

            itemTemplate.SetStakeModifier(nStakeModifier, fGeneratedStakeModifier);
            itemTemplate.nStakeModifierChecksum = StakeModifier.GetStakeModifierChecksum(itemTemplate);

            // TODO: verify stake modifier checkpoints

            // Add to index
            if (block.IsProofOfStake)
            {
                itemTemplate.SetProofOfStake();

                itemTemplate.prevoutStake = block.vtx[1].vin[0].prevout;
                itemTemplate.nStakeTime = block.vtx[1].nTime;
            }

            if (!itemTemplate.WriteToFile(ref fStreamReadWrite, ref block))
            {
                return false;
            }

            if (dbConn.Insert(itemTemplate) == 0)
            {
                return false; // Insert failed
            }

            // Get last RowID.
            itemTemplate.ItemID = dbPlatform.SQLiteApi.LastInsertRowid(dbConn.Handle);

            if (!blockMap.TryAdd(blockHash, itemTemplate))
            {
                return false; // blockMap add failed
            }

            if (itemTemplate.nChainTrust > ChainParams.nBestChainTrust)
            {
                // New best chain

                if (!SetBestChain(ref itemTemplate))
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
            cursor.prev.next = cursor;

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

            bool fScriptChecks = cursor.nHeight >= Checkpoints.TotalBlocksEstimate;
            var scriptFlags = scriptflag.SCRIPT_VERIFY_NOCACHE | scriptflag.SCRIPT_VERIFY_P2SH;

            ulong nFees = 0;
            ulong nValueIn = 0;
            ulong nValueOut = 0;
            uint nSigOps = 0;

            var queuedMerkleNodes = new Dictionary<uint256, CMerkleNode>();
            var queued = new Dictionary<COutPoint, TxOutItem>();

            for (var nTx = 0; nTx < block.vtx.Length; nTx++)
            {
                var tx = block.vtx[nTx];
                var hashTx = tx.Hash;
                var nTxPos = cursor.nBlockPos + block.GetTxOffset(nTx);

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
                    if (!FetchInputs(tx, ref queued, ref inputs, true, out Invalid))
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

                    ulong nTxValueIn = tx.GetValueIn(ref inputs);
                    ulong nTxValueOut = tx.nValueOut;

                    nValueIn += nTxValueIn;
                    nValueOut += nTxValueOut;

                    if (!tx.IsCoinStake)
                    {
                        nFees += nTxValueIn - nTxValueOut;
                    }

                    if (!ConnectInputs(tx, ref inputs, ref queued, ref cursor, true, fScriptChecks, scriptFlags))
                    {
                        return false;
                    }
                }

                for (var i = 0u; i < tx.vout.Length; i++)
                {
                    var mNode = new CMerkleNode(cursor.ItemID, nTxPos, tx);
                    queuedMerkleNodes.Add(hashTx, mNode);

                    var outKey = new COutPoint(hashTx, i);
                    var outData = new TxOutItem();

                    outData.nValue = tx.vout[i].nValue;
                    outData.scriptPubKey = tx.vout[i].scriptPubKey;
                    outData.nOut = i;


                    outData.IsSpent = false;

                    queued.Add(outKey, outData);
                }
            }

            if (!block.IsProofOfStake)
            {
                ulong nBlockReward = CBlock.GetProofOfWorkReward(cursor.nBits, nFees);

                // Check coinbase reward
                if (block.vtx[0].nValueOut > nBlockReward)
                {
                    return false; // coinbase reward exceeded
                }
            }

            cursor.nMint = (long)(nValueOut - nValueIn + nFees);
            cursor.nMoneySupply = (cursor.prev != null ? cursor.prev.nMoneySupply : 0) + (long)nValueOut - (long)nValueIn;

            if (!UpdateDBCursor(ref cursor))
            {
                return false; // Unable to commit changes
            }

            if (fJustCheck)
            {
                return true;
            }

            // Write queued transaction changes
            var actualMerkleNodes = new Dictionary<uint256, CMerkleNode>();
            var queuedOutpointItems = new List<TxOutItem>();
            foreach (KeyValuePair<COutPoint, TxOutItem> outPair in queued)
            {
                uint256 txID = outPair.Key.hash;
                CMerkleNode merkleNode;

                if (actualMerkleNodes.ContainsKey(txID))
                {
                    merkleNode = actualMerkleNodes[txID];
                }
                else
                {
                    merkleNode = queuedMerkleNodes[txID];
                    if (!SaveMerkleNode(ref merkleNode))
                    {
                        // Unable to save merkle tree cursor.
                        return false;
                    }
                    actualMerkleNodes.Add(txID, merkleNode);
                }

                var outItem = outPair.Value;
                outItem.nMerkleNodeID = merkleNode.nMerkleNodeID;

                queuedOutpointItems.Add(outItem);
            }

            if (!SaveOutpoints(ref queuedOutpointItems))
            {
                return false; // Unable to save outpoints
            }

            return true;
        }

        /// <summary>
        /// Insert set of outpoints
        /// </summary>
        /// <param name="queuedOutpointItems">List of TxOutItem objects.</param>
        /// <returns>Result</returns>
        private bool SaveOutpoints(ref List<TxOutItem> queuedOutpointItems)
        {
            return dbConn.InsertAll(queuedOutpointItems, false) != 0;
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

        private bool ConnectInputs(CTransaction tx, ref Dictionary<COutPoint, TxOutItem> inputs, ref Dictionary<COutPoint, TxOutItem> queued, ref CBlockStoreItem cursorBlock, bool fBlock, bool fScriptChecks, scriptflag scriptFlags)
        {
            // Take over previous transactions' spent pointers
            // fBlock is true when this is called from AcceptBlock when a new best-block is added to the blockchain
            // fMiner is true when called from the internal bitcoin miner
            // ... both are false when called from CTransaction::AcceptToMemoryPool

            if (!tx.IsCoinBase)
            {
                ulong nValueIn = 0;
                ulong nFees = 0;
                for (uint i = 0; i < tx.vin.Length; i++)
                {
                    var prevout = tx.vin[i].prevout;
                    Contract.Assert(inputs.ContainsKey(prevout));
                    var input = inputs[prevout];

                    CBlockStoreItem parentBlockCursor;
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
                        queued.Add(prevout, input);
                    }
                }

                if (tx.IsCoinStake)
                {
                    // ppcoin: coin stake tx earns reward instead of paying fee
                    ulong nCoinAge;
                    if (!tx.GetCoinAge(ref inputs, out nCoinAge))
                    {
                        return false; // unable to get coin age for coinstake
                    }

                    ulong nReward = tx.nValueOut - nValueIn;

                    ulong nCalculatedReward = CBlock.GetProofOfStakeReward(nCoinAge, cursorBlock.nBits, tx.nTime) - tx.GetMinFee(1, false, CTransaction.MinFeeMode.GMF_BLOCK) + CTransaction.nCent;

                    if (nReward > nCalculatedReward)
                    {
                        return false; // coinstake pays too much
                    }
                }
                else
                {
                    if (nValueIn < tx.nValueOut)
                    {
                        return false; // value in < value out
                    }

                    // Tally transaction fees
                    ulong nTxFee = nValueIn - tx.nValueOut;
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
        private bool GetProofOfStakeHash(uint256 blockHash, out uint256 hashProofOfStake)
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

            CBlockStoreItem prevBlockCursor = null;
            if (!blockMap.TryGetValue(block.header.prevHash, out prevBlockCursor))
            {
                // Unable to get the cursor.
                return false;
            }

            var prevBlockHeader = prevBlockCursor.BlockHeader;

            // TODO: proof-of-work/proof-of-stake verification
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

            // TODO: Enforce rule that the coinbase starts with serialized block height

            // Write block to file.
            var itemTemplate = new CBlockStoreItem()
            {
                nHeight = nHeight,
            };

            itemTemplate.FillHeader(block.header);

            if (!AddItemToIndex(ref itemTemplate, ref block))
            {
                dbConn.Rollback();

                return false;
            }

            return true;
        }

        /// <summary>
        /// GEt block by hash.
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
        public bool GetBlockByTransactionID(uint256 TxID, ref CBlock block, ref long nBlockPos)
        {
            var queryResult = dbConn.Query<CBlockStoreItem>("select b.* from [BlockStorage] b left join [MerkleNodes] m on (b.[ItemID] = m.[nParentBlockID]) where m.[TransactionHash] = ?", (byte[])TxID);

            if (queryResult.Count == 1)
            {
                CBlockStoreItem blockCursor = queryResult[0];

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
                if (!block.SignatureOK)
                {
                    // Proof-of-Stake signature validation failure.
                    return false;
                }

                // TODO: proof-of-stake validation

                uint256 hashProofOfStake = 0, targetProofOfStake = 0;
                if (!StakeModifier.CheckProofOfStake(block.vtx[1], block.header.nBits, ref hashProofOfStake, ref targetProofOfStake))
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
                    // TODO: limit duplicity on stake
                }

                var block2 = new CBlock(block);
                orphanMap.TryAdd(blockHash, block2);
                orphanMapByPrev.TryAdd(blockHash, block2);

                return true;
            }

            // Store block to disk
            if (!AcceptBlock(ref block))
            {
                // Accept failed
                return false;
            }

            // Recursively process any orphan blocks that depended on this one
            var orphansQueue = new List<uint256>();
            orphansQueue.Add(blockHash);

            for (int i = 0; i < orphansQueue.Count; i++)
            {
                var hashPrev = orphansQueue[i];

                foreach (var pair in orphanMap)
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
                    }
                }

                CBlock dummy2;
                orphanMap.TryRemove(hashPrev, out dummy2);
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
                Console.WriteLine("nCount={0}, Hash={1}, Time={2}", nCount, block.header.Hash, DateTime.Now); // Commit on each 100th block

                /*
                if (nCount % 100 == 0 && nCount != 0)
                {
                    Console.WriteLine("Commit...");
                    dbConn.Commit();
                    dbConn.BeginTransaction();
                }*/
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
