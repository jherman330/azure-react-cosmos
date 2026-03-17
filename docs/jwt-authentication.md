# JWT Authentication with Microsoft Entra ID (WO-3 / AC-FOUNDATION-003)

This document describes the API’s **JWT bearer authentication** and **role-based authorization** using Microsoft Entra ID (formerly Azure AD). The system is **single-tenant**: only tokens from one Entra tenant are accepted.

## Scope

- **Inbound**: Validates JWT bearer tokens on protected endpoints; returns standardized 401/403 when validation or authorization fails.
- **Out of scope**: Frontend MSAL, token refresh, managed identity for backend-to-backend (covered elsewhere).

## Single-tenant assumption

The API is explicitly **single-tenant**:

- **Authority** must be a **tenant-specific** URL (e.g. `https://login.microsoftonline.com/{tenant-id}/v2.0`).
- Do **not** use `common` or `organizations` as the tenant segment. Issuer validation is configured so that only tokens issued by the configured authority (and thus this tenant) are accepted.
- This constraint is enforced in code via `ValidIssuer` set from the configured Authority.

## Stack and Registration

- **Package**: `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Registration**: `AddJwtAuthentication(configuration)` in the composition root configures:
  - Authority (Entra ID tenant-specific issuer)
  - Audience (API application ID)
  - Token validation (signature, expiration, issuer, audience); issuer validation enforces single-tenant
  - Standardized JSON error body for 401 (UNAUTHORIZED) and 403 (FORBIDDEN)

Middleware order: `UseAuthentication()` then `UseAuthorization()`.

## Configuration

Configuration lives under **`Authentication:JwtBearer`** in appsettings (or user secrets / Key Vault).

| Key                   | Description                                                                 | Example |
|-----------------------|-----------------------------------------------------------------------------|---------|
| `Authority`            | Entra ID issuer URL. **Must be tenant-specific** (see Single-tenant assumption). | `https://login.microsoftonline.com/{tenant-id}/v2.0` |
| `Audience`             | API’s client/application ID in Entra                                        | App (client) ID of the API app registration |
| `RequireHttpsMetadata` | Require HTTPS for metadata (JWKS) discovery. Set `false` for local mock ID. | `true` (prod), `false` (dev) |

Example (tenant-specific authority; do not use `common` or `organizations`):

```json
{
  "Authentication": {
    "JwtBearer": {
      "Authority": "https://login.microsoftonline.com/{tenant-id}/v2.0",
      "Audience": "{api-client-id}",
      "RequireHttpsMetadata": true
    }
  }
}
```

In Development, override only what you need (e.g. `RequireHttpsMetadata: false` in `appsettings.Development.json`). Use user secrets or Key Vault for real `Authority`/`Audience` in non-Dev.

## Unauthenticated Endpoints

The following endpoints **do not** require authentication (AC-FOUNDATION-003.8):

- `GET /health`
- `GET /health/live`
- `GET /health/ready`
- `GET /` (root ping)

All other endpoints should require a valid JWT.

## Admin authorization policy

The **"Admin"** policy uses Entra ID **app roles**:

- The policy explicitly requires the **roles** claim to contain **"Admin"**.
- In Entra ID, define an app role named **"Admin"** on the API app registration and assign users or groups to that role. Tokens issued to those principals will include `roles: ["Admin"]`.
- Endpoints that require admin access use `.RequireAuthorization("Admin")`.

## Protecting Endpoints

For **minimal APIs**, require authentication or the Admin role when mapping routes:

- **Any authenticated user**: `.RequireAuthorization()`
- **Admin only** (roles claim must contain "Admin"): `.RequireAuthorization("Admin")`

Example (when adding API routes):

```csharp
app.MapGet("/api/v1/items", () => ...).RequireAuthorization();
app.MapPost("/api/v1/items", () => ...).RequireAuthorization("Admin");
```

## Audit user claim standard

Audit identity (for **CreatedBy**, **UpdatedBy**) is standardized so all audit fields use the same value:

- **Primary**: **oid** (Object ID) — stable, tenant-scoped identifier in Entra ID.
- **Fallback**: **sub** (subject) — used only if `oid` is not present.

`CurrentUserService` and `CosmosDbRepositoryBase` both implement this logic (oid → sub). Use `ICurrentUserService.UserId` in application code when you need the same audit identifier.

- **Repository**: `CosmosDbRepositoryBase` sets `CreatedBy` / `UpdatedBy` on `IAuditableEntity` using this standardized value (oid first, then sub).
- **Application**: Inject `ICurrentUserService` to get `UserId` (same standard) in services when needed.

## Error Response Format

All auth-related errors use the same envelope as the global error handler:

- **401 Unauthorized**: Missing or invalid token  
  `{ "traceId": "...", "errorCode": "UNAUTHORIZED", "message": "Authentication required. Provide a valid JWT bearer token." }`
- **403 Forbidden**: Valid token but insufficient role  
  `{ "traceId": "...", "errorCode": "FORBIDDEN", "message": "Insufficient permissions to access this resource." }`

## Acceptance Criteria Mapping (AC-FOUNDATION-003)

| Criterion | Implementation |
|-----------|----------------|
| AC-FOUNDATION-003.1 | JWT middleware validates tokens from Entra ID (Authority + Audience). |
| AC-FOUNDATION-003.2 | Token validation checks signature, expiration, issuer, audience (TokenValidationParameters). |
| AC-FOUNDATION-003.3 | Authentication failures return HTTP 401 with standardized JSON (OnChallenge). |
| AC-FOUNDATION-003.4 | Protected endpoints use `.RequireAuthorization()` (or Admin policy). |
| AC-FOUNDATION-003.5 | Admin-only endpoints use `.RequireAuthorization("Admin")`. |
| AC-FOUNDATION-003.6 | Authorization failures return HTTP 403 with standardized JSON (OnForbidden). |
| AC-FOUNDATION-003.7 | Audit identity (oid → sub) available via `HttpContext.User` and `ICurrentUserService`; repository sets audit fields. |
| AC-FOUNDATION-003.8 | Health and root endpoints are `AllowAnonymous()`. |
