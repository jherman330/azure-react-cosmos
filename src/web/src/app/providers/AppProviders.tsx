import { FC, PropsWithChildren } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from '../../styles/theme';
import Telemetry from '../../components/shared/Telemetry';

const AppProviders: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return (
        <ThemeProvider applyTo="body" theme={DarkTheme}>
            <BrowserRouter>
                <Telemetry>
                    {children}
                </Telemetry>
            </BrowserRouter>
        </ThemeProvider>
    );
};

export default AppProviders;
