# AGENTS.md

## Setup commands

```bash
# Build the WASM plugin and run all tests (full pipeline)
bash test.sh

# Build WASM
bash build.sh

# Regenerate Go code from quicktemplate (.qtpl) files
qtc  # run from internal/templates/

# Run F# tests only (skip WASM rebuild if Go code hasn't changed)
dotnet test  # run from test/

# Format generated F# code
fantomas .  # run from test/
```

## Code style

### Go (code generator)

- Type mappings live in `internal/core/sqlite_type.go`
- Reader expressions and parameter binding logic live in `internal/core/gen.go`
- Templates use [quicktemplate](https://github.com/valyala/quicktemplate) (`.qtpl` files in `internal/templates/`); after editing a `.qtpl` file, run `qtc` to regenerate the corresponding `.qtpl.go` file

### Generated F# code

- DB methods use PascalCase (`_.GetAuthor`, not `this.getAuthor`)
- Record field names are PascalCase (`Id`, `Name`, `Bio`)
- The `DB` type uses `_` as the self-identifier (not `this`)
- F# code is formatted with [fantomas](https://github.com/fsprojects/fantomas) using the `.editorconfig` in `test/`
- Indent with 2 spaces, max line length 120

### Tests

- Tests use xunit with [Unquote](https://github.com/SwensenSoftware/unquote) for assertions (`test <@ expr @>`)
- SQLite tests use in-memory databases via `Microsoft.Data.Sqlite`
- The sqlite-vec extension is loaded for `F32_BLOB` support
- Test SQL schema lives in `test/sqlite/schema.sql`, queries in `test/sqlite/query.sql`
- Generated code goes to `test/src/sqlite/`, hand-written tests are in `test/src/Sqlite.fs`
