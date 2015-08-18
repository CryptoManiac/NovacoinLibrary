﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

namespace Novacoin
{
    public class AddressTools
    {
        public static string Base58Encode(byte[] bytes)
        {
            const string strDigits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
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
            for (int i = 0; i < nBytes && bytes[i] == 0; ++i)
            {
                strResult = strDigits[0] + strResult;
            }

            return strResult;
        }

        public static string Base58EncodeCheck(byte[] bytes)
        {
            byte[] dataBytes = new byte[bytes.Length + 4];
            byte[] checkSum = Hash256.Compute256(bytes).hashBytes.Take(4).ToArray();

            bytes.CopyTo(dataBytes, 0);
            checkSum.CopyTo(dataBytes, bytes.Length);

            return Base58Encode(dataBytes);
        }
    }
}
