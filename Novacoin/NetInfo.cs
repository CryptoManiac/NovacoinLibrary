namespace Novacoin
{
    /// <summary>
    /// Basic network params.
    /// </summary>
    internal class NetInfo
    {
        /// <summary>
        /// Minimal depth for spending coinbase and coinstake transactions.
        /// </summary>
        public const int nGeneratedMaturity = 500;

        /// <summary>
        /// "standard" scrypt target limit for proof of work, results with 0,000244140625 proof-of-work difficulty
        /// </summary>
        public static uint256 nProofOfWorkLimit = ~(new uint256(0)) >> 20;

        /// <summary>
        /// Proof of stake target limit from block #15000 and until 20 June 2013, results with 0,00390625 proof of stake difficulty
        /// </summary>
        public static uint256 nProofOfStakeLegacyLimit = ~(new uint256(0)) >> 24;

        /// <summary>
        /// Proof of stake target limit since 20 June 2013, equal to 0.03125  proof of stake difficulty
        /// </summary>
        public static uint256 nProofOfStakeLimit = ~(new uint256(0)) >> 27;

        /// <summary>
        /// Disabled temporarily, will be used in the future to fix minimal proof of stake difficulty at 0.25
        /// </summary>
        public static uint256 nProofOfStakeHardLimit = ~(new uint256(0)) >> 30;

        /// <summary>
        /// Difficulty-1 target
        /// </summary>
        public static uint256 nPoWBase = new uint256("00000000ffff0000000000000000000000000000000000000000000000000000");

        /// <summary>
        /// Fri, 20 Sep 2013 00:00:00 GMT
        /// </summary>
        public const uint nChainChecksSwitchTime = 1379635200;

        /// <summary>
        /// Sat, 20 Jul 2013 00:00:00 GMT
        /// </summary>
        public const uint nTargetsSwitchTime = 1374278400;

        /// <summary>
        /// Wed, 20 Aug 2014 00:00:00 GMT
        /// </summary>
        public const uint nStakeValidationSwitchTime = 1408492800;

        /// <summary>
        /// Hash of block #0
        /// </summary>
        public static uint256 nHashGenesisBlock = new uint256("00000a060336cbb72fe969666d337b87198b1add2abaa59cca226820b32933a4");

        public const uint nLockTimeThreshold = 500000000;

        /// <summary>
        /// Allowed clock drift.
        /// </summary>
        private const uint nDrift = 7200;

        /// <summary>
        /// Maximum possible proof-of-work reward.
        /// </summary>
        public const long nMaxMintProofOfWork = CTransaction.nCoin * 100;

        /// <summary>
        /// Maximum possible proof-of-stake reward per coin*year.
        /// </summary>
        public const long nMaxMintProofOfStake = CTransaction.nCoin * 100;

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

        internal static uint256 GetProofOfStakeLimit(uint nHeight, uint nTime)
        {
            if (nTime > nTargetsSwitchTime) // 27 bits since 20 July 2013
                return nProofOfStakeLimit;
            if (nHeight + 1 > 15000) // 24 bits since block 15000
                return nProofOfStakeLegacyLimit;
            if (nHeight + 1 > 14060) // 31 bits since block 14060 until 15000
                return nProofOfStakeHardLimit;

            return nProofOfWorkLimit; // return bnProofOfWorkLimit of none matched
        }
    }
}