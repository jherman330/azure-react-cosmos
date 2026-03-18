# CORS Configuration (WO-6 / AC-FOUNDATION-006)

The API calls `AddConfiguredCors()` (custom `ICorsPolicyProvider` that builds one policy from config on **first use**, so test and host overrides are visible) and `app.UseCors()` (no policy name), **before** authentication.

## Development

- **Any origin, any method, any header**; preflight cached **1 hour**.
- **`Cors:AllowedOrigins` is not read** for CORS behavior (values in `appsettings.Development.json` are documentation only).

## Staging / Production / other

- **Allowed origins**: comma-separated absolute `http`/`https` URLs in **`Cors:AllowedOrigins`** (trailing slashes normalized on entries).
- **`AllowedOrigins` = `*`** outside Development is **invalid**: startup logs **critical**; the app **does not** emit `Access-Control-Allow-Origin` for browser cross-origin calls (fail closed).
- **Methods**: GET, POST, PUT, PATCH, DELETE.
- **Headers**: Authorization, Content-Type, Accept, X-Correlation-Id, X-Request-Id.
- Invalid list entries log at startup; empty allowlist (after parsing) logs **warning**.

## Example

```json
"Cors": {
  "AllowedOrigins": "https://app.example.com,https://staging.example.com"
}
```

Integration tests should set **environment and `Cors:AllowedOrigins`** explicitly (e.g. in-memory configuration) rather than relying on Development-only behavior.
