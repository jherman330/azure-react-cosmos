import {
  FontIcon,
  getTheme,
  IconButton,
  IIconProps,
  IStackStyles,
  mergeStyles,
  Persona,
  PersonaSize,
  Stack,
  Text,
  DefaultButton,
} from '@fluentui/react';
import { FC, ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal, useIsAuthenticated } from '@azure/msal-react';
import config from '../../config';

const theme = getTheme();

const logoStyles: IStackStyles = {
  root: {
    width: '300px',
    background: theme.palette.themePrimary,
    alignItems: 'center',
    padding: '0 20px',
  },
};

const logoIconClass = mergeStyles({
  fontSize: 20,
  paddingRight: 10,
});

const toolStackClass: IStackStyles = {
  root: {
    alignItems: 'center',
    height: 48,
    paddingRight: 10,
  },
};

const iconProps: IIconProps = {
  styles: {
    root: {
      fontSize: 16,
      color: theme.palette.white,
    },
  },
};

const authDisabledTagClass = mergeStyles({
  fontSize: 10,
  fontWeight: 600,
  letterSpacing: '0.1em',
  marginLeft: 10,
  padding: '2px 8px',
  borderRadius: 2,
  backgroundColor: 'rgba(0, 0, 0, 0.25)',
  color: '#e8d88a',
  border: '1px solid rgba(255, 220, 100, 0.35)',
});

/** Header when MSAL is off (no MsalProvider in tree). */
const HeaderWithoutAuth: FC = (): ReactElement => (
  <Stack horizontal>
    <Stack horizontal styles={logoStyles} verticalAlign="center">
      <FontIcon
        aria-label="Check"
        iconName="SkypeCircleCheck"
        className={logoIconClass}
      />
      <Text variant="xLarge">App</Text>
      <span className={authDisabledTagClass} title="MSAL is not configured">
        AUTH DISABLED
      </span>
    </Stack>
    <Stack.Item grow={1}>
      <div />
    </Stack.Item>
    <Stack.Item>
      <Stack horizontal styles={toolStackClass} grow={1}>
        <IconButton
          aria-label="Settings"
          iconProps={{ iconName: 'Settings', ...iconProps }}
        />
        <IconButton
          aria-label="Help"
          iconProps={{ iconName: 'Help', ...iconProps }}
        />
        <Persona
          size={PersonaSize.size24}
          text="Local Dev User"
          secondaryText="Auth disabled"
        />
      </Stack>
    </Stack.Item>
  </Stack>
);

/** Header when MSAL is on (must render under MsalProvider). */
const HeaderWithAuth: FC = (): ReactElement => {
  const navigate = useNavigate();
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  const handleLogout = () => {
    instance.logoutRedirect({
      postLogoutRedirectUri:
        config.auth.postLogoutRedirectUri || window.location.origin,
    });
  };

  const account =
    instance.getActiveAccount() ?? instance.getAllAccounts()[0];
  const personaText =
    account?.name ||
    account?.username ||
    account?.localAccountId ||
    'Signed in';

  return (
    <Stack horizontal>
      <Stack horizontal styles={logoStyles}>
        <FontIcon
          aria-label="Check"
          iconName="SkypeCircleCheck"
          className={logoIconClass}
        />
        <Text variant="xLarge">App</Text>
      </Stack>
      <Stack.Item grow={1}>
        <div />
      </Stack.Item>
      <Stack.Item>
        <Stack horizontal styles={toolStackClass} grow={1}>
          <IconButton
            aria-label="Settings"
            iconProps={{ iconName: 'Settings', ...iconProps }}
          />
          <IconButton
            aria-label="Help"
            iconProps={{ iconName: 'Help', ...iconProps }}
          />
          {isAuthenticated ? (
            <>
              <Persona size={PersonaSize.size24} text={personaText} />
              <DefaultButton
                text="Sign out"
                onClick={handleLogout}
                styles={{ root: { marginLeft: 8 } }}
              />
            </>
          ) : (
            <DefaultButton
              text="Sign in"
              onClick={() => navigate('/login')}
              styles={{ root: { marginLeft: 8 } }}
            />
          )}
        </Stack>
      </Stack.Item>
    </Stack>
  );
};

const Header: FC = (): ReactElement => {
  if (!config.auth.isEnabled) {
    return <HeaderWithoutAuth />;
  }
  return <HeaderWithAuth />;
};

export default Header;
