using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RLib.NiconicoAPI;
using RLib.NiconicoAPI.Test;

namespace ConsoleApplicationCSharp
{
    class Program
    {
        /// <summary>
        /// ヘルパーのインスタンスを一時的に保持
        /// </summary>
        private CommentClientHelper _helper = null;

        /// <summary>
        /// 実行
        /// </summary>
        private void Run()
        {
            // コメントサーバに接続するための必要最低限の情報
            var id = "co1397033";
            var cookieopt = ChromeCookie.getCookie();
            var cookie = cookieopt.Value;

            // コメントサーバ接続ヘルパー
            using (var client = new CommentClientHelper(cookie, id))
            {
                this._helper = client;
                client.Transceiver += new Microsoft.FSharp.Control.FSharpHandler<CommentData>(client_Transceiver);
                client.Start();

                // 無限入力待ち。
                this.InputLoop();
            }
        }

        private void InputLoop()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "/184")
                {
                    var come184 = this._helper.Come184;
                    this._helper.Come184 = !come184;
                    Console.WriteLine(
                        string.Format("184設定を{0}にしました。",
                        (!come184 ? "ON" : "OFF")));
                    continue;
                }
                if (input == "/quit") break;
                if (!String.IsNullOrWhiteSpace(input))
                {
                    this._helper.Send(input);
                }
            }
        }

        /// <summary>
        /// 受信イベント
        /// </summary>
        /// <param name="sender">不明</param>
        /// <param name="args">送受信データの判別共用体</param>
        private void client_Transceiver(object sender, CommentData args)
        {
            if (args.IsReceiveOK)
            {
                var data = args as CommentData.ReceiveOK;
                var rdata = data.Item1;
                if (rdata.IsComment)
                {
                    var comment = rdata as ReciveData.Comment;
                    var chat = comment.Item;
                    Console.WriteLine(
                        String.Format("[{0}][{1}] {2}",
                            this._helper.ElapsedTime,
                            chat.UserID,
                            chat.Comment));

                }
            }
        }

        static void Main(string[] args)
        {
            (new Program()).Run();
            GC.Collect();
            Console.WriteLine("終了します。");
            Console.WriteLine("何かキーを入力して下さい。");
            Console.ReadKey();
        }
    }
}
