using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacoin
{
    public class Entropy
    {
        /// <summary>
        /// Pregenerated table of entropy bits (from genesis to #9689)
        ///
        /// Bits are packed into array of 256 bit integers:
        ///
        /// * array index calculated as nHeight / 256
        /// * position of bit is calculated as nHeight & 0xFF.
        ///
        /// </summary>
        private static uint256[] entropyStore = 
        {
            new uint256("4555b4dcc1d690ddd9b810c90c66e82b18bf4f43cc887246c418383ec120a5ab"),
            new uint256("aa6d1198412fa77608addf6549c9198a22155e8afd7a9ded6179f6b7cfc66b0c"),
            new uint256("9442fabfa4116fb14a9769c2eea003845a1f5c3a0260f36b497d68f3a3cd4078"),
            new uint256("0e769042a9a98e42388195d699574b822d06515f7053ad884c53d7ee059f05b1"),
            new uint256("7005aac20baf70251aebfe3f1b95987d83ef1e3e6963de8fed601d4dd07bf7cf"),
            new uint256("58952c5c3de188f2e33c38d3f53d7bf44f9bc545a4289d266696273fa821be66"),
            new uint256("50b6c2ed780c08aaec3f7665b1b6004206243e3866456fc910b83b52d07eeb63"),
            new uint256("563841eefca85ba3384986c58100408ae3f1ba2ac727e1ac910ce154a06c702f"),
            new uint256("79275b03938b3e27a9b01a7f7953c6c487c58355f5d4169accfbb800213ffd13"),
            new uint256("d783f2538b3ed18f135af90adc687c5646d93aeaeaabc6667be94f7aa0a2d366"),
            new uint256("b441d0c175c40c8e88b09d88ea008af79cbed2d28219427d2e72fda682974db8"),
            new uint256("3204c43bd41f2e19628af3b0c9aca3db15bca4c8705d51056e7b17a319c04715"),
            new uint256("7e80e6ab7857d8f2f261a0a49c783bd800b365b8c9b85cc0e13f73904b0dcaa9"),
            new uint256("efaaee60ed82d2ad145c0e347941fdb131eb8fd289a45eef07121a93f283c5f1"),
            new uint256("3efc86e4334da332c1fd4c12513c40cff689f3efdc7f9913230822adacdda4f9"),
            new uint256("f0d6b8f38599a017fa35d1fbbf9ef51eca5ebc5b286aadba40c4c3e1d9bace0c"),
            new uint256("286a67f27323486036a0a92d35382fc8963c0c00bad331723318b4b9fdb2b56e"),
            new uint256("ecbfaaa6567c54f08c4d5bd0118a2d7b58740f42cbfc73aa1536c1f5f76de87c"),
            new uint256("f9a4de1c5c46520de5aaf10d3796cf0e27ddce98b3398357f5726a949664e308"),
            new uint256("d75e6c4dc4be08401e3478d2467d9ab96a62af4f255c04a82c41af0de0a487bb"),
            new uint256("1a82c3bc6ad6047294c16571b5e2b7316c97bf8813e7da15798b9820d67e39f2"),
            new uint256("b49be0080de564e01829ded7e7971979565a741c5975dc9978dcc020192d396c"),
            new uint256("0d8eed113be67663b5a15a0625a9b49792b5ea59c005c4f405914877acab7000"),
            new uint256("8f9d46e2bc05a218ffa942965b747056197d393b097085523640cd59e07fe7c7"),
            new uint256("7a63ab40bc7f40ac2ebe9ede438d97b45fa6ed6f8419016da8d5f7a670111dda"),
            new uint256("63fbcc080448c43d6cf915c958314feff7a95a52ba43a68c05fc281d3a522d25"),
            new uint256("f834cf824c326d3ea861ea1e85dc3289265e37045981e28208e7344a7f8081d7"),
            new uint256("b4edc22ec98cc49b2f5af5bae3f52f5e6058280f74f2c432c2dd89ae49acceb8"),
            new uint256("0fe596037dcf81bf5c64f39755261c404ed088af5c8c31dd7549b6657ee92365"),
            new uint256("bbad51a0aeba254b01d18c328de9e932b9b859b61e622c325d64e2211b5e413d"),
            new uint256("abf0194cc787be938bc51c7fdf1cae4ec79e65ebab8fa8b8f40541c44ef384b0"),
            new uint256("83bc12d6fdbd3e854cb91c4ca7dfba3c38e8714121af88c8a8abdb33e5002438"),
            new uint256("71a2513026cabaedcbe55aeb6dc8049e5b763a3f54f10c33dd333624f764b38c"),
            new uint256("ee6725632ff5c025dff6a18cd059875dcae20f399b03bccba13d9d5fcf6d9d9a"),
            new uint256("a168a2741d1e7e50cc74b79f695c25ffd3576e6bd61353c2a20e569fd63b2dac"),
            new uint256("6e462d2a87bfde9398b6747f94a8ed6a01e4d96c5b4372df5c910c106c48bd13"),
            new uint256("8eeb696181957c4b22434028990f49cb30006827c73860e77e2eecf5c38be99d"),
            new uint256("3188aaa65877b166f05cdc48f55b1f77a7d6fb221c395596d990ae5647e9ba96")
        };

        /// <summary>
        /// Get entropy bit for given block height and hash.
        /// </summary>
        /// <param name="nHeight">Block height.</param>
        /// <param name="nHash">Block hash.</param>
        /// <returns>Entropy bit.</returns>
        public static byte GetStakeEntropyBit(uint nHeight, uint256 nHash)
        {
            ulong nEntropyBit;

            // Protocol switch to support p2pool at novacoin block #9689
            if (nHeight >= 9689)
            {
                // Take last bit of block hash as entropy bit
                nEntropyBit = (nHash.Low64) & 1UL;

                return (byte)nEntropyBit;
            }

            // Before novacoin block #9689 - get from pregenerated table
            int nBitNum = (int)nHeight & 0xFF;
            int nItemNum = (int)nHeight / 0xFF;

            nEntropyBit = ((entropyStore[nItemNum] & (new uint256(1) << nBitNum)) >> nBitNum).Low64;

            return (byte)nEntropyBit;
        }
    }
}
