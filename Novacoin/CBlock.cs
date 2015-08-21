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

        public CBlock(CBlock b)
        {
            header = new CBlockHeader(b.header);

            for (int i = 0; i < b.vtx.Length; i++)
            {
                vtx[i] = new CTransaction(b.vtx[i]);
            }

            b.signature.CopyTo(signature, 0);
        }

        /// <summary>
        /// Parse byte sequence and initialize new block instance
        /// </summary>
        /// <param name="blockBytes"></param>
		public CBlock (IList<byte> blockBytes)
		{
            WrappedList<byte> wBytes = new WrappedList<byte>(blockBytes);

            // Fill the block header fields
            header = new CBlockHeader(wBytes.GetItems(80));

            // Parse transactions list
            vtx = CTransaction.ReadTransactionsList(ref wBytes);

            // Read block signature
            signature = wBytes.GetItems((int)VarInt.ReadVarInt(ref wBytes));
		}

        public CBlock()
        {
            // Initialize empty array of transactions. Please note that such 
            // configuration is not valid real block since it has to provide 
            // at least one transaction.
            vtx = new CTransaction[0];
        }

        /// <summary>
        /// Convert current instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public IList<byte> Bytes 
        {
            get
            {
                List<byte> r = new List<byte>();

                r.AddRange(header.Bytes);
                r.AddRange(VarInt.EncodeVarInt(vtx.LongLength)); // transactions count

                foreach (CTransaction tx in vtx)
                {
                    r.AddRange(tx.Bytes);
                }

                r.AddRange(VarInt.EncodeVarInt(signature.LongLength));
                r.AddRange(signature);

                return r;
            }
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

