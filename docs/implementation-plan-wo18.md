# Implementation Plan: Work Order 18 — Pre-Commit Quality Gate (Corrected)

**Alignment:** Native Git hooks only. No Husky, lint-staged, or other hook management tools (per work order out-of-scope and CI/CD blueprint).

## Summary

Configure a pre-commit quality gate using **only native Git hooks**: a tracked hook template in the repository is copied into `.git/hooks/pre-commit` by install scripts. Same behavior: FastLocal backend tests, frontend typecheck, frontend ESLint, fail-fast blocking, lightweight execution.

---

## Code changes

### 1. [create] `scripts/pre-commit`

Create the **pre-commit hook template** (tracked in the repo). The script must:

- Skip when `CI` is set (so CI can run validations separately).
- Determine changed areas via `git diff --cached --name-only`.
- If any `src/api/` file is staged: run FastLocal backend tests via `scripts/run-fastlocal-tests.sh`; exit non-zero on failure.
- If any `src/web/` file is staged: run frontend checks via `scripts/run-frontend-checks.sh` (typecheck then ESLint); exit non-zero on failure.
- Block the commit (exit 1) if any check fails; otherwise exit 0.

**Reason:** AC-FOUNDATION-018.1 (hook template in repository). AC-FOUNDATION-018.5 (fail-fast). Blueprint: native pre-commit template approach.

---

### 2. [create] `scripts/run-fastlocal-tests.sh`

Shell script to run FastLocal backend tests:

- Run from repo root or `src/api`: `dotnet test --filter Category=FastLocal --no-build --verbosity quiet` in `src/api`.
- Comment that `--no-build` assumes the solution is already built (developers should build before committing).
- Exit with the test process exit code.

**Reason:** AC-FOUNDATION-018.2 (run FastLocal backend tests before commit). Reusable from hook and CI.

---

### 3. [create] `scripts/run-frontend-checks.sh`

Shell script to run frontend validation:

- In `src/web`: run `npm run typecheck`, then `npm run lint`.
- Exit non-zero on first failure.

**Reason:** AC-FOUNDATION-018.3 and AC-FOUNDATION-018.4 (typecheck and ESLint before commit). Reusable from hook and CI.

---

### 4. [modify] `scripts/install-hooks.sh`

Install **native** pre-commit hook only:

- Resolve repository root (e.g. `git rev-parse --show-toplevel`).
- Copy `scripts/pre-commit` to `.git/hooks/pre-commit`.
- Make `.git/hooks/pre-commit` executable (`chmod +x`).
- No Husky, no npm, no root package.json. Output brief success and usage message (including `git commit --no-verify` for bypass).

**Reason:** Hook template must be installed into Git’s hook directory per blueprint; native approach only.

---

### 5. [modify] `scripts/install-hooks.cmd`

Windows equivalent of `install-hooks.sh`:

- Resolve repo root (e.g. from `%~dp0`).
- Copy `scripts\pre-commit` to `.git\hooks\pre-commit`.
- No Husky, no npm. Same messaging as shell script.

**Reason:** Windows developers need a one-command way to install the same native hook.

---

### 6. [create] `scripts/validate-hooks.sh`

Helper to verify hook installation:

- Check that `.git/hooks/pre-commit` exists and is executable.
- If not, instruct user to run `scripts/install-hooks.sh` (or `.cmd` on Windows).
- Suggest manual runs of `run-fastlocal-tests.sh` and `run-frontend-checks.sh` for testing without committing.

**Reason:** Reduces support burden; developers can confirm native hook setup.

---

### 7. [create] `docs/pre-commit-hooks.md`

Documentation for the **native Git hook** model:

- State that the repo uses **native Git hooks only** (no Husky or other hook managers).
- Describe what runs: FastLocal tests, typecheck, ESLint; when (by staged paths); fail-fast behavior.
- **Installation:** run `scripts/install-hooks.sh` or `scripts/install-hooks.cmd` from repo root; this copies `scripts/pre-commit` to `.git/hooks/pre-commit`. No npm required for hook setup.
- Bypass: `git commit --no-verify`.
- Performance: under 5 seconds for FastLocal (AC-FOUNDATION-018.7); typecheck/ESLint only when frontend changed.
- Troubleshooting: hook not run (re-run install script, check `.git/hooks/pre-commit`), backend/frontend failures, how to update (edit `scripts/pre-commit`, then re-run install script).
- CI: same validations run in CI; reference `validate.yml` or equivalent.

**Reason:** AC-FOUNDATION-018.6 (documentation for installation and usage); alignment with native hook model.

---

### 8. [modify] `README.md`

Add or update **Developer setup: pre-commit hooks**:

- One short paragraph: pre-commit catches quality issues (FastLocal, typecheck, ESLint); **native Git hooks only** (no Husky).
- Quick start: run `bash scripts/install-hooks.sh` or `scripts\install-hooks.cmd` from repo root.
- Link to `docs/pre-commit-hooks.md` for details.

**Reason:** AC-FOUNDATION-018.6; visibility for new developers.

---

### 9. [modify] `src/web/package.json`

Add a **typecheck** script so the hook and CI can run TypeScript-only check:

- Add `"typecheck": "tsc --noEmit"` to `scripts`.
- Do **not** add or rely on Husky/lint-staged for the pre-commit quality gate; lint-staged (if present) remains a separate, optional tool for file-level formatting.

**Reason:** AC-FOUNDATION-018.3 (frontend typecheck before commit).

---

### 10. [create] `.github/workflows/validate.yml`

CI workflow that mirrors the pre-commit checks:

- On pull_request/push to mainline (e.g. `main`, `master`): checkout, setup .NET, setup Node (for frontend), install frontend deps only (no root npm).
- Build backend (`src/api`), then run `scripts/run-fastlocal-tests.sh`, then `scripts/run-frontend-checks.sh`.
- Use the same scripts as the hook so behavior is identical.

**Reason:** Pre-commit can be bypassed; CI enforces the same quality gate (blueprint alignment).

---

## Out of scope (do not implement)

- **Husky** — not used; native Git hooks only.
- **lint-staged** (or similar) for the pre-commit quality gate — not used; hook invokes the above scripts directly.
- **Root package.json** for hooks — not needed; no `npm install` or `prepare` for hook installation.
- **.husky/** directory** — not used; template lives under `scripts/`, installed to `.git/hooks/`.

---

## Acceptance criteria mapping

- **AC-FOUNDATION-018.1:** Pre-commit hook template in repo → `scripts/pre-commit` (tracked).
- **AC-FOUNDATION-018.2:** FastLocal backend tests before commit → `run-fastlocal-tests.sh` invoked from hook when `src/api/` changed.
- **AC-FOUNDATION-018.3:** Frontend typecheck before commit → `run-frontend-checks.sh` (includes `npm run typecheck`) when `src/web/` changed.
- **AC-FOUNDATION-018.4:** Frontend ESLint before commit → same script (`npm run lint`) when `src/web/` changed.
- **AC-FOUNDATION-018.5:** Fail-fast, block commit on failure → hook exits 1 if any script fails.
- **AC-FOUNDATION-018.6:** Documentation for installation and usage → `docs/pre-commit-hooks.md` and README section; native hook model only.
- **AC-FOUNDATION-018.7:** Hook execution quick (e.g. under 5 s for FastLocal) → FastLocal script uses `--no-build` and quiet verbosity; only changed areas run.

This plan replaces any prior implementation plan that relied on Husky or lint-staged for the pre-commit quality gate.
