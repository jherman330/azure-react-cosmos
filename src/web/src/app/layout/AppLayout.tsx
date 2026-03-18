import { FC, ReactElement } from 'react';
import { Outlet } from 'react-router-dom';
import Header from './Header';
import { Stack } from '@fluentui/react';
import { headerStackStyles, mainStackStyles, rootStackStyles } from '../../styles/styles';
import LocalDevAuthBanner from '../../components/shared/LocalDevAuthBanner';

const AppLayout: FC = (): ReactElement => {
    return (
        <Stack styles={rootStackStyles}>
            <Stack.Item styles={headerStackStyles}>
                <Header />
            </Stack.Item>
            <LocalDevAuthBanner />
            <Stack.Item grow={1} styles={mainStackStyles}>
                <Outlet />
            </Stack.Item>
        </Stack>
    );
};

export default AppLayout;
