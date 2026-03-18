import { FC, useEffect } from 'react';
import { useMsal } from '@azure/msal-react';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import {
  registerApiTokenGetter,
  resetApiTokenGetter,
} from '../services/apiClient';
import config from '../config';
import { getTokenRequestScopes } from './msalConfig';

/**
 * Wires MSAL token acquisition into the shared API client (Bearer on all axios instances).
 */
const AuthTokenBridge: FC = () => {
  const { instance, accounts } = useMsal();

  useEffect(() => {
    if (!config.auth.isEnabled) {
      return;
    }
    const first = accounts[0];
    if (first && !instance.getActiveAccount()) {
      instance.setActiveAccount(first);
    }

    registerApiTokenGetter(async () => {
      const account =
        instance.getActiveAccount() ?? accounts[0] ?? instance.getAllAccounts()[0];
      if (!account) {
        return null;
      }
      const scopes = getTokenRequestScopes();
      try {
        const result = await instance.acquireTokenSilent({
          scopes,
          account,
        });
        return result.accessToken;
      } catch (e) {
        if (e instanceof InteractionRequiredAuthError) {
          try {
            await instance.acquireTokenRedirect({
              scopes,
              account,
            });
          } catch {
            /* redirect started */
          }
        }
        return null;
      }
    });

    return () => {
      resetApiTokenGetter();
    };
  }, [instance, accounts]);

  return null;
};

export default AuthTokenBridge;
