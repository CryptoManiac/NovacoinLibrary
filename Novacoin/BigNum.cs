using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Math;

namespace Novacoin
{
    /// <summary>
    /// Wrapper for bouncycastle's Biginteger class.
    /// </summary>
    public class BigNum : IComparable<BigNum>, IEquatable<BigNum>
    {
        /// <summary>
        /// Internal coby of Biginteger object.
        /// </summary>
        private BigInteger bn;

        #region Constructors
        public BigNum(BigInteger bnValue)
        {
            bn = bnValue;
        }

        public BigNum(byte[] dataBytes)
        {
            bn = new BigInteger(dataBytes);
        }

        public BigNum(ulong ulongValue)
        {
            bn = new BigInteger(BitConverter.GetBytes(ulongValue));
        }

        public BigNum(uint256 uint256Value)
        {
            bn = new BigInteger(uint256Value);
        }
        #endregion

        #region Basic arithmetics
        public static BigNum operator +(BigNum a, ulong b)
        {
            var bnValueToAdd = new BigInteger(BitConverter.GetBytes(b));
            return new BigNum(a.bn.Add(bnValueToAdd));
        }

        public static BigNum operator -(BigNum a, ulong b)
        {
            var bnValueToSubstract = new BigInteger(BitConverter.GetBytes(b));
            return new BigNum(a.bn.Subtract(bnValueToSubstract));
        }

        public static BigNum operator +(BigNum a, uint256 b)
        {
            var bnValueToAdd = new BigInteger(b);
            return new BigNum(a.bn.Add(bnValueToAdd));
        }

        public static BigNum operator -(BigNum a, uint256 b)
        {
            var bnValueToSubstract = new BigInteger(b);
            return new BigNum(a.bn.Subtract(bnValueToSubstract));
        }

        public static BigNum operator +(BigNum a, BigNum b)
        {
            return new BigNum(a.bn.Add(b.bn));
        }

        public static BigNum operator -(BigNum a, BigNum b)
        {
            return new BigNum(a.bn.Subtract(b.bn));
        }

        public static BigNum operator /(BigNum a, ulong b)
        {
            var bnDivider = new BigInteger(BitConverter.GetBytes(b));
            return new BigNum(a.bn.Divide(bnDivider));
        }

        public static BigNum operator /(BigNum a, uint256 b)
        {
            var bnDivider = new BigInteger(b);
            return new BigNum(a.bn.Divide(bnDivider));
        }

        public static BigNum operator /(BigNum a, BigNum b)
        {
            return new BigNum(a.bn.Divide(b.bn));
        }

        public static BigNum operator *(BigNum a, ulong b)
        {
            var bnMultiplier = new BigInteger(BitConverter.GetBytes(b));
            return new BigNum(a.bn.Multiply(bnMultiplier));
        }

        public static BigNum operator *(BigNum a, uint256 b)
        {
            var bnMultiplier = new BigInteger(b);
            return new BigNum(a.bn.Multiply(bnMultiplier));
        }

        public static BigNum operator *(BigNum a, BigNum b)
        {
            return new BigNum(a.bn.Multiply(b.bn));
        }
        #endregion

        #region Comparison operations
        public static bool operator <(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) < 0;
        }
        public static bool operator <=(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) <= 0;
        }

        public static bool operator >(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) > 0;
        }

        public static bool operator >=(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) >= 0;
        }

        public static bool operator ==(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) == 0;
        }

        public static bool operator !=(BigNum a, BigNum b)
        {
            return a.bn.CompareTo(b.bn) != 0;
        }

        #endregion

        #region Cast operators
        public static implicit operator BigNum(BigInteger bnValue)
        {
            return new BigNum(bnValue);
        }

        public static implicit operator BigNum(ulong ulongValue)
        {
            return new BigNum(ulongValue);
        }

        public static implicit operator BigNum(uint256 uint256Value)
        {
            return new BigNum(uint256Value);
        }

        public static implicit operator ulong (BigNum a)
        {
            return (ulong)a.bn.LongValue;
        }

        public int CompareTo(BigNum other)
        {
            return bn.CompareTo(other.bn);
        }

        public bool Equals(BigNum other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return this == other;
        }

        public override int GetHashCode()
        {
            return bn.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }

            return this == (obj as BigNum);
        }

        #endregion
    }
}
