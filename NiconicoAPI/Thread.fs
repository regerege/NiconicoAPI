namespace RLib.NiconicoAPI.Xml
#light

open System

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>thread</remarks>
type Thread =
    {
        /// コメントサーバ接続時の日時
        ServerTime : DateTime
        /// チケットID
        Ticket : string
        /// ローカルのコメントサーバ接続時間
        RunDate : DateTime

        /// コメントサーバ接続時のタイミングの経過時間
        BaseElapsedTime : TimeSpan
    }
    /// 経過時間を
    member x.ElapsedTime =
        (DateTime.Now - x.RunDate)
        + x.BaseElapsedTime