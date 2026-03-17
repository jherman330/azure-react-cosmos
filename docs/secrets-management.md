# Secrets Management

This document is the single reference for how secrets and sensitive configuration work in this application (REQ-FOUNDATION-009). **Secrets are never committed to source control.** Use the patterns below so your team can run the app locally and in Azure safely.

---

## Rule: Secrets never go in source control

- Do **not** put connection strings, API keys, passwords, or signing keys in:
  - `appsettings.json`, `appsettings.Development.json`, or any `appsettings.*.json`
  - Environment files committed to the repo (e.g. `.env` in git)
  - Code or comments
- **Local development:** use **dotnet user-secrets** (stored outside the repo on your machine).
- **Deployed environments (Azure):** use **Azure Key Vault**; the API reads secrets via managed identity.

---

## How the app chooses the secrets source

| Environment        | Secrets source     | When it's used |
|--------------------|--------------------|----------------|
| **Development**    | dotnet user-secrets only | Whenever `ASPNETCORE_ENVIRONMENT=Development`. Key Vault is **not** used in Development, even if `AZURE_KEY_VAULT_ENDPOINT` is set. |
| **Non-Development** (e.g. Staging, Production) | Azure Key Vault | When `AZURE_KEY_VAULT_ENDPOINT` is set (e.g. on App Service). App uses managed identity to read secrets. |

So: **Development ⇒ user secrets.** **Non-Development / Azure ⇒ Key Vault.** This keeps local dev from accidentally using Key Vault just because an env var exists.

---

## Local development: dotnet user-secrets

### First-time setup (init)

The API project already has a `UserSecretsId` in `src/api/Todo.Api.csproj`. You do **not** run a separate “init” command; the first time you run `dotnet user-secrets set`, the secrets store is created for that project.

From the **repository root** (or from `src/api` if you prefer):

```bash
# From repo root
cd src/api
dotnet user-secrets set "MySecretName" "my-secret-value"
```

Or from repo root in one shot:

```bash
dotnet user-secrets set "MySecretName" "my-secret-value" --project src/api/Todo.Api.csproj
```

That’s it — the project is now “initialized” for user-secrets for that machine.

### Set, list, remove

**Set a secret (add or overwrite):**

```bash
cd src/api
dotnet user-secrets set "ApiKey" "sk-12345"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
```

**List all secrets (names only; values are not shown):**

```bash
cd src/api
dotnet user-secrets list
```

**Remove one secret:**

```bash
cd src/api
dotnet user-secrets remove "ApiKey"
```

**Remove all secrets for this project:**

```bash
cd src/api
dotnet user-secrets clear
```

Secrets are stored under your user profile (e.g. `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\` on Windows) and are **not** in the repo or in source control.

### Secret naming conventions (user-secrets)

- Use **flat keys** for top-level config: `ApiKey`, `MySecretName`.
- For **hierarchical config** (sections), use a **colon** in user-secrets:
  - `Section:SubSection:Key` → becomes `configuration["Section:SubSection:Key"]` or `configuration.GetSection("Section")["SubSection:Key"]`.
- Example: `ConnectionStrings--Redis` in appsettings would be set as:
  - `dotnet user-secrets set "ConnectionStrings:Redis" "your-connection-string"`
- Keep names consistent with what you’ll use in Key Vault (see below) so the same config keys work in both places.

---

## Deployed environments: Azure Key Vault

- Key Vault is provisioned by Bicep (`infra/modules/key-vault.bicep`). App Service gets `AZURE_KEY_VAULT_ENDPOINT` and has **Key Vault Secrets User** via `infra/modules/rbac.bicep`.
- In **non-Development**, when `AZURE_KEY_VAULT_ENDPOINT` is set, `Program.cs` adds the Key Vault configuration provider with `DefaultAzureCredential` (managed identity in Azure).

### How config keys map to Key Vault secret names

- **Key Vault secret names** may only contain alphanumerics and dashes (no colons).
- The ASP.NET Core Key Vault provider maps **double-dash `--`** in a secret name to a **colon `:`** in configuration (section separator).
- So:
  - Key Vault secret name `MySecretName` → config key `MySecretName` → `configuration["MySecretName"]`.
  - Key Vault secret name `ConnectionStrings--Redis` → config key `ConnectionStrings:Redis` → `configuration["ConnectionStrings:Redis"]` or `configuration.GetSection("ConnectionStrings")["Redis"]`.

**Convention:** Use the same logical names in user-secrets and Key Vault; only the separator differs:

| Config key (in code)     | User-secrets (local)           | Key Vault secret name (Azure)   |
|--------------------------|--------------------------------|----------------------------------|
| `ApiKey`                 | `ApiKey`                       | `ApiKey`                         |
| `ConnectionStrings:Redis`| `ConnectionStrings:Redis`      | `ConnectionStrings--Redis`       |
| `Auth:SigningKey`        | `Auth:SigningKey`              | `Auth--SigningKey`               |

**Add a secret in Azure (after provisioning):**

```bash
az keyvault secret set --vault-name <YourKeyVaultName> --name "ConnectionStrings--Redis" --value "rediss://..."
```

The API then reads it as `configuration["ConnectionStrings:Redis"]` (or via `GetSection("ConnectionStrings")["Redis"]`).

---

## Configuration precedence

Order (later overrides earlier):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. **Development only:** user-secrets
4. **Non-Development only:** Azure Key Vault (when `AZURE_KEY_VAULT_ENDPOINT` is set)

Non-secret settings (URLs, feature flags, log levels) belong in appsettings and are safe to commit. Secrets stay in user-secrets (local) or Key Vault (Azure) and **never in source control**.

---

## Key Vault: soft delete and purge protection (AC-FOUNDATION-009.7)

- **Soft delete** is enabled in our Key Vault Bicep: deleted secrets are retained for 90 days and can be recovered.
- **Purge protection** is optional. For **production**, we recommend setting `enablePurgeProtection: true` in your Bicep parameters so the vault cannot be purged during the soft-delete period.

Example (e.g. in a production parameters file or pipeline):

```json
{
  "enablePurgeProtection": true
}
```

---

## Summary

| Location              | Secrets store        | Gating |
|-----------------------|----------------------|--------|
| Local (Development)   | dotnet user-secrets  | Always in Development; Key Vault is **not** used. |
| Azure (App Service)   | Azure Key Vault      | Only when **not** Development and `AZURE_KEY_VAULT_ENDPOINT` is set. |

**Secrets are never committed to source control.** Use user-secrets locally and Key Vault in Azure, with the naming conventions above so the same config keys work in both.
