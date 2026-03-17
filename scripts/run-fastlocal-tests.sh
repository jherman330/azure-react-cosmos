#!/bin/sh
# Run FastLocal backend tests for pre-commit and CI.
# --no-build assumes the solution is already built; run dotnet build before committing if needed.

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
API_DIR="${REPO_ROOT}/src/api"

cd "$API_DIR" || exit 1

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found; skipping FastLocal tests."
  exit 0
fi

# Run tests with FastLocal category; quiet for speed (target under 5s per AC-FOUNDATION-018.7).
dotnet test --filter "Category=FastLocal" --no-build --verbosity quiet
exit $?
