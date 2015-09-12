using System;

namespace Novacoin
{
    public static class HashCheckpoints
    {
        private static Tuple<uint, uint256, uint>[] checkpoints = new Tuple<uint, uint256, uint>[]
            {
                new Tuple<uint, uint256, uint>(0, NetInfo.nHashGenesisBlock, 1360105017),
                new Tuple<uint, uint256, uint>(200000, new uint256("0000000000029f8bbf66e6ea6f3e5db55009404aae0fe395a53dd33142b2bff2"), 1441127233),
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

    public static class ModifierCheckpoints
    {
        /// <summary>
        /// Stake modifier checkpoints
        /// </summary>
        private static Tuple<uint, uint>[] modifierCheckpoints = new Tuple<uint, uint>[]
            {
                new Tuple<uint, uint>( 0, 0x0e00670bu ),
                new Tuple<uint, uint>(200000, 0x01ec1503u )
            };

        /// <summary>
        /// Check stake modifier checkpoints.
        /// </summary>
        /// <param name="nHeight">Block height.</param>
        /// <param name="nStakeModifierChecksum">Modifier checksum value.</param>
        /// <returns>Result</returns>
        public static bool Verify(uint nHeight, uint nStakeModifierChecksum)
        {
            foreach (var checkpoint in modifierCheckpoints)
            {
                if (checkpoint.Item1 == nHeight)
                {
                    return checkpoint.Item2 == nStakeModifierChecksum;
                }
            }

            return true;
        }
    }
}