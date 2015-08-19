using System;
using System.Linq;
using System.Text;


namespace NovacoinTest
{
    using Novacoin;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            /// Transaction decoding/encoding tests
            string strUserTx = "0100000078b4c95306340d96b77ec4ee9d42b31cadc2fab911e48d48c36274d516f226d5e85bbc512c010000006b483045022100c8df1fc17b6ea1355a39b92146ec67b3b53565e636e028010d3a8a87f6f805f202203888b9b74df03c3960773f2a81b2dfd1efb08bb036a8f3600bd24d5ed694cd5a0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff364c640420de8fa77313475970bf09ce4d0b1f8eabb8f1d6ea49d90c85b202ee010000006b483045022100b651bf3a6835d714d2c990c742136d769258d0170c9aac24803b986050a8655b0220623651077ff14b0a9d61e30e30f2c15352f70491096f0ec655ae1c79a44e53aa0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff7adbd5f2e521f567bfea2cb63e65d55e66c83563fe253464b75184a5e462043d000000006a4730440220183609f2b995993acc9df241aff722d48b9a731b0cd376212934565723ed81f00220737e7ce75ef39bdc061d0dcdba3ee24e43b899696a7c96803cee0a79e1f78ecb0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff999eb03e00a41c2f9fde8865a554ceebbc48d30f4c8ba22dd88da8c9b46fa920030000006b483045022100ec1ab104ef086ba79b0f2611ebf1bfdd22a7a1020f6630fa1c6707546626e0db022056093d4048a999392185ccc735ef736a5497bd68f60b42e6c0c93ba770b54d010121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffc0543b86be257ddd85b014a76718a70fab9eaa3c477460e4ca187094d86f369c0500000069463043021f24275c72f952043174daf01d7f713f878625f0522124a3cab48a0a2e12604202201b47742e6697b0ebdd1e4ba49c74baf142a0228ad0e0ee847488994c9dce78470121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffe1793d4519147782293dd1db6d90e461265d91db2cc6889c37209394d42ad10d050000006a473044022018a0c3d73b2765d75380614ab36ee8e3c937080894a19166128b1e3357b208fb0220233c9609985f535547381431526867ad0255ec4969afe5c360544992ed6b3ed60121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff02e5420000000000001976a91457d84c814b14bd86bf32f106b733baa693db7dc788ac409c0000000000001976a91408c8768d5d6bf7c1d9609da4e766c3f1752247b188ac00000000";

            string strCoinbaseTx = "010000002926d155010000000000000000000000000000000000000000000000000000000000000000ffffffff27030cff02062f503253482f042926d155081ffffffdf60100000d2f6e6f64655374726174756d2f0000000003c04d6a00000000002321021ad6ae76a602310e86957d4ca752c81a8725f142fd2fc40f6a7fc2310bb2c749acd89e0100000000001976a914ecf809f1ec0ba4faa909d5175e405902a21282be88aca81b0000000000001976a91422851477d63a085dbc2398c8430af1c09e7343f688ac00000000";

            CTransaction tx = new CTransaction(Interop.ParseHex(strUserTx).ToList());
            CTransaction txCoinbase = new CTransaction(Interop.ParseHex(strCoinbaseTx).ToList());

            Console.WriteLine("User TX:{0}\n", tx.ToString());
            Console.WriteLine("Coinbase TX: {0}\n", txCoinbase.ToString());

            /// Block encoding/decoding tests
            string strBlock1 = "0600000086e539d77573abc0d81feb7896e1aef41a866001bc78bd24f5fe1a0000000000f5822cea59d999f37d896f66899c86e01e764ed6014706f3ceb58281ed55d0e55ab7d155ada3001d0000000005010000005ab7d155010000000000000000000000000000000000000000000000000000000000000000ffffffff0e0363ff02026d05062f503253482fffffffff0100000000000000000000000000010000005ab7d15501a768f8ed022f4080e3c8866bbe8292c7610b826cd467c49a06a1d0ff2ef7cdd6000000006b483045022100dce689d8cda64ebaffd6b96321952f16df34494256c58d2fd83069db7bce40e5022016020f55dc747d845d2057547c650412aa27d7d628e72238579f72e572dafdfe012102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093ffffffff03000000000000000000d0f1440300000000232102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093acc23f450300000000232102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093ac000000000100000091b4d15502c252c9130b1fd1dc8ef59cdb550ed398c4fe12c7ebf3eb917076bbda039b769d010000004847304402204bee0faac004364cdf6483d492333d00ad6f7c925faa3750fef2c79a9065a28102204a5e2b970f776ea1af2c2c03e36e6381d3d69b529d90b512363ae44815a321c601ffffffffc252c9130b1fd1dc8ef59cdb550ed398c4fe12c7ebf3eb917076bbda039b769d02000000494830450221008bf152a838f6f14f0ed1b2afc27821717e43a528e27aec3569ab42fc82f468aa02202cf6c962ef97db6e5ba32ccdd235afdc9a3cbb7907bfe879f8109446485d66dc01ffffffff0116467210000000001976a914edbf189bece45d4afa9848276e949183936bf6a488ac000000000100000017b5d1550229c74fb0004d45fba5baaefed1d9c229a8f1c85c36590cedf3ce6635335963d5000000006a4730440220319a4dfcf1607682d493c6d90087dc35d778a8bfcebe3549bae0af69e8daecb902206e6622367be30d9ccd4fdd27ed09c2fbcc9e5c858b26dfcdd927a8aba637b327012103b103f5d7e9717bc37cc99984b23babc3fff4677728be6b9c1847f6ce78e557f5ffffffff24b91fa6e9c160cc8da306e485942ee76137117aa8adecf531f6af1aef4e9b680000000049483045022100c9b311b7a7f5adeb0e72f962fb81b4cc1d105e32cfd7b1a7641a0fcc014d67c50220527161371a17301448bae87a26df201598b46d00ff452893177e9aed665c357c01ffffffff028e380000000000001976a91400afc350f81916a642a88b5ce8f73508663b531188ac67f46b00000000001976a91420c10f267f55ff4e05a083a8e1f4e882fbca1f4988ac0000000001000000efb6d15501626835db281e1fe6271620b8f67999f2174bb96df0eb3935fc99771e4ff45acf000000006a47304402206c34deb9c07c5477c47d398eaf91dbdf74aff5229c448e82ed0c1d8e2ee30e2d02203fe609434844b3eee21e747e313bcbf98efa4326727db6d2efba7bb627d2e0ce0121030c86c72f59c66824297aa78e433fe7057fd064e03e44c62ec49201ee0184149bffffffff028be30300000000001976a91481fc5cfb7f41afb3baf4138626022b3081b84e1788ac6abd0000000000001976a91499346dcd8ddfa10326697d5387b7df765004f4e388ac0000000046304402205189911c97354edb2965b4a119e6d76281f4c5da8fcead19c97bf6bcc9990fe102200f56d9dd967b036627b32b1e3ef2f819deaaafcc3244332472df7acfe19f1aa5";
            CBlock b1 = new CBlock(Interop.ParseHex(strBlock1).ToList());

            string strBlock1Bytes = Interop.ToHex(b1.ToBytes());

            string strBlock2 = "06000000eb5ab262c7382e7e009ad0b65c707131b8b6b846f8920a1a6697d929203a22f70e8cbd6bee1c0519a9d06b749b5eb6e599c154b12b732170807e603b6c326abbe0b7d15560e2211b15085b8f0101000000e0b7d155010000000000000000000000000000000000000000000000000000000000000000ffffffff270364ff02062f503253482f04c7b7d15508300000032b0000000d2f6e6f64655374726174756d2f0000000002f87d6b000000000023210287753c456abfc248d1bd155f44742d2ea72a2f29a5290c815fea0e9c55c4e2d0ac488a0000000000001976a914276cdbe21aaab75d58e151e01efea2860d3ef3d088ac0000000000";
            CBlock b2 = new CBlock(Interop.ParseHex(strBlock2).ToList());

            string strBlock2Bytes = Interop.ToHex(b2.ToBytes());

            Console.WriteLine(b1.ToString());
            Console.WriteLine("OK: {0}\n", strBlock1 == strBlock1Bytes);

            Console.WriteLine(b2.ToString());
            Console.WriteLine("Reserialization is OK: {0}\n", strBlock2 == strBlock2Bytes);

            /// ECDSA keypair generation test

            CKeyPair keyPair1 = new CKeyPair();
            CKeyPair keyPair2 = new CKeyPair(keyPair1.Secret);
            CPubKey pubKey = keyPair2.GetPubKey();

            string strPrivKeyBase58 = keyPair1.ToString();

            Console.WriteLine("Privkey in Base58: {0}", strPrivKeyBase58);
            Console.WriteLine("Privkey in Hex: {0}", keyPair1.ToHex());

            CKeyPair keyPair3 = new CKeyPair(strPrivKeyBase58);
            Console.WriteLine("Privkey base58 deserialization is OK: {0}", keyPair3.GetKeyID().ToString() == keyPair1.GetKeyID().ToString());

            Console.WriteLine("Pubkey in Base58: {0}", pubKey.ToString());
            Console.WriteLine("Pubkey in Hex: {0}", pubKey.ToHex());

            Console.WriteLine("Reinitialization is OK: {0}\n", keyPair1.ToString() == keyPair2.ToString());

            /// Address generation test

            CKeyID keyID = keyPair1.GetKeyID();
            Console.WriteLine("Key ID: {0}", Interop.ToHex(keyID.hashBytes));
            Console.WriteLine("Novacoin address: {0}\n", keyID.ToString());

            /// Privkey deserialization test
            CKeyPair keyPair4 = new CKeyPair("MEP3qCtFGmWo3Gurf8fMnUNaDHGNf637DqjoeG8rKium2jSj51sf");
            Console.WriteLine("\nHard-coded privkey in Hex: {0}", keyPair4.ToHex());
            Console.WriteLine("Hard-Coded privkey address: {0}", keyPair4.GetKeyID().ToString());
            Console.WriteLine("Hard-Coded privkey: {0}\n", keyPair4.ToString());

            // Privkey hex deserialization test
            CKeyPair keyPair5 = new CKeyPair(keyPair4.Secret.ToArray());
            Console.WriteLine("Decoded privkey in Hex: {0}", keyPair5.ToHex());
            Console.WriteLine("Decoded privkey address: {0}\n", keyPair5.GetKeyID().ToString());

            /// ECDSA keypair signing test

            string data = "Превед!";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signature = keyPair1.Sign(dataBytes).ToArray();

            Console.WriteLine("Signature: {0}", Interop.ToHex(signature));
            Console.WriteLine("Signature is OK: {0} (CKeyPair)", keyPair1.VerifySignature(dataBytes, signature));
            Console.WriteLine("Signature is OK: {0} (CPubKey)", pubKey.VerifySignature(dataBytes, signature));

            /// Donation address

            string strPubKeyTest = "029780fac8b85b4a47a616acb4e19d7958eaf02acc5123f65e7824ce720b1ae788";
            CPubKey pubKeyTest = new CPubKey(Interop.ParseHex(strPubKeyTest));
            string strDonationAddress = pubKeyTest.GetKeyID().ToString();
            Console.WriteLine("\nDonations may be sent to: {0}", strDonationAddress);
            Console.WriteLine("Address generation is OK: {0}", strDonationAddress == "4T2t8uiDtyHceMwMjMHPn88TyJB3trCg3o");

            /// Address deserialization test

            CNovacoinAddress donationAddress = new CNovacoinAddress(strDonationAddress);
            Console.WriteLine("Address reserialization is OK: {0}", donationAddress.ToString() == pubKeyTest.GetKeyID().ToString());

            /// Block header hashing test
            IEnumerable<byte> dataBytesForScrypt = b1.header.ToBytes();
            ScryptHash256 scryptHash = ScryptHash256.Compute256(dataBytesForScrypt);

            Console.WriteLine("block1 header hash: {0}", scryptHash.ToString());

            Console.ReadLine();
        }
    }
}
