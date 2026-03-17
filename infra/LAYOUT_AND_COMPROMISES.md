# Bicep Layout and Intentional Compromises

## Final Module Structure

```
infra/
├── main.bicep                    # Subscription-scoped orchestration; deploys RG + all modules
├── main.parameters.json          # Parameters (azd env vars)
├── abbreviations.json            # Naming prefixes (AC-FOUNDATION-019.6)
├── bicepconfig.json              # Linter rules
├── README.md
├── LAYOUT_AND_COMPROMISES.md     # This file
├── parameters/
│   ├── dev.parameters.json
│   ├── staging.parameters.json
│   └── prod.parameters.json
├── scripts/
│   └── init-secrets.md           # Post-provision secret seeding guidance
├── modules/                      # New modules (WO-19)
│   ├── app-insights.bicep
│   ├── app-service.bicep        # Plan + API app only
│   ├── cosmos-db.bicep
│   ├── key-vault.bicep
│   ├── redis.bicep
│   ├── static-web-app.bicep
│   ├── rbac.bicep                # Key Vault role assignments only
│   └── apim.bicep
└── app/                          # Existing (kept)
    ├── cosmos-role-assignment.bicep   # Cosmos DB Data Contributor for API
    ├── api-appservice-avm.bicep
    ├── web-appservice-avm.bicep
    ├── db-avm.bicep
    └── ...
```

## Module Paths and Dependencies

- **main.bicep** uses `./modules/<name>.bicep` and `./app/cosmos-role-assignment.bicep` (paths relative to `infra/`).
- **Deployment order** (implicit): monitoring → keyVault → cosmos → staticWebApp → appService → apiCosmosRole → redis → rbac → apim (if useAPIM).
- **modules/*.bicep** load `../abbreviations.json` (relative to each module file).

## Intentional Compromises

1. **Cosmos DB role assignment outside RBAC module**  
   The API’s Cosmos DB Built-in Data Contributor assignment is in **app/cosmos-role-assignment.bicep**, not in **modules/rbac.bicep**, to avoid a circular dependency: App Service needs Cosmos endpoint/database in settings (Cosmos deployed first), and Cosmos role assignment needs the API’s principal ID (App Service deployed first). Keeping the Cosmos role in a separate module that runs after both Cosmos and App Service breaks the cycle.

2. **Key Vault RBAC scope**  
   **modules/rbac.bicep** uses an `existing` Key Vault reference by name (same resource group) so `scope` is a resource symbol, satisfying Bicep’s type requirement. Role assignments are therefore scoped to that Key Vault only.

3. **APIM module**  
   **modules/apim.bicep** creates only the APIM service. Backend API URL and Application Insights integration are not wired in this module to keep it minimal and avoid validation/unused-param noise. They can be added later or configured outside Bicep.

4. **Redis secret outputs**  
   **modules/redis.bicep** exposes `primaryKey` and `connectionString` as `@secure()` outputs for init-secrets/Key Vault seeding. They remain in deployment outputs but are marked secure.

5. **Static Web App**  
   No deployment token output at deploy time (avoid listSecrets on new resource). Token can be retrieved after creation (e.g. Azure CLI) for CI/CD and stored in Key Vault per **scripts/init-secrets.md**.

6. **Frontend hosting**  
   **main.bicep** deploys one App Service (API) and one Static Web App (frontend). The existing **app/** AVM-based web/api App Service definitions are unchanged; **azure.yaml** still references `host: appservice` for both services. Aligning **azure.yaml** with Static Web App for the web service is a separate, optional step.

## Validation

- `az bicep build --file main.bicep` from `infra/` completes successfully with the current **bicepconfig.json** linter settings.
