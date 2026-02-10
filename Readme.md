# sqlc-gen-fsharp

A [sqlc](https://sqlc.dev) WASM plugin that generates type-safe F# code from SQL.

> This is a fork of [kaashyapan/sqlc-gen-fsharp](https://github.com/kaashyapan/sqlc-gen-fsharp).
> The original project generated code targeting the [Fumble](https://github.com/AngelMunoz/Fumble) library.
> This fork generates plain ADO.NET code using `System.Data.IDbConnection` and `System.Data.IDataReader`,
> so the generated code works with any ADO.NET provider (e.g. `Microsoft.Data.Sqlite`).

## What it does

**Inputs**
- A database schema file (DDL)
- A file of SQL queries with [sqlc annotations](https://docs.sqlc.dev/en/latest/reference/query-annotations.html)
- A sqlc configuration file

**Outputs**
- `Models.fs` -- F# record types for tables and query result rows
- `Readers.fs` -- Functions that decode an `IDataReader` into F# records
- `Queries.fs` -- A `DB` class with typed methods for each query

Only **SQLite** is supported as the database engine.

## How to use

1. Install [sqlc](https://docs.sqlc.dev/en/latest/overview/install.html)
2. Create a `schema.sql` with your DDL statements
3. Create a `query.sql` with annotated queries:
    ```sql
    -- name: ListAuthors :many
    SELECT * FROM authors ORDER BY name;
    ```
4. Create `sqlc.json`:
    ```json
    {
      "version": "2",
      "plugins": [
        {
          "name": "fsharp",
          "wasm": {
            "url": "https://github.com/nordfjord/sqlc-gen-fsharp/releases/download/v1.0.1/sqlc-gen-fsharp.wasm",
            "sha256": "<sha256 of the wasm file>"
          }
        }
      ],
      "sql": [
        {
          "engine": "sqlite",
          "schema": "schema.sql",
          "queries": "query.sql",
          "codegen": [
            {
              "out": "src/Database",
              "plugin": "fsharp",
              "options": {
                "namespace": "MyApp.Database"
              }
            }
          ]
        }
      ]
    }
    ```
5. Run `sqlc generate`

See the `examples/` folder for sample setups.

## Generated code

The generated `DB` class takes an `IDbConnection` and exposes a method per query:

```fsharp
open System.Data

let conn : IDbConnection = (* your connection *)
let db = DB(conn)

// :one queries return ValueOption<T>
let author = db.GetAuthor(id = 1L)

// :many queries return ResizeArray<T>
let authors = db.ListAuthors()

// :exec queries return int (rows affected)
let rows = db.DeleteAuthor(id = 1L)
```

Nullable columns map to F# `option` types. Nullable parameters become optional arguments:

```fsharp
db.CreateAuthor(name = "Alice", bio = "Writer")   // bio is ?bio: string
```

## Config options

| Option | JSON key | Type | Description |
|--------|----------|------|-------------|
| Namespace | `namespace` | string | F# namespace for generated code |
| Output directory | `out` | string | Where to write generated files |
| Exact table names | `emit_exact_table_names` | bool | Skip singularization of table names for model types. Default: `false` |
| Exclude from inflection | `inflection_exclude_table_names` | string[] | Table names to exclude from automatic singularization |

## Features

- **Plain ADO.NET** -- generated code depends only on `System.Data` interfaces, no third-party libraries required
- **sqlite-vec / F32_BLOB** -- first-class support for `float32[]` vector columns via `MemoryMarshal`
- **`sqlc.slice()`** -- dynamic `IN (...)` clause expansion with runtime parameter binding
- **F# keyword escaping** -- reserved words like `type` are automatically wrapped in double backticks
- **Nullable handling** -- nullable columns become `option` types; nullable parameters become optional arguments

## Development

```bash
# Build the WASM plugin
bash build.sh

# Full pipeline: build WASM, generate code, format, test
bash test.sh

# Run F# tests only
dotnet test  # from test/

# Format generated F# code
fantomas .   # from test/
```
