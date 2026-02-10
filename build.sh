#!bash
#
rm -f *.wasm
docker run -i --rm -w /src -v .:/src tinygo/tinygo:0.27.0 tinygo build -o sqlc-gen-fsharp.wasm -target wasi plugin/main.go
openssl sha256 sqlc-gen-fsharp.wasm
