To test

```bash
cd sqlite 
sqlc generate
cd ../
fantomas .
dotnet run
```

To build wasm release from directory root
```
docker run -it --rm -w /src -v ./sqlc-gen-fsharp:/src tinygo/tinygo:0.27.0 tinygo build -o sqlc-gen-fsharp.wasm -target wasi plugin/main.go
```
