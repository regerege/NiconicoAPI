namespace RLib.NiconicoAPI

open NUnit.Framework
open RLib.NiconicoAPI
open System

/// UNIX時間の相互変換テスト
[<TestFixture>]
module UnixTimeTest =
    let date = new DateTime(2014, 4, 15, 3, 20, 0, 0, DateTimeKind.Local)
    let time = 1397499600.
    let ts = new TimeSpan(132000L * 100000L)
    let time2 = 132000L

    [<Test>]
    let ``UNIX時間をDateTimeに変換`` () =
        let t = UnixTime.FromUnixTime(time)
        Assert.That(t, Is.EqualTo(date))
    [<Test>]
    let ``DateTimeをUNIX時間に変換`` () =
        let t = UnixTime.ToUnixTime(date)
        Assert.That(t, Is.EqualTo(time))

    [<Test>]
    let ``UNIX時間をTimeSpanに変換`` () =
        let t = UnixTime.FromUnixTimeToTimeSpan(time2)
        Assert.That(t, Is.EqualTo(ts))
    [<Test>]
    let ``TimeSpanをUNIX時間に変換`` () =
        let t = UnixTime.FromTimeSpanToUnixTime(ts)
        Assert.That(t, Is.EqualTo(time2))
        
[<TestFixture>]
module CommonsTest =
    [<Test>]
    [<ExpectedException(typeof<NullReferenceException>)>]
    let ``toHtmlEncode のarugmentexceptionテスト`` () =
        Commons.toHtmlEncode(null) |> ignore
    [<Test>]
    let ``toHtmlEncode のブランク正常テスト`` () =
        let text = Commons.toHtmlEncode("")
        Assert.That(text, Is.EqualTo(""))
        
    [<Test>]
    [<ExpectedException(typeof<NullReferenceException>)>]
    let ``toHtmlDecode のarugmentexceptionテスト`` () =
        Commons.toHtmlDecode(null) |> ignore
    [<Test>]
    let ``toHtmlDecode のブランク正常テスト`` () =
        let text = Commons.toHtmlDecode("")
        Assert.That(text, Is.EqualTo(""))
        
    let getEncodeList = [
        [|"&"; "&amp;"|]
        [|"<"; "&lt;"|]
        [|">"; "&gt;"|]
        [|"\""; "&quot;"|]
        [|"qwertyuiopasdfghjklzxcvbnm1234567890"; "qwertyuiopasdfghjklzxcvbnm1234567890"|]
    ]
    [<TestCaseSource("getEncodeList")>]
    let ``toHtmlEncode の変換正常テスト`` (a,b) =
        let text = Commons.toHtmlEncode(a)
        Assert.That(text, Is.EqualTo(b))

    [<TestCaseSource("getEncodeList")>]
    let ``toHtmlDecode の変換正常テスト`` (b,a) =
        let text = Commons.toHtmlDecode(a)
        Assert.That(text, Is.EqualTo(b))
