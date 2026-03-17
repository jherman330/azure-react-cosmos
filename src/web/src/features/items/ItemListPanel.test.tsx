/**
 * Example tests: loading, error, and empty states with API mocked at service layer (AC-FOUNDATION-017.5, 017.6).
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../test-utils';
import ItemListPanel from './ItemListPanel';
import { itemsService } from '../../services/itemsService';

vi.mock('../../services/itemsService', () => ({
  itemsService: {
    getList: vi.fn(),
  },
}));

describe('ItemListPanel', () => {
  beforeEach(() => {
    vi.mocked(itemsService.getList).mockReset();
  });

  it('shows loading state while fetching', () => {
    vi.mocked(itemsService.getList).mockImplementation(() => new Promise(() => {}));
    renderWithProviders(<ItemListPanel />);
    expect(screen.getByText(/Loading items/i)).toBeInTheDocument();
  });

  it('shows error state when the service rejects', async () => {
    vi.mocked(itemsService.getList).mockRejectedValue(new Error('Network error'));
    renderWithProviders(<ItemListPanel />);
    await waitFor(() => {
      expect(screen.getByText(/Error:/i)).toBeInTheDocument();
    });
    expect(screen.getByText(/Network error/i)).toBeInTheDocument();
  });

  it('shows empty state when the service returns an empty array', async () => {
    vi.mocked(itemsService.getList).mockResolvedValue([]);
    renderWithProviders(<ItemListPanel />);
    await waitFor(() => {
      expect(screen.getByText(/No items found/i)).toBeInTheDocument();
    });
  });

  it('renders the list when the service returns items', async () => {
    vi.mocked(itemsService.getList).mockResolvedValue([
      { id: '1' },
      { id: '2' },
    ]);
    renderWithProviders(<ItemListPanel />);
    await waitFor(() => {
      expect(screen.getByTestId('item-list')).toBeInTheDocument();
    });
    expect(screen.getByTestId('item-1')).toHaveTextContent('1');
    expect(screen.getByTestId('item-2')).toHaveTextContent('2');
    expect(screen.getByText(/Items \(2\)/)).toBeInTheDocument();
  });
});
