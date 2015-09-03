/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


namespace Novacoin
{
    internal class BignumHelper
    {
        const ulong Base = 0x100000000;

        #region Division function from Mono framework

        public static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r)
        {
            int m = u.Length;
            int n = v.Length;

            if (n <= 1)
            {
                //  Divide by single digit
                //
                ulong rem = 0;
                uint v0 = v[0];
                q = new uint[m];
                r = new uint[1];

                for (int j = m - 1; j >= 0; j--)
                {
                    rem *= Base;
                    rem += u[j];

                    ulong div = rem / v0;
                    rem -= div * v0;
                    q[j] = (uint)div;
                }
                r[0] = (uint)rem;
            }
            else if (m >= n)
            {
                int shift = GetNormalizeShift(v[n - 1]);

                uint[] un = new uint[m + 1];
                uint[] vn = new uint[n];

                Normalize(u, m, un, shift);
                Normalize(v, n, vn, shift);

                q = new uint[m - n + 1];
                r = null;

                //  Main division loop
                //
                for (int j = m - n; j >= 0; j--)
                {
                    ulong rr, qq;
                    int i;

                    rr = Base * un[j + n] + un[j + n - 1];
                    qq = rr / vn[n - 1];
                    rr -= qq * vn[n - 1];

                    for (;;)
                    {
                        // Estimate too big ?
                        //
                        if ((qq >= Base) || (qq * vn[n - 2] > (rr * Base + un[j + n - 2])))
                        {
                            qq--;
                            rr += (ulong)vn[n - 1];
                            if (rr < Base)
                                continue;
                        }
                        break;
                    }


                    //  Multiply and subtract
                    //
                    long b = 0;
                    long t = 0;
                    for (i = 0; i < n; i++)
                    {
                        ulong p = vn[i] * qq;
                        t = (long)un[i + j] - (long)(uint)p - b;
                        un[i + j] = (uint)t;
                        p >>= 32;
                        t >>= 32;
                        b = (long)p - t;
                    }
                    t = (long)un[j + n] - b;
                    un[j + n] = (uint)t;

                    //  Store the calculated value
                    //
                    q[j] = (uint)qq;

                    //  Add back vn[0..n] to un[j..j+n]
                    //
                    if (t < 0)
                    {
                        q[j]--;
                        ulong c = 0;
                        for (i = 0; i < n; i++)
                        {
                            c = (ulong)vn[i] + un[j + i] + c;
                            un[j + i] = (uint)c;
                            c >>= 32;
                        }
                        c += (ulong)un[j + n];
                        un[j + n] = (uint)c;
                    }
                }

                Unnormalize(un, out r, shift);
            }
            else
            {
                q = new uint[] { 0 };
                r = u;
            }
        }

        private static int GetNormalizeShift(uint value)
        {
            int shift = 0;

            if ((value & 0xFFFF0000) == 0) { value <<= 16; shift += 16; }
            if ((value & 0xFF000000) == 0) { value <<= 8; shift += 8; }
            if ((value & 0xF0000000) == 0) { value <<= 4; shift += 4; }
            if ((value & 0xC0000000) == 0) { value <<= 2; shift += 2; }
            if ((value & 0x80000000) == 0) { value <<= 1; shift += 1; }

            return shift;
        }

        private static void Unnormalize(uint[] un, out uint[] r, int shift)
        {
            int length = un.Length;
            r = new uint[length];

            if (shift > 0)
            {
                int lshift = 32 - shift;
                uint carry = 0;
                for (int i = length - 1; i >= 0; i--)
                {
                    uint uni = un[i];
                    r[i] = (uni >> shift) | carry;
                    carry = (uni << lshift);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    r[i] = un[i];
                }
            }
        }

        private static void Normalize(uint[] u, int l, uint[] un, int shift)
        {
            uint carry = 0;
            int i;
            if (shift > 0)
            {
                int rshift = 32 - shift;
                for (i = 0; i < l; i++)
                {
                    uint ui = u[i];
                    un[i] = (ui << shift) | carry;
                    carry = ui >> rshift;
                }
            }
            else
            {
                for (i = 0; i < l; i++)
                {
                    un[i] = u[i];
                }
            }

            while (i < un.Length)
            {
                un[i++] = 0;
            }

            if (carry != 0)
            {
                un[l] = carry;
            }
        }
        #endregion

    }
}
