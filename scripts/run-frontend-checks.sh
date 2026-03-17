#!/bin/sh
# Run frontend validation: TypeScript typecheck and ESLint (for pre-commit and CI).

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
WEB_DIR="${REPO_ROOT}/src/web"

cd "$WEB_DIR" || exit 1

if ! command -v npm >/dev/null 2>&1; then
  echo "npm not found; skipping frontend checks."
  exit 0
fi

echo "[frontend] TypeScript typecheck..."
if ! npm run typecheck; then
  echo "[frontend] TypeScript typecheck failed."
  exit 1
fi

echo "[frontend] ESLint..."
if ! npm run lint; then
  echo "[frontend] ESLint failed."
  exit 1
fi

exit 0
