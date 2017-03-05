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
        private static String myWif = "cT94hu1suZgyC6BkpqG1nW4zarmkMX3Wrhc63Sg6PuRfL7vsA5dD";
        // 自分のBitcoinアドレス
        private static String myAddress = "myiyp8HdFW5SmG2Ns2MgkcxfxxxdLG9NCt";

        /////////////////////
        // 演習3
        //
        // 送金に使用できるTransactionIdと、そのOutputのindex
        private static String txIdToSpend = "866bfa9764a94a87369f9d18428b305fb6b4e4181196b1c7f5ae7accb17a56ab";

        /////////////////////
        // 演習4
        //
        // 送金に使用できるOutputのindex
        private static int indexOutputToSpend = 1;

        
        /////////////////////
        // 演習3
        //
        // Android用Bitcoin WalletアプリのBitcoinアドレス
        private static String AndroidAddress = "mumS7dey1b84Djs3b12KH3mfEUVhf8zGK7";

        /////////////////////
        // 演習4
        //
        // txIdToSpendのOutputsのうち、自分が使えるOutputのindex


        static void Main(string[] args)
        {
            // 演習1:   秘密鍵を生成し、自分のBitcoinアドレスを作成しよう
            //          作成した秘密鍵とBitcoinアドレスをソースコードに埋め込んでおこう
            //createKeys();

            // 演習2:   自分のアドレス宛に、TestNet FaucetからBitcoinを送金しよう
            //          http://tpfaucet.appspot.com/

            // 演習3:   AndroidのBitcoin Walletアプリをインストールしよう
            //          Play Storeで「Bitcoin Wallet for Testnet」アプリを検索。「Bitcoin Wallet」はMainnet用なので注意
            //          また、AndroidのBitcoin WalletのBitcoinアドレスを調べて、ソースコードに埋め込んでおこう
            //          (ホーム画面のQRコードをタップ。スペース不要、大文字小文字区別あり)

            // 演習4:   自分のアドレスの残金を確認しよう
            //          また、支払いに使用できるTransactionのIDをソースコードのtxIdToSpendに埋め込んでおこう
            CheckMyBalance();

            // 演習5:   自分宛てのトランザクションの内容を確認し、自分が使えるOutPointのindexをソースコードのindexOutputToSpendに埋め込んでおこう
            //findOutPointToSpent();

            // 演習5:   自分のアドレスから、AndroidのWalletに1.0BTC送金してみよう
            //txBtcToAndroidWallet((decimal)1.0);
        }

        // 演習1: 秘密鍵およびBitcoinアドレス鍵の生成
        public static void createKeys()
        {
            Key privateKey = new Key(); //秘密鍵の生成

            // 秘密鍵は通常、WIF(Wallet Import Format) = Bitcoin Secret形式で扱う
            BitcoinSecret testNetSecret = privateKey.GetBitcoinSecret(Network.TestNet); // NetWork.Main / Network.TestNet
            Console.WriteLine("Bitcoin Secret(WIF): {0}", testNetSecret);

            PubKey publicKey = privateKey.PubKey; //公開鍵の生成

            // Bitcoin Addressへ変換
            BitcoinPubKeyAddress testNetAddress = publicKey.GetAddress(Network.TestNet);
            Console.WriteLine("Address: {0}", testNetAddress);
        }

        // 演習3:   自分のアドレスの残金確認
        public static void CheckMyBalance()
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            BalanceModel balance = client.GetBalance(new BitcoinPubKeyAddress(myAddress).ScriptPubKey).Result;

            foreach(var tx in balance.Operations.Select((v, i) => new { v, i }))
            {
                Console.Write("Tx{0}:{1}\n\tTxId: {2}\n", tx.i, tx.v.Amount, tx.v.TransactionId);
                foreach(var rcvd in tx.v.ReceivedCoins.Select((v, i) => new { v, i }))
                    Console.Write("\tRcvd{0}:{1}\n", rcvd.i, rcvd.v.Amount);
                foreach (var spent in tx.v.SpentCoins.Select((v, i) => new { v, i }))
                    Console.Write("\tSpent{0}:{1}\n", spent.i, spent.v.Amount);
            }
        }

        // 演習4: 自分が使えるOutPointを特定
        public static void findOutPointToSpent()
        {
            // 自分のScriptPubKeyを表示
            Console.WriteLine("My ScriptPubKey = {0}", new BitcoinPubKeyAddress(myAddress).ScriptPubKey);

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
        public static void txBtcToAndroidWallet(decimal txValue)
        {
            decimal txFee = (decimal)0.01;

            // トランザクションを作成
            Transaction txToAndroid = new Transaction();

            // 入力
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            GetTransactionResponse txForInput = client.GetTransaction(uint256.Parse(txIdToSpend)).Result;
            decimal txFund = (decimal)50.0;
            var a = txForInput.ReceivedCoins[indexOutputToSpend].Amount;

            txToAndroid.Inputs.Add(new TxIn()
            {
                PrevOut = txForInput.ReceivedCoins[indexOutputToSpend].Outpoint,
                ScriptSig = BitcoinAddress.Create(myAddress).ScriptPubKey,
            });

            // 出力1: Andoroidへの送金
            txToAndroid.Outputs.Add(new TxOut()
            {
                Value = Money.Coins((decimal)txValue),
                ScriptPubKey = new BitcoinPubKeyAddress(myAddress).ScriptPubKey,
            });

            // 出力2: 自分に返ってくるおつり
            txToAndroid.Outputs.Add(new TxOut()
            {
                Value = Money.Coins((decimal)(txFund - txFee - txValue)),
                ScriptPubKey = new BitcoinPubKeyAddress(AndroidAddress).ScriptPubKey,
            });

            // トランザクションに署名
            txToAndroid.Sign(new BitcoinSecret(myWif), false);

            // ブロックチェーンにメッセージを送信
            BroadcastResponse res = client.Broadcast(txToAndroid).Result;

        }
    }
}
