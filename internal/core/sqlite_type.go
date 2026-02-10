package core

import (
	"strings"

	"github.com/sqlc-dev/plugin-sdk-go/plugin"
	"github.com/sqlc-dev/plugin-sdk-go/sdk"
)

// https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types
// ReaderTyp now stores ADO.NET IDataReader getter method names (e.g. GetInt32, GetString)
// Nullability is handled separately via IsNull + IsDBNull checks in the reader/pipeline generation
func sqliteType(req *plugin.GenerateRequest, col *plugin.Column) (string, string, string, bool) {

	columnType := strings.ToLower(sdk.DataType(col.Type))
	notNull := col.NotNull || col.IsArray

	switch columnType {

	case "int", "integer", "tinyint", "smallint", "mediumint", "bigint", "unsignedbigint", "int2", "int8":
		if notNull {
			return "int64", "GetInt64", "int64", false
		} else {
			return "int64 option", "GetInt64", "int64OrNone", false
		}
	case "blob":
		if notNull {
			return "byte[]", "GetBytes", "bytes", false
		} else {
			return "byte[] option", "GetBytes", "bytesOrNone", false
		}
	case "real", "double", "doubleprecision", "float":
		if notNull {
			return "double", "GetDouble", "double", false
		} else {
			return "double option", "GetDouble", "doubleOrNone", false
		}
	case "boolean", "bool":
		if col.NotNull {
			return "bool", "GetBoolean", "bool", false
		} else {
			return "bool option", "GetBoolean", "boolOrNone", false
		}

	case "date", "datetime":
		if notNull {
			return "DateTime", "GetDateTime", "dateTime", false
		} else {
			return "DateTime option", "GetDateTime", "dateTimeOrNone", false
		}
	case "timestamp":
		if notNull {
			return "DateTimeOffset", "GetDateTimeOffset", "dateTimeOffset", false
		} else {
			return "DateTimeOffset option", "GetDateTimeOffset", "dateTimeOffsetOrNone", false
		}

	}

	switch {

	case strings.HasPrefix(columnType, "character"),
		strings.HasPrefix(columnType, "varchar"),
		strings.HasPrefix(columnType, "varyingcharacter"),
		strings.HasPrefix(columnType, "nchar"),
		strings.HasPrefix(columnType, "nativecharacter"),
		strings.HasPrefix(columnType, "nvarchar"),
		columnType == "text",
		columnType == "clob":
		if notNull {
			return "string", "GetString", "string", false
		} else {
			return "string option", "GetString", "stringOrNone", false
		}

	case strings.HasPrefix(columnType, "f32_blob"):
		if notNull {
			return "float32[]", "GetF32Blob", "bytes", false
		} else {
			return "float32[] option", "GetF32Blob", "bytesOrNone", false
		}

	case strings.HasPrefix(columnType, "decimal"), columnType == "numeric":
		if notNull {
			return "decimal", "GetDecimal", "decimal", false
		} else {
			return "decimal option", "GetDecimal", "decimalOrNone", false
		}

	default:
		return columnType, columnType + "_unhandled_report_issue", columnType + "_unhandled_report_issue", false

	}

}
