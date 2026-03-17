# Frontend — Feature-Oriented Architecture

This app uses a **feature-oriented** structure so that features are self-contained and the codebase stays maintainable as it grows.

## Directory structure

| Directory | Purpose |
|-----------|--------|
| **`src/app/`** | Application shell: routing, providers, layout. Entry point is `App.tsx`; `providers/`, `routes/`, and `layout/` live here. |
| **`src/features/`** | Feature modules (vertical slices). Each feature has its own folder with pages, feature-specific components, and hooks. |
| **`src/components/`** | Shared components: `ui/` for primitives (buttons, inputs, dialogs), `shared/` for cross-cutting pieces (e.g. telemetry). |
| **`src/services/`** | HTTP client and external integrations. All API calls go through services; no raw fetch/axios in components. |
| **`src/hooks/`** | Reusable React hooks used across multiple features. |
| **`src/state/`** | Global application state (e.g. React Context for user, theme, feature flags). |
| **`src/types/`** | Shared TypeScript interfaces and types. |
| **`src/utils/`** | Domain-agnostic helper functions. |
| **`src/styles/`** | Theme and shared style tokens (Fluent UI theme, stack styles, etc.). |
| **`src/config/`** | App configuration (API base URL, feature flags, env-based settings). |

## Where to put new code

- **New feature** → Add a folder under `src/features/<name>/`, register routes in `app/routes/AppRoutes.tsx`.
- **Reusable UI primitive** → `src/components/ui/`.
- **Cross-cutting component** (e.g. error boundary) → `src/components/shared/`.
- **Reusable hook** → `src/hooks/`.
- **API or external integration** → `src/services/`.
- **Shared types** → `src/types/`.
- **Generic helpers** → `src/utils/`.

## Import conventions

- Use path aliases if configured (e.g. `@/components/...`); otherwise use relative paths from `src/`.
- Prefer barrel exports (`index.ts`) for features and shared modules to keep imports clean.
- Features should not import from other features; shared code goes in `components/`, `hooks/`, `services/`, etc.

## Reference

See the **Client** blueprint for detailed architectural guidance and dependency rules.
