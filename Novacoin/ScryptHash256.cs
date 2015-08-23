/**
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Novacoin
{
    /// <summary>
    /// Representation of scrypt hash
    /// </summary>
    public class ScryptHash256 : Hash
    {
        // 32 bytes
        public override int hashSize
        {
            get { return 32; }
        }

        public ScryptHash256() : base() { }
        public ScryptHash256(byte[] bytes, int offset = 0) : base(bytes, offset) { }
        public ScryptHash256(IEnumerable<byte> bytes, int skip = 0) : base(bytes, skip) { }
        public ScryptHash256(ScryptHash256 h) : base(h) { }

        /// <summary>
        /// Calculate scrypt hash and return new instance of ScryptHash256 class
        /// </summary>
        /// <param name="inputBytes">Byte sequence to hash</param>
        /// <returns>Hashing result instance</returns>
        public static ScryptHash256 Compute256(IEnumerable<byte> inputBytes)
        {
            var V = new uint[(131072 + 63) / sizeof(uint)];

            var dataBytes = inputBytes.ToArray();
            var keyBytes1 = CryptoUtils.PBKDF2_Sha256(128, dataBytes, dataBytes, 1);
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
            var keyBytes2 = CryptoUtils.PBKDF2_Sha256(32, dataBytes, xBytes, 1);

            return new ScryptHash256(keyBytes2);
        }

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
    }
}
