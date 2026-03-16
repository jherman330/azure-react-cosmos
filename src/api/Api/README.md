# API Layer

## Purpose

The API layer is the entry point for HTTP traffic. It handles HTTP concerns only and delegates business logic to the Application layer.

## Responsibilities

- **Routing** — Map HTTP verbs and URLs to handlers.
- **Request/response binding** — Deserialize request bodies, bind query/route parameters, format responses.
- **Status codes** — Return appropriate HTTP status codes and headers.
- **Delegation** — Call Application layer services; do not implement business rules here.

## Boundaries

**Belongs here:**

- Controllers or minimal API endpoints
- DTOs (Data Transfer Objects) for request/response contracts
- Filters, model binding, and HTTP-specific middleware concerns

**Does not belong here:**

- Business logic or domain rules
- Data access or repository usage
- Validation logic (beyond basic request shape); use Application layer validators

## Dependencies

This layer depends on the Application layer (service interfaces). It must not reference Domain entities directly in responses unless they are explicitly exposed as DTOs, or reference Infrastructure.
