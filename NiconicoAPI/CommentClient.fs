[<AutoOpen>]
module CommentClient

open System
open System.Text
open FSharp.Net
open System.Net

do Net.ServicePointManager.ServerCertificateValidationCallback <- Net.Security.RemoteCertificateValidationCallback(fun _ _ _ _ -> true)

///コメント投稿用キーの変更タイミング通知情報。
type PostKey = { mutable PostKey:string;  mutable No:int }


///ニコ生コメントサーバの為の非同期送受信を提供します。
type CommentClient (cc:CookieContainer,id) as __ =
  /// ニコニコ動画API getplayerstatus を呼び出す。
  let getplayerstatus (id:string) =
    let getplayerstatus = sprintf "http://live.nicovideo.jp/api/getplayerstatus?v=%s" id
    Http.Request(getplayerstatus, cookieContainer = cc)
    |> GetPlayerStatus.Parse

  /// ニコニコ動画API getpostkey を呼び出す。
  let getpostkey threadid no =
    let getpostkey = sprintf "http://live.nicovideo.jp/api/getpostkey?thread=%d&block_no=%d" threadid (no/100)
    let s = Http.Request(getpostkey, cookieContainer = cc)
    s.[ s.IndexOf "=" + 1 .. ]
  let mutable gps = getplayerstatus id

  let _client = new Sockets.TcpClient(gps.Ms.Addr, gps.Ms.Port)
  let _stream = _client.GetStream()

  ///非同期送信イベント
  let commentReceived = Event<CommentData>()
  
  let mutable _thread : Thread.DomainTypes.Thread option = None
  let mutable _chat : Chat.DomainTypes.Chat option = None
  let mutable _postkey = { PostKey = ""; No = 0 }
  let mutable _recomment = ""

  //イニシャライズ時の設定
  let onPostKeyChanged () =
    /// 不正コメ番号の場合はステータスXMLから再取得する。
    if _postkey.No = -1 then
      gps <- getplayerstatus  gps.Stream.Id
      _postkey.No <- gps.Stream.CommentCount 
    _postkey.PostKey <- getpostkey  gps.Ms.Thread  _postkey.No 
            
  member __.Cookie = cc
  member __.GetStatus = gps
  ///経過時間を取得する。
  member __.ElapsedTime = _thread |> Option.map(fun __ -> DateTime.Now - XmlCommon.toDate (__.ServerTime - gps.Time))
  member val Come184 = false with get,set
  ///コメント受信イベントを提供する。
  member __.CommentReceived = commentReceived.Publish

  /// 受信データの判別結果を取得する。
  member private __.GetReceiveData text =
    let (|StartsWith|_|) pat (input:string) = if input.StartsWith pat then Some () else None
    match text with
    | StartsWith "<thread" ->
      _thread <- Some <| Thread.Parse text
      if _chat.IsNone then onPostKeyChanged ()
      ReciveData.ConnectionStatus _thread.Value
    | StartsWith "<chat_result" ->
      let cr = ChatResult.Parse text
      // コメ番不正などによって計算が狂う場合があるので、コメ番を-1に置き換えて、イベント側で再取得させる。）
      if enum cr.Status = ChatResultStatus.PostKeyError then
        _postkey.No <- -1
        onPostKeyChanged ()
        __.ReSendComment ()
      ReciveData.CommentResult cr
    | StartsWith "<chat" ->
      let chat = Chat.Parse text
      _chat <- Some chat
      _postkey.No <- chat.No
      if 0 < chat.No && chat.No%100 = 0 then onPostKeyChanged ()
      ReciveData.Comment chat
    | _ -> ReciveData.Unknown text

  /// コメントを非同期で送信する。
  member __.Send (comment:string) =
    async {
    commentReceived.Trigger <|
      try
        let b = [| yield! Encoding.UTF8.GetBytes comment; yield 0uy |] 
        _stream.Write(b, 0, b.Length)
        CommentData.SendOK comment
      with ex -> CommentData.SendNG ex
    } |> Async.Start

  ///CommentClientSettingで設定したスレッドIDを送信する。
  member __.SendThreadId () =
    sprintf """<thread thread="%d" version="20061206" res_from="-200"/>""" gps.Ms.Thread
    |> __.Send
      
  ///コメントXMLに変換後に送信する。
  member __.SendComment comment =
    if _thread.IsSome then
      _recomment <- comment
      let comment = Web.HttpUtility.HtmlDecode comment
      let vpos = UnixTime.FromTimeSpanToUnixTime __.ElapsedTime.Value 
      let mail = if __.Come184 then @"mail=""184""" else ""
      sprintf """<chat thread="%d" ticket="%s" vpos="%d" postkey="%s" %s user_id="%s" premium="%d">%s</chat>"""
        gps.Ms.Thread  _thread.Value.Ticket  vpos  _postkey.PostKey  mail  gps.User.UserId  gps.User.IsPremium  comment
      |> __.Send 
  ///前回のコメントを再送信する。
  member __.ReSendComment() = __.SendComment _recomment

  ///永続的に非同期コメント受信を行う。
  member __.ReceiveStart() =
    // 永続非同期受信を行う
    async {
    let nextdata = ref [||]
    while true do
      try
        let! data = _stream.AsyncRead 1
        nextdata := Array.append !nextdata data
        if data.[0] = 0uy then
          let data = Array.filter ((<>)0uy) !nextdata |> Encoding.UTF8.GetString |> __.GetReceiveData
          CommentData.ReceiveOK data |> commentReceived.Trigger
          nextdata := [||]
      with ex -> CommentData.ReceiveNG ex |> commentReceived.Trigger
    } |> Async.Start

  member __.Start () =
    __.ReceiveStart()
    __.SendThreadId()

  interface IDisposable with
    member __.Dispose() =
      Async.CancelDefaultToken()
      try
        _stream.Dispose()
        _client.Close()
        GC.Collect() // 必要？ 念のため相手側のサーバのコネクションを完全に消すために使用。
      with _ -> ()