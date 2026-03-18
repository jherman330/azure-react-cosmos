import { FC, useEffect } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import {
  PrimaryButton,
  Stack,
  Text,
  MessageBar,
  MessageBarType,
} from '@fluentui/react';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import config from '../../config';
import { getLoginRequestScopes } from '../../auth/msalConfig';
import LocalDevAuthBanner from '../../components/shared/LocalDevAuthBanner';

/** /login when MSAL is off — no MsalProvider; must not call useMsal. */
const LoginPageAuthDisabled: FC = () => (
  <Stack tokens={{ childrenGap: 16 }} styles={{ root: { minHeight: '100vh' } }}>
    <LocalDevAuthBanner />
    <Stack tokens={{ childrenGap: 16 }} styles={{ root: { padding: 24 } }}>
      <Text variant="xxLarge">Sign in</Text>
      <MessageBar messageBarType={MessageBarType.info}>
        Microsoft Entra sign-in is not configured. Set{' '}
        <code>VITE_MSAL_CLIENT_ID</code> (and optional{' '}
        <code>VITE_MSAL_API_SCOPES</code>) to enable real authentication. In
        this mode there is no sign-in flow and no API tokens.
      </MessageBar>
      <Link to="/">Back to home</Link>
    </Stack>
  </Stack>
);

const LoginPageWithMsal: FC = () => {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const location = useLocation();
  const navigate = useNavigate();
  const from =
    (location.state as { from?: string } | null)?.from ?? '/';

  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, from, navigate]);

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleLogin = () => {
    instance.loginRedirect({
      scopes: getLoginRequestScopes(),
    });
  };

  return (
    <Stack
      horizontalAlign="center"
      verticalAlign="center"
      styles={{ root: { minHeight: '60vh', padding: 24 } }}
      tokens={{ childrenGap: 20 }}
    >
      <Text variant="xxLarge">Sign in</Text>
      <Text>
        Use your Microsoft Entra ID (work or school) account to continue.
      </Text>
      <PrimaryButton text="Sign in with Microsoft" onClick={handleLogin} />
      <Link to="/">Cancel</Link>
    </Stack>
  );
};

const LoginPage: FC = () => {
  if (!config.auth.isEnabled) {
    return <LoginPageAuthDisabled />;
  }
  return <LoginPageWithMsal />;
};

export default LoginPage;
