import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function RequireRole({ allowedRoles }: { allowedRoles: string[] }) {
  const { role } = useAuth();
  return allowedRoles.includes(role || '') ? <Outlet /> : <Navigate to="/403" replace />;
}