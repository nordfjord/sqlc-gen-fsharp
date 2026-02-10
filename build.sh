#!bash
#
rm -f *.wasm
docker run -it --rm -w /src -v .:/src tinygo/tinygo:0.27.0 tinygo build -o sqlc-gen-fsharp.wasm -target wasi plugin/main.go
