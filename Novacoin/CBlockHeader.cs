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
using System.Text;
using System.Collections.Generic;

namespace Novacoin
{
    /// <summary>
    /// Block header
    /// </summary>
    public class CBlockHeader
	{
		/// <summary>
		/// Version of block schema.
		/// </summary>
		public uint nVersion = 6;

		/// <summary>
		/// Previous block hash.
		/// </summary>
		public Hash256 prevHash = new Hash256();

		/// <summary>
		/// Merkle root hash.
		/// </summary>
		public Hash256 merkleRoot = new Hash256();

		/// <summary>
		/// Block timestamp.
		/// </summary>
		public uint nTime = 0;

		/// <summary>
		/// Compressed difficulty representation.
		/// </summary>
		public uint nBits = 0;

		/// <summary>
		/// Nonce counter.
		/// </summary>
		public uint nNonce = 0;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
		public CBlockHeader ()
		{
		}

        public CBlockHeader(CBlockHeader h)
        {
            nVersion = h.nVersion;
            prevHash = new Hash256(h.prevHash);
            merkleRoot = new Hash256(h.merkleRoot);
            nTime = h.nTime;
            nBits = h.nBits;
            nNonce = h.nNonce;
        }

        public CBlockHeader(byte[] bytes)
        {
            nVersion = BitConverter.ToUInt32(bytes, 0);
            prevHash = new Hash256(bytes, 4);
            merkleRoot = new Hash256(bytes, 36);
            nTime = BitConverter.ToUInt32(bytes, 68);
            nBits = BitConverter.ToUInt32(bytes, 72);
            nNonce = BitConverter.ToUInt32(bytes, 76);
        }

        /// <summary>
        /// Convert current block header instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public byte[] Bytes
        {
            get
            {
                var r = new List<byte>();

                r.AddRange(BitConverter.GetBytes(nVersion));
                r.AddRange(prevHash.hashBytes);
                r.AddRange(merkleRoot.hashBytes);
                r.AddRange(BitConverter.GetBytes(nTime));
                r.AddRange(BitConverter.GetBytes(nBits));
                r.AddRange(BitConverter.GetBytes(nNonce));

                return r.ToArray();
            }
        }

        public ScryptHash256 Hash
        {
            get {
                return ScryptHash256.Compute256(Bytes);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("CBlockHeader(nVersion={0}, prevHash={1}, merkleRoot={2}, nTime={3}, nBits={4}, nNonce={5})", nVersion, prevHash.ToString(), merkleRoot.ToString(), nTime, nBits, nNonce);
            return sb.ToString();
        }
	}
}
