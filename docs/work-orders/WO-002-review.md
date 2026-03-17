# Work Order 2 — Acceptance Criteria Review

**Work Order:** Implement Repository Pattern and Data Access Layer  
**Review date:** 2025-03  
**Criteria source:** REQ-FOUNDATION-002 (AC-FOUNDATION-002.1–002.7)

---

## 1. AC-FOUNDATION-002.7 — Repositories registered in DI and injectable via interfaces

**Question:** Is repository registration actually implemented and injectable end-to-end?

**Finding:** The *mechanism* is implemented: `AddCosmosDbRepository<T>(...)` correctly registers `IRepository<T>` → `CosmosDbRepositoryBase<T>` in DI. However, **Program.cs only calls `AddCosmosDbClient(...)` and shows `AddCosmosDbRepository<T>(...)` as a comment.** No concrete `IRepository<T>` is registered for any entity type, so no repository is injectable in the current app. End-to-end injection is therefore not demonstrated.

**Assessment:** **Partial**

**Recommendation:** Register at least one concrete repository (e.g. a minimal placeholder entity) so that 002.7 is satisfied end-to-end, or clearly document that WO-2 scope is “registration mechanism only” and that first concrete repository is added in a later WO.

---

## 2. Audit field handling — CreatedAt/CreatedBy preserved on update

**Requirement:** On update, preserve CreatedAt/CreatedBy; only update UpdatedAt/UpdatedBy.

**Finding:**  
- `PopulateAuditOnCreate` sets only `CreatedAt`, `UpdatedAt` (and leaves CreatedBy/UpdatedBy to caller).  
- `PopulateAuditOnUpdate` sets only `UpdatedAt`; it does **not** modify `CreatedAt` or `CreatedBy`.  
- `ReplaceItemAsync` sends the in-memory entity as the full document; that entity already has CreatedAt/CreatedBy from create or read, so they are preserved.

**Assessment:** **Pass** — CreatedAt/CreatedBy are preserved on update; only UpdatedAt (and optionally UpdatedBy by caller) change.

**Optional improvement:** Document in XML/comments that the repository never overwrites CreatedAt/CreatedBy on update.

---

## 3. ETag optimistic concurrency — update and delete; conflict behavior

**Requirement:** ETag enforced on both update and delete; clarify exception/result on conflict.

**Finding:**  
- **Update:** `BuildRequestOptionsForUpdate` sets `IfMatchEtag` when the entity implements `IConcurrencyEntity` and `Etag` is non-empty. So optimistic concurrency is enforced on update when ETag is present.  
- **Delete:** `IfMatchEtag` is set only when the `etag` parameter is non-empty. If the caller omits `etag`, delete is unconditional. So ETag is *optional* on delete; when provided, it is enforced.  
- **On conflict:** Cosmos DB throws `CosmosException` with `StatusCode == HttpStatusCode.PreconditionFailed` (412). The current code does **not** catch or translate this; it bubbles up as a raw Cosmos exception. There is no domain-level result type or documented contract for “concurrency conflict.”

**Assessment:** **Partial** — ETag is enforced on update (when entity has Etag) and on delete (when etag parameter is supplied). Concurrency conflict behavior is not documented and is not translated to a domain-friendly exception or result.

**Recommendation:**  
- Document that 412 from update/delete indicates an ETag conflict.  
- Optionally introduce a small domain exception (e.g. `ConcurrencyConflictException`) that wraps `CosmosException` when StatusCode is 412, and throw it from the repository so callers can handle “conflict” without depending on Cosmos types.

---

## 4. RU monitoring — logging vs structured telemetry

**Requirement:** Clarify whether RU monitoring is only request-charge logging or integrated into structured telemetry.

**Finding:** Request charge is only written via `_logger.LogDebug("Cosmos DB {Operation} request charge: {RequestCharge} RUs", ...)`. So it is **logging only** (and at Debug level). It is **not** integrated with Application Insights or other structured telemetry (e.g. custom metrics or dependency telemetry with RU as a property).

**Assessment:** **Partial** — RU monitoring is implemented as request-charge logging only; not integrated into structured telemetry.

**Recommendation:** Either (a) document that “RU monitoring” in 002.6 means “logged request charge” and that structured telemetry is out of scope for WO-2, or (b) add optional integration (e.g. record request charge on Application Insights as a metric or property on dependency telemetry) so that RU is visible in structured telemetry.

---

## 5. QuerySpec and Cosmos DB coupling in Domain

**Requirement:** Assess whether QuerySpec introduces undesirable Cosmos DB coupling in the Domain layer.

**Finding:** `QuerySpec` is a record with `QueryText` and `Parameters` (keyed dictionary). The XML already says “Infrastructure translates this to the underlying store (e.g. Cosmos SQL).” So:  
- **Coupling:** The *shape* (text + named parameters) is generic; the *content* (e.g. `SELECT * FROM c WHERE c.partitionKey = @pk`) is written by the application layer and is Cosmos SQL. So Domain does not reference Cosmos types, but **callers** (Application/API) must know Cosmos SQL and parameter conventions (e.g. `@param`) to use `QueryAsync`. That is application-level coupling to Cosmos, not Domain-type coupling.  
- Domain itself stays free of Cosmos references; the trade-off is that the abstraction is “thin” and query authoring is store-specific.

**Assessment:** **Pass with caveat** — No Cosmos coupling *in Domain types*. QuerySpec is store-agnostic in form; in practice, query text will be Cosmos SQL and parameters Cosmos-style. Acceptable for foundation; a future “query abstraction” (e.g. specification object or expression-based) could reduce application-level coupling if needed.

**Recommendation:** Add a short note in Domain/Repositories (e.g. README or QuerySpec XML) that query text and parameter names are defined by the concrete repository (e.g. Cosmos SQL with @params) and that Domain remains free of store references.

---

## Summary: Pass / Partial / Fail by AC

| AC | Criterion | Assessment | Notes |
|----|-----------|------------|--------|
| 002.1 | Repository interfaces in Domain | **Pass** | IRepository&lt;T&gt;, entity interfaces in Domain. |
| 002.2 | Base repository CRUD + Query | **Pass** | Create, Read, Update, Delete, Query implemented. |
| 002.3 | Async-only repository methods | **Pass** | All methods are async / IAsyncEnumerable. |
| 002.4 | ETag for optimistic concurrency | **Partial** | Enforced on update (when Etag set) and delete (when etag param set). Conflict (412) not documented or translated. |
| 002.5 | Repositories populate audit fields | **Pass** | Create sets CreatedAt/UpdatedAt; update only UpdatedAt; CreatedAt/CreatedBy preserved. |
| 002.6 | Cosmos client: session consistency + RU monitoring | **Partial** | Session consistency and client options in place. RU = logging only, not structured telemetry. |
| 002.7 | Repositories registered in DI, injectable | **Partial** | Registration API implemented; no concrete IRepository&lt;T&gt; registered in Program.cs → not injectable end-to-end. |

---

## Code changes recommended to fully satisfy WO-2

1. **002.7 — End-to-end injectable repository**  
   - Add a minimal placeholder entity (e.g. `Domain/Entities/PlaceholderEntity.cs`) and register `AddCosmosDbRepository<PlaceholderEntity>(...)` in Program.cs using config (e.g. database/container from configuration), **or**  
   - Add a one-line call in Program.cs that registers a single repository when a “default” database/container is configured, so that at least one `IRepository<T>` is injectable.  
   - Ensure the app builds and, if possible, add a minimal test or endpoint that resolves `IRepository<PlaceholderEntity>` to prove injection.

2. **002.4 — ETag conflict behavior**  
   - Document in `IRepository` (or repository README) that update/delete may throw when ETag does not match (e.g. “Concurrency conflict: CosmosException with StatusCode 412”).  
   - Optionally: add a domain exception (e.g. `ConcurrencyConflictException`) in Domain and throw it from the repository when Cosmos returns 412; catch `CosmosException` in the repository and wrap before rethrowing.

3. **002.6 — RU and telemetry**  
   - Either document that RU monitoring is “request charge logged at Debug” and structured telemetry is out of scope, **or**  
   - Add optional integration (e.g. use `TelemetryClient.TrackMetric("CosmosRequestCharge", requestCharge)` or attach to dependency telemetry) when Application Insights is available.

4. **Audit and QuerySpec (no change required)**  
   - Add one sentence in repository/base or Domain README: audit fields CreatedAt/CreatedBy are never overwritten on update.  
   - Add one sentence that QuerySpec query text and parameter names are defined by the implementation (e.g. Cosmos SQL).

---

## Tests to add before marking WO-2 complete

1. **Unit tests for CosmosDbRepositoryBase (with container mock or in-memory)**  
   - Create: audit fields (CreatedAt/UpdatedAt) set when entity is IAuditableEntity.  
   - Update: only UpdatedAt changed; CreatedAt/CreatedBy unchanged (and, when IConcurrencyEntity.Etag set, that IfMatchEtag is used).  
   - Delete: when etag supplied, request uses IfMatchEtag.  
   - GetByIdAsync: returns null when item not found (NotFound); sets Etag on resource when IConcurrencyEntity.  
   - QueryAsync: builds QueryDefinition with text and parameters.

2. **Integration test (optional, if test Cosmos or emulator available)**  
   - Resolve `IRepository<T>` from DI (T = a registered entity).  
   - CreateAsync → GetByIdAsync → UpdateAsync (with same Etag) → GetByIdAsync; verify UpdatedAt changed, CreatedAt unchanged.  
   - UpdateAsync with stale Etag → expect ConcurrencyConflictException.

3. **DI / registration test**  
   - When AZURE_COSMOS_ENDPOINT is set, build ServiceProvider and resolve `IRepository<Item>`; assert not null.  
   - When AZURE_COSMOS_ENDPOINT is unset, do not register repository; optional: resolve IRepository<Item> should not be attempted (or document that it is only registered when Cosmos is configured).

---

## Post-review code changes (implemented)

| Change | File(s) | Purpose |
|--------|---------|---------|
| **002.7** | `Domain/Entities/Item.cs`, `Program.cs` | Minimal entity and conditional registration of `IRepository<Item>` when Cosmos endpoint is set so a repository is injectable end-to-end. |
| **002.4** | `Domain/Exceptions/ConcurrencyConflictException.cs`, `CosmosDbRepositoryBase.cs`, `IRepository.cs` | On Cosmos 412, throw `ConcurrencyConflictException`; document in interface. |
| **002.5 / 002.6 / QuerySpec** | `CosmosDbRepositoryBase.cs`, `QuerySpec.cs`, `IRepository.cs` | XML: audit never overwrites CreatedAt/CreatedBy; RU is logging-only, telemetry optional; QuerySpec text/params implementation-defined. |

Implementing the recommended code changes next (002.7 demo registration, ETag conflict documentation/wrapper, RU documentation or telemetry, and short doc comments).