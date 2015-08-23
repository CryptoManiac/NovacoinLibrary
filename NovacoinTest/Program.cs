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

            CTransaction tx = new CTransaction(Interop.HexToList(strUserTx));
            CTransaction txCoinbase = new CTransaction(Interop.HexToList(strCoinbaseTx));

            Console.WriteLine("User TX:{0}\n", tx.ToString());
            Console.WriteLine("Coinbase TX: {0}\n", txCoinbase.ToString());

            /// Block encoding/decoding tests
            string strBlock1 = "0600000086e539d77573abc0d81feb7896e1aef41a866001bc78bd24f5fe1a0000000000f5822cea59d999f37d896f66899c86e01e764ed6014706f3ceb58281ed55d0e55ab7d155ada3001d0000000005010000005ab7d155010000000000000000000000000000000000000000000000000000000000000000ffffffff0e0363ff02026d05062f503253482fffffffff0100000000000000000000000000010000005ab7d15501a768f8ed022f4080e3c8866bbe8292c7610b826cd467c49a06a1d0ff2ef7cdd6000000006b483045022100dce689d8cda64ebaffd6b96321952f16df34494256c58d2fd83069db7bce40e5022016020f55dc747d845d2057547c650412aa27d7d628e72238579f72e572dafdfe012102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093ffffffff03000000000000000000d0f1440300000000232102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093acc23f450300000000232102916e12c72a41913a5307bf7477db80dd499ea20f1a6bd99a2bdae6229f5aa093ac000000000100000091b4d15502c252c9130b1fd1dc8ef59cdb550ed398c4fe12c7ebf3eb917076bbda039b769d010000004847304402204bee0faac004364cdf6483d492333d00ad6f7c925faa3750fef2c79a9065a28102204a5e2b970f776ea1af2c2c03e36e6381d3d69b529d90b512363ae44815a321c601ffffffffc252c9130b1fd1dc8ef59cdb550ed398c4fe12c7ebf3eb917076bbda039b769d02000000494830450221008bf152a838f6f14f0ed1b2afc27821717e43a528e27aec3569ab42fc82f468aa02202cf6c962ef97db6e5ba32ccdd235afdc9a3cbb7907bfe879f8109446485d66dc01ffffffff0116467210000000001976a914edbf189bece45d4afa9848276e949183936bf6a488ac000000000100000017b5d1550229c74fb0004d45fba5baaefed1d9c229a8f1c85c36590cedf3ce6635335963d5000000006a4730440220319a4dfcf1607682d493c6d90087dc35d778a8bfcebe3549bae0af69e8daecb902206e6622367be30d9ccd4fdd27ed09c2fbcc9e5c858b26dfcdd927a8aba637b327012103b103f5d7e9717bc37cc99984b23babc3fff4677728be6b9c1847f6ce78e557f5ffffffff24b91fa6e9c160cc8da306e485942ee76137117aa8adecf531f6af1aef4e9b680000000049483045022100c9b311b7a7f5adeb0e72f962fb81b4cc1d105e32cfd7b1a7641a0fcc014d67c50220527161371a17301448bae87a26df201598b46d00ff452893177e9aed665c357c01ffffffff028e380000000000001976a91400afc350f81916a642a88b5ce8f73508663b531188ac67f46b00000000001976a91420c10f267f55ff4e05a083a8e1f4e882fbca1f4988ac0000000001000000efb6d15501626835db281e1fe6271620b8f67999f2174bb96df0eb3935fc99771e4ff45acf000000006a47304402206c34deb9c07c5477c47d398eaf91dbdf74aff5229c448e82ed0c1d8e2ee30e2d02203fe609434844b3eee21e747e313bcbf98efa4326727db6d2efba7bb627d2e0ce0121030c86c72f59c66824297aa78e433fe7057fd064e03e44c62ec49201ee0184149bffffffff028be30300000000001976a91481fc5cfb7f41afb3baf4138626022b3081b84e1788ac6abd0000000000001976a91499346dcd8ddfa10326697d5387b7df765004f4e388ac0000000046304402205189911c97354edb2965b4a119e6d76281f4c5da8fcead19c97bf6bcc9990fe102200f56d9dd967b036627b32b1e3ef2f819deaaafcc3244332472df7acfe19f1aa5";
            CBlock b1 = new CBlock(Interop.HexToList(strBlock1));

            string strBlock1Bytes = Interop.ToHex(b1.Bytes);

            string strBlock2 = "06000000eb5ab262c7382e7e009ad0b65c707131b8b6b846f8920a1a6697d929203a22f70e8cbd6bee1c0519a9d06b749b5eb6e599c154b12b732170807e603b6c326abbe0b7d15560e2211b15085b8f0101000000e0b7d155010000000000000000000000000000000000000000000000000000000000000000ffffffff270364ff02062f503253482f04c7b7d15508300000032b0000000d2f6e6f64655374726174756d2f0000000002f87d6b000000000023210287753c456abfc248d1bd155f44742d2ea72a2f29a5290c815fea0e9c55c4e2d0ac488a0000000000001976a914276cdbe21aaab75d58e151e01efea2860d3ef3d088ac0000000000";
            CBlock b2 = new CBlock(Interop.HexToList(strBlock2));

            string strBlock2Bytes = Interop.ToHex(b2.Bytes);

            Console.WriteLine(b1.ToString());
            Console.WriteLine("OK: {0}\n", strBlock1 == strBlock1Bytes);

            Console.WriteLine(b2.ToString());
            Console.WriteLine("Reserialization is OK: {0}\n", strBlock2 == strBlock2Bytes);

            /// ECDSA keypair generation test

            CKeyPair keyPair1 = new CKeyPair();
            CKeyPair keyPair2 = new CKeyPair(keyPair1.SecretBytes);
            CPubKey pubKey = keyPair2.PubKey;

            string strPrivKeyBase58 = keyPair1.ToString();

            Console.WriteLine("Privkey in Base58: {0}", strPrivKeyBase58);
            Console.WriteLine("Privkey in Hex: {0}", keyPair1.ToHex());

            CKeyPair keyPair3 = new CKeyPair(strPrivKeyBase58);
            Console.WriteLine("Privkey base58 deserialization is OK: {0}", keyPair3.KeyID.ToString() == keyPair1.KeyID.ToString());

            Console.WriteLine("Pubkey in Base58: {0}", pubKey.ToString());
            Console.WriteLine("Pubkey in Hex: {0}", pubKey.ToHex());

            Console.WriteLine("Reinitialization is OK: {0}\n", keyPair1.ToString() == keyPair2.ToString());

            /// Address generation test

            CKeyID keyID = keyPair1.KeyID;
            Console.WriteLine("Key ID: {0}", Interop.ToHex(keyID.hashBytes));
            Console.WriteLine("Novacoin address: {0}\n", keyID.ToString());

            /// Privkey deserialization test
            CKeyPair keyPair4 = new CKeyPair("MEP3qCtFGmWo3Gurf8fMnUNaDHGNf637DqjoeG8rKium2jSj51sf");
            Console.WriteLine("\nHard-coded privkey in Hex: {0}", keyPair4.ToHex());
            Console.WriteLine("Hard-Coded privkey address: {0}", keyPair4.KeyID.ToString());
            Console.WriteLine("Hard-Coded privkey: {0}\n", keyPair4.ToString());

            // Privkey hex deserialization test
            CKeyPair keyPair5 = new CKeyPair(keyPair4.SecretBytes.ToArray());
            Console.WriteLine("Decoded privkey in Hex: {0}", keyPair5.ToHex());
            Console.WriteLine("Decoded privkey address: {0}\n", keyPair5.KeyID.ToString());

            /// ECDSA keypair signing test

            string data = "Превед!";
            Hash256 sigHash =  Hash256.Compute256(Encoding.UTF8.GetBytes(data));
            byte[] signature = keyPair1.Sign(sigHash).ToArray();

            Console.WriteLine("Signature: {0}", Interop.ToHex(signature));
            Console.WriteLine("Signature is OK: {0} (CKeyPair)", keyPair1.VerifySignature(sigHash, signature));
            Console.WriteLine("Signature is OK: {0} (CPubKey)", pubKey.VerifySignature(sigHash, signature));

            /// Donation address

            string strPubKeyTest = "029780fac8b85b4a47a616acb4e19d7958eaf02acc5123f65e7824ce720b1ae788";
            CPubKey pubKeyTest = new CPubKey(Interop.HexToEnumerable(strPubKeyTest));
            string strDonationAddress = pubKeyTest.KeyID.ToString();
            Console.WriteLine("\nDonations may be sent to: {0}", strDonationAddress);
            Console.WriteLine("Address generation is OK: {0}", strDonationAddress == "4T2t8uiDtyHceMwMjMHPn88TyJB3trCg3o");

            /// Address deserialization test

            CNovacoinAddress donationAddress = new CNovacoinAddress(strDonationAddress);
            Console.WriteLine("Address reserialization is OK: {0}", donationAddress.ToString() == pubKeyTest.KeyID.ToString());

            /// Block header hashing test
            IEnumerable<byte> dataBytesForScrypt = b1.header.Bytes;
            ScryptHash256 scryptHash = ScryptHash256.Compute256(dataBytesForScrypt);

            Console.WriteLine("\nblock1 header hash: {0}", scryptHash.ToString());

            /// Solver tests
            CScript scriptPubKey = new CScript(Interop.HexToEnumerable("21021ad6ae76a602310e86957d4ca752c81a8725f142fd2fc40f6a7fc2310bb2c749ac"));
            CScript scriptPubKeyHash = new CScript(Interop.HexToEnumerable("76a914edbf189bece45d4afa9848276e949183936bf6a488ac"));

            txnouttype typeRet;
            IList<IEnumerable<byte>> solutions;

            Console.WriteLine("\nscriptPubKey solved: {0}", ScriptCode.Solver(scriptPubKey, out typeRet, out solutions));
            Console.WriteLine("scriptPubKey address: {0}\n", new CPubKey(solutions.First()).KeyID.ToString());

            Console.WriteLine("scriptPubKeyHash solved: {0}", ScriptCode.Solver(scriptPubKeyHash, out typeRet, out solutions));
            Console.WriteLine("scriptPubKeyHash address: {0}\n", new CKeyID(new Hash160(solutions.First())).ToString());

            /// Some SetDestination tests
            CScript scriptDestinationTest = new CScript();

            
            Console.WriteLine("Creating and decoding new destination with {0} as public key.\n", keyPair1.PubKey.ToString());

            Console.WriteLine("Pay-to-Pubkey:");

            scriptDestinationTest.SetDestination(keyPair1.PubKey);

            Console.WriteLine("\tscriptDestinationTest solved: {0}", ScriptCode.Solver(scriptDestinationTest, out typeRet, out solutions));
            Console.WriteLine("\tscriptDestinationTest address: {0}\n", new CPubKey(solutions.First()).KeyID.ToString());

            Console.WriteLine("Pay-to-PubkeyHash:");

            scriptDestinationTest.SetDestination(keyPair1.PubKey.KeyID);

            Console.WriteLine("\tscriptDestinationTest solved: {0}", ScriptCode.Solver(scriptDestinationTest, out typeRet, out solutions));
            Console.WriteLine("\tscriptDestinationTest address: {0}\n", new CKeyID(new Hash160(solutions.First())).ToString());

            Console.WriteLine("Multisig with three random keys:");

            CKeyPair k1 = new CKeyPair(), k2 = new CKeyPair(), k3 = new CKeyPair();

            scriptDestinationTest.SetMultiSig(2, new CPubKey[] { k1.PubKey, k2.PubKey, k3.PubKey });

            Console.WriteLine("\nscriptDestinationTest solved: {0}", ScriptCode.Solver(scriptDestinationTest, out typeRet, out solutions));
            Console.WriteLine("scriptDestinationTest addresses: \n");

            int nRequired = solutions.First().First();
            int nKeys = solutions.Last().First();

            foreach (IEnumerable<byte> keyBytes in solutions.Skip(1).Take(nKeys))
            {
                Console.WriteLine("\t{0}", (new CPubKey(keyBytes)).KeyID.ToString());
            }

            Console.WriteLine("\nnRequired={0}\n", nRequired);

            Console.WriteLine("Script code: \n\n{0}", scriptDestinationTest.ToString());

            Console.WriteLine("\nPay-to-ScriptHash with same script:\n");

            CScript scriptP2SHTest = new CScript();
            scriptP2SHTest.SetDestination(scriptDestinationTest.ScriptID);

            Console.WriteLine("\tscriptP2SHTest solved: {0}", ScriptCode.Solver(scriptP2SHTest, out typeRet, out solutions));
            Console.WriteLine("\tscriptP2SHTest address: {0}\n", new CScriptID(new Hash160(solutions.First())).ToString());

            // SignatureHash tests
            CTransaction txS = new CTransaction(Interop.HexToEnumerable("01000000ccfe9e550d083902781746c80954e3af56e930235befb798f987667021a2f32dc0099499cd010000006b483045022100b5f6783af4f7f60866c889fd668c93ee110ecc3751208fe0b49cc7ace47e52e8022075652e1e960a50b27436ab04f2728b3bba09d07a858691559d99c1ac5dd74f16012103fe065856d7fa8cd41d0047600af2ec1ebe8c6198c1a889e90d8ce6b2f1f8afd7ffffffff2c6009a7494f38f7797bb9ef2aeafb093ae433208171a504367373df4164399d010000004847304402205f1b74bbc37219918f3de13ff645ecc7093512fecda4fcbcac2174c44144361102202149f1adcfcd473ec8a662b5b166b600208d92596e30b33fb402b4720bac3da101ffffffffbf1dd11394d2c0d3cbd4e0b56c74e7463ed5a19f22fe219ee700c61f834b3948010000006a473044022004824a9e071c40707e5309e510fd2cc105fd430504ceefce48aeed1c8fcf3bd7022023f4a43c58e4012284d8df25b55940199d19d4ca664053e2d5c1cc93ef441c3c012103fe065856d7fa8cd41d0047600af2ec1ebe8c6198c1a889e90d8ce6b2f1f8afd7ffffffffea06d18d8034a3645a8d5da75ed5c5f68a9dd09a798a876bef5d2cc9db8819390100000048473044022018e016973d87a53d6f14ae9929aa3c426d3d3a76eb81b3f1e996f0ec24ebacb302203668f165e6e9d5818eb3d108d23e2390213a6921ddfd51dbfca4ffebad73029601ffffffff5bda9a2a98debbda4ddad400a340190fcba4f4b3268f0a9d88eb5541bd7dadfc0100000049483045022100c4d210a6cd3edc6bc9cbfee1a8506ff239ef60baf7ebd46ffefb43e20a575d6c022019ea10cf480dadbb6332a03a404a3991437ffc9fef044c07112e2d15f3de74de01ffffffff5bda9a2a98debbda4ddad400a340190fcba4f4b3268f0a9d88eb5541bd7dadfc020000004948304502210084f5781ff88c201caca29b724f89fad5d72320a578239a3a2834ba669ea92b7e02207651ef9f7c60c2cc4fe187c98587252f3196fb1c31ed8f6c1f1f41e9a90d75ff01ffffffff61a9d75745092786bcbd48cc5860845beea607b8994790e9734f9fa68951bb66010000006b483045022100d2d3f925472970b9a0d365a120a9a6c9b7b0b3b3aacedaa40532397c6252da2f02206939d3cade4bada339799a4d651a7a33cb381640f6acef5fbecbe55ae1fa2364012103fe065856d7fa8cd41d0047600af2ec1ebe8c6198c1a889e90d8ce6b2f1f8afd7ffffffff635c711ba32bd587d349521475d2bd133a402c178543183c027cc1414a7837500100000049483045022100aece1ca9d902eaece08ec9704005196046f3a0b6f561cd17e9b09c01fed1447602207e68e21be4fcb895f045337741b42f43d218bb5c681f8e2eac5f9f3ffc8caf8301ffffffff7ddd7385fd7b81f19ccaa6ccd629bd2ab2a0af7ca69831c6dbb3b02c31de95db0100000049483045022100b260f2065dea407006e424d3cbb20009c807f422cee4ac8fb3553e4a81d62a7702204c67c99e792542cfe19a6956b101b4fd754a01fb1538b54e5f2141210729f09b01ffffffff8faebe377bc4211e41bd7e4a551e1de530040f8a55797f82e12d4d3c6a0b9fff010000004847304402204637911286c073fa0a8211e8427a6c63201bdac73e7b2760d3d9c7d748c9267c02205df709fdd06e3fb600ab81a17a1becc829769f1cb117b1520755d4f2a38429f001fffffffff1be969005bfcab4bcdaf835470680e2a309290b97d79fe63f7cbe904560b2d601000000484730440220352ad1a1ea5d92ddc13b7507a05180574c7309822f684ecf7321b7e925e5104302201e6a06e2f2d05a3d665cc6180fafacb658514a2f1bb632de99e88e4c26149e2501fffffffffa2019204a766fb4614be3d12bfb1ab35ad756e144193249e83660ca78898b3b0200000048473044022024f21eaf955291a9aec2cd45f42add62d8a30626aa9246664226fb3b56bf632f02205a3b46ec2857fec2fcb73663a57f99e4fcda328e8f1283198e8e9c5b4a2a3e0f01ffffffffd25e023fbdcd571ae346bf7aa142f5e32ca1aec23adae314ee209af22572cf1f000000006a4730440220551627592cbb7d970222a4d57a32aed50f1e93e81ae69958f26e56ca3b561715022019b12e560ff31013d0941ca2100ecdf9a3c3602b5c76b83d3b3c87d723d32ce3012103fe065856d7fa8cd41d0047600af2ec1ebe8c6198c1a889e90d8ce6b2f1f8afd7ffffffff02402d0000000000001976a91479f1d300be0da277e7ae217e99c6cc8a4f8717fe88ac00943577000000001976a914cbc5a055ae068d34b4a93e4c9adb9cb10262ae4f88ac00000000").ToList());

            Hash256 sigHashAll = ScriptCode.SignatureHash(txS.vout[0].scriptPubKey, txS, 1, (int)sigflag.SIGHASH_ALL);
            Hash256 sigHashNone = ScriptCode.SignatureHash(txS.vout[0].scriptPubKey, txS, 1, (int)sigflag.SIGHASH_NONE);
            Hash256 sigHashSingle = ScriptCode.SignatureHash(txS.vout[0].scriptPubKey, txS, 1, (int)sigflag.SIGHASH_SINGLE);
            Hash256 sigHashAnyone = ScriptCode.SignatureHash(txS.vout[0].scriptPubKey, txS, 1, (int)sigflag.SIGHASH_ANYONECANPAY);

            Console.WriteLine("sigHashAll={0}", sigHashAll.ToString());
            Console.WriteLine("sigHashNone={0}", sigHashNone.ToString());
            Console.WriteLine("sigHashSingle={0}", sigHashSingle.ToString());
            Console.WriteLine("sigHashAnyone={0}\n", sigHashAnyone.ToString());

            // Testing some opcode functionality

            for (int i = 0; i < 17; i++)
            {
                Console.WriteLine("{0} is encoded as {1}", i, ScriptCode.GetOpName(ScriptCode.EncodeOP_N(i)));
            }

            Console.WriteLine("In addition, -1 is encoded as {0}", ScriptCode.GetOpName(ScriptCode.EncodeOP_N(-1, true)));

            Console.WriteLine(ScriptCode.GetOpName(instruction.OP_TRUE));
            Console.WriteLine(ScriptCode.GetOpName(instruction.OP_FALSE));

            // Script validation test

            CTransaction txTo = new CTransaction(Interop.HexToList("0100000078b4c95306340d96b77ec4ee9d42b31cadc2fab911e48d48c36274d516f226d5e85bbc512c010000006b483045022100c8df1fc17b6ea1355a39b92146ec67b3b53565e636e028010d3a8a87f6f805f202203888b9b74df03c3960773f2a81b2dfd1efb08bb036a8f3600bd24d5ed694cd5a0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff364c640420de8fa77313475970bf09ce4d0b1f8eabb8f1d6ea49d90c85b202ee010000006b483045022100b651bf3a6835d714d2c990c742136d769258d0170c9aac24803b986050a8655b0220623651077ff14b0a9d61e30e30f2c15352f70491096f0ec655ae1c79a44e53aa0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff7adbd5f2e521f567bfea2cb63e65d55e66c83563fe253464b75184a5e462043d000000006a4730440220183609f2b995993acc9df241aff722d48b9a731b0cd376212934565723ed81f00220737e7ce75ef39bdc061d0dcdba3ee24e43b899696a7c96803cee0a79e1f78ecb0121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff999eb03e00a41c2f9fde8865a554ceebbc48d30f4c8ba22dd88da8c9b46fa920030000006b483045022100ec1ab104ef086ba79b0f2611ebf1bfdd22a7a1020f6630fa1c6707546626e0db022056093d4048a999392185ccc735ef736a5497bd68f60b42e6c0c93ba770b54d010121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffc0543b86be257ddd85b014a76718a70fab9eaa3c477460e4ca187094d86f369c0500000069463043021f24275c72f952043174daf01d7f713f878625f0522124a3cab48a0a2e12604202201b47742e6697b0ebdd1e4ba49c74baf142a0228ad0e0ee847488994c9dce78470121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffffe1793d4519147782293dd1db6d90e461265d91db2cc6889c37209394d42ad10d050000006a473044022018a0c3d73b2765d75380614ab36ee8e3c937080894a19166128b1e3357b208fb0220233c9609985f535547381431526867ad0255ec4969afe5c360544992ed6b3ed60121030dd13e6d3c63fa10cc0b6bf968fbbfcb9a988b333813b1f22d04fa60e344bc4cffffffff02e5420000000000001976a91457d84c814b14bd86bf32f106b733baa693db7dc788ac409c0000000000001976a91408c8768d5d6bf7c1d9609da4e766c3f1752247b188ac00000000"));
            CTransaction txPrev = new CTransaction(Interop.HexToList("0100000079755d53010000000000000000000000000000000000000000000000000000000000000000ffffffff0b03d26401062f503253482fffffffff1cb3250000000000001976a914a1876be00980e343d9fb60b8219e0e8345fc744b88ac7b2c0000000000001976a914549ff7e6fe90d762e2e51678bda26f54fbbfcce388ac4f380000000000001976a9146c923ed1d87738df97301892aca83ba6b262cc4d88acfecc0000000000001976a91458d438e17b2e9f09a79b3920c06e3fd4bdc4cb3788ac84e80000000000001976a914797d85f272d66990adc30a30974c5d643a5dab3f88acc85e0100000000001976a9149a430715e524e951ddd5d947614c6cd4b412ed5888ac4e850100000000001976a914ae0da10f13d51e66fe5559023a2a32ed15ca335f88ac1d180200000000001976a914e2431bf313ac865fd8f1289d1627cf9391fabd7788ac472c0200000000001976a914fcbe5b3267acfea71fe3d768597fdbe0b8a9b16488ace0af0200000000001976a91498facbedc6424820a79c381fdd3c6e7c4389dc3888ac6cbb0200000000001976a9140c3a1469e5088100f11ec56b77fdeba61c25650c88acb8ec0200000000001976a914d99b39f6d1e06c1e2292eb6786188e193c5dec5988acb33c0300000000001976a9141b49aedc9ed1026722b1686e1322be08b998acfe88acca620300000000001976a914753cf85eef5fd49cc9694b67f3ed366508d0562488acc06f0300000000001976a914c157ab891e5f8dfe37a8e7598b53a646eed4028888acb59c0400000000001976a914cad1890de0e59796e501131dc11bd31a6acf96b488ac49070500000000001976a914c7b6639b55215e5d17590f3b5df46ad7ab37ee6d88ac34b20500000000001976a914a8eaab473a06570cafdf2aa44d976d09cc29814d88ac23db0600000000001976a914407356864b5e1b3df8a863d678183140be6ef17588acb55e0700000000001976a914037a31012e8e1962e251c0432d9258058326cdcf88ac278f0800000000001976a914e0e70b5b3192dd3e418642055a50840a19ffcf7088acccad0800000000001976a914640ebcbda496cc9e4fdb2281aeecaa00691007d288ac85ef0800000000001976a91451978f1d2ad964c556127fdb9e201e31a6ca474d88ac45790d00000000001976a9148c0b235018aa2a8f8b884e5fad1e1d1fce4b6cb288aca5a01500000000001976a914b1c9131340b7dbcfc060d977200c43686bcb73f188ac4dce1700000000001976a914684d742a36d06ca1bce3560b00135ca1432b984288ac2dac0000000000004341043b253cc0b5c8ce26f24b84bb955bec955cbb4643f19ab7ea073884f22874abdafc42040b97efec3c9eeb29ce69022a96cc1772f8bc805f78af0d3dc5c441db5fac00000000000000002a6a284d9ca0d38f66957fa8dd5de67fa23034284a51d669ee34b0608e060f56f4dd93000000000200000000000000"));

            Console.WriteLine("Script validation result: {0}", ScriptCode.VerifyScript(txTo.vin[0].scriptSig, txPrev.vout[1].scriptPubKey, txTo, 0, (int)scriptflag.SCRIPT_VERIFY_P2SH, 0));

            Console.ReadLine();
        }
    }
}
