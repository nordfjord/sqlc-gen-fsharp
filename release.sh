#!/usr/bin/env bash
set -euo pipefail

if [ -z "${1:-}" ]; then
  echo "Usage: $0 <tag>" >&2
  echo "Example: $0 v1.2.0" >&2
  exit 1
fi

TAG="$1"

echo "Building sqlc-gen-fsharp.wasm..."
bash ./build.sh

echo "Creating GitHub release ${TAG}..."
gh release create "$TAG" --generate-notes

echo "Uploading sqlc-gen-fsharp.wasm to release ${TAG}..."
gh release upload "$TAG" sqlc-gen-fsharp.wasm

echo "Done. Release ${TAG} created with sqlc-gen-fsharp.wasm attached."
