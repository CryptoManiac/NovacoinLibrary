/**
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
using System.Diagnostics.Contracts;
using System.IO;

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
		public uint nVersion;

		/// <summary>
		/// Previous block hash.
		/// </summary>
		public ScryptHash256 prevHash;

		/// <summary>
		/// Merkle root hash.
		/// </summary>
		public Hash256 merkleRoot;

		/// <summary>
		/// Block timestamp.
		/// </summary>
		public uint nTime;

		/// <summary>
		/// Compressed difficulty representation.
		/// </summary>
		public uint nBits;

		/// <summary>
		/// Nonce counter.
		/// </summary>
		public uint nNonce;

        /// <summary>
        /// Initialize an empty instance
        /// </summary>
		public CBlockHeader ()
		{
            nVersion = 6;
            prevHash = new ScryptHash256();
            merkleRoot = new Hash256();
            nTime = Interop.GetTime();
            nBits = 0;
            nNonce = 0;
		}

        public CBlockHeader(CBlockHeader h)
        {
            nVersion = h.nVersion;
            prevHash = new ScryptHash256(h.prevHash);
            merkleRoot = new Hash256(h.merkleRoot);
            nTime = h.nTime;
            nBits = h.nBits;
            nNonce = h.nNonce;
        }

        internal CBlockHeader(ref BinaryReader reader)
        {
            nVersion = reader.ReadUInt32();
            prevHash = new ScryptHash256(reader.ReadBytes(32));
            merkleRoot = new Hash256(reader.ReadBytes(32));
            nTime = reader.ReadUInt32();
            nBits = reader.ReadUInt32();
            nNonce = reader.ReadUInt32();
        }

        /// <summary>
        /// Init block header with bytes.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        public CBlockHeader(byte[] bytes)
        {
            Contract.Requires<ArgumentException>(bytes.Length == 80, "Any valid block header is exactly 80 bytes long.");

            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);

            nVersion = reader.ReadUInt32();
            prevHash = new ScryptHash256(reader.ReadBytes(32));
            merkleRoot = new Hash256(reader.ReadBytes(32));
            nTime = reader.ReadUInt32();
            nBits = reader.ReadUInt32();
            nNonce = reader.ReadUInt32();

            reader.Close();
        }

        /// <summary>
        /// Convert current block header instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public static implicit operator byte[] (CBlockHeader h)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(h.nVersion);
            writer.Write(h.prevHash);
            writer.Write(h.merkleRoot);
            writer.Write(h.nTime);
            writer.Write(h.nBits);
            writer.Write(h.nNonce);

            var resultBytes = stream.ToArray();

            writer.Close();

            return resultBytes;
        }

        public ScryptHash256 Hash
        {
            get {
                return ScryptHash256.Compute256(this);
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
