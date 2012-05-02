namespace RLib.NiconicoAPI.Xml
#light

open System

(*
<chat_result thread="1169990486" status="1"/>
*)

/// 送信結果ステータス
type ChatResultStatus =
    /// 送信成功
    | OK
    /// 送信失敗
    | NG
    /// スレッドIDエラー
    | ThreadError
    /// チケットエラー
    | TicketError
    /// POSTKEYエラー
    | PostKeyError
    /// コメントブロックエラー
    | BlockError
    /// 不明
    | Unknown
    ///<summary>数値からステータスを取得。</summary>
    ///<param name="v">ステータス値</param>
    static member GetStatus v =
        match v with
        | 0 -> OK
        | 1 -> NG
        | 2 -> ThreadError
        | 3 -> TicketError
        | 4 -> PostKeyError
        | 5 -> BlockError
        | _ -> Unknown

/// <summary>送信結果ステータスXMLのレコード</summary>
/// <remarks>chat_result</remarks>
type ChatResult = {
    /// スレッドID
    Thread : string
    /// コメント発言順序
    No : int
    /// 送信結果のステータス
    Status : ChatResultStatus
}
