all: sqlc-gen-fsharp sqlc-gen-fsharp.wasm

sqlc-gen-fsharp:
	cd plugin && go build -o ~/bin/sqlc-gen-fsharp ./main.go

sqlc-gen-fsharp.wasm:
	cd plugin && tinygo build -o sqlc-gen-fsharp.wasm -target=wasi main.go
	openssl sha256 plugin/sqlc-gen-fsharp.wasm

