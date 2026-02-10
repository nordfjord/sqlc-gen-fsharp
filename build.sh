#!bash
#
rm -f *.wasm
GOOS=wasip1 GOARCH=wasm go build -o sqlc-gen-fsharp.wasm plugin/main.go
openssl sha256 sqlc-gen-fsharp.wasm
