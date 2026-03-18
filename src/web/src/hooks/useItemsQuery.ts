import { useQuery } from '@tanstack/react-query';
import { itemsService } from '../services/itemsService';
import { queryKeys } from './queryKeys';

/**
 * Server state for the items list (REQ-FOUNDATION-014.4).
 * Replaces imperative useEffect + itemsService.getList in components.
 */
export function useItemsQuery() {
  return useQuery({
    queryKey: queryKeys.items.list(),
    queryFn: () => itemsService.getList(),
  });
}
