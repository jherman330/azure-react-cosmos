# Features

This directory follows a **feature-oriented architecture**. Each feature is a vertical slice containing everything specific to that feature.

## Principles

- **Self-contained**: A feature folder holds feature-specific components, hooks, types, and service usage. It should not import from other feature folders.
- **Vertical slice**: Prefer keeping UI, logic, and API usage for a capability in one feature rather than spreading across shared folders.
- **Shared code**: UI primitives (buttons, inputs, dialogs), common utilities, and cross-cutting concerns live in `components/`, `hooks/`, `utils/`, and `services/`, not inside a single feature.

## What belongs here

- Feature pages (e.g. `HomePage`, `ProductListPage`)
- Feature-specific components used only within that feature
- Feature-specific hooks and types
- Calls to shared services (e.g. `apiClient`, feature services in `services/`)

## What does not belong here

- Reusable UI primitives → use `components/ui/`
- Cross-cutting components (telemetry, error boundaries) → use `components/shared/`
- Shared hooks or state → use `hooks/` and `state/`
- Generic utilities → use `utils/`

## Adding a new feature

1. Create a folder under `features/<feature-name>/`.
2. Add your page component, and any feature-only components/hooks.
3. Export the public surface via an `index.ts` barrel if desired.
4. Register the route in `app/routes/AppRoutes.tsx`.

See the Client blueprint for full feature-oriented architecture guidance.
