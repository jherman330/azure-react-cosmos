# TanStack Query (server state) — REQ-FOUNDATION-014

The web app uses **TanStack Query** for server state: caching, background refetch, and mutation lifecycle. Imperative `useEffect` + service calls for the same data should be avoided in favor of hooks.

## Setup

| Piece | Location |
|-------|----------|
| QueryClient defaults | `src/web/src/query/createQueryClient.ts` |
| Provider + DevTools (dev only) | `src/web/src/query/AppQueryProvider.tsx` |
| Test client (no retries) | `src/web/src/query/createTestQueryClient.ts` |

`AppQueryProvider` wraps the app inside `AppProviders`. In development, **React Query DevTools** appear (bottom-left toggle).

## Patterns

### Reads (`useQuery`)

1. Add stable **query keys** in `src/web/src/hooks/queryKeys.ts`.
2. Create a hook in `src/web/src/hooks/` that calls `useQuery({ queryKey, queryFn })`.
3. `queryFn` should call a thin function in `services/` (e.g. `itemsService.getList()`), not inline `fetch`, so tests can mock the service.

Example: `useItemsQuery` + `itemsService.getList`.

### Writes (`useMutation`)

1. Put the HTTP call in `services/` (e.g. `sandboxService.postSandboxValidate`).
2. Expose `useMutation({ mutationFn })` from `hooks/`.

Example: `useSandboxValidateMutation`.

After a successful mutation, invalidate related queries with `queryClient.invalidateQueries({ queryKey: queryKeys.items.all })` when the server data changes.

## Migration from RestService / raw effects

| Before | After |
|--------|--------|
| `useEffect` + `itemsService.getList()` + local `useState` | `useItemsQuery()` |
| Ad-hoc POST in a handler | `useMutation` + service function |
| Subclassing `RestService` in `apiClient.ts` | Optional; prefer feature services + query/mutation hooks |

The legacy `RestService` base class in `apiClient.ts` remains for entity-style CRUD if needed; new code should still surface data through TanStack Query hooks.

## Defaults (production)

- **staleTime** 60s — refetch only after data is stale.
- **gcTime** 5m — unused cache eviction.
- **retry** 2 on queries, 0 on mutations.
- **refetchOnWindowFocus** / **refetchOnReconnect** enabled.

Adjust per-query with `staleTime` / `gcTime` on individual `useQuery` calls when needed.
