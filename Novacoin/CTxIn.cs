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
using System.IO;

namespace Novacoin
{
    [Serializable]
    public class TxInConstructorException : Exception
    {
        public TxInConstructorException()
        {
        }

        public TxInConstructorException(string message)
                : base(message)
        {
        }

        public TxInConstructorException(string message, Exception inner)
                : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Transaction input.
    /// </summary>
    public class CTxIn
	{
        /// <summary>
        /// Previous input data
        /// </summary>
        public COutPoint prevout;

		/// <summary>
		/// First half of script, signatures for the scriptPubKey
		/// </summary>
        public CScript scriptSig;

		/// <summary>
		/// Transaction variant number, irrelevant if nLockTime isn't specified. Its value is 0xffffffff by default.
		/// </summary>
        public uint nSequence = uint.MaxValue;

        /// <summary>
        /// Initialize new CTxIn instance as copy of another one.
        /// </summary>
        /// <param name="i">CTxIn instance.</param>
        public CTxIn(CTxIn i)
        {
            prevout = new COutPoint(i.prevout);
            scriptSig = i.scriptSig;
            nSequence = i.nSequence;
        }

        /// <summary>
        /// Initialize an empty instance of CTxIn class
        /// </summary>
        public CTxIn()
        {
            prevout = new COutPoint();
            scriptSig = new CScript();
        }

        /// <summary>
        /// Read vin list from byte sequence.
        /// </summary>
        /// <param name="wBytes">Reference to binary reader</param>
        /// <returns>Inputs array</returns>
        internal static CTxIn[] ReadTxInList(ref BinaryReader reader)
        {
            try
            {
                // Get amount
                int nInputs = (int)VarInt.ReadVarInt(ref reader);
                var vin = new CTxIn[nInputs];

                for (int nIndex = 0; nIndex < nInputs; nIndex++)
                {
                    // Fill inputs array
                    vin[nIndex] = new CTxIn();
                    vin[nIndex].prevout = new COutPoint(reader.ReadBytes(36));
                    vin[nIndex].scriptSig = new CScript(reader.ReadBytes((int)VarInt.ReadVarInt(ref reader)));
                    vin[nIndex].nSequence = reader.ReadUInt32();
                }

                // Return inputs array
                return vin;
            }
            catch (Exception e)
            {
                throw new TxInConstructorException("Desirealization failed.", e);
            }
        }

        /// <summary>
        /// Serialized size
        /// </summary>
        public int Size
        {
            get {
                int nSize = 40; // COutPoint, nSequence
                nSize += VarInt.GetEncodedSize(scriptSig.Size);
                nSize += scriptSig.Size;

                return nSize;
            }
        }

        /// <summary>
        /// Get raw bytes representation of our input.
        /// </summary>
        /// <returns>Byte sequence.</returns>
        public static implicit operator byte[] (CTxIn input)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(input.prevout); // prevout
            writer.Write(VarInt.EncodeVarInt(input.scriptSig.Size)); // scriptSig length
            writer.Write(input.scriptSig); // scriptSig
            writer.Write(input.nSequence); // nSequence

            var inputBytes = stream.ToArray();
            writer.Close();
            return inputBytes;
        }

        public bool IsFinal
        {
            get { return (nSequence == uint.MaxValue); }
        }
        public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

            sb.AppendFormat("CTxIn(");
            sb.Append(prevout.ToString());

            if(prevout.IsNull)
            {
                sb.AppendFormat(", coinbase={0}", Interop.ToHex((byte[])scriptSig));
            }
            else
            {
                sb.AppendFormat(", scriptsig={0}", scriptSig.ToString());
            }

            if (nSequence != uint.MaxValue)
            {
                sb.AppendFormat(", nSequence={0}", nSequence);
            }

            sb.Append(")");


            return sb.ToString ();
		}

	}
}
