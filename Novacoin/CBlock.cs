﻿/**
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

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

        /// <summary>
        /// Was this signed correctly?
        /// </summary>
        public bool SignatureOK
        {
            get
            {
                if (IsProofOfStake)
                {
                    if (signature.Length == 0)
                    {
                        return false; // No signature
                    }

                    txnouttype whichType;
                    IList<byte[]> solutions;

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
        /// Get current instance as sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public IList<byte> Bytes 
        {
            get
            {
                var r = new List<byte>();

                r.AddRange(header.Bytes);
                r.AddRange(VarInt.EncodeVarInt(vtx.LongLength)); // transactions count

                foreach (var tx in vtx)
                {
                    r.AddRange(tx.Bytes);
                }

                r.AddRange(VarInt.EncodeVarInt(signature.LongLength));
                r.AddRange(signature);

                return r;
            }
        }

        /// <summary>
        /// MErkle root
        /// </summary>
        public Hash256 hashMerkleRoot
        {
            get {
                
                var merkleTree = new List<byte>();

                foreach (var tx in vtx)
                {
                    merkleTree.AddRange(tx.Hash.hashBytes);
                }

                var hasher = new SHA256Managed();
                hasher.Initialize();

                int j = 0;
                for (int nSize = vtx.Length; nSize > 1; nSize = (nSize + 1) / 2)
                {
                    for (int i = 0; i < nSize; i += 2)
                    {
                        int i2 = Math.Min(i + 1, nSize - 1);

                        var pair = new List<byte>();

                        pair.AddRange(merkleTree.GetRange((j + i)*32, 32));
                        pair.AddRange(merkleTree.GetRange((j + i2)*32, 32));

                        var digest1 = hasher.ComputeHash(pair.ToArray());
                        var digest2 = hasher.ComputeHash(digest1);

                        merkleTree.AddRange(digest2);
                    }
                    j += nSize;
                }

                return (merkleTree.Count == 0) ? new Hash256() : new Hash256(merkleTree.GetRange(merkleTree.Count-32, 32));
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("CBlock(\n header={0},\n", header.ToString());

            foreach(var tx in vtx)
            {
                sb.AppendFormat("{0}", tx.ToString());
            }

            if (IsProofOfStake)
            {
                sb.AppendFormat(", signature={0}, signatureOK={1}\n", Interop.ToHex(signature), SignatureOK);
            }

            sb.Append(")");
            
            return sb.ToString();
        }
	}
}

