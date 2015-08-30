using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;

using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Attributes;


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
        /// Next block hash
        /// </summary>
        public byte[] NextHash { get; set; }

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
                header.prevHash = new Hash256(prevHash);
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
            catch (BlockConstructorException)
            {
                // Constructor exception
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
            var buffer = new byte[250000]; // Max transaction size is 250kB
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

    /// <summary>
    /// Block chain node
    /// </summary>
    public class CChainNode
    {
        /// <summary>
        /// Block number
        /// </summary>
        public int nDepth;

        /// <summary>
        /// Block header
        /// </summary>
        public CBlockHeader blockHeader;

        /// <summary>
        /// Block type flag
        /// </summary>
        public BlockType blockType;

        /// <summary>
        /// Next block hash
        /// </summary>
        public ScryptHash256 hashNextBlock;
    }

    public class CBlockStore : IDisposable
    {
        private bool disposed = false;
        private object LockObj = new object();
        private SQLiteConnection dbConn = null;
        private string strBlockFile;

        private ConcurrentDictionary<ScryptHash256, CChainNode> blockMap = new ConcurrentDictionary<ScryptHash256, CChainNode>();
        private ConcurrentDictionary<Hash256, CTransactionStoreItem> txMap = new ConcurrentDictionary<Hash256, CTransactionStoreItem>();
        private CBlock genesisBlock = new CBlock(Interop.HexToArray("0100000000000000000000000000000000000000000000000000000000000000000000007b0502ad2f9f675528183f83d6385794fbcaa914e6d385c6cb1d866a3b3bb34c398e1151ffff0f1ed30918000101000000398e1151010000000000000000000000000000000000000000000000000000000000000000ffffffff4d04ffff001d020f274468747470733a2f2f626974636f696e74616c6b2e6f72672f696e6465782e7068703f746f7069633d3133343137392e6d736731353032313936236d736731353032313936ffffffff010000000000000000000000000000"));

        public static CBlockStore Instance;

        /// <summary>
        /// Block file stream
        /// </summary>
        private Stream reader;

        /// <summary>
        /// Init the block storage manager.
        /// </summary>
        /// <param name="IndexDB">Path to index database</param>
        /// <param name="BlockFile">Path to block file</param>
        public CBlockStore(string IndexDB = "blockstore.dat", string BlockFile = "bootstrap.dat")
        {
            strBlockFile = BlockFile;

            bool firstInit = !File.Exists(IndexDB);
            dbConn = new SQLiteConnection(new SQLitePlatformGeneric(), IndexDB);

            if (firstInit)
            {
                lock (LockObj)
                {
                    // Create tables
                    dbConn.CreateTable<CBlockStoreItem>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<CTransactionStoreItem>(CreateFlags.ImplicitPK);

                    // Init store with genesis block

                    var NewBlockItem = new CBlockStoreItem()
                    {
                        BlockTypeFlag = genesisBlock.IsProofOfStake ? BlockType.PROOF_OF_STAKE : BlockType.PROOF_OF_WORK,
                        nBlockPos = 8,
                        nBlockSize = ((byte[])genesisBlock).Length
                    };

                    var HeaderHash = NewBlockItem.FillHeader(genesisBlock.header);
                    var NewNode = new CChainNode() { blockHeader = genesisBlock.header, blockType = BlockType.PROOF_OF_WORK };

                    blockMap.TryAdd(HeaderHash, NewNode);
                    dbConn.Insert(NewBlockItem);

                    var NewTxItem = new CTransactionStoreItem()
                    {
                        TransactionHash = genesisBlock.vtx[0].Hash,
                        BlockHash = HeaderHash,
                        txType = TxType.TX_COINBASE,
                        nTxPos = 8 + genesisBlock.GetTxOffset(0),
                        nTxSize = genesisBlock.vtx[0].Size
                    };

                    dbConn.Insert(NewTxItem);
                }
            }
            else
            {
                var QueryGet = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] order by [ItemId] asc");

                // Init list of block items
                foreach (var storeItem in QueryGet)
                {
                    var currentNode = new CChainNode() { blockHeader = new CBlockHeader(storeItem.BlockHeader), blockType = storeItem.BlockTypeFlag };
                    blockMap.TryAdd(new ScryptHash256(storeItem.Hash), currentNode);
                }
            }

            var fStream1 = File.OpenRead(strBlockFile);
            reader = new BinaryReader(fStream1).BaseStream;

            Instance = this;

        }

        public bool GetTransaction(Hash256 TxID, ref CTransaction tx)
        {
            var QueryTx = dbConn.Query<CTransactionStoreItem>("select * from [TransactionStorage] where [TransactionHash] = ?", (byte[])TxID);

            if (QueryTx.Count == 1)
            {
                return QueryTx[0].ReadFromFile(ref reader, out tx);
            }

            // Tx not found

            return false;
        }

        public bool GetBlock(ScryptHash256 blockHash, ref CBlock block)
        {
            var QueryBlock = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] where [Hash] = ?", (byte[])blockHash);

            if (QueryBlock.Count == 1)
            {
                return QueryBlock[0].ReadFromFile(ref reader, out block);
            }

            // Block not found

            return false;
        }

        public bool ParseBlockFile(string BlockFile = "bootstrap.dat")
        {
            strBlockFile = BlockFile;

            // TODO: Rewrite completely.

            var QueryGet = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] order by [ItemId] desc limit 1");

            var nOffset = 0L;

            if (QueryGet.Count() == 1)
            {
                var res = QueryGet.First();
                nOffset = res.nBlockPos + res.nBlockSize;
            }

            var buffer = new byte[1000000]; // Max block size is 1Mb
            var intBuffer = new byte[4];

            var fStream2 = File.OpenRead(strBlockFile);
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

                if (block.header.merkleRoot != block.hashMerkleRoot)
                {
                    Console.WriteLine("MerkleRoot mismatch: {0} vs. {1} in block {2}.", block.header.merkleRoot, block.hashMerkleRoot, block.header.Hash);
                    continue;
                }

                if (block.IsProofOfStake && !block.SignatureOK)
                {
                    Console.WriteLine("Proof-of-Stake signature is invalid for block {0}.", block.header.Hash);
                    continue;
                }

                var NewStoreItem = new CBlockStoreItem()
                {
                    BlockTypeFlag = block.IsProofOfStake ? BlockType.PROOF_OF_STAKE : BlockType.PROOF_OF_WORK,
                    nBlockPos = nOffset,
                    nBlockSize = nBlockSize
                };

                var NewChainNode = new CChainNode()
                {
                    blockHeader = block.header,
                    blockType = NewStoreItem.BlockTypeFlag
                };

                var HeaderHash = NewStoreItem.FillHeader(block.header);

                int nCount = blockMap.Count;
                Console.WriteLine("nCount={0}, Hash={1}, Time={2}", nCount, HeaderHash, DateTime.Now); // Commit on each 100th block

                if (nCount % 100 == 0)
                {
                    Console.WriteLine("Commit...");
                    dbConn.Commit();
                    dbConn.BeginTransaction();
                }

                if (!blockMap.TryAdd(HeaderHash, NewChainNode))
                {
                    Console.WriteLine("Duplicate block: {0}", HeaderHash);
                    continue;
                }

                // Verify transactions

                foreach (var tx in block.vtx)
                {
                    if (!tx.VerifyScripts())
                    {
                        Console.WriteLine("Error checking tx {0}", tx.Hash);
                        continue;
                    }
                }

                dbConn.Insert(NewStoreItem);

                for (int i = 0; i < block.vtx.Length; i++)
                {
                    // Handle trasactions

                    var nTxOffset = nOffset + block.GetTxOffset(i);
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
                        BlockHash = HeaderHash,
                        nTxPos = nTxOffset,
                        nTxSize = block.vtx[i].Size,
                        txType = txnType
                    };

                    dbConn.Insert(NewTxItem);
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

                    reader.Dispose();
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
