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
		public CTransaction[] tx;

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
            tx = CTransaction.ReadTransactionsList(ref wBytes);

            // Read block signature
            signature = wBytes.GetItems((int)VarInt.ReadVarInt(ref wBytes));
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CBlock(\n header={0},\n", header.ToString());

            foreach(CTransaction t in tx)
            {
                sb.AppendFormat("{0}, \n", t.ToString());
            }

            sb.AppendFormat("signature={0})\n", Interop.ToHex(signature));
            

            // TODO
            return sb.ToString();
        }
	}
}

