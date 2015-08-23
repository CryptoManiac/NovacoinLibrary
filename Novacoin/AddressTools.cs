using System;
using System.Linq;
using System.Text;

using Org.BouncyCastle.Math;

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
        private const string strDigits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly BigInteger _base = BigInteger.ValueOf(58);

        /// <summary>
        /// Encode a byte sequence as a base58-encoded string
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Encoding result</returns>
        public static string Base58Encode(byte[] input)
        {
            // TODO: This could be a lot more efficient.
            var bi = new BigInteger(1, input);
            var s = new StringBuilder();
            while (bi.CompareTo(_base) >= 0)
            {
                var mod = bi.Mod(_base);
                s.Insert(0, new[] { strDigits[mod.IntValue] });
                bi = bi.Subtract(mod).Divide(_base);
            }
            s.Insert(0, new[] { strDigits[bi.IntValue] });
            // Convert leading zeros too.
            foreach (var anInput in input)
            {
                if (anInput == 0)
                    s.Insert(0, new[] { strDigits[0] });
                else
                    break;
            }
            return s.ToString();
        }


        /// <summary>
        /// Encode a byte sequence to a base58-encoded string, including checksum
        /// </summary>
        /// <param name="bytes">Byte sequence</param>
        /// <returns>Base58(data+checksum)</returns>
        public static string Base58EncodeCheck(byte[] bytes)
        {
            var dataBytes = new byte[bytes.Length + 4];
            bytes.CopyTo(dataBytes, 0);
            var checkSum = Hash256.Compute256(bytes).hashBytes.Take(4).ToArray();
            checkSum.CopyTo(dataBytes, dataBytes.Length - 4); // add 4-byte hash check to the end

            return Base58Encode(dataBytes);
        }

        /// <summary>
        /// // Decode a base58-encoded string into byte array
        /// </summary>
        /// <param name="strBase58">Base58 data string</param>
        /// <returns>Byte array</returns>
        public static byte[] Base58Decode(string input)
        {
            var bytes = DecodeToBigInteger(input).ToByteArray();
            // We may have got one more byte than we wanted, if the high bit of the next-to-last byte was not zero. This
            // is because BigIntegers are represented with twos-compliment notation, thus if the high bit of the last
            // byte happens to be 1 another 8 zero bits will be added to ensure the number parses as positive. Detect
            // that case here and chop it off.
            var stripSignByte = bytes.Length > 1 && bytes[0] == 0 && bytes[1] >= 0x80;
            // Count the leading zeros, if any.
            var leadingZeros = 0;
            for (var i = 0; input[i] == strDigits[0]; i++)
            {
                leadingZeros++;
            }
            var tmp = new byte[bytes.Length - (stripSignByte ? 1 : 0) + leadingZeros];
            Array.Copy(bytes, stripSignByte ? 1 : 0, tmp, leadingZeros, tmp.Length - leadingZeros);
            return tmp;
        }

        public static BigInteger DecodeToBigInteger(string input)
        {
            var bi = BigInteger.ValueOf(0);
            // Work backwards through the string.
            for (var i = input.Length - 1; i >= 0; i--)
            {
                var alphaIndex = strDigits.IndexOf(input[i]);
                if (alphaIndex == -1)
                {
                    throw new FormatException("Illegal character " + input[i] + " at " + i);
                }
                bi = bi.Add(BigInteger.ValueOf(alphaIndex).Multiply(_base.Pow(input.Length - 1 - i)));
            }
            return bi;
        }

        public static byte[] Base58DecodeCheck(string strBase58Check)
        {
            var rawData = Base58Decode(strBase58Check).ToArray();

            if (rawData.Length < 4)
            {
                throw new Base58Exception("Data is too short.");
            }

            var result = new byte[rawData.Length - 4];
            var resultCheckSum = new byte[4];

            Array.Copy(rawData, result, result.Length);
            Array.Copy(rawData, result.Length, resultCheckSum, 0, 4);

            var checkSum = Hash256.Compute256(result).hashBytes.Take(4).ToArray();

            if (!checkSum.SequenceEqual(resultCheckSum))
            {
                throw new Base58Exception("Incorrect checksum.");
            }

            return result;
        }
    }
}
