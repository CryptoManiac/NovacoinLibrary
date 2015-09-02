using System;

namespace Novacoin
{
    internal class NetUtils
    {
        public static uint256 nProofOfWorkLimit = ~(new uint256(0)) >> 20;        // "standard" scrypt target limit for proof of work, results with 0,000244140625 proof-of-work difficulty
        public static uint256 nProofOfStakeLegacyLimit = ~(new uint256(0)) >> 24; // proof of stake target limit from block #15000 and until 20 June 2013, results with 0,00390625 proof of stake difficulty

        public static uint256 nProofOfStakeLimit = ~(new uint256(0)) >> 27;       // proof of stake target limit since 20 June 2013, equal to 0.03125  proof of stake difficulty
        public static uint256 nProofOfStakeHardLimit = ~(new uint256(0)) >> 30;   // disabled temporarily, will be used in the future to fix minimal proof of stake difficulty at 0.25

        public static uint256 nPoWBase = new uint256("00000000ffff0000000000000000000000000000000000000000000000000000"); // difficulty-1 target

        public static readonly uint nLockTimeThreshold = 500000000;
        private static readonly uint nDrift = 7200;

        public static uint GetAdjustedTime()
        {
            return Interop.GetTime();
        }

        public static uint FutureDrift(uint nTime)
        {
            return nTime + nDrift; // up to 2 hours from the future
        }

        public static uint PastDrift(uint nTime)
        {
            return nTime - nDrift; // up to 2 hours from the past
        }
    }
}