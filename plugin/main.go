package main

import (
	"github.com/sqlc-dev/plugin-sdk-go/codegen"

	fsharp "github.com/kaashyapan/sqlc-gen-fsharp/internal"
)

func main() {
	codegen.Run(fsharp.Generate)
}
