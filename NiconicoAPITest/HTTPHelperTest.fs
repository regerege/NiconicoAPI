namespace RLib.NiconicoAPI

open NUnit.Framework

open RLib.NiconicoAPI
open RLib.NiconicoAPI.Test

open System
open System.Net
open System.Xml.Linq
open System.Web

(*
HTTPHelperモジュールの設計書
*)
//[<Explicit()>]
[<TestFixture>]
module HTTPHelperTest =
    let cookie = ChromeCookie.getCookie()
    let threadid =
        let xn s = XName.Get s
        try
            HTTPHelper.getplayerstatus
                <| (fun () -> cookie.Value)
                <| "co1397033"
            |> XDocument.Parse
            |> (fun xml ->
                    xml
                        .Element(xn "getplayerstatus")
                        .Element(xn "ms")
                        .Element(xn "thread")
                        .Value
            )
        with
        | _ -> ""
        
//#region Cookie関係の仕様設計orテスト

//    [<Test;>]
//    let ``Google Chrome Cookieを取得`` () =
//        Assert.That(cookie.IsSome, Is.True)
//        ()

//#endregion

//#region wgetの設計
    [<Test;ExpectedException(typeof<WebException>)>]
    let ``wgetの存在しないURLの例外テスト`` () =
        let url = "http://abcdefg.co.jp/"
        let html : string =
            HTTPHelper.wget
                <| (fun _ -> ())
                <| url
                <| ""
        ()

    [<Test;>]
    let ``wgetのGET送信テスト`` () =
        let url = "http://localhost/GETPOSTTest/"
        let data =
            [("data", "あいうえお");]
            |> List.map(fun (a,b) -> sprintf "%s&%s" a <| HttpUtility.UrlEncode(b))
            |> (fun s -> String.Join("&", s))
        let html =
            HTTPHelper.wget
                <| (fun x ->
//                        x.Headers.Add("User-Agent", "NiconicoAPI")
//                        x.ContentType <- "application/x-www-form-urlencoded"
                        x.Method <- "GET"
                        )
                <| (url + "?" + data)
                <| ""
        Assert.That(html, Is.Not.EqualTo(""))
        Assert.That(html, Is.Not.Empty)

    [<Test;>]
    let ``wgetのPOST送信テスト`` () =
        let url = "http://localhost/GETPOSTTest/"
        let data =
            [
                ("data", "あいうえお");
                ("test", "test");
            ]
            |> List.map(fun (a,b) -> sprintf "%s=%s" a <| HttpUtility.UrlEncode(b))
            |> (fun s -> String.Join("&", s))
        let html =
            HTTPHelper.wget
                <| (fun x ->
                        x.UserAgent <- "NiconicoAPI"
                        x.ContentType <- "application/x-www-form-urlencoded"
                        x.Method <- "POST"
                        )
                <| url
                <| data
        Assert.That(html, Is.Not.EqualTo(""))
        Assert.That(html, Is.Not.Empty)

    [<Test;>]
    let ``HTMLの正常ダウンロードテスト`` () =
        let url = "http://localhost/GETPOSTTest/"
        let html : string =
            HTTPHelper.wget
                <| (fun _ -> ())
                <| url
                <| ""
        Assert.IsNotEmpty(html)
//#endregion

//#region getplayerstatusの設計

    let private getIDErrorDatas =
        [
            "aa25623";
            "おああ1397033";
            "aa87935797";
            "ああ87933262";
        ]
    [<TestCaseSource("getIDErrorDatas")>]
    [<ExpectedException(typeof<NiconicoIDException>)>]
    let ``getplayerstatus にIDの入力規則外例外のテスト1`` id =
        HTTPHelper.getplayerstatus
            <| (fun () -> new CookieContainer())
            <| id
        |> ignore

    let private getIDDatas =
        [
            "co1397033";
            "co1223363";
//            "co1288844";
//            "lv88478300";
        ]
    [<Explicit()>]
    [<TestCaseSource("getIDDatas")>]
    let ``getplayerstatus 正常IDパターンテスト`` id =
        if cookie.IsNone then failwith "Cookieを取得出来ませんでした。"
        let html =
            HTTPHelper.getplayerstatus
                <| (fun () -> cookie.Value)
                <| id
//        printfn "%s" html
        Assert.That(html.IndexOf("ok"), Is.GreaterThanOrEqualTo(0))

    let private getNoneIDDatas = ["co156713";"co9";"co99";]
    [<Explicit()>]
    [<TestCaseSource("getNoneIDDatas")>]
    let ``getplayerstatus 存在しないIDのテスト`` id =
        if cookie.IsNone then failwith "Cookieを取得出来ませんでした。"
        let html =
            HTTPHelper.getplayerstatus
                <| (fun () -> cookie.Value)
                <| id
//        printfn "%s" html
        Assert.That(html.IndexOf("fail"), Is.GreaterThanOrEqualTo(0))

//#endregion

//#region getpostkeyの設計

    [<Test;>]
//    [<ExpectedException(typeof<NullReferenceException>)>]
    let ``getpostkey のNULLエラー`` () =
        let key =
            HTTPHelper.getpostkey
                <| (fun () -> cookie.Value)
                <| 0
                <| null
        Assert.That(key, Is.EqualTo(""))
    [<Test;>]
    let ``getpostkey に両方指定不正送信`` () =
        let key =
            HTTPHelper.getpostkey
                <| (fun () -> new CookieContainer())
                <| 0
                <| "af@20r3:a!"
        Assert.That(key, Is.EqualTo(""))
    [<Test;>]
    let ``getpostkey にクッキーなし送信`` () =
        let key =
            HTTPHelper.getpostkey
                <| (fun () -> new CookieContainer())
                <| 0
                <| threadid
        Assert.That(key, Is.EqualTo(""))
    [<Test;>]
    let ``getpostkey に不正スレッドID送信`` () =
        let key =
            HTTPHelper.getpostkey
                <| (fun () -> cookie.Value)
                <| 0
                <| "fasdf!981d[@@"
        Assert.That(key, Is.EqualTo(""))

    [<Test;>]
    let ``getpostkey に成功`` () =
        let key =
            HTTPHelper.getpostkey
            <| (fun () -> cookie.Value)
            <| 0
            <| threadid
        Assert.That(key, Is.Not.Empty)

//#endregion

(*
XML ドキュメント (F#)
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
http://msdn.microsoft.com/ja-jp/library/dd233217.aspx
*)
