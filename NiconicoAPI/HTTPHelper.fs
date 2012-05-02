namespace RLib.NiconicoAPI
#light

open System
open System.Net
open System.Net.Security
open System.Net.Sockets
open System.IO
open System.Text

/// HTTP通信ヘルパー
module public HTTPHelper =
    /// HTTPCommunicationモジュール内専用のURLフォーマット定義
    module private NiconicoURL =
        /// ニコニコ動画のログインURL
        let Login = "https://secure.nicovideo.jp/secure/login?site=niconico"
        /// ニコニコ動画のステータス取得API
        let Status = "http://live.nicovideo.jp/api/getplayerstatus?v={0}"
        /// POSTKEY取得API
        let PostKey = "http://live.nicovideo.jp/api/getpostkey?thread={0}&block_no={1}"
        
    // イニシャライズ時にSSL証明書を無条件で許可する設定を行う。
    do
        ServicePointManager.ServerCertificateValidationCallback <-
            new RemoteCertificateValidationCallback(
                fun _ _ _ _ -> true)

    ///<summary>送信文字列をASCIIのバイナリデータに変換する。</summary>
    ///<param name="post">POSTデータ</param>
    let getData (post:string) =
        Encoding.ASCII.GetBytes(post)
        |> (fun x -> (x, int64 <| x.Length))

    /// 指定URLに対して文字列のデータをASCIIバイナリで送信を行い、
    /// HTMLをダウンロードする。
    let wget (f : HttpWebRequest -> unit) (url:string) post =
        let req = WebRequest.Create(url) :?> HttpWebRequest
        f req
        // 送信
        let (data,len) = getData post
        if 0L < len then
            req.ContentLength <- len
            use reqs = req.GetRequestStream()
            reqs.Write(data, 0, data.Length)
        // 受信
        use res = req.GetResponse()
        use ress = res.GetResponseStream()
        use sr = new StreamReader(ress, Encoding.UTF8)
        sr.ReadToEnd()

    /// ニコニコ動画API getplayerstatus を呼び出す。
    let getplayerstatus f id =
        match id with
        | NiconicoID.LiveID id
        | NiconicoID.CommunityID id ->
            wget
                <| (fun x -> x.CookieContainer <- f())
                <| String.Format(NiconicoURL.Status, id)
                <| ""

    /// ニコニコ動画API getpostkey を呼び出す。
    let getpostkey f block threadid =
        wget
            <| (fun x -> x.CookieContainer <- f())
            <| String.Format(NiconicoURL.PostKey, threadid, block)
            <| ""
        |> (fun (s:string) -> s.[(s.IndexOf("=")+1)..])
    
open RLib.NiconicoAPI.Xml
///<summary>ニコ生用のHTTP通信を提供</summary>
///<param name="cookie">Cookie情報</param>
type NiconicoHttp (cookie : CookieContainer) =
    let f = (fun () -> cookie)
    member x.GetStatus id =
        HTTPHelper.getplayerstatus f id
        |> XmlReaders.GetPlayerStatusReader
    member x.GetPostkey block id =
        HTTPHelper.getpostkey f block id
