# Key Vault Secret Initialization

After `azd provision`, store these secrets in Key Vault so the API can read them via managed identity:

| Secret Name | Source |
|-------------|--------|
| `CosmosDbConnectionString` | Cosmos deployment (or AZURE_COSMOS_ENDPOINT + key from portal) |
| `RedisConnectionString` | Redis module output (`connectionString`) |
| `ApplicationInsightsConnectionString` | App Insights deployment output |
| `StaticWebAppDeploymentToken` | Static Web App: `az staticwebapp secrets list` (for CI/CD) |

The deployment principal needs **Key Vault Secrets Officer** (granted by `infra/modules/rbac.bicep`) to create/update secrets. After seeding, the API (with **Key Vault Secrets User**) reads them at runtime.

## Example (Azure CLI)

```bash
KV_NAME=<your-keyvault-name>
az keyvault secret set --vault-name $KV_NAME --name "RedisConnectionString" --value "<redis-connection-string>"
az keyvault secret set --vault-name $KV_NAME --name "ApplicationInsightsConnectionString" --value "<app-insights-connection-string>"
```

Do not commit secret values to source control.
