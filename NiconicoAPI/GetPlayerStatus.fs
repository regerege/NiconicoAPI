namespace RLib.NiconicoAPI.Xml
#light

open System

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>/getplayerstatus/stream</remarks>
type GetPlayerStatusStream = {
    /// LiveID
    Id               : string
    /// 来場者数？
    WatchCount       : int
    /// 放送タイトル
    Title            : string
    /// 放送内容
    Description      : string
    /// 総コメント数
    CommentCount     : int
    /// コミュニティID
    DefaultCommunity : string
    /// ベース日付
    BaseTime         : DateTime
    /// ？？？日時
    OpenTime         : DateTime
    /// 放送終了日付
    EndTime          : DateTime
    /// 放送開始日付
    StartTime        : DateTime
}

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>/getplayerstatus/user</remarks>
type GetPlayerStatusUser = {
    /// ユーザID
    UserID    : string
    /// プレミアム会員フラグ
    IsPremium : int
}

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>/getplayerstatus/rtmp</remarks>
type GetPlayerStatusRTMP = {
//    Url : uri
    /// チケットID？
    Ticket : string
}

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>/getplayerstatus/ms</remarks>
type GetPlayerStatusMs = {
    /// コメントサーバのHOST
    Addr   : string
    /// コメントサーバのPORT
    Port   : int
    /// コメントスレッドID
    Thread : string
}

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>/getplayerstatus/marquee</remarks>
type GetPlayerStatusMarquee = {
    /// カテゴリ
    Category : string
}

/// <summary>受信XMLのレコード</getplayerstatus>
/// <remarks>getplayerstatus</remarks>
type GetPlayerStatus = {
    /// status属性
    Status  : string
    /// 開始日時？
    Time    : DateTime
    /// <summary>放送情報</summary>
    /// <remarks>streamタグ</remarks>
    Stream  : GetPlayerStatusStream
    /// <summary>接続ユーザー情報</summary>
    /// <remarks>userタグ</remarks>
    User    : GetPlayerStatusUser
    /// <summary>RTMPサーバ情報</summary>
    /// <remarks>rtmpタグ</remarks>
    RTMP    : GetPlayerStatusRTMP
    /// <summary>コメントサーバ情報</summary>
    /// <remarks>msタグ</remarks>
    Ms      : GetPlayerStatusMs
    /// <summary>その他の情報</summary>
    /// <remarks>marqueeタグ</remarks>
    Marquee : GetPlayerStatusMarquee
}
