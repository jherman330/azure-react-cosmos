@echo off
REM Batch script to install git hooks (pre-commit, commit-msg, etc.)

echo Installing pre-commit hooks...

set "FRONTEND_DIR=csharp-cosmos\src\web"

if exist "%FRONTEND_DIR%\package.json" (
    echo Installing Husky in frontend...
    cd "%FRONTEND_DIR%"
    call npm install husky --save-dev
    call npx husky install ..\..\.husky
    cd ..\..\..
    echo Husky hooks installed successfully.
)

echo Pre-commit hooks are ready.
echo.
echo Available hooks:
echo   - pre-commit: Runs linting and type checks before committing
echo.
echo To bypass hooks if needed: git commit -n
