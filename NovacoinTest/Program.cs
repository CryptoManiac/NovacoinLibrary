﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NovacoinTest
{
    using Novacoin;

    class Program
    {
        static void Main(string[] args)
        {
            string strUserTx = "0100000078b4c95306340d96b77ec4ee9d42b31cadc2fab911e48d48c36274d516f226d5e85bbc512c010000006b483045022100c8df1fc17b6ea1355a39b92146ec67b3b53565e636e028010d3a8a87f6f805f202203888b9b74df03c3960773f2a81b2dfd1efb08bb036a8f3600bd24d5ed694cd5a0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff364c640420de8fa77313475970bf09ce4d0b1f8eabb8f1d6ea49d90c85b202ee010000006b483045022100b651bf3a6835d714d2c990c742136d769258d0170c9aac24803b986050a8655b0220623651077ff14b0a9d61e30e30f2c15352f70491096f0ec655ae1c79a44e53aa0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff7adbd5f2e521f567bfea2cb63e65d55e66c83563fe253464b75184a5e462043d000000006a4730440220183609f2b995993acc9df241aff722d48b9a731b0cd376212934565723ed81f00220737e7ce75ef39bdc061d0dcdba3ee24e43b899696a7c96803cee0a79e1f78ecb0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff999eb03e00a41c2f9fde8865a554ceebbc48d30f4c8ba22dd88da8c9b46fa920030000006b483045022100ec1ab104ef086ba79b0f2611ebf1bfdd22a7a1020f6630fa1c6707546626e0db022056093d4048a999392185ccc735ef736a5497bd68f60b42e6c0c93ba770b54d010121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffc0543b86be257ddd85b014a76718a70fab9eaa3c477460e4ca187094d86f369c0500000069463043021f24275c72f952043174daf01d7f713f878625f0522124a3cab48a0a2e12604202201b47742e6697b0ebdd1e4ba49c74baf142a0228ad0e0ee847488994c9dce78470121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffe1793d4519147782293dd1db6d90e461265d91db2cc6889c37209394d42ad10d050000006a473044022018a0c3d73b2765d75380614ab36ee8e3c937080894a19166128b1e3357b208fb0220233c9609985f535547381431526867ad0255ec4969afe5c360544992ed6b3ed60121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff02e5420000000000001976a91457d84c814b14bd86bf32f106b733baa693db7dc788ac409c0000000000001976a91408c8768d5d6bf7c1d9609da4e766c3f1752247b188ac00000000";

            string strCoinbaseTx = "010000002926d155010000000000000000000000000000000000000000000000000000000000000000ffffffff27030cff02062f503253482f042926d155081ffffffdf60100000d2f6e6f64655374726174756d2f0000000003c04d6a00000000002321021ad6ae76a602310e86957d4ca752c81a8725f142fd2fc40f6a7fc2310bb2c749acd89e0100000000001976a914ecf809f1ec0ba4faa909d5175e405902a21282be88aca81b0000000000001976a91422851477d63a085dbc2398c8430af1c09e7343f688ac00000000";

            CTransaction tx = new CTransaction(Interop.ParseHex(strUserTx).ToList());
            CTransaction txCoinbase = new CTransaction(Interop.ParseHex(strCoinbaseTx).ToList());

            Console.WriteLine("User TX:{0}\n", tx.ToString());
            Console.WriteLine("Coinbase TX: {0}\n", txCoinbase.ToString());

            Console.ReadLine();
        }
    }
}