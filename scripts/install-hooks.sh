#!/bin/sh
# Install native Git hooks by copying the repository template into .git/hooks.
# No third-party hook managers (Husky, etc.). Run from repository root.

set -e
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$REPO_ROOT"

TEMPLATE="$REPO_ROOT/scripts/pre-commit"
HOOK="$REPO_ROOT/.git/hooks/pre-commit"

if [ ! -f "$TEMPLATE" ]; then
  echo "Template not found: scripts/pre-commit"
  exit 1
fi

echo "Installing pre-commit hook (native Git)..."
cp "$TEMPLATE" "$HOOK"
chmod +x "$HOOK"
echo "Pre-commit hook installed at .git/hooks/pre-commit."
echo ""
echo "The hook runs on every 'git commit'. It runs:"
echo "  - FastLocal backend tests (when src/api/ changes)"
echo "  - Frontend typecheck + ESLint (when src/web/ changes)"
echo ""
echo "To bypass in emergencies: git commit --no-verify"
