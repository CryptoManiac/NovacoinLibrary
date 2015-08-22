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
        public byte[] signature = new byte[0];

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
            ByteQueue wBytes = new ByteQueue(blockBytes);

            // Fill the block header fields
            header = new CBlockHeader(wBytes.Get(80));

            // Parse transactions list
            vtx = CTransaction.ReadTransactionsList(ref wBytes);

            // Read block signature
            signature = wBytes.Get((int)wBytes.GetVarInt());
		}

        public CBlock()
        {
            // Initialize empty array of transactions. Please note that such 
            // configuration is not valid real block since it has to provide 
            // at least one transaction.
            vtx = new CTransaction[0];
        }

        /// <summary>
        /// Is this a Proof-of-Stake block?
        /// </summary>
        public bool IsProofOfStake
        {
            get
            {
                return (vtx.Length > 1 && vtx[1].IsCoinStake);
            }
        }

        public bool SignatureOK
        {
            get
            {
                IList<IEnumerable<byte>> solutions;
                txnouttype whichType;

                if (IsProofOfStake)
                {
                    if (signature.Length == 0)
                    {
                        return false; // No signature
                    }

                    if (!ScriptCode.Solver(vtx[1].vout[1].scriptPubKey, out whichType, out solutions))
                    {
                        return false; // No solutions found
                    }

                    if (whichType == txnouttype.TX_PUBKEY)
                    {
                        CPubKey pubkey;

                        try
                        {
                            pubkey = new CPubKey(solutions[0]);
                        }
                        catch (Exception)
                        {
                            return false; // Error while loading public key
                        }

                        return pubkey.VerifySignature(header.Hash, signature);
                    }
                }
                else
                {
                    // Proof-of-Work blocks have no signature

                    return true;
                }

                return false;
            }
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
            sb.AppendFormat("signatureOK={0})\n", SignatureOK);


            // TODO
            return sb.ToString();
        }
	}
}

