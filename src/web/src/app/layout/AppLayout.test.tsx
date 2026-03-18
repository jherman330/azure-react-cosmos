import { describe, it, expect } from 'vitest';
import { screen } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { renderWithProviders } from '../../test-utils';
import AppLayout from './AppLayout';

describe('AppLayout', () => {
  it('shows AUTH DISABLED banner when MSAL is not configured', () => {
    renderWithProviders(
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<div>Page</div>} />
        </Route>
      </Routes>
    );
    expect(
      screen.getByRole('status', {
        name: /authentication disabled for local development/i,
      })
    ).toBeInTheDocument();
    expect(screen.getAllByText(/AUTH DISABLED/i).length).toBeGreaterThanOrEqual(
      1
    );
  });
});
