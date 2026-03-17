# Pre-Commit Hooks (Quality Gate)

Pre-commit hooks run validation before each commit so you catch quality issues early. They are **optional but strongly recommended**. This repository uses **native Git hooks only** (no Husky or other hook management tools).

## What runs

| Check | When | Description |
|-------|------|-------------|
| **FastLocal backend tests** | When any file under `src/api/` is staged | `dotnet test --filter Category=FastLocal` in `src/api` (target: under 5 seconds). |
| **Frontend typecheck** | When any file under `src/web/` is staged | `npm run typecheck` in `src/web` (TypeScript). |
| **Frontend ESLint** | When any file under `src/web/` is staged | `npm run lint` in `src/web`. |

Only the relevant checks run based on staged files, so commits that touch only one area stay fast.

## Installation (native Git hooks)

The hook is a **tracked template** in the repo. Install it by copying into Git’s hooks directory:

From the **repository root**:

```bash
# Linux/macOS
bash scripts/install-hooks.sh

# Windows (cmd)
scripts\install-hooks.cmd
```

This copies `scripts/pre-commit` to `.git/hooks/pre-commit` and makes it executable. No npm or Node is required for hook installation. After this, the pre-commit hook runs automatically on `git commit`.

## Bypassing hooks (emergencies only)

To commit without running the hook (e.g. WIP or known temporary breakage):

```bash
git commit --no-verify
```

Use sparingly; CI will still run the same validations on push.

## Performance

- **FastLocal tests**: Designed to complete in under 5 seconds. They use `--no-build`; ensure the solution is built before committing (`dotnet build src/api`) if you changed backend code.
- **Frontend checks**: Typecheck and ESLint run only when frontend files are staged.

## Troubleshooting

### Hook doesn't run

- Run the install script from the **repository root**: `bash scripts/install-hooks.sh` (or `scripts\install-hooks.cmd` on Windows).
- Check that `.git/hooks/pre-commit` exists and is executable: `ls -la .git/hooks/pre-commit` (Unix).
- Run the validation script: `bash scripts/validate-hooks.sh`.

### Backend tests fail

- Build first: `dotnet build src/api` (or open and build in Visual Studio).
- If there are no tests with `Category=FastLocal`, add that category to fast unit tests; the hook expects at least the ability to run this filter.

### Frontend checks fail

- Install frontend deps: `cd src/web && npm install`.
- Fix TypeScript and ESLint errors reported; the hook blocks the commit until they pass.

### Updating the hook

Edit `scripts/pre-commit` (the template) and commit the change. Developers who already installed the hook should run the install script again to copy the updated template into `.git/hooks/pre-commit`.

## CI

The same validations (FastLocal tests, typecheck, ESLint) run in CI (e.g. `.github/workflows/validate.yml`). So even if you use `--no-verify`, the branch will not pass CI until these checks succeed.
