using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Numerics;

namespace Novacoin
{
    public class Base58Exception : Exception
    {
        public Base58Exception()
        {
        }

        public Base58Exception(string message)
            : base(message)
        {
        }

        public Base58Exception(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    public class AddressTools
    {
        const string strDigits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Encode a byte sequence as a base58-encoded string
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Encoding result</returns>
        public static string Base58Encode(byte[] bytes)
        {
            string strResult = "";

            int nBytes = bytes.Length;
            BigInteger arrayToInt = 0;
            BigInteger encodeSize = strDigits.Length;

            for (int i = 0; i < nBytes; ++i)
            {
                arrayToInt = arrayToInt * 256 + bytes[i];
            }
            while (arrayToInt > 0)
            {
                int rem = (int)(arrayToInt % encodeSize);
                arrayToInt /= encodeSize;
                strResult = strDigits[rem] + strResult;
            }

            // Leading zeroes encoded as base58 zeros
            for (int i = 0; i < nBytes && bytes[i] == 0; ++i)
            {
                strResult = strDigits[0] + strResult;
            }

            return strResult;
        }

        /// <summary>
        /// Encode a byte sequence to a base58-encoded string, including checksum
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Base58(data+checksum)</returns>
        public static string Base58EncodeCheck(IEnumerable<byte> bytes)
        {
            byte[] dataBytes = bytes.ToArray();
            Array.Resize(ref dataBytes, dataBytes.Length + 4);

            byte[] checkSum = Hash256.Compute256(bytes).hashBytes.Take(4).ToArray();

            checkSum.CopyTo(dataBytes, dataBytes.Length - 4); // add 4-byte hash check to the end

            return Base58Encode(dataBytes);
        }

        /// <summary>
        /// // Decode a base58-encoded string into byte array
        /// </summary>
        /// <param name="strBase58">Base58 data string</param>
        /// <returns>Byte array</returns>
        public static IEnumerable<byte> Base58Decode(string strBase58)
        {
            // Remove whitespaces
            strBase58 = Regex.Replace(strBase58, @"s", "");

            BigInteger intData = 0;
            for (int i = 0; i < strBase58.Length; i++)
            {
                int digit = strDigits.IndexOf(strBase58[i]);

                if (digit < 0)
                {
                    throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", strBase58[i], i));
                }

                intData = intData * 58 + digit;
            }

            // Leading zero bytes get encoded as leading `1` characters
            int leadingZeroCount = strBase58.TakeWhile(c => c == '1').Count();

            IEnumerable<byte> leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            IEnumerable<byte> bytesWithoutLeadingZeros = intData.ToByteArray().Reverse().SkipWhile(b => b == 0);

            return leadingZeros.Concat(bytesWithoutLeadingZeros);
        }
        public static IEnumerable<byte> Base58DecodeCheck(string strBase58Check)
        {
            byte[] rawData = Base58Decode(strBase58Check).ToArray();

            if (rawData.Length < 4)
            {
                throw new Base58Exception("Data is too short.");
            }

            byte[] result = new byte[rawData.Length - 4];
            byte[] resultCheckSum = new byte[4];

            Array.Copy(rawData, result, result.Length);
            Array.Copy(rawData, result.Length, resultCheckSum, 0, 4);

            byte[] checkSum = Hash256.Compute256(result).hashBytes.Take(4).ToArray();

            if (!checkSum.SequenceEqual(resultCheckSum))
            {
                throw new Base58Exception("Incorrect checksum.");
            }

            return result;
        }
    }
}
