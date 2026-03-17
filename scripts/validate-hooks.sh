#!/bin/sh
# Validate that the native pre-commit hook is installed (template copied to .git/hooks).

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
HOOK="$REPO_ROOT/.git/hooks/pre-commit"

echo "Validating pre-commit hook setup..."
echo ""

if [ ! -f "$HOOK" ]; then
  echo "FAIL: .git/hooks/pre-commit not found."
  echo "Run from repo root: bash scripts/install-hooks.sh  (or scripts\\install-hooks.cmd on Windows)"
  exit 1
fi

if [ ! -x "$HOOK" ]; then
  echo "FAIL: .git/hooks/pre-commit is not executable."
  echo "Run: chmod +x .git/hooks/pre-commit"
  exit 1
fi

echo "OK: .git/hooks/pre-commit exists and is executable."
echo ""
echo "The hook runs automatically on 'git commit'. To test without committing:"
echo "  scripts/run-fastlocal-tests.sh   # backend"
echo "  scripts/run-frontend-checks.sh   # frontend"
echo ""
echo "To bypass the hook in emergencies: git commit --no-verify"
exit 0
