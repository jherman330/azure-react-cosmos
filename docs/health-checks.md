# Health checks (WO-4 / AC-FOUNDATION-004)

Orchestrators use **liveness** and **readiness** probes to manage traffic and restarts.

## Endpoints

| Path | Purpose |
|------|---------|
| `/health` | Simple OK (legacy / minimal uptime). |
| `/health/live` | **Liveness** — process is responsive; **no** Cosmos/Redis checks. |
| `/health/ready` | **Readiness** — Cosmos DB and Redis when configured for the environment. |

All health routes are **anonymous** (no JWT).

## HTTP status

- **200** — aggregate status is `Healthy`.
- **503** — any included check is `Degraded` or `Unhealthy` (readiness fails closed).

## JSON shape

**Liveness** (`GET /health/live`):

- `status` — e.g. `Healthy`
- `duration` — total probe duration

**Readiness** (`GET /health/ready`):

- `status` — aggregate status
- `duration` — total probe duration
- `checks` — map of check name → `{ status, description?, duration }`

## When checks run

- **Cosmos** — registered when `AZURE_COSMOS_ENDPOINT` is set. Uses database id from `AZURE_COSMOS_DATABASE_NAME` (default `App`). Timeout **2s**.
- **Redis** — registered in non-Development when `ConnectionStrings:Redis` is set (same rule as distributed cache). Uses shared `IConnectionMultiplexer` with the cache. PING timeout **1s**.
- **Neither** (e.g. local / `Testing`) — a single `dependencies` check reports healthy with a note that external deps are not configured.

## Configuration

Align with Cosmos and cache settings in `appsettings` / Key Vault / environment variables (`AZURE_COSMOS_*`, `ConnectionStrings:Redis`).
