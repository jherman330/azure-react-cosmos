# Work Order 2 — Implementation Plan

**Title:** Implement Repository Pattern and Data Access Layer  
**Status:** ready → implemented  
**AC:** REQ-FOUNDATION-002 (AC-FOUNDATION-002.1–002.7)

---

## Summary

Repository pattern for Cosmos DB: domain interfaces, base Cosmos implementation, async-only API, ETag for optimistic concurrency, audit fields, Cosmos client configuration (session consistency, RU monitoring), and DI registration.

---

## Implementation Plan (files created/updated)

| Action | Path | Description |
|--------|------|-------------|
| **create** | `src/api/Domain/Entities/IDomainEntity.cs` | Interface: `Id`, `PartitionKeyValue` for repository operations (AC-FOUNDATION-002.1). |
| **create** | `src/api/Domain/Entities/IAuditableEntity.cs` | Interface: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` for audit (AC-FOUNDATION-002.5). |
| **create** | `src/api/Domain/Entities/IConcurrencyEntity.cs` | Interface: `Etag` for optimistic concurrency (AC-FOUNDATION-002.4). |
| **create** | `src/api/Domain/Repositories/QuerySpec.cs` | Record: `QueryText`, `Parameters` for query operations. |
| **create** | `src/api/Domain/Repositories/IRepository.cs` | Generic `IRepository<T>`: `CreateAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`, `QueryAsync` — async-only (AC-FOUNDATION-002.2, 002.3). |
| **create** | `src/api/Infrastructure/Data/CosmosDbRepositoryBase.cs` | Base Cosmos implementation: CRUD, query, ETag via `IfMatchEtag`, audit population, request charge logging (AC-FOUNDATION-002.2–002.6). |
| **create** | `src/api/Infrastructure/Configuration/CosmosServiceCollectionExtensions.cs` | `AddCosmosDbClient(config)` — session consistency, optional region; `AddCosmosDbRepository<T>(db, container, partitionKeyPath)` — register `IRepository<T>` (AC-FOUNDATION-002.6, 002.7). |
| **modify** | `src/api/Program.cs` | Use `AddCosmosDbClient(builder.Configuration)`; remove raw `CosmosClient` registration; add comment for `AddCosmosDbRepository<T>` when adding entities. |

---

## Out of scope (per WO-2)

- Specific domain entity repositories (Product, Order, etc.)
- Change feed, bulk operations, advanced indexing, schema versioning, TTL

---

## Usage

1. **Cosmos client:** `AddCosmosDbClient(config)` registers `CosmosClient` only when `AZURE_COSMOS_ENDPOINT` is set; uses session consistency and optional `AZURE_LOCATION`.
2. **Repository:** For each entity implementing `IDomainEntity`, call  
   `services.AddCosmosDbRepository<MyEntity>(databaseId, containerId, partitionKeyPath)`.
3. **Audit / ETag:** Implement `IAuditableEntity` and/or `IConcurrencyEntity` on the entity; the base repository fills audit and uses ETag on update/delete.
4. **Query:** Use `QuerySpec("SELECT * FROM c WHERE c.type = @type", new Dictionary<string, object> { ["@type"] = "MyType" })` and `IRepository<T>.QueryAsync(spec, ct)`.

---

## Verification

- `dotnet build` in `src/api` succeeds.
- Domain has no references to Cosmos or Infrastructure.
- All repository methods are async; ETag and audit behavior as above; RU logged in repository.
