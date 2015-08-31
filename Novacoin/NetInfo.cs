using System;

namespace Novacoin
{
    internal class NetUtils
    {
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