import { FC, PropsWithChildren } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from '../../styles/theme';
import Telemetry from '../../components/shared/Telemetry';
import config from '../../config';
import MsalAppProvider from '../../auth/MsalAppProvider';
import AuthTokenBridge from '../../auth/AuthTokenBridge';
import AppQueryProvider from '../../query/AppQueryProvider';

const AppProviders: FC<PropsWithChildren<unknown>> = ({ children }) => {
  const routerTree = (
    <BrowserRouter>
      {config.auth.isEnabled ? <AuthTokenBridge /> : null}
      <Telemetry>{children}</Telemetry>
    </BrowserRouter>
  );

  return (
    <ThemeProvider applyTo="body" theme={DarkTheme}>
      <AppQueryProvider>
        {config.auth.isEnabled ? (
          <MsalAppProvider>{routerTree}</MsalAppProvider>
        ) : (
          routerTree
        )}
      </AppQueryProvider>
    </ThemeProvider>
  );
};

export default AppProviders;
