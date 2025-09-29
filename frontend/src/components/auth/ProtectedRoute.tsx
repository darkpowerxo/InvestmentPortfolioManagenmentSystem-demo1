import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Box, CircularProgress, Typography } from '@mui/material';
import { useAuth } from '../../contexts/AuthContext';
import { UserRole } from '../../types';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: UserRole[];
  requireAuth?: boolean;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRoles = [],
  requireAuth = true,
}) => {
  const { user, isLoading, hasRole } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking authentication
  if (isLoading) {
    return (
      <Box
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        minHeight="100vh"
        gap={2}
      >
        <CircularProgress size={48} />
        <Typography variant="body1" color="text.secondary">
          Verifying authentication...
        </Typography>
      </Box>
    );
  }

  // Redirect to login if authentication is required but user is not authenticated
  if (requireAuth && !user) {
    return (
      <Navigate
        to="/login"
        state={{ 
          from: location,
          message: 'Please sign in to access this page.'
        }}
        replace
      />
    );
  }

  // Check role-based access if roles are specified
  if (requiredRoles.length > 0 && user) {
    const hasRequiredRole = requiredRoles.some(role => hasRole(role));
    
    if (!hasRequiredRole) {
      return (
        <Box
          display="flex"
          flexDirection="column"
          alignItems="center"
          justifyContent="center"
          minHeight="100vh"
          gap={2}
          textAlign="center"
          px={3}
        >
          <Typography variant="h4" color="error.main">
            Access Denied
          </Typography>
          <Typography variant="body1" color="text.secondary" maxWidth={500}>
            You don't have the required permissions to access this page. 
            Contact your administrator if you believe this is an error.
          </Typography>
          <Typography variant="body2" color="text.disabled">
            Required roles: {requiredRoles.join(', ')}
          </Typography>
        </Box>
      );
    }
  }

  // Render children if all checks pass
  return <>{children}</>;
};

// Higher-order component version for class components
export const withProtectedRoute = <P extends object>(
  Component: React.ComponentType<P>,
  requiredRoles?: UserRole[],
  requireAuth?: boolean
) => {
  const WrappedComponent: React.FC<P> = (props) => (
    <ProtectedRoute requiredRoles={requiredRoles} requireAuth={requireAuth}>
      <Component {...props} />
    </ProtectedRoute>
  );

  WrappedComponent.displayName = `withProtectedRoute(${Component.displayName || Component.name})`;
  
  return WrappedComponent;
};

export default ProtectedRoute;