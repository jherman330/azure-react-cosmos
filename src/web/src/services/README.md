# Services

All HTTP communication and external integrations go through this layer. Components should not perform raw API calls; they use these services instead.

## Responsibilities

- **API client** — `apiClient.ts` provides the centralized HTTP client (base URL, headers, error handling). Use it or extend `RestService` for entity-based resources.
- **Feature services** — Add feature-specific service modules here (e.g. `productService.ts`) that use the API client to talk to backend endpoints.
- **External integrations** — Telemetry, analytics, and other third-party calls are also encapsulated here or in dedicated modules.

## Usage

- Import `apiClient` or a concrete service in your feature or component.
- Do not import `axios` or `fetch` directly in components; go through a service.
- Token attachment (e.g. MSAL) will be wired in the API client when authentication is added.

See the Client blueprint for API integration standards.
