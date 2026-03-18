/// <reference types="vite/client" />

export interface ApiConfig {
  baseUrl: string;
}

export interface ObservabilityConfig {
  connectionString: string;
}

/** Entra ID / MSAL settings (browser-safe; client ID is public). */
export interface AuthConfig {
  /** When false, MSAL is not loaded; routes are open and no Bearer token is sent. */
  isEnabled: boolean;
  clientId: string;
  authority: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  /** Scopes for the backend API (e.g. api://{api-app-id}/access_as_user). Comma-separated in env. */
  apiScopes: string[];
}

export interface AppConfig {
  api: ApiConfig;
  observability: ObservabilityConfig;
  auth: AuthConfig;
}

function parseApiScopes(raw: string | undefined): string[] {
  if (!raw?.trim()) return [];
  return raw
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean);
}

const clientId = (import.meta.env.VITE_MSAL_CLIENT_ID ?? '').trim();

const config: AppConfig = {
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:3100',
  },
  observability: {
    connectionString:
      import.meta.env.VITE_APPLICATIONINSIGHTS_CONNECTION_STRING || '',
  },
  auth: {
    isEnabled: Boolean(clientId),
    clientId,
    authority:
      (import.meta.env.VITE_MSAL_AUTHORITY ?? '').trim() ||
      'https://login.microsoftonline.com/common',
    redirectUri: (import.meta.env.VITE_MSAL_REDIRECT_URI ?? '').trim(),
    postLogoutRedirectUri: (
      import.meta.env.VITE_MSAL_POST_LOGOUT_REDIRECT_URI ?? ''
    ).trim(),
    apiScopes: parseApiScopes(import.meta.env.VITE_MSAL_API_SCOPES),
  },
};

export default config;
