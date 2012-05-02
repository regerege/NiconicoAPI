namespace RLib.NiconicoAPI
#light

open System
open System.Text.RegularExpressions

/// ニコニコIDの入力規則が一致しない場合に投げられる例外
exception NiconicoIDException

///<summary>ニコニコIDに関するモジュール</summary>
module internal NiconicoID =
    ///<summary>ライブ放送IDまたはコミュニティーIDを判別する。</summary>
    ///<param name="id">ニコニコID</param>
    ///<exception cref="NiconicoIDException"></exception>
    let (|LiveID|CommunityID|) (id:string) =
        let lv = Regex.Match(id, "^lv\d{1,9}$")
        let co = Regex.Match(id, "^co\d{1,9}$")
        match (lv.Success,co.Success) with
        | true,_ -> LiveID(id)
        | _,true -> CommunityID(id)
        | _,_ -> raise NiconicoIDException

module Commons =
    let private entitys =
        [
            ("&amp;", "&")
            ("&lt;", "<")
            ("&gt;", ">")
            ("&quot;", "\"")
        ]
    /// <summary>HTMLエンコードを行う。</summary>
    /// <param name="text">文字列</param>
    let toHtmlEncode text =
        entitys
        |> List.fold (fun (acc:string) (e,s) ->
            acc.Replace(s,e)) text
    /// <summary>HTMLデコードを行う。</summary>
    /// <param name="text">文字列</param>
    let toHtmlDecode text =
        entitys
        |> List.fold (fun (acc:string) (e,s) ->
            acc.Replace(e,s)) text

///UNIX時間と日本時間の変換をサポート
module UnixTime =
    /// 日本時間とUNIX時間の差
    let UnixEpoch =
        new DateTime(1970, 1, 1, 9, 0, 0, DateTimeKind.Local)
    let UnixEpochTicks = 100000L

    ///<summary>UNIX時間をDateTimeに変換する。</summary>
    ///<param name="id">UNIX時間</param>
    let FromUnixTime (time) =
        UnixEpoch.AddSeconds(time)

    ///<summary>UNIX時間をTimeSpanに変換する。</summary>
    ///<param name="id">UNIX時間</param>
    let FromUnixTimeToTimeSpan (time) =
        new TimeSpan(time * UnixEpochTicks)

    ///<summary>DateTimeをUNIX時間に変換する。</summary>
    ///<param name="id">DateTime</param>
    let ToUnixTime (date:DateTime) =
        date.Subtract(UnixEpoch).TotalSeconds

    ///<summary>TimeSpanをUNIX時間に変換する。</summary>
    ///<param name="id">UNIX時間</param>
    let FromTimeSpanToUnixTime (ts:TimeSpan) =
        ts.Ticks / UnixEpochTicks
