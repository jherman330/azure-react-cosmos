# Frontend Testing (AC-FOUNDATION-017)

Vitest and React Testing Library are used for component and behavior-based tests.

## Stack

- **Vitest** — test runner (configured in `vite.config.ts`)
- **React Testing Library** — component tests via DOM queries and user-centric assertions
- **jsdom** — browser-like environment in Node
- **@testing-library/jest-dom** — matchers (e.g. `toBeInTheDocument()`)

## Running tests

```bash
cd src/web
npm run test          # single run
npm run test:watch    # watch mode
npm run test:ui       # Vitest UI
npm run test:coverage # run with coverage report
```

## Conventions

1. **File location** — Place tests next to the code: `*.test.tsx` or `*.spec.tsx` in the same directory as the component (or in a `__tests__` folder).
2. **Rendering** — Use `renderWithProviders()` from `src/test-utils.tsx` for components that need Fluent theme and/or React Router.
3. **Queries** — Prefer `getByRole`, `getByLabelText`, `getByTestId` (use `data-testid` sparingly for list items or when role/label are not sufficient).
4. **Mocking** — Mock at the **service layer** with `vi.mock('../services/itemsService')` so components receive data/errors without real HTTP. Avoid mocking implementation details.
5. **Async** — Use `waitFor()` for state updates after async operations (e.g. after a mocked service resolves).
6. **Coverage** — Coverage is configured in `vite.config.ts`; exclude setup, types, and test files. Run `npm run test:coverage` to generate reports.

## Example patterns

- **Rendering**: Assert that expected text, roles, or links are in the document.
- **Interaction**: Use `@testing-library/user-event` for clicks and input; assert on resulting UI.
- **Loading / error / empty**: Mock the service to resolve or reject; assert the correct message or list is shown.

## CI

The pipeline runs `npm ci` and `npm run test` in `src/web` on every push to main/master.
