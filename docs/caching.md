# Distributed Caching

This document describes the caching strategy and patterns used by the API (REQ-FOUNDATION-010, Infrastructure blueprint).

## Overview

The API uses the **IDistributedCache** abstraction so that the underlying store can be switched via configuration. Behavior is explicit:

| Environment | ConnectionStrings:Redis | Behavior |
|-------------|-------------------------|----------|
| **Development** | any | In-memory cache (no Redis required). |
| **Non-Development** | set (e.g. from Key Vault) | Azure Cache for Redis. |
| **Non-Development** | not set | In-memory fallback; a **startup warning** is logged. Cache is not shared across instances. Startup does not fail. |

Application-level cache operations are **bounded to 2 seconds** by wrapper policy (CancellationToken). If the underlying store does not respond in time or fails, the code logs a warning and degrades (returns null / no-op) so callers can fall back to the database. The application never throws for cache failures.

## Configuration

| Setting | Description |
|--------|-------------|
| `ConnectionStrings:Redis` | Redis connection string. In Azure, store in Key Vault as secret `ConnectionStrings--Redis`. Locally, use `dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"` if you want to test Redis. |
| `Cache:OperationTimeoutSeconds` | Timeout in seconds for each cache Get/Set/Remove (default: 2). |

When `ConnectionStrings:Redis` is not set in Development, in-memory is used. When it is not set in non-Development, in-memory is also used and a startup warning is logged (see Overview).

## Cache key namespaces

Keys are separated by namespace to avoid collisions between application cache and rate limiting (same Redis instance):

| Prefix | Use |
|--------|-----|
| `cache:` | Application data (e.g. `cache:product:123`, `cache:search:abc`) |
| `rl:sw:` (and related) | Rate limiting — enforced only by <code>DistributedRateLimitingMiddleware</code> (REQ-FOUNDATION-005); not via ASP.NET Core <code>UseRateLimiter</code>. |

Use the constants in `Todo.Api.Infrastructure.Caching.CacheKeyNamespaces` when building keys. Keys are case-sensitive; use lowercase with colon separators.

## Cache-aside pattern

1. **Read:** Try cache first. On hit, return. On miss (or timeout/failure), load from the database, then optionally set the cache and return.
2. **Write:** Update the database, then remove or update the cached entry so subsequent reads see fresh data.
3. **Failure handling:** Never throw on cache errors. Log a warning and proceed without cache (degrade to database).

Example (conceptual):

```csharp
// Get
var cached = await cache.GetJsonAsync<MyDto>(key, token);
if (cached != null) return cached;
var fromDb = await repository.GetAsync(id, token);
if (fromDb != null)
    await cache.SetJsonAsync(key, fromDb, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }, token);
return fromDb;

// Invalidate on update
await repository.UpdateAsync(entity, token);
await cache.RemoveAsync(key, token);
```

## Serialization

Cached values are serialized with **System.Text.Json** (camelCase, same style as the API). Use the extension methods in `DistributedCacheJsonExtensions` (`GetJsonAsync<T>`, `SetJsonAsync<T>`) for typed get/set.

## Timeout and resilience

- **Application-level bound:** Get, Set, Refresh, and Remove are bounded to **2 seconds** (configurable via `Cache:OperationTimeoutSeconds`) by wrapper policy using a cancellation token. Whether the underlying provider (e.g. Redis) actually honors cancellation is implementation-dependent; we do not claim a guaranteed hard cutoff.
- **Redis client:** Where Redis is used, the StackExchange.Redis client is configured with connection and sync timeouts of ~2 seconds so that timeouts are enforced at the client level as well, where supported.
- Redis connection or serialization errors are caught, logged as warnings, and do not propagate. Callers should implement cache-aside so that a cache miss or failure simply results in a database read.

## References

- Infrastructure blueprint: Distributed Caching and Caching Strategy
- REQ-FOUNDATION-010 acceptance criteria (AC-FOUNDATION-010.1–010.8)
- Secrets: `docs/secrets-management.md` (Key Vault and user-secrets for `ConnectionStrings:Redis`)
