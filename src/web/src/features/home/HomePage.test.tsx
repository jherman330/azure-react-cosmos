/**
 * Example component tests: rendering behavior (AC-FOUNDATION-017.3).
 */
import { describe, it, expect } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../test-utils';
import HomePage from './HomePage';

describe('HomePage', () => {
  it('renders the main heading', () => {
    renderWithProviders(<HomePage />);
    expect(screen.getByText(/Add your own application code/i)).toBeInTheDocument();
  });

  it('renders the scaffold description', () => {
    renderWithProviders(<HomePage />);
    expect(screen.getByText(/minimal scaffold with React/i)).toBeInTheDocument();
  });

  it('includes a link to Azure Developer CLI docs', () => {
    renderWithProviders(<HomePage />);
    const link = screen.getByRole('link', { name: /Azure Developer CLI docs/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', 'https://learn.microsoft.com/azure/developer/azure-developer-cli/');
  });
});
