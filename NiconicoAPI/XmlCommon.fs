namespace RLib.NiconicoAPI.Xml

(*
XML読み込みライブラリの作り方
１．try catch してでも、出来る限りエラーを
　　出さないように作り込む。
２．必須項目が足りない場合は
　　「XmlDefinedException」例外を投げる。
３．その他プログラム実行に必要な箇所の例外は
　　try catch しない。
*)

open RLib.NiconicoAPI

open System
open System.Xml
open System.Xml.Linq

/// ニコニコ動画のXMLとして必須項目が足りないXMLを読み込んだ時のエラー
exception XmlDefinedException of string

/// XML関連モジュール
module XmlCommon =
    /// 文字列のUnix時間をDateTimeに変換する
    let toDate str =
        str
        |> Double.Parse
        |> UnixTime.FromUnixTime
    let toTimeSpan str =
        str
        |> Int64.Parse
        |> UnixTime.FromUnixTimeToTimeSpan

    type XAttribute with
        member x.toInt =
            let (ret,value) = Int32.TryParse <| x.Value
            if ret then value
            else -1
        member x.toDate =
            toDate <| x.Value
        member x.toTimeSpan =
            toTimeSpan <| x.Value

    /// XElement に string 型引数の拡張メソッドを追加
    type XElement with
        member x.Attribute name =
            let n = XName.Get name
            let att = x.Attribute n
            if att = null then
                new XAttribute(n, "")
            else
                att
        member x.Element name =
            let n = XName.Get name
            let newEle = new XElement(n, "")
            if x.HasElements then
                let ele = x.Element n
                if ele = null then
                    newEle
                else
                    ele
            else
                newEle
        member x.toInt =
            let (ret,value) = Int32.TryParse <| x.Value
            if ret then value
            else -1
        member x.toDate =
            toDate <| x.Value
        member x.toTimeSpan =
            toTimeSpan <| x.Value

/// ニコニコ動画用XML読み込みライブラリ
module XmlReaders =
    // 拡張メソッドの読み込み
    open XmlCommon
        
    ///<summary>chat_result XMLを読み込みレコード型として返す。</summary>
    ///<param name="xml">threadテキスト</param>
    let ChatResultReader xml =
        let root = XElement.Parse xml
        {
            Thread = root.Attribute("thread").Value
            No = root.Attribute("no").toInt
            Status =
                root.Attribute("status").toInt
                |> ChatResultStatus.GetStatus
        }
        
    ///<summary>thread XMLを読み込みレコード型として返す。</summary>
    ///<param name="status">放送またはコミュニティのステータス情報</param>
    ///<param name="thread">コメントサーバ接続時情報</param>
    ///<param name="xml">threadテキスト</param>
    let ChatReader (status:GetPlayerStatus, thread:Thread, xml) =
        let root = XElement.Parse xml
        let date = root.Attribute("date").toDate
        let t = root.Attribute("mail").Value
        let n = root.Attribute("user_id").Value
        let tp = root.Attribute("premium").toInt
        {
            Thread = root.Attribute("thread").Value
            No = root.Attribute("no").toInt
            Vpos = root.Attribute("vpos").toTimeSpan
            Date = date
            Mail = root.Attribute("mail").Value
            UserID = root.Attribute("user_id").Value
            Premium = root.Attribute("premium").toInt
            Anonymity = root.Attribute("anonymity").toInt
            Comment =
                root.Value
                |> Commons.toHtmlDecode
        }

    ///<summary>thread XMLを読み込みレコード型として返す。</summary>
    ///<param name="status">放送またはコミュニティのステータス情報</param>
    ///<param name="xml">threadテキスト</param>
    let ThreadReader (status:GetPlayerStatus, xml) =
        let root = XElement.Parse xml
        let servertime = root.Attribute("server_time").toDate
        {
            ServerTime = servertime
            Ticket = root.Attribute("ticket").Value

            RunDate = DateTime.Now
            BaseElapsedTime =
                servertime - status.Stream.StartTime
        }

    ///<summary>getplayerstatus XMLを読み込みレコード型として返す。</summary>
    ///<param name="xml">getplayerstatusテキスト</param>
    let GetPlayerStatusReader xml =
        let root = XElement.Parse xml
        let status = root.Attribute("status").Value
        let time = toDate <| root.Attribute("time").Value

        let stream = root.Element "stream"
        if not <| stream.HasElements then
            raise <| XmlDefinedException "streamタグが存在しません。"
        let id = stream.Element("id").Value
        let r_stream : GetPlayerStatusStream =
            {
                Id               = stream.Element("id").Value
                WatchCount       = stream.Element("watch_count").toInt
                Title            = stream.Element("title").Value
                Description      = stream.Element("description").Value
                CommentCount     = stream.Element("comment_count").toInt
                DefaultCommunity = stream.Element("default_community").Value
                BaseTime         = stream.Element("base_time").toDate
                OpenTime         = stream.Element("open_time").toDate
                EndTime          = stream.Element("end_time").toDate
                StartTime        = stream.Element("start_time").toDate
            }

        let user = root.Element "user"
        let r_user : GetPlayerStatusUser =
            {
                UserID = user.Element("user_id").Value
                IsPremium = user.Element("is_premium").toInt
            }
            
        let rtmp = root.Element "rtmp"
        let r_rtmp : GetPlayerStatusRTMP =
            {
                Ticket = rtmp.Element("ticket").Value
            }

        let ms = root.Element "ms"
        let r_ms : GetPlayerStatusMs =
            {
                Addr = ms.Element("addr").Value
                Port = ms.Element("port").toInt
                Thread = ms.Element("thread").Value
            }

        let marquee = root.Element "marquee"
        let r_marquee : GetPlayerStatusMarquee =
            {
                Category = marquee.Element("category").Value
            }

        {
            Status = status
            Time = time
            Stream = r_stream
            User = r_user
            RTMP = r_rtmp
            Ms = r_ms
            Marquee = r_marquee
        }
