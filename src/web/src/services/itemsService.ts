/**
 * Items service — typed contract for items API (REQ-FOUNDATION-016).
 */
import { apiClient } from './apiClient';

export interface ItemDto {
  id: string;
  createdAt?: string;
  updatedAt?: string;
}

const ROUTE = '/items';

/** Service interface: add methods here as the API grows. */
export interface ItemsListService {
  getList(): Promise<ItemDto[]>;
}

export const itemsService: ItemsListService = {
  async getList() {
    const { data } = await apiClient.get<ItemDto[]>(ROUTE);
    return data ?? [];
  },
};
