namespace RLib.NiconicoAPI

open RLib.NiconicoAPI.Test
open RLib.NiconicoAPI.Xml

open NUnit.Framework
open System

[<TestFixture>]
module CommentClientTest =
    let cookie = ChromeCookie.getCookie()
    
    [<Explicit()>]
    [<Test;>]
    let ``CommentClientの正常パターンテスト``() =
        let xml = 
            HTTPHelper.getplayerstatus
                <| (fun () -> cookie.Value)
                <| "co1397033"
        let gps = XmlReaders.GetPlayerStatusReader xml
        use client = new CommentClient(gps)
        (*
            m: メインスレッドID
            s: async内のスレッドID（つまり非同期のスレッドID）
            ex: Exception
        *)
        //送受信イベント
        client.Transceiver.Add(function
            | CommentData.SendOK (comment,m,s) ->
                printfn "Send[%d, %d] %s" m s comment
            | CommentData.ReceiveOK(data,m,s) ->
                match data with
                | ReciveData.Comment d ->
                    printfn "Recv[%d, %d] %s" m s d.Comment
                | x -> printfn "[??] %A" x
            | CommentData.SendNG (ex,m,s)
            | CommentData.ReceiveNG (ex,m,s) ->
                printfn "Err[%d, %d] %s" m s
                    <| ex.ToString()
        )
        // 受信待ちを開始
        client.ReceiveStart()

        System.Threading.Thread.Sleep(5 * 1000)
        Assert.True(true)
