import { FC } from 'react';
import { mergeStyles } from '@fluentui/react';
import config from '../../config';

const stripClass = mergeStyles({
  display: 'block',
  width: '100%',
  boxSizing: 'border-box',
  fontSize: 11,
  lineHeight: '18px',
  padding: '4px 12px',
  textAlign: 'center',
  backgroundColor: 'rgba(180, 140, 0, 0.12)',
  color: '#d4c77e',
  borderBottom: '1px solid rgba(180, 140, 0, 0.25)',
  letterSpacing: '0.02em',
});

const labelClass = mergeStyles({
  fontWeight: 600,
  letterSpacing: '0.08em',
  marginRight: 8,
});

/**
 * Visible reminder when MSAL is off (no VITE_MSAL_CLIENT_ID): local dev only, not real auth.
 */
const LocalDevAuthBanner: FC = () => {
  if (config.auth.isEnabled) {
    return null;
  }

  return (
    <div
      className={stripClass}
      role="status"
      aria-label="Authentication disabled for local development"
    >
      <span className={labelClass}>AUTH DISABLED</span>
      <span>
        Local development only — you are not signed in; API requests send no{' '}
        <code style={{ fontSize: 10 }}>Authorization</code> header and no
        tokens are issued.
      </span>
    </div>
  );
};

export default LocalDevAuthBanner;
