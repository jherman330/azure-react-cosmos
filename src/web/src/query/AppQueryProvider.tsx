import { FC, PropsWithChildren, useState } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { createQueryClient } from './createQueryClient';

/**
 * REQ-FOUNDATION-014.3, 014.7: QueryClientProvider + DevTools in development.
 */
const AppQueryProvider: FC<PropsWithChildren> = ({ children }) => {
  const [queryClient] = useState(createQueryClient);
  return (
    <QueryClientProvider client={queryClient}>
      {children}
      {import.meta.env.DEV ? (
        <ReactQueryDevtools initialIsOpen={false} buttonPosition="bottom-left" />
      ) : null}
    </QueryClientProvider>
  );
};

export default AppQueryProvider;
