module SqliteTests

open Xunit
open Swensen.Unquote
open Microsoft.Data.Sqlite
open FSharp.Data.LiteralProviders
open SAuthors

[<Literal>]
let initsql = TextFile<"sqlite/schema.sql">.Text

let createConnection () =
  let conn = new SqliteConnection "Data Source=:memory:"
  conn.Open()
  use cmd = conn.CreateCommand()
  cmd.CommandText <- initsql
  cmd.ExecuteNonQuery() |> ignore
  conn

[<Fact>]
let ``listAuthors returns empty list on fresh db`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.listAuthors ()
  test <@ result = [] @>

[<Fact>]
let ``createAuthor inserts and returns the author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.createAuthor ("Alice", "Author bio")
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Alice", Some "Author bio") @>

[<Fact>]
let ``createAuthor with None bio`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.createAuthor ("Bob")
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Bob", None) @>

[<Fact>]
let ``getAuthor returns the correct author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.createAuthor ("Alice", "Bio") |> ignore
  let result = db.getAuthor 1
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Alice", Some "Bio") @>

[<Fact>]
let ``getAuthor2 returns partial columns`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.createAuthor ("Alice", "Bio") |> ignore
  let result = db.getAuthor2 1
  test <@ result = ValueSome { Id = 1; Name = "Alice"; Bio = Some "Bio" } @>

[<Fact>]
let ``deleteAuthor removes the author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.createAuthor ("Alice", "Bio") |> ignore
  db.deleteAuthor 1 |> ignore
  let result = db.listAuthors ()
  test <@ result = [] @>

[<Fact>]
let ``listAuthors returns all authors in name order`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.createAuthor ("Charlie") |> ignore
  db.createAuthor ("Alice") |> ignore
  db.createAuthor ("Bob") |> ignore

  let result = db.listAuthors ()
  let names = result |> List.map (fun a -> a.Name)
  test <@ names = [ "Alice"; "Bob"; "Charlie" ] @>

[<Fact>]
let ``countAuthors returns correct count`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let empty = db.countAuthors ()
  test <@ empty = ValueSome 0 @>

  db.createAuthor ("Alice") |> ignore
  db.createAuthor ("Bob") |> ignore
  let result = db.countAuthors ()
  test <@ result = ValueSome 2 @>

[<Fact>]
let ``totalBooks returns count and sum`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.createAuthor ("Alice") |> ignore
  db.createAuthor ("Bob") |> ignore

  let result = db.totalBooks ()
  test <@ result = ValueSome { Cnt = 2; TotalBooks = Some 3.0 } @>

[<Fact>]
let ``dbString returns literal string`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.dbString ()
  test <@ result = ValueSome "Hello world" @>
