using System;
using System.Security.Cryptography;

namespace Novacoin
{
    public class CryptoUtils
    {
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
                if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                {
                    throw new ArgumentOutOfRangeException("dklen");
                }
                if (dklen % hashLength != 0)
                {
                    keyLength++;
                }
                byte[] extendedkey = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
                using (var ms = new System.IO.MemoryStream())
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
                        byte[] u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, salt.Length, 4);

                        /* T_i = U_1 ... */
                        byte[] f = u;
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
                    byte[] dk = new byte[dklen];

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
    }
}
