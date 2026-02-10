module SqliteTests

open Xunit
open Swensen.Unquote
open Microsoft.Data.Sqlite
open FSharp.Data.LiteralProviders
open SAuthors

[<Literal>]
let initsql = TextFile<"sqlite/schema.sql">.Text

let createConnection () =
  let conn = new SqliteConnection("Data Source=:memory:")
  conn.Open()
  conn.LoadExtension("vec0")
  use cmd = conn.CreateCommand()
  cmd.CommandText <- initsql
  cmd.ExecuteNonQuery() |> ignore
  conn

[<Fact>]
let ``listAuthors returns empty list on fresh db`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.ListAuthors()
  test <@ result = [] @>

[<Fact>]
let ``createAuthor inserts and returns the author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.CreateAuthor("Alice", "Author bio")
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Alice", Some "Author bio") @>

[<Fact>]
let ``createAuthor with None bio`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.CreateAuthor("Bob")
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Bob", None) @>

[<Fact>]
let ``getAuthor returns the correct author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice", "Bio") |> ignore
  let result = db.GetAuthor 1
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Alice", Some "Bio") @>

[<Fact>]
let ``getAuthor2 returns partial columns`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice", "Bio") |> ignore
  let result = db.GetAuthor2 1
  test <@ result = ValueSome { Id = 1; Name = "Alice"; Bio = Some "Bio" } @>

[<Fact>]
let ``deleteAuthor removes the author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice", "Bio") |> ignore
  db.DeleteAuthor 1 |> ignore
  let result = db.ListAuthors()
  test <@ result = [] @>

[<Fact>]
let ``listAuthors returns all authors in name order`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Charlie") |> ignore
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore

  let result = db.ListAuthors()
  let names = result |> List.map (fun a -> a.Name)
  test <@ names = [ "Alice"; "Bob"; "Charlie" ] @>

[<Fact>]
let ``countAuthors returns correct count`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let empty = db.CountAuthors()
  test <@ empty = ValueSome 0 @>

  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore
  let result = db.CountAuthors()
  test <@ result = ValueSome 2 @>

[<Fact>]
let ``totalBooks returns count and sum`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore

  let result = db.TotalBooks()
  test <@ result = ValueSome { Cnt = 2; TotalBooks = Some 3.0 } @>

[<Fact>]
let ``dbString returns literal string`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.DbString()
  test <@ result = ValueSome "Hello world" @>

#nowarn "25"

[<Fact>]
let ``createEmbedding inserts and returns the embedding`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let embedding = Array.init 1536 (fun i -> float32 i)
  let (ValueSome result) = db.CreateEmbedding embedding
  test <@ result.Embedding = embedding @>

[<Fact>]
let ``getEmbedding round trips float32 array`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let embedding = Array.init 1536 (fun i -> float32 i * 0.1f)
  let (ValueSome e) = db.CreateEmbedding embedding
  let result = db.GetEmbedding e.Id
  test <@ result |> ValueOption.map (fun x -> x.Embedding) = ValueSome embedding @>
