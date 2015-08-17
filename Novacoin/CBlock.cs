using System;
using System.Text;
using System.Collections.Generic;

namespace Novacoin
{
	/// <summary>
	/// Represents the block. Block consists of header, transaction array and header signature.
	/// </summary>
	public class CBlock
	{
		/// <summary>
		/// Block header.
		/// </summary>
		public CBlockHeader header;

		/// <summary>
		/// Transactions array.
		/// </summary>
		public CTransaction[] vtx;

		/// <summary>
		/// Block header signature.
		/// </summary>
		public byte[] signature;

        /// <summary>
        /// Parse byte sequence and initialize new block instance
        /// </summary>
        /// <param name="blockBytes"></param>
		public CBlock (List<byte> blockBytes)
		{
            header = new CBlockHeader();

            WrappedList<byte> wBytes = new WrappedList<byte>(blockBytes);

            // Fill the block header fields
            header.nVersion = Interop.LEBytesToUInt32(wBytes.GetItems(4));
            header.prevHash = new Hash256(wBytes.GetItems(32));
            header.merkleRoot = new Hash256(wBytes.GetItems(32));
            header.nTime = Interop.LEBytesToUInt32(wBytes.GetItems(4));
            header.nBits = Interop.LEBytesToUInt32(wBytes.GetItems(4));
            header.nNonce = Interop.LEBytesToUInt32(wBytes.GetItems(4));

            // Parse transactions list
            vtx = CTransaction.ReadTransactionsList(ref wBytes);

            // Read block signature
            signature = wBytes.GetItems((int)VarInt.ReadVarInt(ref wBytes));
		}

        /// <summary>
        /// Convert current instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public IList<byte> ToBytes()
        {
            List<byte> r = new List<byte>();

            r.AddRange(header.ToBytes());
            r.AddRange(VarInt.EncodeVarInt(vtx.LongLength)); // transactions count

            foreach (CTransaction tx in vtx)
            {
                r.AddRange(tx.ToBytes());
            }

            r.AddRange(VarInt.EncodeVarInt(signature.LongLength));
            r.AddRange(signature);

            return r;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CBlock(\n header={0},\n", header.ToString());

            foreach(CTransaction tx in vtx)
            {
                sb.AppendFormat("{0},\n", tx.ToString());
            }

            sb.AppendFormat("signature={0})\n", Interop.ToHex(signature));
            
            // TODO
            return sb.ToString();
        }
	}
}

