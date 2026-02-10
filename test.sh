#!/bin/bash
set -e
bash ./build.sh
cd ./test/sqlite
sqlc generate
cd ../
fantomas .
dotnet test
