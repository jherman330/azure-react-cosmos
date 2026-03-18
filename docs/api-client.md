# Typed API client (REQ-FOUNDATION-016)

All backend HTTP calls go through the shared **axios** instance in `src/web/src/services/apiClient.ts`. Components must not use raw `fetch` or create ad hoc axios clients.

## What the client does

| Concern | Behavior |
|--------|----------|
| **Base URL** | `VITE_API_BASE_URL` (see `config/index.ts`). |
| **Bearer token** | When MSAL is enabled, `AuthTokenBridge` registers a token getter; every request gets `Authorization: Bearer …`. |
| **Correlation** | Each request sends `X-Correlation-ID` (UUID unless you override — see below). Matches backend logging/traces. |
| **Errors** | Failed responses are rejected as **`ApiError`** (`apiErrors.ts`), not raw `AxiosError`. |
| **Idempotency** | For POST/PUT/PATCH/DELETE, pass `idempotencyKey` on the axios config to send `Idempotency-Key`. |

## Feature services (typed boundaries)

Define a small module per domain area with **strongly typed** request/response models:

```ts
// itemsService.ts — example pattern
export interface ItemDto {
  id: string;
}

export interface ItemsListService {
  getList(): Promise<ItemDto[]>;
}

export const itemsService: ItemsListService = {
  async getList() {
    const { data } = await apiClient.get<ItemDto[]>('/items');
    return data ?? [];
  },
};
```

Hooks call the service in `queryFn` / `mutationFn` (see `docs/tanstack-query.md`). **Do not** implement domain services inside components.

## Writes and idempotency

For mutating calls that must be safe to retry:

```ts
await apiClient.post('/api/v1/orders', body, {
  idempotencyKey: crypto.randomUUID(),
});
```

Or wrap in your service:

```ts
export async function createOrder(dto: CreateOrderDto, idempotencyKey?: string) {
  return apiClient.post<OrderDto>('/orders', dto, idempotencyKey ? { idempotencyKey } : {});
}
```

## Handling errors in the UI

Import `isApiError` and optionally branch on `kind`:

```ts
import { isApiError } from '../services/apiErrors';

if (isApiError(error)) {
  switch (error.kind) {
    case 'validation':
      // error.fieldErrors — ASP.NET-style { field: ["msg"] }
      break;
    case 'authentication':
      break;
    case 'authorization':
      break;
    case 'not_found':
      break;
    case 'conflict':
      break;
    case 'transient':
      break;
    default:
      break;
  }
}
```

`ApiError.message` is suitable for generic alerts (uses `detail` / `title` from Problem Details when present).

## Custom correlation ID

For a single logical operation spanning multiple API calls, pin one id:

```ts
import { setCorrelationIdProvider } from '../services/apiClient';

const id = crypto.randomUUID();
setCorrelationIdProvider(() => id);
try {
  await step1();
  await step2();
} finally {
  setCorrelationIdProvider(null);
}
```

Default is a **new UUID per request**.

## `RestService` base class

Optional CRUD base in `apiClient.ts` uses the same interceptors as `apiClient`. Prefer explicit service modules + TanStack Query for new features.

## Creating another client instance

If you need a different base path but the same policies:

```ts
import { createApiInstance } from './apiClient';

const legacy = createApiInstance({ baseURL: `${config.api.baseUrl}/legacy` });
```

Do not use `axios.create()` directly for app API traffic.
