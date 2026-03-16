# Application Layer

## Purpose

The Application layer orchestrates use cases. It coordinates between the API and Domain layers and contains application-level workflows and validation.

## Responsibilities

- **Use case orchestration** — Coordinate domain logic and infrastructure to fulfill a request.
- **Validation** — Validate input and enforce application-level rules (e.g. with FluentValidation when used).
- **Application interfaces** — Define service contracts and abstractions consumed by the API layer.
- **Transaction boundaries** — Define where units of work begin and end, when applicable.

## Boundaries

**Belongs here:**

- Application services that implement use cases
- Validators and application-level interfaces
- Mapping between API DTOs and domain models, when needed

**Does not belong here:**

- Domain logic or business rules (those belong in Domain)
- Data access implementations (those belong in Infrastructure)
- HTTP concerns such as status codes or headers (those belong in API)

## Dependencies

This layer depends on the Domain layer (entities and domain interfaces). Infrastructure implements domain interfaces and is registered at the composition root; Application code depends on abstractions, not concrete implementations.
