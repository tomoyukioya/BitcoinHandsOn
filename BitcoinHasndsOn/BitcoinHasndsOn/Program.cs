using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace BitcoinHasndsOn
{
    class Program
    {
        /////////////////////
        // 演習1
        //
        // 自分の秘密鍵
        private static String myWifTestNet = "cT94hu1suZgyC6BkpqG1nW4zarmkMX3Wrhc63Sg6PuRfL7vsA5dD";
        private static String myWifMainNet = "L2n5Ez22UVzi2eiVSRStRBZvxdULh4wpnfTcw2Datnmf5NpLFRnY";
        // 自分のBitcoinアドレス
        private static String myAddressTestNet = "myiyp8HdFW5SmG2Ns2MgkcxfxxxdLG9NCt";
        private static String myAddressMainNet = "KD2X5CeSUeBz9Ym9TPJvhkM6yMvNafXeY";

        /////////////////////
        // 演習2
        //
        // TestNet Faucetから自分への送金のTransactionId
        private static String txIdToSpend = "6faef63a6a671876a1148e0fa344a824823cfdc03dcbedb466db16c1005a9b9a";

        /////////////////////
        // 演習3
        //
        // Android用Bitcoin WalletアプリのBitcoinアドレス
        private static String AndroidAddressTestNet = "mumS7dey1b84Djs3b12KH3mfEUVhf8zGK7";

        /////////////////////
        // 演習4
        //
        // txIdToSpendのOutputsのうち、自分が使えるOutputのindex
        private static int indexOutputToSpend = 1;


        static void Main(string[] args)
        {
            // 演習1:   秘密鍵を生成し、自分のBitcoinアドレスを作成しよう
            //          作成した秘密鍵とBitcoinアドレスをソースコードに埋め込んでおこう
            //createKeys();

            // 演習2:   自分のアドレス宛に、TestNet FaucetからBitcoinを送金しよう
            //          http://tpfaucet.appspot.com/
            //          また、そのTransactionがconfirmするまで待ち、Transaction IDをソースコードに埋め込んでおこう
            //          https://testnet.blockexplorer.com/

            // 演習3:   AndroidのBitcoin Walletアプリをインストールしよう
            //          Play Storeで「Bitcoin Wallet for Testnet」アプリを検索。「Bitcoin Wallet」はMainnet用なので注意
            //          また、AndroidのBitcoin WalletのBitcoinアドレスを調べて、ソースコードに埋め込んでおこう
            //          (ホーム画面のQRコードをタップ。スペース不要、大文字小文字区別あり)

            // 演習4:   自分が使えるOutPointを特定し、そのindexをソースコードのindexOutputToSpendに埋め込んでおこう
            //findOutPointToSpent();
            
            // 演習5:   自分のアドレスから、AndroidのWalletに1.0BTC送金してみよう
            txBtcToAndroidWallet(1.0);
        }

        // 演習1: 秘密鍵およびBitcoinアドレス鍵の生成
        public static void createKeys()
        {
            Key privateKey = new Key(); //秘密鍵の生成

            // 秘密鍵は通常、WIF(Wallet Import Format) = Bitcoin Secret形式で扱う
            BitcoinSecret testNetSecret = privateKey.GetBitcoinSecret(Network.TestNet);
            BitcoinSecret mainNetSecret = privateKey.GetBitcoinSecret(Network.Main);
            Console.WriteLine("Bitcoin Secret(WIF) for testnet: {0}", testNetSecret);
            Console.WriteLine("Bitcoin Secret(WIF) for mainnet: {0}", mainNetSecret);

            PubKey publicKey = privateKey.PubKey; //公開鍵の生成

            // Bitcoin Addressへ変換
            BitcoinPubKeyAddress testNetAddress = publicKey.GetAddress(Network.TestNet);
            BitcoinPubKeyAddress mainNetAddress = publicKey.GetAddress(Network.Main);
            Console.WriteLine("Address for testnet: {0}", testNetAddress);
            Console.WriteLine("Address for mainnet: {0}", mainNetAddress);
        }

        // 演習4: 自分が使えるOutPointを特定
        public static void findOutPointToSpent()
        {
            // 自分のScriptPubKeyを表示
            Console.WriteLine("My ScriptPubKey = {0}", new BitcoinPubKeyAddress(myAddressTestNet).ScriptPubKey);

            // QBitNinjaライブラリを使用して、トランザクション情報を取得
            // {Value(BTC)}, {scriptPubKey}形式で出力
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            Transaction transaction = client.GetTransaction(uint256.Parse(txIdToSpend)).Result.Transaction;

            foreach (var txout in transaction.Outputs.Select((v, i)=>new { v, i }))
            {
                Console.WriteLine("index = {0}, value = {1}, ScriptPubKey = {2}", txout.i, txout.v.Value, txout.v.ScriptPubKey);
            }
        }

        // 演習5: Androidへ送金してみよう
        public static void txBtcToAndroidWallet(double txValue)
        {
            double txFee = 0.0001;

            // トランザクションを作成
            Transaction txToAndroid = new Transaction();

            // 入力
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            Transaction txForInput = client.GetTransaction(uint256.Parse(txIdToSpend)).Result.Transaction;
            decimal txFund = txForInput.Outputs[indexOutputToSpend].Value;

            txToAndroid.Inputs.Add(new TxIn()
            {
                PrevOut = new OutPoint(txForInput.GetHash(), indexOutputToSpend)
            });

            // 出力1: 自分に返ってくるおつり
            txToAndroid.Outputs.Add(new TxOut()
            {
                Value = Money.Coins((decimal)(value - txfee)
            });
        }
    }
}
