/**
 * Test utilities for component tests (AC-FOUNDATION-017).
 * Use renderWithProviders for components that need Router, Fluent theme, and TanStack Query (REQ-FOUNDATION-014).
 */
/* eslint-disable react-refresh/only-export-components -- test helpers + RTL re-exports */
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { TestProviders } from './test/TestProviders';

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, {
    wrapper: TestProviders,
    ...options,
  });
}

export * from '@testing-library/react';
