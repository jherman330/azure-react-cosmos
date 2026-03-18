/** Central query keys for cache identity and invalidation (REQ-FOUNDATION-014). */
export const queryKeys = {
  items: {
    all: ['items'] as const,
    list: () => [...queryKeys.items.all, 'list'] as const,
  },
} as const;
