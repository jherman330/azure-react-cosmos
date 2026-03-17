import { FC, useEffect, useState } from 'react';
import { Spinner, Stack, Text } from '@fluentui/react';
import { itemsService, ItemDto } from '../../services/itemsService';

type Status = 'idle' | 'loading' | 'success' | 'error';

const ItemListPanel: FC = () => {
  const [status, setStatus] = useState<Status>('idle');
  const [items, setItems] = useState<ItemDto[]>([]);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setStatus('loading');
    setErrorMessage(null);
    itemsService
      .getList()
      .then((data) => {
        if (!cancelled) {
          setItems(data ?? []);
          setStatus('success');
        }
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setErrorMessage(err.message ?? 'Failed to load');
          setStatus('error');
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  if (status === 'loading') {
    return (
      <Stack tokens={{ childrenGap: 8 }}>
        <Spinner label="Loading items..." />
      </Stack>
    );
  }

  if (status === 'error') {
    return (
      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="large" styles={{ root: { color: 'var(--errorText)' } }}>
          Error: {errorMessage}
        </Text>
      </Stack>
    );
  }

  if (status === 'success' && items.length === 0) {
    return (
      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="medium">No items found.</Text>
      </Stack>
    );
  }

  return (
    <Stack tokens={{ childrenGap: 8 }}>
      <Text variant="medium">Items ({items.length})</Text>
      <ul data-testid="item-list">
        {items.map((item) => (
          <li key={item.id} data-testid={`item-${item.id}`}>
            {item.id}
          </li>
        ))}
      </ul>
    </Stack>
  );
};

export default ItemListPanel;
