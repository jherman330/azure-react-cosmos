# Domain Layer

## Purpose

The Domain layer is the core of the application. It contains business entities, business rules, and domain logic. It has no outward dependencies.

## Responsibilities

- **Entities** — Rich domain entities with behavior (e.g. `UpdatePrice()`, `Publish()`), not anemic DTOs.
- **Business rules** — Invariants, validation rules, and domain logic that define what is valid in the domain.
- **Domain interfaces** — Abstractions (e.g. repository interfaces) that outer layers implement; the domain defines the contract.

## Boundaries

**Belongs here:**

- Domain entities and value objects
- Domain exceptions
- Repository and other domain-facing interfaces

**Does not belong here:**

- Validation frameworks or application-level validation
- HTTP, serialization, or API concerns
- Data access implementations or external service calls
- References to ASP.NET Core, Cosmos DB, or other infrastructure

## Dependencies

This layer has **zero** outward dependencies. No project references to Application, Infrastructure, or API. Other layers depend on Domain; Domain does not depend on them.
