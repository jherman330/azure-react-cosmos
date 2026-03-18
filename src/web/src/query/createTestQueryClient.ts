import { QueryClient } from '@tanstack/react-query';

/** Fast, deterministic defaults for Vitest (no retry backoff). */
export function createTestQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}
