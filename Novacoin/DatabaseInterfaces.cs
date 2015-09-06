using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using SQLiteNetExtensions.Attributes;
using System.Diagnostics.Contracts;

namespace Novacoin
{
    /// <summary>
    /// Block headers table
    /// </summary>
    public interface IBlockStorageItem
    {
        /// <summary>
        /// Item ID in the database
        /// </summary>
        long ItemID { get; set; }

        /// <summary>
        /// PBKDF2+Salsa20 of block hash
        /// </summary>
        byte[] Hash { get; set; }

        /// <summary>
        /// Version of block schema
        /// </summary>
        uint nVersion { get; set; }

        /// <summary>
        /// Previous block hash.
        /// </summary>
        byte[] prevHash { get; set; }

        /// <summary>
        /// Merkle root hash.
        /// </summary>
        byte[] merkleRoot { get; set; }

        /// <summary>
        /// Block timestamp.
        /// </summary>
        uint nTime { get; set; }

        /// <summary>
        /// Compressed difficulty representation.
        /// </summary>
        uint nBits { get; set; }

        /// <summary>
        /// Nonce counter.
        /// </summary>
        uint nNonce { get; set; }

        /// <summary>
        /// Next block hash.
        /// </summary>
        byte[] nextHash { get; set; }

        /// <summary>
        /// Block type flags
        /// </summary>
        BlockType BlockTypeFlag { get; set; }

        /// <summary>
        /// Stake modifier
        /// </summary>
        long nStakeModifier { get; set; }

        /// <summary>
        /// Proof-of-Stake hash
        /// </summary>
        byte[] hashProofOfStake { get; set; }

        /// <summary>
        /// Stake generation outpoint.
        /// </summary>
        byte[] prevoutStake { get; set; }

        /// <summary>
        /// Stake generation time.
        /// </summary>
        uint nStakeTime { get; set; }

        /// <summary>
        /// Block height
        /// </summary>
        uint nHeight { get; set; }

        /// <summary>
        /// Block position in file
        /// </summary>
        byte[] BlockPos { get; set; }

        /// <summary>
        /// Block size in bytes
        /// </summary>
        byte[] BlockSize { get; set; }
    };

    public interface IMerkleNode
    {
        /// <summary>
        /// Node identifier
        /// </summary>
        long nMerkleNodeID { get; set; }

        /// <summary>
        /// Reference to parent block database item.
        /// </summary>
        long nParentBlockID { get; set; }

        /// <summary>
        /// Transaction type flag
        /// </summary>
        TxFlags TransactionFlags { get; set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        byte[] TransactionHash { get; set; }

        /// <summary>
        /// Transaction offset from the beginning of block header, encoded in VarInt format.
        /// </summary>
        byte[] TxOffset { get; set; }

        /// <summary>
        /// Transaction size, encoded in VarInt format.
        /// </summary>
        byte[] TxSize { get; set; }
    }

    public interface ITxOutItem
    {
        /// <summary>
        /// Reference to transaction item.
        /// </summary>
        long nMerkleNodeID { get; set; }

        /// <summary>
        /// Output flags
        /// </summary>
        OutputFlags outputFlags { get; set; }

        /// <summary>
        /// Output number in VarInt format.
        /// </summary>
        byte[] OutputNumber { get; set; }

        /// <summary>
        /// Output value in VarInt format.
        /// </summary>
        byte[] OutputValue { get; set; }

        /// <summary>
        /// Second half of script which contains spending instructions.
        /// </summary>
        byte[] scriptPubKey { get; set; }

        /// <summary>
        /// Getter for output number.
        /// </summary>
        uint nOut { get; }

        /// <summary>
        /// Getter for output value.
        /// </summary>
        ulong nValue { get; }

        /// <summary>
        /// Getter ans setter for IsSpent flag.
        /// </summary>
        bool IsSpent { get; set; }
    }

}
