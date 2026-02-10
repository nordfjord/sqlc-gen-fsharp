module SqlcTest

open System

[<EntryPoint>]
let main args =

  Sqlite.run ()
  0
