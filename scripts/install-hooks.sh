#!/bin/sh
# Script to install git hooks (pre-commit, commit-msg, etc.)

# Install husky hooks in frontend if npm is available
echo "Installing pre-commit hooks..."

FRONTEND_DIR="csharp-cosmos/src/web"

if [ -d "$FRONTEND_DIR" ] && [ -f "$FRONTEND_DIR/package.json" ]; then
  echo "Installing Husky in frontend..."
  cd "$FRONTEND_DIR"
  npm install husky --save-dev
  npx husky install ../../.husky
  cd - > /dev/null
  echo "Husky hooks installed successfully."
fi

echo "Pre-commit hooks are ready."
echo ""
echo "Available hooks:"
echo "  - pre-commit: Runs linting and type checks before committing"
echo ""
echo "To bypass hooks if needed: git commit -n"
