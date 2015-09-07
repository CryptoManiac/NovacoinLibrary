using System;

namespace Novacoin
{
    public class Checkpoints
    {
        private static Tuple<uint, uint256, uint>[] checkpoints = new Tuple<uint, uint256, uint>[]
            {
                new Tuple<uint, uint256, uint>(0, NetInfo.nHashGenesisBlock, 1360105017)
            };

        /// <summary>
        /// Last checkpoint height.
        /// </summary>
        public static uint TotalBlocksEstimate { get { return checkpoints[checkpoints.Length - 1].Item1; } }

        /// <summary>
        /// Last checkpoint timestamp.
        /// </summary>
        public static uint LastCheckpointTime { get { return checkpoints[checkpoints.Length - 1].Item3; } }

        /// <summary>
        /// Block hash verification.
        /// </summary>
        /// <param name="nHeight">Block height.</param>
        /// <param name="nBlockHash">Block hash.</param>
        /// <returns></returns>
        public static bool Verify(uint nHeight, uint256 nBlockHash)
        {
            foreach (var checkpoint in checkpoints)
            {
                if (checkpoint.Item1 == nHeight)
                {
                    return nBlockHash == checkpoint.Item2;
                }
            }

            return true;
        }
    }
}