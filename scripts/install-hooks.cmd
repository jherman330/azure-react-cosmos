@echo off
REM Install native Git hooks by copying the repository template into .git/hooks.
REM No third-party hook managers (Husky, etc.). Run from repository root.

set "REPO_ROOT=%~dp0.."
cd /d "%REPO_ROOT%"

set "TEMPLATE=%REPO_ROOT%\scripts\pre-commit"
set "HOOK=%REPO_ROOT%\.git\hooks\pre-commit"

if not exist "%TEMPLATE%" (
    echo Template not found: scripts\pre-commit
    exit /b 1
)

echo Installing pre-commit hook (native Git)...
copy /Y "%TEMPLATE%" "%HOOK%" >nul
echo Pre-commit hook installed at .git\hooks\pre-commit.
echo.
echo The hook runs on every 'git commit'. It runs:
echo   - FastLocal backend tests (when src/api/ changes)
echo   - Frontend typecheck + ESLint (when src/web/ changes)
echo.
echo To bypass in emergencies: git commit --no-verify
