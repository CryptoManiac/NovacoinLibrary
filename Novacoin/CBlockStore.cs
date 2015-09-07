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
using System.Text;

namespace Novacoin
{
    [Table("ChainState")]
    public class ChainState
    {
        [PrimaryKey, AutoIncrement]
        public long itemId { get; set; }

        /// <summary>
        /// Hash of top block in the best chain
        /// </summary>
        public byte[]  HashBestChain { get; set; }

        /// <summary>
        /// Total trust score of best chain
        /// </summary>
        public byte[] BestChainTrust { get; set; }

        public uint nBestHeight { get; set;}

        [Ignore]
        public uint256 nBestChainTrust
        {
            get { return BestChainTrust; }
            set { BestChainTrust = value; }
        }

        [Ignore]
        public uint256 nHashBestChain
        {
            get { return HashBestChain; }
            set { HashBestChain = value; }
        }
    }

    [Table("BlockStorage")]
    public class CBlockStoreItem : IBlockStorageItem
    {
        #region IBlockStorageItem
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
        [Column("nVersion")]
        public uint nVersion { get; set; }

        /// <summary>
        /// Previous block hash.
        /// </summary>
        [Column("prevHash")]
        public byte[] prevHash { get; set; }

        /// <summary>
        /// Merkle root hash.
        /// </summary>
        [Column("merkleRoot")]
        public byte[] merkleRoot { get; set; }

        /// <summary>
        /// Block timestamp.
        /// </summary>
        [Column("nTime")]
        public uint nTime { get; set; }

        /// <summary>
        /// Compressed difficulty representation.
        /// </summary>
        [Column("nBits")]
        public uint nBits { get; set; }

        /// <summary>
        /// Nonce counter.
        /// </summary>
        [Column("nNonce")]
        public uint nNonce { get; set; }

        /// <summary>
        /// Next block hash.
        /// </summary>
        [Column("nextHash")]
        public byte[] nextHash { get; set; }

        /// <summary>
        /// Block type flags
        /// </summary>
        [Column("BlockTypeFlag")]
        public BlockType BlockTypeFlag { get; set; }

        /// <summary>
        /// Stake modifier
        /// </summary>
        [Column("nStakeModifier")]
        public long nStakeModifier { get; set; }

        /// <summary>
        /// Proof-of-Stake hash
        /// </summary>
        [Column("hashProofOfStake")]
        public byte[] hashProofOfStake { get; set; }

        /// <summary>
        /// Stake generation outpoint.
        /// </summary>
        [Column("prevoutStake")]
        public byte[] prevoutStake { get; set; }

        /// <summary>
        /// Stake generation time.
        /// </summary>
        [Column("nStakeTime")]
        public uint nStakeTime { get; set; }

        /// <summary>
        /// Block height, encoded in VarInt format
        /// </summary>
        [Column("nHeight")]
        public uint nHeight { get; set; }

        /// <summary>
        /// Chain trust score, serialized and trimmed uint256 representation.
        /// </summary>
        [Column("ChainTrust")]
        public byte[] ChainTrust { get; set; }

        /// <summary>
        /// Block position in file, encoded in VarInt format
        /// </summary>
        [Column("BlockPos")]
        public byte[] BlockPos { get; set; }

        /// <summary>
        /// Block size in bytes, encoded in VarInt format
        /// </summary>
        [Column("BlockSize")]
        public byte[] BlockSize { get; set; }
        #endregion

        /// <summary>
        /// Accessor and mutator for BlockPos value.
        /// </summary>
        [Ignore]
        public long nBlockPos
        {
            get { return (long)VarInt.DecodeVarInt(BlockPos); }
            set { BlockPos = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Accessor and mutator for BlockSize value.
        /// </summary>
        [Ignore]
        public int nBlockSize
        {
            get { return (int)VarInt.DecodeVarInt(BlockSize); }
            set { BlockSize = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Fill database item with data from given block header.
        /// </summary>
        /// <param name="header">Block header</param>
        /// <returns>Header hash</returns>
        public uint256 FillHeader(CBlockHeader header)
        {
            uint256 _hash = header.Hash;

            Hash = _hash;

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
        public CBlockStoreItem prev
        {
            get { return CBlockStore.Instance.GetMapCursor(prevHash); }
        }

        /// <summary>
        /// Next block cursor
        /// </summary>
        [Ignore]
        public CBlockStoreItem next
        {
            get
            {
                if (nextHash == null)
                {
                    return null;
                }

                return CBlockStore.Instance.GetMapCursor(nextHash);
            }
            set
            {
                nextHash = value.Hash;

                CBlockStore.Instance.UpdateMapCursor(this);
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
                if (nTime < NetInfo.nChainChecksSwitchTime)
                {
                    return IsProofOfStake ? (new uint256(1) << 256) / (nTarget + 1) : 1;
                }

                /* New protocol */

                // Calculate work amount for block
                var nPoWTrust = NetInfo.nPoWBase / (nTarget + 1);

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
        [Ignore]
        public uint256 nChainTrust {
            get { return Interop.AppendWithZeros(ChainTrust); }
            set { ChainTrust = Interop.TrimArray(value); }
        }

        public long nMint { get; internal set; }
        public long nMoneySupply { get; internal set; }
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
    public enum TxFlags : byte
    {
        TX_COINBASE,
        TX_COINSTAKE,
        TX_USER
    }

    /// <summary>
    /// Output flags.
    /// </summary>
    public enum OutputFlags : byte
    {
        AVAILABLE, // Unspent output
        SPENT      // Spent output
    }

    [Table("MerkleNodes")]
    public class CMerkleNode : IMerkleNode
    {
        #region IMerkleNode
        /// <summary>
        /// Node identifier
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long nMerkleNodeID { get; set; }

        /// <summary>
        /// Reference to parent block database item.
        /// </summary>
        [ForeignKey(typeof(CBlockStoreItem), Name = "ItemId")]
        public long nParentBlockID { get; set; }

        /// <summary>
        /// Transaction type flag
        /// </summary>
        [Column("TransactionFlags")]
        public TxFlags TransactionFlags { get; set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [Column("TransactionHash")]
        public byte[] TransactionHash { get; set; }

        /// <summary>
        /// Transaction offset from the beginning of block header, encoded in VarInt format.
        /// </summary>
        [Column("TxOffset")]
        public byte[] TxOffset { get; set; }

        /// <summary>
        /// Transaction size, encoded in VarInt format.
        /// </summary>
        [Column("TxSize")]
        public byte[] TxSize { get; set; }
        #endregion

        /// <summary>
        /// Read transaction from file.
        /// </summary>
        /// <param name="reader">Stream with read access.</param>
        /// <param name="tx">CTransaction reference.</param>
        /// <returns>Result</returns>
        public bool ReadFromFile(ref Stream reader, long nBlockPos, out CTransaction tx)
        {
            var buffer = new byte[CTransaction.nMaxTxSize];

            tx = null;

            try
            {
                reader.Seek(nBlockPos + nTxOffset, SeekOrigin.Begin); // Seek to transaction offset

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
        /// Transaction offset accessor
        /// </summary>
        [Ignore]
        public long nTxOffset
        {
            get { return (long) VarInt.DecodeVarInt(TxOffset); }
            private set { TxOffset = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Transaction size accessor
        /// </summary>
        [Ignore]
        public int nTxSize
        {
            get { return (int)VarInt.DecodeVarInt(TxSize); }
            private set { TxSize = VarInt.EncodeVarInt(value); }
        }

        public CMerkleNode(CTransaction tx)
        {
            nTxOffset = -1;
            nParentBlockID = -1;

            nTxSize = tx.Size;
            TransactionHash = tx.Hash;

            if (tx.IsCoinBase)
            {
                TransactionFlags |= TxFlags.TX_COINBASE;
            }
            else if (tx.IsCoinStake)
            {
                TransactionFlags |= TxFlags.TX_COINSTAKE;
            }
            else
            {
                TransactionFlags |= TxFlags.TX_USER;
            }
        }

        public CMerkleNode(long nBlockId, long nOffset, CTransaction tx)
        {
            nParentBlockID = nBlockId;

            nTxOffset = nOffset;
            nTxSize = tx.Size;
            TransactionHash = tx.Hash;

            if (tx.IsCoinBase)
            {
                TransactionFlags |= TxFlags.TX_COINBASE;
            }
            else if (tx.IsCoinStake)
            {
                TransactionFlags |= TxFlags.TX_COINSTAKE;
            }
            else
            {
                TransactionFlags |= TxFlags.TX_USER;
            }
        }

    }

    [Table("Outputs")]
    public class TxOutItem : ITxOutItem
    {
        /// <summary>
        /// Reference to transaction item.
        /// </summary>
        [ForeignKey(typeof(CMerkleNode), Name = "nMerkleNodeID")]
        public long nMerkleNodeID { get; set; }

        /// <summary>
        /// Output flags
        /// </summary>
        public OutputFlags outputFlags { get; set; }

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
        /// Getter for output number.
        /// </summary>
        [Ignore]
        public uint nOut
        {
            get { return (uint)VarInt.DecodeVarInt(OutputNumber); }
            private set { OutputNumber = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Getter for output value.
        /// </summary>
        [Ignore]
        public ulong nValue
        {
            get { return VarInt.DecodeVarInt(OutputValue); }
            private set { OutputValue = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Getter ans setter for IsSpent flag.
        /// </summary>
        [Ignore]
        public bool IsSpent
        {
            get { return (outputFlags & OutputFlags.SPENT) != 0; }
            set { outputFlags |= value ? OutputFlags.SPENT : OutputFlags.AVAILABLE; }
        }

        public TxOutItem(CTxOut o, uint nOut)
        {
            nValue = o.nValue;
            scriptPubKey = o.scriptPubKey;
            this.nOut = nOut;
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

        interface InputsJoin : ITxOutItem
        {
            byte[] TransactionHash { get; set; }
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

                var inputsKey =  new COutPoint(item.TransactionHash, item.nOut);

                item.IsSpent = true;

                // Add output data to dictionary
                inputs.Add(inputsKey, (TxOutItem) item);
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

            // Make sure it's successfully written to disk 
            dbConn.Commit();

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

            dbConn.Commit();

            // Delete redundant memory transactions
            foreach (var tx in block.vtx)
            {
                CTransaction dummy;
                mapUnconfirmedTx.TryRemove(tx.Hash, out dummy);
            }

            return true;
        }

        private bool ConnectBlock(CBlockStoreItem cursor, ref CBlock block, bool fJustCheck=false)
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
                    nSigOps += tx.GetP2SHSigOpCount(inputs);
                    if (nSigOps > CBlock.nMaxSigOps)
                    {
                        return false; // too many sigops
                    }

                    ulong nTxValueIn = tx.GetValueIn(inputs);
                    ulong nTxValueOut = tx.nValueOut;

                    nValueIn += nTxValueIn;
                    nValueOut += nTxValueOut;

                    if (!tx.IsCoinStake)
                    {
                        nFees += nTxValueIn - nTxValueOut;
                    }

                    if (!ConnectInputs(tx, inputs, queued, cursor, fScriptChecks, scriptFlags))
                    {
                        return false;
                    }
                }

                for (var i = 0u; i < tx.vout.Length; i++)
                {
                    var mNode = new CMerkleNode(cursor.ItemID, nTxPos, tx);
                    queuedMerkleNodes.Add(hashTx, mNode);

                    var outKey = new COutPoint(hashTx, i);
                    var outData = new TxOutItem(tx.vout[i], i);

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

            cursor.nMint = (long) (nValueOut - nValueIn + nFees);
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
            foreach(KeyValuePair<COutPoint, TxOutItem> outPair in queued)
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

        private bool ConnectInputs(CTransaction tx, Dictionary<COutPoint, TxOutItem> inputs, Dictionary<COutPoint, TxOutItem> queued, CBlockStoreItem cursor, bool fScriptChecks, scriptflag scriptFlags)
        {
            throw new NotImplementedException();
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
        /// Interface for join
        /// </summary>
        interface IBlockJoinMerkle : IBlockStorageItem, IMerkleNode
        {
        }

        /// <summary>
        /// Get block and transaction by transaction hash.
        /// </summary>
        /// <param name="TxID">Transaction hash</param>
        /// <param name="block">Block reference</param>
        /// <param name="tx">Transaction reference</param>
        /// <param name="nBlockPos">Block position reference</param>
        /// <param name="nTxPos">Transaction position reference</param>
        /// <returns>Result of operation</returns>
        public bool GetBlockByTransactionID(uint256 TxID, ref CBlock block, ref CTransaction tx, ref long nBlockPos, ref long nTxPos)
        {
            var queryResult = dbConn.Query<IBlockJoinMerkle>("select *  from [BlockStorage] b left join [MerkleNodes] m on (b.[ItemID] = m.[nParentBlockID]) where m.[TransactionHash] = ?", (byte[])TxID);

            if (queryResult.Count == 1)
            {
                CBlockStoreItem blockCursor = (CBlockStoreItem) queryResult[0];
                CMerkleNode txCursor = (CMerkleNode)queryResult[0];

                var reader = new BinaryReader(fStreamReadWrite).BaseStream;

                if (!txCursor.ReadFromFile(ref reader, blockCursor.nBlockPos, out tx))
                {
                    return false; // Unable to read transaction
                }

                return blockCursor.ReadFromFile(ref reader, out block);
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

        public bool WriteNodes(ref CMerkleNode[] merkleNodes)
        {
            

            return true;
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
            var readerForBlocks = new BinaryReader(fStream2).BaseStream;

            readerForBlocks.Seek(nOffset, SeekOrigin.Begin); // Seek to previous offset + previous block length

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

                /*
                if (nCount % 100 == 0 && nCount != 0)
                {
                    Console.WriteLine("Commit...");
                    dbConn.Commit();
                    dbConn.BeginTransaction();
                }*/
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
