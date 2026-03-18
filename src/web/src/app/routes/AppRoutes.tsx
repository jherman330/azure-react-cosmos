import { Routes, Route } from 'react-router-dom';
import AppLayout from '../layout/AppLayout';
import HomePage from '../../features/home';
import { LoginPage } from '../../features/auth';
import ProtectedRoute from '../../auth/ProtectedRoute';

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AppLayout />}>
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<HomePage />} />
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRoutes;
