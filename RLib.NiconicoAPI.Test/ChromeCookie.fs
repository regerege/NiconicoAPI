namespace RLib.NiconicoAPI.Test

open System
open System.Data
open System.Data.SQLite
open System.IO
open System.Net

///<summary>Google Chrome用Cookie取得モジュール</summary>
///<remarks>ライブラリとしては追加しない廃止予定モジュール</remarks>
module ChromeCookie =
    let private getChromeCookieSql = @"
SELECT creation_utc, host_key, name, value, path, expires_utc, secure, httponly, last_access_utc, has_expires, persistent
FROM cookies
WHERE (host_key LIKE '%nicovideo%')
"
    let getCookie() =
        let root = @"Google\Chrome\User Data\Default\Cookies"
        let path =
            Environment.SpecialFolder.LocalApplicationData
            |> Environment.GetFolderPath
            |> (fun p -> Path.Combine(p,root))
        let cpath =
            Path.GetTempPath()
            |> (fun p -> Path.Combine(p,root))
        let mkdir (p:string) =
            if not <| Directory.Exists(p) then
                Directory.CreateDirectory(p) |> ignore
//        printfn "from:`%s`、to:`%s`" path cpath
        let rmdir (p:string) =
            if Directory.Exists(p) then
                Directory.Delete(p,true)
        let cp (f:string) (t:string) =
            if not <| File.Exists t then
                File.Copy(f,t)
        mkdir <| Path.GetDirectoryName(cpath)
        cp path cpath
        let rm () = rmdir <| Path.Combine(Path.GetTempPath(), "Google")
        let cs =
            @"Data Source={0};Pooling=true;FailIfMissing=false"
            |> (fun s -> String.Format(s,cpath))
        let dbseq = seq {
            use conn = new SQLiteConnection(cs)
            use cmd = new SQLiteCommand()
            cmd.CommandText <- getChromeCookieSql
            cmd.Connection <- conn
            conn.Open()
            use dr = cmd.ExecuteReader()
            while dr.Read() do
                let c = new Cookie()
                c.Domain <- dr.GetString(1)
                c.Name <- dr.GetString(2)
                c.Value <- dr.GetString(3)
                c.Path <- dr.GetString(4)
//                c.Expires <- new DateTime(dr.GetInt64(5), DateTimeKind.Utc)
//                c.Secure <- dr.GetInt32(6) = 0
//                c.HttpOnly <- dr.GetInt32(7) = 0
                yield c
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
            GC.Collect()
        }
        try
            let cc = new CookieContainer()
            dbseq |> Seq.iter(fun c -> cc.Add(c))
            Some cc
        finally
            rm ()
