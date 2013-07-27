open RLib.NiconicoAPI
open System
open RLib.NiconicoAPI.Test

/// コメントサーバに接続するための必要最低限の情報
let id = "co1397033"
let cookie = ChromeCookie.getCookie()
if cookie.IsNone then
    failwith "Cookieの取得に失敗しました。"

/// コメントサーバ接続ヘルパー
let client = new CommentClientHelper(cookie.Value, id)
// イベントを後から追加するとどうなるか。
client.Transceiver.Add(fun x ->
    match x with
    | CommentData.ReceiveOK(x,m,s) ->
        match x with
        | ReciveData.Comment d ->
            printfn "[%A][%s] %s"
                <| client.ElapsedTime
                <| d.UserID
                <| d.Comment
            ()
        | ReciveData.CommentResult d ->
            ()
        | ReciveData.ConnectionStatus d ->
            ()
        | ReciveData.Unknown text -> ()
    | CommentData.ReceiveNG(ex,m,s) -> ()
    | _ -> ()
)
client.Start()

/// 入力待ちを行う。
/// "/quit" と入力するとプログラムを終了します。
let rec InputLoop() =
    let comment = stdin.ReadLine()
    if comment = "/184" then
        let come184 = client.Come184
        client.Come184 <- not come184
        printfn "184設定を%sにしました。"
            <| (if not come184 then "ON" else "OFF")
    if comment <> "/quit" then
        if  not <| (comment = "/184" ||
                    String.IsNullOrWhiteSpace(comment)) then
            client.Send comment
        InputLoop()
InputLoop()

GC.Collect()
printfn "終了します。"
printfn "何かキーを入力して下さい。"
stdin.Read() |> ignore
