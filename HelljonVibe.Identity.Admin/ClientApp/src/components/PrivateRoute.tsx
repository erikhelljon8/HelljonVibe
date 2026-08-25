import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { AuthService } from '../services/authService';

interface PrivateRouteProps {
  children: React.ReactNode;
  roles?: string[];
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children, roles }) => {
  const location = useLocation();
  const user = AuthService.getCurrentUser();

  if (!user) {
    // Redirect to login page, but save the attempted location
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check if user has required roles
  if (roles && roles.length > 0) {
    const hasRequiredRole = user.roles && user.roles.some((role: string) => roles.includes(role));
    if (!hasRequiredRole) {
      // User doesn't have required role, redirect to dashboard or access denied
      return <Navigate to="/dashboard" replace />;
    }
  }

  return <>{children}</>;
};

export default PrivateRoute;
