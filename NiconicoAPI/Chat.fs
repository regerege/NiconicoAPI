namespace RLib.NiconicoAPI.Xml
#light

open System

/// <summary>受信XMLのレコード</summary>
/// <remarks>chat</remarks>
type Chat = {
    /// スレッドID
    Thread : string
    /// コメント発言順序
    No : int
    /// 経過時間（サーバから送られてくるものだから、実は正しい？）
    Vpos : TimeSpan
    /// コメントサーバにコメントが届いた日時
    Date : DateTime
    /// メールアドレス？ 184だと匿名希望者
    Mail : string
    /// ユーザID
    UserID : string
    /// プレミアム会員フラグ
    Premium : int
    /// 不明
    Anonymity : int
    /// コメント
    Comment : string
//
//    /// 経過時間（偽？）
//    ElapsedTime : TimeSpan
}
