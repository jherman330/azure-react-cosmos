# CI/CD Pipeline

This document describes the GitHub Actions CI/CD pipeline defined in
`.github/workflows/azure-dev.yml`.

## Workflow Triggers

| Trigger | Behaviour |
|---------|-----------|
| **Push to `main`** | Build, test, provision infrastructure, and deploy. |
| **Pull request targeting `main`** | Build and test only (no deployment). |
| **Manual (`workflow_dispatch`)** | Full build, test, provision, and deploy on demand. |

## Jobs

The pipeline is split into four parallel/sequential jobs:

```
┌──────────────────┐  ┌──────────────────┐  ┌─────────────────────────┐
│ build-backend    │  │ build-frontend   │  │ validate-infrastructure │
│ (.NET 8 restore, │  │ (npm ci,         │  │ (az bicep build)        │
│  build, test)    │  │  typecheck, lint, │  │                         │
│                  │  │  test, build)    │  │                         │
└────────┬─────────┘  └────────┬─────────┘  └────────────┬────────────┘
         │                     │                          │
         └─────────────────────┼──────────────────────────┘
                               ▼
                   ┌───────────────────────┐
                   │ deploy                │
                   │ (azd provision +      │
                   │  azd deploy)          │
                   │ Skipped on PRs        │
                   └───────────────────────┘
```

### build-backend

Runs on every trigger.

1. Checkout repository
2. Setup .NET 8 SDK
3. `dotnet restore src/api/Todo.Api.sln`
4. `dotnet build` in Release configuration
5. `dotnet test` — executes all backend unit tests (xUnit)

### build-frontend

Runs on every trigger.

1. Checkout repository
2. Setup Node 20 with npm cache
3. `npm ci` (reproducible install)
4. `npm run typecheck` — TypeScript compilation check
5. `npm run lint` — ESLint validation
6. `npm run test` — Vitest unit tests
7. `npm run build` — production Vite build

### validate-infrastructure

Runs on every trigger.

1. Checkout repository
2. `az bicep build --file infra/main.bicep` — compiles and validates all Bicep
   templates

### deploy

Runs only on push to `main` and `workflow_dispatch` (skipped for pull requests).
Waits for all three validation jobs to pass.

1. Checkout repository
2. Install Azure Developer CLI (`azd`)
3. Authenticate to Azure (federated credentials preferred, service principal
   fallback)
4. `azd provision --no-prompt` — deploy/update infrastructure via Bicep
5. `azd deploy --no-prompt` — deploy backend and frontend application artifacts

## Required Status Checks

Configure the following as required status checks in GitHub branch protection for
`main`:

- **Build & Test Backend**
- **Build & Test Frontend**
- **Validate Infrastructure**

This ensures pull requests cannot merge until all three checks pass.

## Branch Protection Rules

Recommended settings for the `main` branch:

- Require pull request before merging
- Require at least one approving review
- Require status checks to pass before merging (see above)
- Require branches to be up to date before merging
- Do not allow bypassing the above settings

## Secrets and Variables

### Repository Variables (`vars.*`)

| Variable | Description |
|----------|-------------|
| `AZURE_CLIENT_ID` | Azure AD application (client) ID for federated auth |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription |
| `AZURE_ENV_NAME` | azd environment name (e.g. `dev`) |
| `AZURE_LOCATION` | Azure region (e.g. `eastus2`) |

### Repository Secrets (`secrets.*`)

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | Service principal JSON (fallback when federated credentials are not configured) |
| `AZD_INITIAL_ENVIRONMENT_CONFIG` | Initial azd environment configuration for first-time provisioning |

### Setting Up Azure Authentication

**Preferred — Federated Identity Credentials (OIDC):**

```bash
azd pipeline config
```

This configures an Azure AD app registration with federated credentials for the
GitHub repository and populates the required variables automatically.

**Fallback — Service Principal:**

Create a service principal and store its JSON output as the `AZURE_CREDENTIALS`
repository secret:

```bash
az ad sp create-for-rbac --name "github-deploy" --role contributor \
  --scopes /subscriptions/<subscription-id> --sdk-auth
```

## Deployment Flow

```
Developer pushes to main
        │
        ▼
GitHub Actions triggered
        │
        ├── build-backend ──── restore → build → test
        ├── build-frontend ─── install → typecheck → lint → test → build
        └── validate-infra ─── bicep compile
                │
                ▼  (all three pass)
             deploy
                │
                ├── azd provision (Bicep → Azure resources)
                └── azd deploy    (artifacts → App Service + Static Web App)
```

## Relationship to Pre-Commit Hooks

The pre-commit hook (`scripts/run-fastlocal-tests.sh` and
`scripts/run-frontend-checks.sh`) provides fast local feedback before commits.
The CI pipeline runs a superset of those checks — full test suites, production
builds, and infrastructure validation — ensuring nothing slips through even if
hooks are bypassed.
