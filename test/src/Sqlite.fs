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
  test <@ result.Count = 0 @>

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
  let result = db.GetAuthor 1L
  test <@ result |> ValueOption.map (fun x -> x.Name, x.Bio) = ValueSome("Alice", Some "Bio") @>

[<Fact>]
let ``getAuthor2 returns partial columns`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice", "Bio") |> ignore
  let result = db.GetAuthor2 1L
  test <@ result = ValueSome { Id = 1L; Name = "Alice"; Bio = Some "Bio" } @>

[<Fact>]
let ``deleteAuthor removes the author`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice", "Bio") |> ignore
  db.DeleteAuthor 1L |> ignore
  let result = db.ListAuthors()
  test <@ result.Count = 0 @>

[<Fact>]
let ``listAuthors returns all authors in name order`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Charlie") |> ignore
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore

  let result = db.ListAuthors()
  let names = result |> Seq.map (fun a -> a.Name) |> Seq.toList
  test <@ names = [ "Alice"; "Bob"; "Charlie" ] @>

[<Fact>]
let ``countAuthors returns correct count`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let empty = db.CountAuthors()
  test <@ empty = ValueSome 0L @>

  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore
  let result = db.CountAuthors()
  test <@ result = ValueSome 2L @>

[<Fact>]
let ``totalBooks returns count and sum`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore

  let result = db.TotalBooks()
  test <@ result = ValueSome { Cnt = 2L; TotalBooks = Some 3.0 } @>

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

[<Fact>]
let ``createEvent with reserved keyword column`` () =
  use conn = createConnection ()
  let db = DB(conn)
  let result = db.CreateEvent("click", "btn")
  test <@ result |> ValueOption.map (fun x -> x.Type, x.Val) = ValueSome("click", Some "btn") @>

[<Fact>]
let ``getEventsByType filters by type`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateEvent("click", "a") |> ignore
  db.CreateEvent("hover", "b") |> ignore
  db.CreateEvent("click", "c") |> ignore

  let result = db.GetEventsByType("click")
  let vals = result |> Seq.map (fun e -> e.Val) |> Seq.toList
  test <@ vals = [ Some "a"; Some "c" ] @>

[<Fact>]
let ``maxAuthorId returns max id using aggregation`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore
  db.CreateAuthor("Charlie") |> ignore

  let result = db.MaxAuthorId()
  test <@ result = ValueSome 3L @>

[<Fact>]
let ``getAuthorsByIds returns matching authors in order`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice") |> ignore
  db.CreateAuthor("Bob") |> ignore
  db.CreateAuthor("Charlie") |> ignore

  let result = db.GetAuthorsByIds([ 1L; 3L ])
  let names = result |> Seq.map (fun a -> a.Name) |> Seq.toList
  test <@ names = [ "Alice"; "Charlie" ] @>

[<Fact>]
let ``getAuthorsByIds with empty list returns empty`` () =
  use conn = createConnection ()
  let db = DB(conn)
  db.CreateAuthor("Alice") |> ignore

  let result = db.GetAuthorsByIds(Seq.empty<int64>)
  test <@ result.Count = 0 @>
