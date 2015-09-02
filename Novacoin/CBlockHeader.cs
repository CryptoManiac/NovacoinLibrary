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
		public uint256 prevHash;

		/// <summary>
		/// Merkle root hash.
		/// </summary>
		public uint256 merkleRoot;

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
            prevHash = new uint256();
            merkleRoot = new uint256();
            nTime = Interop.GetTime();
            nBits = 0;
            nNonce = 0;
		}

        public CBlockHeader(CBlockHeader header)
        {
            nVersion = header.nVersion;
            prevHash = header.prevHash;
            merkleRoot = header.merkleRoot;
            nTime = header.nTime;
            nBits = header.nBits;
            nNonce = header.nNonce;
        }

        internal CBlockHeader(ref BinaryReader reader)
        {
            nVersion = reader.ReadUInt32();
            prevHash = reader.ReadBytes(32);
            merkleRoot = reader.ReadBytes(32);
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
            prevHash = reader.ReadBytes(32);
            merkleRoot = reader.ReadBytes(32);
            nTime = reader.ReadUInt32();
            nBits = reader.ReadUInt32();
            nNonce = reader.ReadUInt32();

            reader.Close();
        }

        /// <summary>
        /// Convert current block header instance into sequence of bytes
        /// </summary>
        /// <returns>Byte sequence</returns>
        public static implicit operator byte[] (CBlockHeader header)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(header.nVersion);
            writer.Write(header.prevHash);
            writer.Write(header.merkleRoot);
            writer.Write(header.nTime);
            writer.Write(header.nBits);
            writer.Write(header.nNonce);

            var resultBytes = stream.ToArray();

            writer.Close();

            return resultBytes;
        }

        public uint256 Hash
        {
            get {
                return CryptoUtils.ComputeScryptHash256(this);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("CBlockHeader(nVersion={0}, prevHash={1}, merkleRoot={2}, nTime={3}, nBits={4}, nNonce={5})", nVersion, prevHash, merkleRoot, nTime, nBits, nNonce);
            return sb.ToString();
        }
	}
}
