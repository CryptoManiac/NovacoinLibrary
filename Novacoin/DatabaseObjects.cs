using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.IO;

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
        public byte[] HashBestChain { get; set; }

        /// <summary>
        /// Total trust score of best chain
        /// </summary>
        public byte[] BestChainTrust { get; set; }

        public uint nBestHeight { get; set; }

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
    public class CBlockStoreItem
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
        public uint256 nChainTrust
        {
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
    public class CMerkleNode
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
        /// Transaction timestamp
        /// </summary>
        [Column("nTime")]
        public uint nTime { get; set; }

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
            get { return (long)VarInt.DecodeVarInt(TxOffset); }
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

        [Ignore]
        public bool IsCoinBase
        {
            get { return TransactionFlags == TxFlags.TX_COINBASE; }
        }

        [Ignore]
        public bool IsCoinStake
        {
            get { return TransactionFlags == TxFlags.TX_COINSTAKE; }
        }

        public CMerkleNode(CTransaction tx)
        {
            nTime = tx.nTime;

            nTxOffset = -1;
            nParentBlockID = -1;

            nTxSize = tx.Size;
            TransactionHash = tx.Hash;

            if (tx.IsCoinBase)
            {
                TransactionFlags = TxFlags.TX_COINBASE;
            }
            else if (tx.IsCoinStake)
            {
                TransactionFlags = TxFlags.TX_COINSTAKE;
            }
            else
            {
                TransactionFlags = TxFlags.TX_USER;
            }
        }

        public CMerkleNode(long nBlockId, long nOffset, CTransaction tx)
        {
            nTime = tx.nTime;

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
    public class TxOutItem
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
            set { OutputNumber = VarInt.EncodeVarInt(value); }
        }

        /// <summary>
        /// Getter for output value.
        /// </summary>
        [Ignore]
        public ulong nValue
        {
            get { return VarInt.DecodeVarInt(OutputValue); }
            set { OutputValue = VarInt.EncodeVarInt(value); }
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
    }


    /// <summary>
    /// TxOut + transaction hash
    /// </summary>
    public class InputsJoin : TxOutItem
    {
        public byte[] TransactionHash { get; set; }
    }



}
