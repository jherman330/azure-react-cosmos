import React from 'react';
import ReactDOM from 'react-dom/client';
import { mergeStyles } from '@fluentui/react';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import App from './app/App';
import reportWebVitals from './reportWebVitals';

initializeIcons(undefined, { disableWarnings: true });

// Inject some global styles
mergeStyles({
  ':global(body,html,#root)': {
    margin: 0,
    padding: 0,
    height: '100vh',
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
 <React.StrictMode>
    <App />
 </React.StrictMode>,
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
