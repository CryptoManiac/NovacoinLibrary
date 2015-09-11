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

using System.Text;
using System.IO;

namespace Novacoin
{
    /// <summary>
    /// Transaction output.
    /// </summary>
    public class CTxOut
	{
		/// <summary>
		/// Input value.
		/// </summary>
        public long nValue = unchecked((long)0xffffffffffffffff);

		/// <summary>
		/// Second half of script which contains spending instructions.
		/// </summary>
        public CScript scriptPubKey;

        /// <summary>
        /// Initialize new outpoint using provided value and script.
        /// </summary>
        /// <param name="nValue">Input value</param>
        /// <param name="scriptPubKey">Spending instructions.</param>
        public CTxOut(long nValue, CScript scriptPubKey)
        {
            this.nValue = nValue;
            this.scriptPubKey = scriptPubKey;
        }

        /// <summary>
        /// Initialize new CTxOut instance as a copy of another instance.
        /// </summary>
        /// <param name="o">CTxOut instance.</param>
        public CTxOut(CTxOut o)
        {
            nValue = o.nValue;
            scriptPubKey = o.scriptPubKey;
        }

        /// <summary>
        /// Initialize an empty instance of CTxOut class
        /// </summary>
        public CTxOut()
        {
            SetEmpty();
        }

        /// <summary>
        /// Read vout list from byte sequence.
        /// </summary>
        /// <param name="wBytes">Reference to binary reader</param>
        /// <returns>Outputs array</returns>
        internal static CTxOut[] ReadTxOutList(ref BinaryReader reader)
        {
            int nOutputs = (int)VarInt.ReadVarInt(ref reader);
            var vout =new CTxOut[nOutputs];

            for (int nIndex = 0; nIndex < nOutputs; nIndex++)
            {
                // Fill outputs array
                vout[nIndex] = new CTxOut();
                vout[nIndex].nValue = reader.ReadInt64();

                int nScriptPKLen = (int)VarInt.ReadVarInt(ref reader);
                vout[nIndex].scriptPubKey = new CScript(reader.ReadBytes(nScriptPKLen));
            }

            return vout;
        }

        /// <summary>
        /// Deserialize outputs array.
        /// </summary>
        /// <param name="outBytes">Byte array</param>
        /// <returns>Outputs array</returns>
        public static CTxOut[] DeserializeOutputsArray(byte[] outBytes)
        {
            var stream = new MemoryStream(outBytes);
            var reader = new BinaryReader(stream);

            CTxOut[] result = ReadTxOutList(ref reader);

            reader.Close();

            return result;
        }

        /// <summary>
        /// Create serialized representation of outputs array.
        /// </summary>
        /// <param name="vout">Outputs array</param>
        /// <returns>Byte array</returns>
        public static byte[] SerializeOutputsArray(CTxOut[] vout)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(VarInt.EncodeVarInt(vout.Length));

            foreach (var o in vout)
            {
                writer.Write(o);
            }

            var resultBytes = stream.ToArray();

            writer.Close();

            return resultBytes;
        }


        /// <summary>
        /// Get raw bytes representation of our output.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public static implicit operator byte[] (CTxOut output)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(output.nValue); // txout value
            writer.Write(VarInt.EncodeVarInt(output.scriptPubKey.Size)); // scriptPubKey length
            writer.Write(output.scriptPubKey);  // scriptPubKey

            var resultBytes = stream.ToArray();

            writer.Close();

            return resultBytes;
        }

        /// <summary>
        /// Null prevouts have -1 value
        /// </summary>
        public void SetNull()
        {
            nValue = unchecked((long)0xffffffffffffffff);
            scriptPubKey = new CScript();
        }

        /// <summary>
        /// Empty outputs have zero value and empty scriptPubKey
        /// </summary>
        public void SetEmpty()
        {
            nValue = 0;
            scriptPubKey = new CScript();
        }

        public bool IsNull
        {
            get { return nValue == unchecked((long)0xffffffffffffffff); }
        }

        public bool IsEmpty
        {
           get { return nValue == 0 && scriptPubKey.IsNull; }
        }

        /// <summary>
        /// Serialized size
        /// </summary>
        public uint Size
        {
            get
            {
                var nScriptSize = scriptPubKey.Size;
                return 8 + VarInt.GetEncodedSize(nScriptSize) + nScriptSize;
            }
        }

        public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.AppendFormat ("CTxOut(nValue={0}, scriptPubKey={1})", nValue, scriptPubKey.ToString());

			return sb.ToString ();
		}
	}
}

