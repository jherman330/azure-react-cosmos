# Bicep Infrastructure as Code

This folder contains Bicep templates for provisioning Azure resources used by this application. The design follows AC-FOUNDATION-019 and the Infrastructure blueprint.

## Overview

- **main.bicep** – Subscription-scoped orchestration template. Creates a resource group and deploys all service modules.
- **modules/** – Reusable Bicep modules per Azure service:
  - **app-insights.bicep** – Log Analytics workspace and Application Insights
  - **app-service.bicep** – App Service Plan and API App Service (managed identity, App Insights, Key Vault refs)
  - **cosmos-db.bicep** – Cosmos DB account (serverless), database, container, and optional role assignment
  - **key-vault.bicep** – Key Vault with RBAC (no access policies)
  - **redis.bicep** – Azure Cache for Redis
  - **static-web-app.bicep** – Static Web App for the React frontend
  - **rbac.bicep** – Key Vault role assignments (Secrets User for API, Secrets Officer for deployment principal)
  - **apim.bicep** – Optional API Management (when `useAPIM` is true)
- **parameters/** – Environment-specific overrides: `dev.parameters.json`, `staging.parameters.json`, `prod.parameters.json`.
- **abbreviations.json** – Naming prefixes used by main and modules (e.g. `cosmos-`, `kv-`, `app-`).

## Deployment with Azure Developer CLI (azd)

From the repository root:

```bash
# Provision infrastructure (Bicep) and set up the environment
azd up

# Or provision only (no app deploy)
azd provision

# Deploy application services only (after provision)
azd deploy --all
```

`azd` uses **main.parameters.json** and injects environment variables (e.g. `AZURE_ENV_NAME`, `AZURE_LOCATION`, `AZURE_PRINCIPAL_ID`). For a specific environment, you can pass a parameters file:

```bash
azd provision --parameter-file infra/parameters/dev.parameters.json
```

## Parameter files

- **main.parameters.json** – Default parameters; values often reference `${AZURE_*}` or `${USE_APIM=false}` for azd.
- **parameters/dev.parameters.json** – Dev overrides (e.g. Basic SKUs, no APIM).
- **parameters/staging.parameters.json** – Staging (Standard SKUs, production-like).
- **parameters/prod.parameters.json** – Production (e.g. Premium App Service, APIM, Standard Redis).

## RBAC and managed identity

- The **API App Service** has a system-assigned managed identity.
- It receives **Key Vault Secrets User** (via **rbac.bicep**) so it can read secrets at runtime.
- It receives **Cosmos DB Built-in Data Contributor** (via **app/cosmos-role-assignment.bicep**).
- The **deployment principal** (e.g. your user or pipeline) gets **Key Vault Secrets Officer** so it can seed secrets once; restrict or remove this after initial setup if desired.

## Security

- Key Vault uses **RBAC authorization** only (no access policies).
- Cosmos DB has **local auth disabled**; access is via managed identity and role assignments.
- App Service uses **HTTPS only** and **minimum TLS 1.2**.

## Post-provision: secrets (init-secrets)

After the first provision, store runtime secrets in Key Vault (e.g. Cosmos key, Redis connection string, App Insights connection string). Options:

1. Use **infra/scripts/init-secrets.md** for guidance and Azure CLI examples to populate Key Vault from deployment outputs.
2. Manually add secrets in the Azure Portal or with Azure CLI.

The deployment principal needs **Key Vault Secrets Officer** (granted by **rbac.bicep**) to create/update secrets. After seeding, the API reads them via managed identity.

## Troubleshooting

- **Deployment fails on a module** – Check the deployment name in the Azure Portal (Resource group → Deployments) for the exact error.
- **API cannot read Key Vault** – Ensure **rbac.bicep** ran and the API’s managed identity has **Key Vault Secrets User** on the vault.
- **API cannot access Cosmos DB** – Ensure **app/cosmos-role-assignment.bicep** ran and the API’s principal has Cosmos DB Data Contributor.
- **CORS errors** – `API_ALLOW_ORIGINS` is set from the Static Web App default hostname; ensure the frontend origin matches.

## Modifying templates

- Add new resources in the appropriate **modules/** file or add a new module and reference it from **main.bicep**.
- Keep naming in **abbreviations.json** and pass **tags** from **main.bicep** for cost and governance.
- Run **bicep build** (or **az bicep build**) from the repo root or `infra/` to validate.
