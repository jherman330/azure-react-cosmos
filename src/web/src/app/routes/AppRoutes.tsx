import { Routes, Route } from 'react-router-dom';
import AppLayout from '../layout/AppLayout';
import HomePage from '../../features/home';

const AppRoutes: React.FC = () => {
    return (
        <Routes>
            <Route element={<AppLayout />}>
                <Route path="/" element={<HomePage />} />
            </Route>
        </Routes>
    );
};

export default AppRoutes;
