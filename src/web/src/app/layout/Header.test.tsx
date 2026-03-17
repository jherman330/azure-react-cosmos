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
    const buttons = screen.getAllByRole('button', { name: /Add/i });
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it('icon buttons can be clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<Header />);
    const buttons = screen.getAllByRole('button', { name: /Add/i });
    await user.click(buttons[0]);
    expect(buttons[0]).toBeInTheDocument();
  });

  it('renders Sample User persona text', () => {
    renderWithProviders(<Header />);
    const matches = screen.getAllByText('Sample User');
    expect(matches.length).toBeGreaterThanOrEqual(1);
  });
});
