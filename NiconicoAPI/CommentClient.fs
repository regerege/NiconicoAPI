namespace RLib.NiconicoAPI
#light
#nowarn "40"

open RLib.NiconicoAPI.Xml
open System
open System.Diagnostics
open System.Net.Sockets
open System.IO
open System.Text

///<summary>コメント投稿用キーの変更タイミング通知情報。</summary>
///<param name="no">発言順番</param>
///<param name="postkey">コメント投稿用キー</param>
type PostKeyArgs(no:int, postkey:string) =
    let mutable _no = no
    let mutable _postkey = postkey
    ///送信時に必要なキーの設定または取得
    member x.PostKey
        with get() = _postkey
        and set v = _postkey <- v
    ///最新の発言順番
    member x.No
        with get() = _no
        and set v = _no <- v
    ///<summary>POSTKEYのブロック番号の取得</summary>
    ///<remarks>POSTKEYは発言順番が100の倍数になったら切り替わる。</remarks>
    member x.BlockNo = _no/100
    ///<summary>コメント投稿用キーの変更タイミング通知情報。</summary>
    ///<param name="postkey">コメント投稿用キー</param>
    new(postkey) =
        new PostKeyArgs(0, postkey)

///受信データの判別共用体
type ReciveData =
    | ConnectionStatus of Thread
    | CommentResult of ChatResult
    | Comment of Chat
    | Unknown of string

///送受信データの判別共用体
type CommentData =
    ///受信エラー
    | ReceiveNG of Exception * int * int
    ///正常受信
    | ReceiveOK of ReciveData * int * int
    ///送信エラー
    | SendNG of Exception * int * int
    ///正常送信
    | SendOK of string * int * int

///<summary>ニコ生コメントサーバの為の非同期送受信を提供します。</summary>
///<param name="setting">CommentClientの設定情報</param>
type CommentClient (gps:GetPlayerStatus) =
//#region メンバ
    let _enc = Encoding.UTF8
    let _commentThreadID = "<thread thread=\"{0}\" version=\"20061206\" res_from=\"-200\"/>"
    let _commentSendTemp = @"<chat thread=""{0}"" ticket=""{1}"" vpos=""{2}"" postkey=""{3}"" mail=""{4}"" user_id=""{5}"" premium=""{6}"">{7}</chat>"

    let getThreadID() =
        System.Threading.Thread.CurrentThread.ManagedThreadId
    let _mainThreadID = getThreadID()

    let _client = new TcpClient(gps.Ms.Addr, gps.Ms.Port)
    let _stream = _client.GetStream()

    let _threadID = gps.Ms.Thread
    let _initTime = new TimeSpan()
    let rec _come184seq =
        seq { yield true; yield false; yield! _come184seq; }
    let _come184 = _come184seq.GetEnumerator()

    ///キャンセルトークン
    let token = Async.DefaultCancellationToken
    ///非同期送信イベント
    let transceiverEvent = new Event<CommentData>()
    ///非同期送信イベント
    let changedPostKeyEvent = new Event<PostKeyArgs>()
//#endregion

//#region 副作用のメンバ
    let _thread : Thread option ref = ref None
    let _chat : Chat option ref = ref None
    let _postkey = new PostKeyArgs("")
    let _recomment = ref ""
//#endregion

    /// 送信文字列をbyte配列に変換してNULLを付け足す。
    let toByte (comment:string) =
        Array.append
            <| _enc.GetBytes(comment)
            <| Array.zeroCreate 1
            
//#region 副作用のプロパティ
    ///経過時間を取得する。
    member x.ElapsedTime =
        if _thread.Value.IsSome then
            _thread.Value.Value.ElapsedTime
        else _initTime
    ///184の設定または取得する。
    member x.Come184
        with get () = _come184.Current
        and set v =
            if _come184.Current <> v then
                _come184.MoveNext() |> ignore

    /// 受信データの判別結果を取得する。
    member private x.GetReceiveData (text:string) =
        if text.StartsWith("<thread") then
            _thread := Some <| XmlReaders.ThreadReader(gps, text)
            //POSTKEYの変更通知
            if _chat.Value.IsNone then
                changedPostKeyEvent.Trigger(_postkey)
            ReciveData.ConnectionStatus _thread.Value.Value
        elif text.StartsWith("<chat_result") then
            let cr = XmlReaders.ChatResultReader text
            //POSTKEYの変更通知（コメ番不正などによって計算が狂う場合があるので、コメ番を-1に置き換えて、イベント側で再取得させる。）
            if cr.Status = ChatResultStatus.PostKeyError then
                _postkey.No <- -1
                changedPostKeyEvent.Trigger(_postkey)
                x.ReSendComment()
            ReciveData.CommentResult cr
        elif text.StartsWith("<chat") then
            _chat := Some <| XmlReaders.ChatReader(gps, _thread.Value.Value, text)
            let no = _chat.Value.Value.No
            _postkey.No <- no
            //POSTKEYの変更通知
            if 0 < no && no%100 = 0 then
                changedPostKeyEvent.Trigger(_postkey)
            ReciveData.Comment _chat.Value.Value
        else
            ReciveData.Unknown text
//#endregion

    ///送受信イベントを提供する。
    [<CLIEventAttribute>]
    member x.Transceiver = transceiverEvent.Publish
    ///PostKeyの変更タイミングイベントを提供する。
    [<CLIEventAttribute>]
    member x.ChangedPostKeyEvent = changedPostKeyEvent.Publish

    ///CommentClientSettingで設定したスレッドIDを送信する。
    member x.SendThreadId () =
        x.Send
            <| String.Format(_commentThreadID, _threadID)
            
    ///<summary>コメントXMLに変換後に送信する。</summary>
    ///<param name="comment">コメント</param>
    member x.SendComment comment =
        if _thread.Value.IsSome then
            _recomment := comment

            let comment = Commons.toHtmlDecode comment

            let t = _thread.Value.Value
            let xml =
                String.Format(
                    _commentSendTemp,
                    _threadID,
                    t.Ticket,
                    UnixTime.FromTimeSpanToUnixTime t.ElapsedTime,
                    _postkey.PostKey,
                    (if x.Come184 then "184" else ""),
                    gps.User.UserID,
                    gps.User.IsPremium,
                    comment)
            let xml = xml.Replace("mail=\"\"","")
            Debug.WriteLine(sprintf "%s" xml)
            x.Send xml
    ///<summary>前回のコメントを再送信する。</summary>
    member x.ReSendComment() = x.SendComment !_recomment

    ///<summary>コメントを非同期で送信する。</summary>
    ///<param name="comment">コメント</param>
    member x.Send (comment:string) =
        let asyncSend =
            async {
                let threadID = getThreadID()
                try
                    let b = toByte comment
                    do! _stream.AsyncWrite(b, 0, b.Length)
                    transceiverEvent.Trigger
                        <| CommentData.SendOK(comment, _mainThreadID, threadID)
                with
                | ex ->
                    transceiverEvent.Trigger
                        <| CommentData.SendNG(ex, _mainThreadID, threadID)
            }
        Async.Start(asyncSend, token)

    ///<summary>永続的に非同期コメント受信を行う。</summary>
    member x.ReceiveStart() =
        // 永続非同期受信を行う
        let rec loop arr = async {
            let threadID = getThreadID()
            try
                let! data = _stream.AsyncRead(1)
                let nextdata = Array.append arr data
                match data with
                | [| 0uy |] ->
                    //コメントに変換
                    let text =
                        nextdata
                        |> Array.filter ((<>)0uy)
                        |> _enc.GetString

                    Debug.WriteLine(sprintf "%s" text)
                    //各取得 XML で判別する。
                    let data = x.GetReceiveData text

                    //受信イベントの発行
                    transceiverEvent.Trigger
                        <| CommentData.ReceiveOK(data, _mainThreadID, threadID)
                | _ ->
                    //次の受信待ちを呼び出す。
                    do! loop nextdata
            with
            | ex ->
                transceiverEvent.Trigger
                    <| CommentData.ReceiveNG(ex, _mainThreadID, threadID)

            do! loop Array.empty
        }
        Async.Start(loop Array.empty, token)

    interface IDisposable with
        /// CommentClientに割り当てられたリソースをすべて解放する。
        member x.Dispose() =
            Async.CancelDefaultToken()
            try
                _stream.Dispose()
                _client.Close()
                // 必要？ 念のため相手側のサーバのコネクションを
                // 完全に消すために使用。
                GC.Collect()
            with | _ -> ()

///<summary>コメントを非同期で送信する。</summary>
///<param name="gps">コメント</param>
///<param name="gps">コメント</param>
type CommentClientHelper (cookie, id:string) =
    let _nhttp = new NiconicoHttp(cookie)
    let _gps = _nhttp.GetStatus(id)
    let _client = new CommentClient(_gps)
    
    //イニシャライズ時の設定
    do
        _client.ChangedPostKeyEvent.Add(fun args ->
            /// 不正コメ番号の場合はステータスXMLから再取得する。
            let (threadid,blockno) =
                if args.No = -1 then
                    let _gps = _nhttp.GetStatus(_gps.Stream.Id)
                    _gps.Ms.Thread, (_gps.Stream.CommentCount/100)
                else
                    _gps.Ms.Thread, args.BlockNo

            let postkey = _nhttp.GetPostkey blockno threadid
            Debug.WriteLine(
                sprintf "[%d,%d] POSTKEYのブロックNoが変更されました。"
                    <| args.No
                    <| args.BlockNo)
            args.PostKey <- postkey
        )

    ///ステータスXMLを取得する。
    member x.GetStatus = _gps
    ///コメントサーバクライアントを取得する。
    member x.Client = _client
    ///経過時間を取得する。
    member x.ElapsedTime = _client.ElapsedTime
    ///184の設定または取得する。
    member x.Come184
        with get() = _client.Come184
        and set v = _client.Come184 <- v

    ///送受信イベントを提供する。
    [<CLIEventAttribute>]
    member x.Transceiver = _client.Transceiver  // イベントの橋渡し

    ///<summary>コメントを送信する。</summary>
    ///<param name="comment">コメント</param>
    member x.Send comment =
        _client.SendComment comment
        
    ///<summary>コメントサーバへの接続を開始します。</summary>
    ///<remarks>開始前と同時にコメント受信が行われる為、開始前にイベントを設定する必要があります。</remarks>
    member x.Start () =
        _client.ReceiveStart()
        _client.SendThreadId()

    interface IDisposable with
        /// CommentClientに割り当てられたリソースをすべて解放する。
        member x.Dispose() =
            let i = _client :> IDisposable
            i.Dispose()
