import { describe, it, expect } from 'vitest';

describe('Example Test Suite', () => {
  it('should pass a basic assertion', () => {
    expect(true).toBe(true);
  });

  it('should perform simple arithmetic', () => {
    expect(2 + 2).toBe(4);
  });

  it('should work with strings', () => {
    const greeting = 'Hello, World!';
    expect(greeting).toContain('Hello');
  });
});
