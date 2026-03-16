# Infrastructure Layer

## Purpose

The Infrastructure layer contains framework-specific implementations and external concerns: data access, external services, and configuration that supports the application.

## Responsibilities

- **Data access** — Concrete implementations of domain repository interfaces (e.g. Cosmos DB repositories).
- **External integrations** — HTTP clients, messaging, or other external service adapters.
- **Configuration** — Dependency injection registration, middleware configuration, Azure Key Vault or App Configuration usage.
- **Persistence details** — Container names, connection handling, serialization settings for Cosmos DB or other stores.

## Boundaries

**Belongs here:**

- Repository implementations
- Database and external service configuration
- Caching implementations, HTTP clients, file system access
- DI registration for infrastructure services

**Does not belong here:**

- Business logic or domain rules
- API contracts or DTOs
- Domain entities (use them; do not redefine them)

## Dependencies

This layer depends on the Domain layer (implements domain interfaces). It must not be referenced by Domain or Application; it is wired at the composition root (e.g. `Program.cs`) so that dependencies flow inward: API → Application → Domain ← Infrastructure.
