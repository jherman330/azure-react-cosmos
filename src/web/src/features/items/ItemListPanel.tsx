import { FC } from 'react';
import { Spinner, Stack, Text } from '@fluentui/react';
import { useItemsQuery } from '../../hooks/useItemsQuery';

const ItemListPanel: FC = () => {
  const { data: items = [], isPending, isError, error } = useItemsQuery();

  if (isPending) {
    return (
      <Stack tokens={{ childrenGap: 8 }}>
        <Spinner label="Loading items..." />
      </Stack>
    );
  }

  if (isError) {
    const message = error instanceof Error ? error.message : 'Failed to load';
    return (
      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="large" styles={{ root: { color: 'var(--errorText)' } }}>
          Error: {message}
        </Text>
      </Stack>
    );
  }

  if (items.length === 0) {
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
