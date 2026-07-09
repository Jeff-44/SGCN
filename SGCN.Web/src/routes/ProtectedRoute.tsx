import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../store/authStore';
import { hasAnyRole } from '../utils/roles';

type ProtectedRouteProps = {
  children: ReactNode;
  roles?: string[];
};

export default function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate replace to="/login" />;
  }

  if (user?.forcePasswordChange && location.pathname !== '/change-password') {
    return <Navigate replace to="/change-password" />;
  }

  if (roles && roles.length > 0 && !hasAnyRole(user, roles)) {
    return <Navigate replace to="/" />;
  }

  return children;
}
