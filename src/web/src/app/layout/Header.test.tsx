/**
 * Example component tests: rendering and user interaction (AC-FOUNDATION-017.3, 017.4).
 */
import { describe, it, expect } from 'vitest';
import userEvent from '@testing-library/user-event';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../test-utils';
import Header from './Header';

describe('Header', () => {
  it('renders the app logo text', () => {
    renderWithProviders(<Header />);
    expect(screen.getByText('App')).toBeInTheDocument();
  });

  it('renders Settings and Help icon buttons', () => {
    renderWithProviders(<Header />);
    expect(screen.getByRole('button', { name: /Settings/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Help/i })).toBeInTheDocument();
  });

  it('icon buttons can be clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<Header />);
    const buttons = [
      screen.getByRole('button', { name: /Settings/i }),
      screen.getByRole('button', { name: /Help/i }),
    ];
    await user.click(buttons[0]);
    expect(buttons[0]).toBeInTheDocument();
  });

  it('renders Local Dev User and auth-disabled label when MSAL is off', () => {
    renderWithProviders(<Header />);
    expect(screen.getAllByText('Local Dev User').length).toBeGreaterThanOrEqual(
      1
    );
    expect(screen.getAllByText('Auth disabled').length).toBeGreaterThanOrEqual(
      1
    );
    expect(screen.getByTitle('MSAL is not configured')).toHaveTextContent(
      'AUTH DISABLED'
    );
  });
});
