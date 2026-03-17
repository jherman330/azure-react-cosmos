/**
 * Items service for list/get operations. Used by components; mock this module in tests (AC-FOUNDATION-017.6).
 */
import { apiClient } from './apiClient';

export interface ItemDto {
  id: string;
  createdAt?: string;
  updatedAt?: string;
}

const ROUTE = '/items';

export const itemsService = {
  async getList(): Promise<ItemDto[]> {
    const { data } = await apiClient.get<ItemDto[]>(ROUTE);
    return data ?? [];
  },
};
