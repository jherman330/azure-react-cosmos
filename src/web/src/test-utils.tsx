/**
 * Test utilities for component tests (AC-FOUNDATION-017).
 * Use renderWithProviders for components that need Router and/or Fluent theme.
 */
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from './styles/theme';
import { MemoryRouter } from 'react-router-dom';

const AllThemes: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ThemeProvider applyTo="body" theme={DarkTheme}>
    <MemoryRouter>{children}</MemoryRouter>
  </ThemeProvider>
);

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, {
    wrapper: AllThemes,
    ...options,
  });
}

export * from '@testing-library/react';
