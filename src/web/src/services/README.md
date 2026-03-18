# Services

All HTTP communication and external integrations go through this layer. **Components should use TanStack Query hooks** (`src/web/src/hooks/`) that call these services in `queryFn` / `mutationFn`, not invoke services directly from `useEffect` for server state.

## Responsibilities

- **API client** — `apiClient.ts`: base URL, MSAL Bearer, `X-Correlation-ID`, `Idempotency-Key`, normalized `ApiError` (`apiErrors.ts`). See **docs/api-client.md**. `RestService` is optional for entity CRUD; prefer typed service objects + hooks.
- **Feature services** — Small modules (e.g. `itemsService.ts`, `sandboxService.ts`) used by query/mutation hooks.
- **External integrations** — Telemetry and other third-party calls live here or in dedicated modules.

## Usage

- New reads: add `queryKey` + `useXxxQuery` hook calling a service function.
- New writes: add service function + `useXxxMutation` hook.
- Do not import `axios` or `fetch` directly in components.

See **docs/tanstack-query.md** and the Client blueprint.
