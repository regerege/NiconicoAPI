namespace RLib.NiconicoAPI.Test

open System
open System.Data
open System.Data.Common
open System.IO
open System.Net

///<summary>Google Chrome用Cookie取得モジュール</summary>
///<remarks>ライブラリとしては追加しない廃止予定モジュール</remarks>
module ChromeCookie =
    let private getChromeCookieSql = @"
SELECT creation_utc, host_key, name, value, path, expires_utc, secure, httponly, last_access_utc, has_expires, persistent
FROM cookies
WHERE (host_key LIKE '%.nicovideo.jp') AND (name = 'user_session')
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
        let rm () =
            try
                if File.Exists cpath then
                    File.Delete(cpath)
                rmdir <| Path.Combine(Path.GetTempPath(), "Google")
            with | _ -> ()
        rm()
        mkdir <| Path.GetDirectoryName(cpath)
        cp path cpath
        let cs =
            @"Data Source={0};Pooling=true;FailIfMissing=false"
            |> (fun s -> String.Format(s,cpath))
        let createCookie (dr:DbDataReader) =
            try
                let c = new Cookie()
                c.Domain <- string dr.["host_key"]
                c.Name <- string dr.["name"]
                c.Value <- string dr.["value"]
                c.Path <- string dr.["path"]
                Some c
            with
            | ex ->
                System.Diagnostics.Trace.WriteLine(ex.Message)
                None
        let dbseq = seq {
            let factory = DbProviderFactories.GetFactory("System.Data.SQLite")
            use conn = factory.CreateConnection()
            use cmd = factory.CreateCommand()
            conn.ConnectionString <- cs
            cmd.Connection <- conn
            cmd.CommandText <- getChromeCookieSql
            conn.Open()
            use dr = cmd.ExecuteReader()
            while dr.Read() do
                yield createCookie dr
            dr.Dispose()
            cmd.Dispose()
            conn.Dispose()
            GC.Collect()
        }
        try
            let cc = new CookieContainer()
            dbseq |> Seq.choose id |> Seq.iter(fun c ->
                try
                    cc.Add(c)
                with | _ -> ())
            Some cc
        finally
            rm ()
            ()
