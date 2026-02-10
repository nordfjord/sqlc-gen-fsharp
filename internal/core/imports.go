package core

import (
	"fmt"
	"strings"

	plugin "github.com/tabbed/sqlc-go/codegen"
)

type Importer struct {
	Settings    *plugin.Settings
	DataClasses []Struct
	Enums       []Enum
	Queries     []Query
}

func (i *Importer) Imports(filename string, pkgName string) []string {
	switch filename {
	case "Models.fs":
		return i.modelImports()
	case "Readers.fs":
		return i.readersImports(pkgName)
	default:
		return i.queryImports(pkgName)
	}
}

func (i *Importer) readersImports(pkgName string) []string {
	uses := func(name string) bool {
		for _, q := range i.Queries {
			if !q.Ret.isEmpty() {
				if strings.HasPrefix(q.Ret.Type(), name) {
					return true
				}
			}
			if !q.Arg.isEmpty() {
				for _, f := range q.Arg.Struct.Fields {
					if strings.HasPrefix(f.Type.Name, name) {
						return true
					}
				}
			}
		}
		return false
	}

	std := stdImports(uses)
	stds := make([]string, 0, len(std))
	stds = append(stds, std...)
	stds = append(stds, "System.Data")

	return stds
}

func (i *Importer) modelImports() []string {
	uses := func(name string) bool {
		for _, q := range i.Queries {
			if !q.Ret.isEmpty() {
				if q.Ret.Struct != nil {
					for _, f := range q.Ret.Struct.Fields {
						if f.Type.Name == name {
							return true
						}
					}
				}
				if q.Ret.Type() == name {
					return true
				}
			}
			if !q.Arg.isEmpty() {
				for _, f := range q.Arg.Struct.Fields {
					if f.Type.Name == name {
						return true
					}
				}
			}
		}
		return false
	}

	std := stdImports(uses)
	std = append(std, "System.Data")

	return std
}

func stdImports(uses func(name string) bool) []string {
	out := []string{"System"}
	return out
}

func (i *Importer) queryImports(pkgName string) []string {

	uses := func(name string) bool {
		for _, q := range i.Queries {
			if !q.Ret.isEmpty() {
				if q.Ret.Struct != nil {
					for _, f := range q.Ret.Struct.Fields {
						if f.Type.Name == name {
							return true
						}
					}
				}
				if q.Ret.Type() == name {
					return true
				}
			}
			if !q.Arg.isEmpty() {
				for _, f := range q.Arg.Struct.Fields {
					if f.Type.Name == name {
						return true
					}
				}
			}
		}
		return false
	}

	std := stdImports(uses)

	stds := make([]string, 0, len(std))
	stds = append(stds, std...)
	stds = append(stds, "System.Data")

	packageImports := []string{fmt.Sprintf("%s.Readers", pkgName)}
	stds = append(stds, packageImports...)

	return stds
}
