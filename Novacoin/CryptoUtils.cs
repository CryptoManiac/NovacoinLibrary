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
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;

namespace Novacoin
{
    /// <summary>
    /// Hashing functionality.
    /// </summary>
    public class CryptoUtils
    {
        #region Private instances for various hashing algorithm implementations.
        /// <summary>
        /// Computes the SHA1 hash for the input data using the managed library.
        /// </summary>
        private static SHA1Managed _sha1 = new SHA1Managed();
        
        /// <summary>
        /// Computes the SHA256 hash for the input data using the managed library.
        /// </summary>
        private static SHA256Managed _sha256 = new SHA256Managed();

        /// <summary>
        /// Computes the SHA1 hash for the input data using the managed library.
        /// </summary>
        private static RIPEMD160Managed _ripe160 = new RIPEMD160Managed();
        #endregion

        /// <summary>
        /// Sha1 calculation
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeSha1(byte[] inputBytes)
        {
            return _sha1.ComputeHash(inputBytes, 0, inputBytes.Length);
        }

        /// <summary>
        /// Sha256 calculation
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeSha256(byte[] inputBytes)
        {
            return _sha256.ComputeHash(inputBytes, 0, inputBytes.Length);
        }

        /// <summary>
        /// RIPEMD-160 calculation
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeRipeMD160(byte[] inputBytes)
        {
            return _ripe160.ComputeHash(inputBytes, 0, inputBytes.Length);
        }

        /// <summary>
        /// RipeMD160(Sha256(X)) calculation
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeHash160(byte[] inputBytes)
        {
            var digest1 = _sha256.ComputeHash(inputBytes, 0, inputBytes.Length);
            return _ripe160.ComputeHash(digest1, 0, digest1.Length);
        }

        /// <summary>
        /// Sha256(Sha256(X)) calculation
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeHash256(byte[] dataBytes)
        {
            var digest1 = _sha256.ComputeHash(dataBytes, 0, dataBytes.Length);
            return _sha256.ComputeHash(digest1, 0, digest1.Length);
        }

        /// <summary>
        /// Sha256(Sha256(X)) calculation
        /// </summary>
        /// <param name="input1">Reference to first half of data</param>
        /// <param name="input2">Reference to second half of data</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeHash256(ref byte[] input1, ref byte[] input2)
        {
            var buffer = new byte[input1.Length + input2.Length];

            // Fill the buffer
            input1.CopyTo(buffer, 0);
            input2.CopyTo(buffer, input1.Length);

            var digest1 = _sha256.ComputeHash(buffer, 0, buffer.Length);
            return _sha256.ComputeHash(digest1, 0, digest1.Length);
        }

        /// <summary>
        /// Calculate PBKDF2-SHA256(SALSA20/8(PBKDF2-SHA256(X)))
        /// </summary>
        /// <param name="inputBytes">Bytes to hash</param>
        /// <returns>Hashing result</returns>
        public static byte[] ComputeScryptHash256(byte[] inputBytes)
        {
            var V = new uint[(131072 + 63) / sizeof(uint)];

            var keyBytes1 = PBKDF2_Sha256(128, inputBytes, inputBytes, 1);
            var X = Interop.ToUInt32Array(keyBytes1);

            for (var i = 0; i < 1024; i++)
            {
                Array.Copy(X, 0, V, i * 32, 32);

                xor_salsa8(ref X, 0, ref X, 16);
                xor_salsa8(ref X, 16, ref X, 0);
            }
            for (var i = 0; i < 1024; i++)
            {
                var j = 32 * (X[16] & 1023);
                for (var k = 0; k < 32; k++)
                {
                    X[k] ^= V[j + k];
                }
                xor_salsa8(ref X, 0, ref X, 16);
                xor_salsa8(ref X, 16, ref X, 0);
            }

            var xBytes = Interop.LEBytes(X);

            return PBKDF2_Sha256(32, inputBytes, xBytes, 1);
        }


        #region PBKDF2-SHA256
        /// <summary>
        /// Managed implementation of PBKDF2-SHA256.
        /// </summary>
        /// <param name="dklen">Key length</param>
        /// <param name="password">Password</param>
        /// <param name="salt">Salt</param>
        /// <param name="iterationCount">Amount of derive iterations.</param>
        /// <returns>Derived key</returns>
        public static byte[] PBKDF2_Sha256(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            /* Init HMAC state. */
            using (var hmac = new HMACSHA256(password))
            {
                int hashLength = hmac.HashSize / 8;
                if ((hmac.HashSize & 7) != 0)
                {
                    hashLength++;
                }
                int keyLength = dklen / hashLength;
                if (dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                {
                    throw new ArgumentOutOfRangeException("dklen");
                }
                if (dklen % hashLength != 0)
                {
                    keyLength++;
                }
                var extendedkey = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
                using (var ms = new MemoryStream())
                {
                    /* Iterate through the blocks. */
                    for (int i = 0; i < keyLength; i++)
                    {
                        /* Generate INT(i + 1). */
                        extendedkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                        extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                        extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                        extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);

                        /* Compute U_1 = PRF(P, S || INT(i)). */
                        var u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, salt.Length, 4);

                        /* T_i = U_1 ... */
                        var f = u;
                        for (int j = 1; j < iterationCount; j++)
                        {
                            /* Compute U_j. */
                            u = hmac.ComputeHash(u);
                            for (int k = 0; k < f.Length; k++)
                            {
                                /* ... xor U_j ... */
                                f[k] ^= u[k];
                            }
                        }

                        /* Copy as many bytes as necessary into memory stream. */
                        ms.Write(f, 0, f.Length);
                        Array.Clear(u, 0, u.Length);
                        Array.Clear(f, 0, f.Length);
                    }
                    ms.Position = 0;

                    /* Initialize result array. */
                    var dk = new byte[dklen];

                    /* Read key from memory stream. */
                    ms.Read(dk, 0, dklen);

                    ms.Position = 0;
                    for (long i = 0; i < ms.Length; i++)
                    {
                        ms.WriteByte(0);
                    }
                    Array.Clear(extendedkey, 0, extendedkey.Length);
                    return dk;
                }
            }
        }
        #endregion

        #region SALSA20/8
        private static void xor_salsa8(ref uint[] B, int indexB, ref uint[] Bx, int indexBx)
        {
            uint x00, x01, x02, x03, x04, x05, x06, x07, x08, x09, x10, x11, x12, x13, x14, x15;
            byte i;

            x00 = (B[indexB + 0] ^= Bx[indexBx + 0]);
            x01 = (B[indexB + 1] ^= Bx[indexBx + 1]);
            x02 = (B[indexB + 2] ^= Bx[indexBx + 2]);
            x03 = (B[indexB + 3] ^= Bx[indexBx + 3]);
            x04 = (B[indexB + 4] ^= Bx[indexBx + 4]);
            x05 = (B[indexB + 5] ^= Bx[indexBx + 5]);
            x06 = (B[indexB + 6] ^= Bx[indexBx + 6]);
            x07 = (B[indexB + 7] ^= Bx[indexBx + 7]);
            x08 = (B[indexB + 8] ^= Bx[indexBx + 8]);
            x09 = (B[indexB + 9] ^= Bx[indexBx + 9]);
            x10 = (B[indexB + 10] ^= Bx[indexBx + 10]);
            x11 = (B[indexB + 11] ^= Bx[indexBx + 11]);
            x12 = (B[indexB + 12] ^= Bx[indexBx + 12]);
            x13 = (B[indexB + 13] ^= Bx[indexBx + 13]);
            x14 = (B[indexB + 14] ^= Bx[indexBx + 14]);
            x15 = (B[indexB + 15] ^= Bx[indexBx + 15]);

            Func<uint, int, uint> R = (a, b) => (((a) << (b)) | ((a) >> (32 - (b))));

            for (i = 0; i < 8; i += 2)
            {
                /* Operate on columns. */
                x04 ^= R(x00 + x12, 7); x09 ^= R(x05 + x01, 7);
                x14 ^= R(x10 + x06, 7); x03 ^= R(x15 + x11, 7);

                x08 ^= R(x04 + x00, 9); x13 ^= R(x09 + x05, 9);
                x02 ^= R(x14 + x10, 9); x07 ^= R(x03 + x15, 9);

                x12 ^= R(x08 + x04, 13); x01 ^= R(x13 + x09, 13);
                x06 ^= R(x02 + x14, 13); x11 ^= R(x07 + x03, 13);

                x00 ^= R(x12 + x08, 18); x05 ^= R(x01 + x13, 18);
                x10 ^= R(x06 + x02, 18); x15 ^= R(x11 + x07, 18);

                /* Operate on rows. */
                x01 ^= R(x00 + x03, 7); x06 ^= R(x05 + x04, 7);
                x11 ^= R(x10 + x09, 7); x12 ^= R(x15 + x14, 7);

                x02 ^= R(x01 + x00, 9); x07 ^= R(x06 + x05, 9);
                x08 ^= R(x11 + x10, 9); x13 ^= R(x12 + x15, 9);

                x03 ^= R(x02 + x01, 13); x04 ^= R(x07 + x06, 13);
                x09 ^= R(x08 + x11, 13); x14 ^= R(x13 + x12, 13);

                x00 ^= R(x03 + x02, 18); x05 ^= R(x04 + x07, 18);
                x10 ^= R(x09 + x08, 18); x15 ^= R(x14 + x13, 18);
            }

            B[indexB + 0] += x00;
            B[indexB + 1] += x01;
            B[indexB + 2] += x02;
            B[indexB + 3] += x03;
            B[indexB + 4] += x04;
            B[indexB + 5] += x05;
            B[indexB + 6] += x06;
            B[indexB + 7] += x07;
            B[indexB + 8] += x08;
            B[indexB + 9] += x09;
            B[indexB + 10] += x10;
            B[indexB + 11] += x11;
            B[indexB + 12] += x12;
            B[indexB + 13] += x13;
            B[indexB + 14] += x14;
            B[indexB + 15] += x15;
        }
        #endregion
    }
}
