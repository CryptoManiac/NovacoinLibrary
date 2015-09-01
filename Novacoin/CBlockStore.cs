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

namespace Novacoin
{
    /// <summary>
    /// Block headers table
    /// </summary>
    [Table("BlockStorage")]
    class CBlockStoreItem
    {
        /// <summary>
        /// Item ID in the database
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ItemID { get; set; }

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
        /// Block type flags
        /// </summary>
        public BlockType BlockTypeFlag { get; set; }

        /// <summary>
        /// Stake modifier
        /// </summary>
        public long nStakeModifier { get; set; }

        /// <summary>
        /// Stake entropy bit
        /// </summary>
        public byte nEntropyBit { get; set; }

        /// <summary>
        /// Next block hash
        /// </summary>
        public byte[] NextHash { get; set; }

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
        public ScryptHash256 FillHeader(CBlockHeader header)
        {
            ScryptHash256 _hash;
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
                header.prevHash = new ScryptHash256(prevHash);
                header.merkleRoot = new Hash256(merkleRoot);
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
    }

    /// <summary>
    /// Block type.
    /// </summary>
    public enum BlockType
    {
        PROOF_OF_WORK,
        PROOF_OF_WORK_MODIFIER,
        PROOF_OF_STAKE,
        PROOF_OF_STAKE_MODIFIER
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
        private ConcurrentDictionary<ScryptHash256, CBlockStoreItem> blockMap = new ConcurrentDictionary<ScryptHash256, CBlockStoreItem>();

        /// <summary>
        /// Orphaned blocks map.
        /// </summary>
        private ConcurrentDictionary<ScryptHash256, CBlock> orphanMap = new ConcurrentDictionary<ScryptHash256, CBlock>();
        private ConcurrentDictionary<ScryptHash256, CBlock> orphanMapByPrev = new ConcurrentDictionary<ScryptHash256, CBlock>();

        /// <summary>
        /// Map of unspent items.
        /// </summary>
        private ConcurrentDictionary<Hash256, CTransactionStoreItem> txMap = new ConcurrentDictionary<Hash256, CTransactionStoreItem>();

        public static CBlockStore Instance;

        /// <summary>
        /// Block file stream with read access
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
                    blockMap.TryAdd(new ScryptHash256(item.Hash), item);
                }
            }

            Instance = this;
        }

        public bool GetTransaction(Hash256 TxID, ref CTransaction tx)
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
            var blockHash = new ScryptHash256(itemTemplate.Hash);

            if (blockMap.ContainsKey(blockHash))
            {
                // Already have this block.
                return false;
            }

            // TODO: compute chain trust, set stake entropy bit, record proof-of-stake hash value

            // TODO: compute stake modifier

            // Add to index
            itemTemplate.BlockTypeFlag = block.IsProofOfStake ? BlockType.PROOF_OF_STAKE : BlockType.PROOF_OF_WORK;

            if (!itemTemplate.WriteToFile(ref writer, ref block))
            {
                return false;
            }

            dbConn.Insert(itemTemplate);

            // We have no SetBestChain and ConnectBlock/Disconnect block yet, so adding these transactions manually.
            for (int i = 0; i < block.vtx.Length; i++)
            {
                // Handle trasactions

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

            return blockMap.TryAdd(blockHash, itemTemplate);
        }

        public bool AcceptBlock(ref CBlock block)
        {
            ScryptHash256 hash = block.header.Hash;

            if (blockMap.ContainsKey(hash))
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
                nHeight = nHeight
            };

            itemTemplate.FillHeader(block.header);

            if (!AddItemToIndex(ref itemTemplate, ref block))
            {
                return false;
            }

            return true;
        }

        public bool GetBlock(ScryptHash256 blockHash, ref CBlock block)
        {
            var reader = new BinaryReader(fStreamReadWrite).BaseStream;

            var QueryBlock = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])blockHash);

            if (QueryBlock.Count == 1)
            {
                return QueryBlock[0].ReadFromFile(ref reader, out block);
            }

            // Block not found

            return false;
        }

        public bool ProcessBlock(ref CBlock block)
        {
            ScryptHash256 blockHash = block.header.Hash;

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
                if (!block.SignatureOK || !block.vtx[1].VerifyScripts())
                {
                    // Proof-of-Stake signature validation failure.
                    return false;
                }

                // TODO: proof-of-stake validation
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
            var orphansQueue = new List<ScryptHash256>();
            orphansQueue.Add(blockHash);

            for (int i = 0; i < orphansQueue.Count; i++)
            {
                ScryptHash256 hashPrev = orphansQueue[i];

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
