# Components

Shared components used across the application. Feature-specific components live in their feature folders under `features/`.

## Structure

- **`ui/`** — Domain-agnostic reusable UI primitives: buttons, inputs, dialogs, tables, layout elements. Use these to build feature UIs without duplicating low-level styling or behavior.
- **`shared/`** — Cross-cutting infrastructure components: telemetry providers, error boundaries, and other shared wrappers that are not tied to a single feature.

## Guidelines

- Shared components should stay reusable and avoid depending on specific feature domains.
- If a component is only used by one feature, keep it inside that feature folder.
- Prefer composing from `ui/` and `shared/` in features rather than adding one-off components here.
