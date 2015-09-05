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
using System.Linq;
using System.Collections.Concurrent;

using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novacoin
{
    /// <summary>
    /// Block headers table
    /// </summary>
    [Table("BlockStorage")]
    public class CBlockStoreItem
    {
        /// <summary>
        /// Item ID in the database
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long ItemID { get; set; }

        /// <summary>
        /// PBKDF2+Salsa20 of block hash
        /// </summary>
        [Unique]
        public byte[] Hash { get; set; }

        /// <summary>
        /// Version of block schema
        /// </summary>
        public uint nVersion { get; set; }

        /// <summary>
        /// Previous block hash.
        /// </summary>
        public byte[] prevHash { get; set; }

        /// <summary>
        /// Merkle root hash.
        /// </summary>
        public byte[] merkleRoot { get; set; }

        /// <summary>
        /// Block timestamp.
        /// </summary>
        public uint nTime { get; set; }

        /// <summary>
        /// Compressed difficulty representation.
        /// </summary>
        public uint nBits { get; set; }

        /// <summary>
        /// Nonce counter.
        /// </summary>
        public uint nNonce { get; set; }

        /// <summary>
        /// Next block hash.
        /// </summary>
        public byte[] nextHash { get; set; }

        /// <summary>
        /// Block type flags
        /// </summary>
        public BlockType BlockTypeFlag { get; set; }

        /// <summary>
        /// Stake modifier
        /// </summary>
        public long nStakeModifier { get; set; }

        /// <summary>
        /// Proof-of-Stake hash
        /// </summary>
        public byte[] hashProofOfStake { get; set; }

        /// <summary>
        /// Stake generation outpoint.
        /// </summary>
        public byte[] prevoutStake { get; set; }

        /// <summary>
        /// Stake generation time.
        /// </summary>
        public uint nStakeTime { get; set; }
        
        /// <summary>
        /// Block height
        /// </summary>
        public uint nHeight { get; set; }

        /// <summary>
        /// Block position in file
        /// </summary>
        public long nBlockPos { get; set; }

        /// <summary>
        /// Block size in bytes
        /// </summary>
        public int nBlockSize { get; set; }

        /// <summary>
        /// Fill database item with data from given block header.
        /// </summary>
        /// <param name="header">Block header</param>
        /// <returns>Header hash</returns>
        public uint256 FillHeader(CBlockHeader header)
        {
            uint256 _hash;
            Hash = _hash = header.Hash;

            nVersion = header.nVersion;
            prevHash = header.prevHash;
            merkleRoot = header.merkleRoot;
            nTime = header.nTime;
            nBits = header.nBits;
            nNonce = header.nNonce;

            return _hash;
        }

        /// <summary>
        /// Reconstruct block header from item data.
        /// </summary>
        public CBlockHeader BlockHeader
        {
            get
            {
                CBlockHeader header = new CBlockHeader();

                header.nVersion = nVersion;
                header.prevHash = prevHash;
                header.merkleRoot = merkleRoot;
                header.nTime = nTime;
                header.nBits = nBits;
                header.nNonce = nNonce;

                return header;
            }
        }

        /// <summary>
        /// Read block from file.
        /// </summary>
        /// <param name="reader">Stream with read access.</param>
        /// <param name="reader">CBlock reference.</param>
        /// <returns>Result</returns>
        public bool ReadFromFile(ref Stream reader, out CBlock block)
        {
            var buffer = new byte[nBlockSize];
            block = null;

            try
            {
                reader.Seek(nBlockPos, SeekOrigin.Begin);

                if (nBlockSize != reader.Read(buffer, 0, nBlockSize))
                {
                    return false;
                }

                block = new CBlock(buffer);

                return true;
            }
            catch (IOException)
            {
                // I/O error
                return false;
            }
            catch (BlockException)
            {
                // Constructor exception
                return false;
            }
        }

        /// <summary>
        /// Writes given block to file and prepares cursor object for insertion into the database.
        /// </summary>
        /// <param name="writer">Stream with write access.</param>
        /// <param name="block">CBlock reference.</param>
        /// <returns>Result</returns>
        public bool WriteToFile(ref Stream writer, ref CBlock block)
        {
            try
            {
                byte[] blockBytes = block;

                var magicBytes = BitConverter.GetBytes(CBlockStore.nMagicNumber);
                var blkLenBytes = BitConverter.GetBytes(blockBytes.Length);

                // Seek to the end and then append magic bytes there.
                writer.Seek(0, SeekOrigin.End);
                writer.Write(magicBytes, 0, magicBytes.Length);
                writer.Write(blkLenBytes, 0, blkLenBytes.Length);

                // Save block size and current position in the block cursor fields.
                nBlockPos = writer.Position;
                nBlockSize = blockBytes.Length;                

                // Write block and flush the stream.
                writer.Write(blockBytes, 0, blockBytes.Length);
                writer.Flush();

                return true;
            }
            catch (IOException)
            {
                // I/O error
                return false;
            }
            catch (Exception)
            {
                // Some serialization error
                return false;
            }
        }

        /// <summary>
        /// Previous block cursor
        /// </summary>
        [Ignore]
        public CBlockStoreItem prev {
            get { return CBlockStore.Instance.GetCursor(prevHash); }
        }

        /// <summary>
        /// Next block cursor
        /// </summary>
        [Ignore]
        public CBlockStoreItem next
        {
            get { return CBlockStore.Instance.GetCursor(nextHash); }
            set
            {
                CBlockStoreItem newCursor = this;
                newCursor.nextHash = value.Hash;

                CBlockStore.Instance.UpdateCursor(this, ref newCursor);
            }
        }

        [Ignore]
        bool IsInMainChain
        {
            get { return (next != null); }
        }

        /// <summary>
        /// STake modifier generation flag
        /// </summary>
        [Ignore]
        public bool GeneratedStakeModifier
        {
            get { return (BlockTypeFlag & BlockType.BLOCK_STAKE_MODIFIER) != 0; }
        }

        /// <summary>
        /// Stake entropy bit
        /// </summary>
        [Ignore]
        public uint StakeEntropyBit
        {
            get { return ((uint)(BlockTypeFlag & BlockType.BLOCK_STAKE_ENTROPY) >> 1); }
        }

        /// <summary>
        /// Sets stake modifier and flag.
        /// </summary>
        /// <param name="nModifier">New stake modifier.</param>
        /// <param name="fGeneratedStakeModifier">Set generation flag?</param>
        public void SetStakeModifier(long nModifier, bool fGeneratedStakeModifier)
        {
            nStakeModifier = nModifier;
            if (fGeneratedStakeModifier)
                BlockTypeFlag |= BlockType.BLOCK_STAKE_MODIFIER;
        }

        /// <summary>
        /// Set entropy bit.
        /// </summary>
        /// <param name="nEntropyBit">Entropy bit value (0 or 1).</param>
        /// <returns>False if value is our of range.</returns>
        public bool SetStakeEntropyBit(byte nEntropyBit)
        {
            if (nEntropyBit > 1)
                return false;
            BlockTypeFlag |= (nEntropyBit != 0 ? BlockType.BLOCK_STAKE_ENTROPY : 0);
            return true;
        }

        /// <summary>
        /// Set proof-of-stake flag.
        /// </summary>
        public void SetProofOfStake()
        {
            BlockTypeFlag |= BlockType.BLOCK_PROOF_OF_STAKE;
        }

        /// <summary>
        /// Block has no proof-of-stake flag.
        /// </summary>
        [Ignore]
        public bool IsProofOfWork
        {
            get { return (BlockTypeFlag & BlockType.BLOCK_PROOF_OF_STAKE) == 0; }
        }

        /// <summary>
        /// Block has proof-of-stake flag set.
        /// </summary>
        [Ignore]
        public bool IsProofOfStake 
        {
            get { return (BlockTypeFlag & BlockType.BLOCK_PROOF_OF_STAKE) != 0; }
        }

        /// <summary>
        /// Block trust score.
        /// </summary>
        [Ignore]
        public uint256 nBlockTrust
        {
            get
            {
                uint256 nTarget = 0;
                nTarget.Compact = nBits;

                /* Old protocol */
                if (nTime < NetUtils.nChainChecksSwitchTime)
                {
                    return IsProofOfStake ? (new uint256(1) << 256) / (nTarget + 1) : 1;
                }

                /* New protocol */

                // Calculate work amount for block
                var nPoWTrust = NetUtils.nPoWBase / (nTarget + 1);

                // Set nPowTrust to 1 if we are checking PoS block or PoW difficulty is too low
                nPoWTrust = (IsProofOfStake || !nPoWTrust) ? 1 : nPoWTrust;

                // Return nPoWTrust for the first 12 blocks
                if (prev == null || prev.nHeight < 12)
                    return nPoWTrust;

                CBlockStoreItem currentIndex = prev;

                if (IsProofOfStake)
                {
                    var nNewTrust = (new uint256(1) << 256) / (nTarget + 1);

                    // Return 1/3 of score if parent block is not the PoW block
                    if (!prev.IsProofOfWork)
                    {
                        return nNewTrust / 3;
                    }

                    int nPoWCount = 0;

                    // Check last 12 blocks type
                    while (prev.nHeight - currentIndex.nHeight < 12)
                    {
                        if (currentIndex.IsProofOfWork)
                        {
                            nPoWCount++;
                        }
                        currentIndex = currentIndex.prev;
                    }

                    // Return 1/3 of score if less than 3 PoW blocks found
                    if (nPoWCount < 3)
                    {
                        return nNewTrust / 3;
                    }

                    return nNewTrust;
                }
                else
                {
                    var nLastBlockTrust = prev.nChainTrust - prev.prev.nChainTrust;

                    // Return nPoWTrust + 2/3 of previous block score if two parent blocks are not PoS blocks
                    if (!prev.IsProofOfStake || !prev.prev.IsProofOfStake)
                    {
                        return nPoWTrust + (2 * nLastBlockTrust / 3);
                    }

                    int nPoSCount = 0;

                    // Check last 12 blocks type
                    while (prev.nHeight - currentIndex.nHeight < 12)
                    {
                        if (currentIndex.IsProofOfStake)
                        {
                            nPoSCount++;
                        }
                        currentIndex = currentIndex.prev;
                    }

                    // Return nPoWTrust + 2/3 of previous block score if less than 7 PoS blocks found
                    if (nPoSCount < 7)
                    {
                        return nPoWTrust + (2 * nLastBlockTrust / 3);
                    }

                    nTarget.Compact = prev.nBits;

                    if (!nTarget)
                    {
                        return 0;
                    }

                    var nNewTrust = (new uint256(1) << 256) / (nTarget + 1);

                    // Return nPoWTrust + full trust score for previous block nBits
                    return nPoWTrust + nNewTrust;
                }
            }
        }

        /// <summary>
        /// Stake modifier checksum.
        /// </summary>
        public uint nStakeModifierChecksum;

        /// <summary>
        /// Chain trust score
        /// </summary>
        public uint256 nChainTrust;
    }

    /// <summary>
    /// Block type.
    /// </summary>
    public enum BlockType
    {
        BLOCK_PROOF_OF_STAKE = (1 << 0), // is proof-of-stake block
        BLOCK_STAKE_ENTROPY = (1 << 1), // entropy bit for stake modifier
        BLOCK_STAKE_MODIFIER = (1 << 2), // regenerated stake modifier
    };

    /// <summary>
    /// Transaction type.
    /// </summary>
    public enum TxType
    {
        TX_COINBASE,
        TX_COINSTAKE,
        TX_USER
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public enum OutputType
    {
        TX_USER      = (1 << 0), // User output
        TX_COINBASE  = (1 << 1), // Coinbase output
        TX_COINSTAKE = (1 << 2), // Coinstake output
        TX_AVAILABLE = (2 << 0), // Unspent output
        TX_SPENT     = (2 << 1)  // Spent output
    }

    [Table("MerkleNodes")]
    public class MerkleNode
    {
        [PrimaryKey, AutoIncrement]
        public long nMerkleNodeID { get; set; }

        /// <summary>
        /// Reference to parent block database item.
        /// </summary>
        [ForeignKey(typeof(CBlockStoreItem), Name = "ItemId")]
        public long nParentBlockID { get; set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        public byte[] TransactionHash { get; set; }

        public static bool QueryParentBlockCursor(uint256 transactionHash, out CBlockStoreItem cursor)
        {
            throw new NotImplementedException();
        }
    }

    [Table("Outputs")]
    public class TxOutItem
    {
        /// <summary>
        /// Link the transaction hash with database item identifier.
        /// </summary>
        private static ConcurrentDictionary<uint256, long> outMap = new ConcurrentDictionary<uint256, long>();

        /// <summary>
        /// Reference to transaction item.
        /// </summary>
        [ForeignKey(typeof(MerkleNode), Name = "nMerkleNodeID")]
        public long nMerkleNodeID { get; set; }

        /// <summary>
        /// Output flags
        /// </summary>
        public OutputType outputFlags { get; set; }

        /// <summary>
        /// Output number in VarInt format.
        /// </summary>
        public byte[] OutputNumber { get; set; }

        /// <summary>
        /// Output value in VarInt format.
        /// </summary>
        public byte[] OutputValue { get; set; }

        /// <summary>
        /// Second half of script which contains spending instructions.
        /// </summary>
        public byte[] scriptPubKey { get; set; }

        /// <summary>
        /// Construct new item from provided transaction data.
        /// </summary>
        /// <param name="o"></param>
        public TxOutItem(CTransaction tx, uint nOut)
        {
            Contract.Requires<ArgumentException>(nOut < tx.vout.Length);

            long nMerkleId = 0;
            if (!outMap.TryGetValue(tx.Hash, out nMerkleId))
            {
                // Not in the blockchain
                nMerkleNodeID = -1;
            }

            OutputNumber = VarInt.EncodeVarInt(nOut);
            OutputValue = VarInt.EncodeVarInt(tx.vout[nOut].nValue);
            scriptPubKey = tx.vout[nOut].scriptPubKey;

            if (tx.IsCoinBase)
            {
                outputFlags |= OutputType.TX_COINBASE;
            }
            else if (tx.IsCoinStake)
            {
                outputFlags |= OutputType.TX_COINSTAKE;
            }
        }

        /// <summary>
        /// Getter for output number.
        /// </summary>
        [Ignore]
        public uint nOut
        {
            get { return (uint)VarInt.DecodeVarInt(OutputNumber); }
        }

        /// <summary>
        /// Getter for output value.
        /// </summary>
        [Ignore]
        public ulong nValue
        {
            get { return VarInt.DecodeVarInt(OutputValue); }
        }

        /// <summary>
        /// Is this a user transaction output?
        /// </summary>
        [Ignore]
        public bool IsUser
        {
            get { return (outputFlags & OutputType.TX_USER) != 0; }
        }

        /// <summary>
        /// Is this a coinbase transaction output?
        /// </summary>
        [Ignore]
        public bool IsCoinBase
        {
            get { return (outputFlags & OutputType.TX_COINBASE) != 0;  }
        }

        /// <summary>
        /// Is this a coinstake transaction output?
        /// </summary>
        [Ignore]
        public bool IsCoinStake
        {
            get { return (outputFlags & OutputType.TX_COINSTAKE) != 0; }
        }

        /// <summary>
        /// Getter ans setter for IsSpent flag.
        /// </summary>
        [Ignore]
        public bool IsSpent
        {
            get { return (outputFlags & OutputType.TX_SPENT) != 0; }
            set { outputFlags |= value ? OutputType.TX_SPENT : OutputType.TX_AVAILABLE; }
        }
    }


    [Table("TransactionStorage")]
    public class CTransactionStoreItem
    {
        /// <summary>
        /// Transaction hash
        /// </summary>
        [PrimaryKey]
        public byte[] TransactionHash { get; set; }

        /// <summary>
        /// Block hash
        /// </summary>
        [ForeignKey(typeof(CBlockStoreItem), Name = "Hash")]
        public byte[] BlockHash { get; set; }

        /// <summary>
        /// Transaction type flag
        /// </summary>
        public TxType txType { get; set; }

        /// <summary>
        /// Tx position in file
        /// </summary>
        public long nTxPos { get; set; }

        /// <summary>
        /// Transaction size
        /// </summary>
        public int nTxSize { get; set; }

        /// <summary>
        /// Serialized output array
        /// </summary>
        public byte[] vOut { get; set; }

        /// <summary>
        /// Read transaction from file.
        /// </summary>
        /// <param name="reader">Stream with read access.</param>
        /// <param name="tx">CTransaction reference.</param>
        /// <returns>Result</returns>
        public bool ReadFromFile(ref Stream reader, out CTransaction tx)
        {
            var buffer = new byte[CTransaction.nMaxTxSize];
            tx = null;

            try
            {
                reader.Seek(nTxPos, SeekOrigin.Begin); // Seek to transaction offset

                if (nTxSize != reader.Read(buffer, 0, nTxSize))
                {
                    return false;
                }

                tx = new CTransaction(buffer);

                return true;
            }
            catch (IOException)
            {
                // I/O error
                return false;
            }
            catch (TransactionConstructorException)
            {
                // Constructor error
                return false;
            }
        }

        /// <summary>
        /// Outputs array access
        /// </summary>
        [Ignore]
        public CTxOut[] Outputs {
            get { return CTxOut.DeserializeOutputsArray(vOut); }
            set { vOut = CTxOut.SerializeOutputsArray(value); }
        }
    }

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
        /// Block file.
        /// </summary>
        private string strBlockFile;

        /// <summary>
        /// Index database file.
        /// </summary>
        private string strDbFile;

        /// <summary>
        /// Map of block tree nodes.
        /// </summary>
        private ConcurrentDictionary<uint256, CBlockStoreItem> blockMap = new ConcurrentDictionary<uint256, CBlockStoreItem>();

        /// <summary>
        /// Orphaned blocks map.
        /// </summary>
        private ConcurrentDictionary<uint256, CBlock> orphanMap = new ConcurrentDictionary<uint256, CBlock>();
        private ConcurrentDictionary<uint256, CBlock> orphanMapByPrev = new ConcurrentDictionary<uint256, CBlock>();

        /// <summary>
        /// Map of unspent items.
        /// </summary>
        private ConcurrentDictionary<uint256, CTransactionStoreItem> txMap = new ConcurrentDictionary<uint256, CTransactionStoreItem>();
        
        /// <summary>
        /// Map of the proof-of-stake hashes. This is necessary for stake duplication checks.
        /// </summary>
        private ConcurrentDictionary<uint256, uint256> mapProofOfStake = new ConcurrentDictionary<uint256, uint256>();


        private ConcurrentDictionary<COutPoint, uint> mapStakeSeen = new ConcurrentDictionary<COutPoint, uint>();
        private ConcurrentDictionary<COutPoint, uint> mapStakeSeenOrphan = new ConcurrentDictionary<COutPoint, uint>();

        /// <summary>
        /// Unconfirmed transactions.
        /// </summary>
        private ConcurrentDictionary<uint256, CTransaction> mapUnconfirmedTx = new ConcurrentDictionary<uint256, CTransaction>();

        /// <summary>
        /// Trust score for the longest chain.
        /// </summary>
        private uint256 nBestChainTrust = 0;

        /// <summary>
        /// Top block of the best chain.
        /// </summary>
        private uint256 nHashBestChain = 0;

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
            dbConn = new SQLiteConnection(new SQLitePlatformGeneric(), strDbFile);

            fStreamReadWrite = File.Open(strBlockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Instance = this;

            if (firstInit)
            {
                lock (LockObj)
                {
                    // Create tables
                    dbConn.CreateTable<CBlockStoreItem>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<CTransactionStoreItem>(CreateFlags.ImplicitPK);

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
            }
        }

        public bool GetTransaction(uint256 TxID, ref CTransaction tx)
        {
            var reader = new BinaryReader(fStreamReadWrite).BaseStream;
            var QueryTx = dbConn.Query<CTransactionStoreItem>("select * from [TransactionStorage] where [TransactionHash] = ?", (byte[])TxID);

            if (QueryTx.Count == 1)
            {
                return QueryTx[0].ReadFromFile(ref reader, out tx);
            }

            // Tx not found

            return false;
        }

        private bool AddItemToIndex(ref CBlockStoreItem itemTemplate, ref CBlock block)
        {
            var writer = new BinaryWriter(fStreamReadWrite).BaseStream;
            uint256 blockHash = itemTemplate.Hash;

            if (blockMap.ContainsKey(blockHash))
            {
                // Already have this block.
                return false;
            }

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

            if (!itemTemplate.WriteToFile(ref writer, ref block))
            {
                return false;
            }

            if (dbConn.Insert(itemTemplate) == 0 || !blockMap.TryAdd(blockHash, itemTemplate))
            {
                return false;
            }

            if (itemTemplate.nChainTrust > nBestChainTrust)
            {
                // New best chain

                // TODO: SetBestChain implementation

                /*
                if (!SetBestChain(ref itemTemplate))
                {
                    return false; // SetBestChain failed.
                }
                */
            }

            // We have no SetBestChain and ConnectBlock/Disconnect block yet, so adding these transactions manually.
            for (int i = 0; i < block.vtx.Length; i++)
            {
                // Handle trasactions using our temporary stub algo

                if (!block.vtx[i].VerifyScripts())
                {
                    return false;
                }

                var nTxOffset = itemTemplate.nBlockPos + block.GetTxOffset(i);
                TxType txnType = TxType.TX_USER;

                if (block.vtx[i].IsCoinBase)
                {
                    txnType = TxType.TX_COINBASE;
                }
                else if (block.vtx[i].IsCoinStake)
                {
                    txnType = TxType.TX_COINSTAKE;
                }

                var NewTxItem = new CTransactionStoreItem()
                {
                    TransactionHash = block.vtx[i].Hash,
                    BlockHash = blockHash,
                    nTxPos = nTxOffset,
                    nTxSize = block.vtx[i].Size,
                    txType = txnType
                };

                dbConn.Insert(NewTxItem);
            }

            return true;
        }

        private bool SetBestChain(ref CBlockStoreItem cursor)
        {
            dbConn.BeginTransaction();

            uint256 hashBlock = cursor.Hash;

            if (genesisBlockCursor == null && hashBlock == NetUtils.nHashGenesisBlock)
            {
                genesisBlockCursor = cursor;
            }
            else if (nHashBestChain == (uint256)cursor.prevHash)
            {
                if (!SetBestChainInner(cursor))
                {
                    return false;
                }
            }
            else
            {
                // the first block in the new chain that will cause it to become the new best chain
                CBlockStoreItem cursorIntermediate = cursor;

                // list of blocks that need to be connected afterwards
                List<CBlockStoreItem> secondary = new List<CBlockStoreItem>();

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
                    dbConn.Rollback();
                    InvalidChainFound(cursor);
                    return false; // reorganize failed
                }


            }


            throw new NotImplementedException();
        }

        private void InvalidChainFound(CBlockStoreItem cursor)
        {
            throw new NotImplementedException();
        }

        private bool Reorganize(CBlockStoreItem cursorIntermediate)
        {
            throw new NotImplementedException();
        }

        private bool SetBestChainInner(CBlockStoreItem cursor)
        {
            uint256 hash = cursor.Hash;
            CBlock block;

            // Adding to current best branch
            if (!ConnectBlock(cursor, false, out block) || !WriteHashBestChain(hash))
            {
                dbConn.Rollback();
                InvalidChainFound(cursor);
                return false;
            }

            // Add to current best branch
            cursor.prev.next = cursor;

            dbConn.Commit();

            // Delete redundant memory transactions
            foreach (var tx in block.vtx)
            {
                CTransaction dummy;
                mapUnconfirmedTx.TryRemove(tx.Hash, out dummy);
            }

            return true;
        }

        private bool ConnectBlock(CBlockStoreItem cursor, bool fJustCheck, out CBlock block)
        {
            var reader = new BinaryReader(fStreamReadWrite).BaseStream;
            if (cursor.ReadFromFile(ref reader, out block))
            {
                return false; // Unable to read block from file.
            }

            // Check it again in case a previous version let a bad block in, but skip BlockSig checking
            if (!block.CheckBlock(!fJustCheck, !fJustCheck, false))
            {
                return false; // Invalid block found.
            }

            // TODO: the remaining stuff lol :D

            throw new NotImplementedException();
        }

        private bool WriteHashBestChain(uint256 hash)
        {
            throw new NotImplementedException();
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
            if (NetUtils.FutureDrift(block.header.nTime) < prevBlockHeader.nTime)
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
                return false;
            }

            return true;
        }

        public bool GetBlock(uint256 blockHash, ref CBlock block, ref long nBlockPos)
        {
            var reader = new BinaryReader(fStreamReadWrite).BaseStream;

            var QueryBlock = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])blockHash);

            if (QueryBlock.Count == 1)
            {
                nBlockPos = QueryBlock[0].nBlockPos;
                return QueryBlock[0].ReadFromFile(ref reader, out block);
            }

            // Block not found

            return false;
        }

        public bool GetBlockByTransactionID(uint256 TxID, ref CBlock block, ref CTransaction tx, ref long nBlockPos, ref long nTxPos)
        {
            var QueryTx = dbConn.Query<CTransactionStoreItem>("select * from [TransactionStorage] where [TransactionHash] = ?", (byte[])TxID);

            if (QueryTx.Count == 1)
            {
                nTxPos = QueryTx[0].nTxPos;
                return GetBlock(QueryTx[0].BlockHash, ref block, ref nBlockPos);
            }

            // Tx not found

            return false;
        }

        /// <summary>
        /// Get block cursor from map.
        /// </summary>
        /// <param name="blockHash">block hash</param>
        /// <returns>Cursor or null</returns>
        public CBlockStoreItem GetCursor(uint256 blockHash)
        {
            if (blockHash == 0)
            {
                // Genesis block has zero prevHash and no parent.
                return null;
            }

            // First, check our block map.
            CBlockStoreItem item = null;
            if (blockMap.TryGetValue(blockHash, out item))
            {
                return item;
            }

            // Trying to get cursor from the database.
            var QueryBlockCursor = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])blockHash);

            if (QueryBlockCursor.Count == 1)
            {
                blockMap.TryAdd(blockHash, QueryBlockCursor[0]);

                return QueryBlockCursor[0];
            }

            // Nothing found.
            return null;
        }

        /// <summary>
        /// Update cursor in memory and on disk.
        /// </summary>
        /// <param name="originalItem">Original cursor</param>
        /// <param name="newItem">New cursor</param>
        /// <returns></returns>
        public bool UpdateCursor(CBlockStoreItem originalItem, ref CBlockStoreItem newItem)
        {
            if (blockMap.TryUpdate(originalItem.Hash, newItem, originalItem))
            {
                return dbConn.Update(newItem) != 0;
            }

            return false;
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
            var readerForBlocks = new BinaryReader(fStream2).BaseStream;

            readerForBlocks.Seek(nOffset, SeekOrigin.Begin); // Seek to previous offset + previous block length

            dbConn.BeginTransaction();

            while (readerForBlocks.Read(buffer, 0, 4) == 4) // Read magic number
            {
                var nMagic = BitConverter.ToUInt32(buffer, 0);
                if (nMagic != 0xe5e9e8e4)
                {
                    throw new Exception("Incorrect magic number.");
                }

                var nBytesRead = readerForBlocks.Read(buffer, 0, 4);
                if (nBytesRead != 4)
                {
                    throw new Exception("BLKSZ EOF");
                }

                var nBlockSize = BitConverter.ToInt32(buffer, 0);

                nOffset = readerForBlocks.Position;

                nBytesRead = readerForBlocks.Read(buffer, 0, nBlockSize);

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

                if (nCount % 100 == 0 && nCount != 0)
                {
                    Console.WriteLine("Commit...");
                    dbConn.Commit();
                    dbConn.BeginTransaction();
                }
            }

            dbConn.Commit();

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
