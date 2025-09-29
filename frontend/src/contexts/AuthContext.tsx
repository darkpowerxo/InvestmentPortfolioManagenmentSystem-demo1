import React, { createContext, useContext, useReducer, useEffect, ReactNode } from 'react';
import { 
  AuthState, 
  AuthUser, 
  LoginRequest, 
  RegisterRequest, 
  UpdateProfileRequest,
  ChangePasswordRequest 
} from '../types';
import AuthService, { tokenManager } from '../services/authService';

// Action Types
type AuthAction =
  | { type: 'AUTH_START' }
  | { type: 'AUTH_SUCCESS'; payload: { user: AuthUser; token: string; refreshToken: string } }
  | { type: 'AUTH_FAILURE'; payload: string }
  | { type: 'AUTH_LOGOUT' }
  | { type: 'AUTH_UPDATE_USER'; payload: AuthUser }
  | { type: 'AUTH_CLEAR_ERROR' }
  | { type: 'AUTH_SET_LOADING'; payload: boolean };

// Initial State
const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  token: null,
  refreshToken: null,
  isLoading: true, // Start with loading true to check existing auth
  error: null,
};

// Reducer
const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'AUTH_START':
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case 'AUTH_SUCCESS':
      return {
        ...state,
        isAuthenticated: true,
        user: action.payload.user,
        token: action.payload.token,
        refreshToken: action.payload.refreshToken,
        isLoading: false,
        error: null,
      };

    case 'AUTH_FAILURE':
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        token: null,
        refreshToken: null,
        isLoading: false,
        error: action.payload,
      };

    case 'AUTH_LOGOUT':
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        token: null,
        refreshToken: null,
        isLoading: false,
        error: null,
      };

    case 'AUTH_UPDATE_USER':
      return {
        ...state,
        user: action.payload,
      };

    case 'AUTH_CLEAR_ERROR':
      return {
        ...state,
        error: null,
      };

    case 'AUTH_SET_LOADING':
      return {
        ...state,
        isLoading: action.payload,
      };

    default:
      return state;
  }
};

// Context Interface
interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<boolean>;
  register: (userData: RegisterRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  updateProfile: (profileData: UpdateProfileRequest) => Promise<boolean>;
  changePassword: (passwordData: ChangePasswordRequest) => Promise<boolean>;
  clearError: () => void;
  refreshUserProfile: () => Promise<void>;
  hasRole: (role: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
}

// Create Context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Provider Props
interface AuthProviderProps {
  children: ReactNode;
}

// Provider Component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // Initialize auth state on app start
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        if (AuthService.isAuthenticated()) {
          const user = AuthService.getCurrentUser();
          const token = tokenManager.getToken();
          const refreshToken = tokenManager.getRefreshToken();

          if (user && token && refreshToken) {
            dispatch({
              type: 'AUTH_SUCCESS',
              payload: { user, token, refreshToken }
            });

            // Refresh user profile to ensure data is up to date
            try {
              const profileResponse = await AuthService.getProfile();
              if (profileResponse.success && profileResponse.data) {
                dispatch({
                  type: 'AUTH_UPDATE_USER',
                  payload: profileResponse.data
                });
              }
            } catch (error) {
              console.warn('Failed to refresh user profile:', error);
            }
          } else {
            dispatch({ type: 'AUTH_LOGOUT' });
          }
        } else {
          dispatch({ type: 'AUTH_LOGOUT' });
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        dispatch({ type: 'AUTH_LOGOUT' });
      } finally {
        dispatch({ type: 'AUTH_SET_LOADING', payload: false });
      }
    };

    initializeAuth();
  }, []);

  // Login function
  const login = async (credentials: LoginRequest): Promise<boolean> => {
    dispatch({ type: 'AUTH_START' });

    try {
      const response = await AuthService.login(credentials);

      if (response.success && response.data) {
        const { user, token, refreshToken } = response.data;
        dispatch({
          type: 'AUTH_SUCCESS',
          payload: { user, token, refreshToken }
        });
        return true;
      } else {
        dispatch({
          type: 'AUTH_FAILURE',
          payload: response.message || 'Login failed'
        });
        return false;
      }
    } catch (error: any) {
      dispatch({
        type: 'AUTH_FAILURE',
        payload: error.message || 'Login failed'
      });
      return false;
    }
  };

  // Register function
  const register = async (userData: RegisterRequest): Promise<boolean> => {
    dispatch({ type: 'AUTH_START' });

    try {
      const response = await AuthService.register(userData);

      if (response.success) {
        // After successful registration, automatically log in
        const loginResponse = await AuthService.login({
          email: userData.email,
          password: userData.password
        });

        if (loginResponse.success && loginResponse.data) {
          const { user, token, refreshToken } = loginResponse.data;
          dispatch({
            type: 'AUTH_SUCCESS',
            payload: { user, token, refreshToken }
          });
          return true;
        }
      }

      dispatch({
        type: 'AUTH_FAILURE',
        payload: response.message || 'Registration failed'
      });
      return false;
    } catch (error: any) {
      dispatch({
        type: 'AUTH_FAILURE',
        payload: error.message || 'Registration failed'
      });
      return false;
    }
  };

  // Logout function
  const logout = async (): Promise<void> => {
    dispatch({ type: 'AUTH_SET_LOADING', payload: true });

    try {
      await AuthService.logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      dispatch({ type: 'AUTH_LOGOUT' });
    }
  };

  // Update profile function
  const updateProfile = async (profileData: UpdateProfileRequest): Promise<boolean> => {
    try {
      const response = await AuthService.updateProfile(profileData);

      if (response.success && response.data) {
        dispatch({
          type: 'AUTH_UPDATE_USER',
          payload: response.data
        });
        return true;
      } else {
        dispatch({
          type: 'AUTH_FAILURE',
          payload: response.message || 'Profile update failed'
        });
        return false;
      }
    } catch (error: any) {
      dispatch({
        type: 'AUTH_FAILURE',
        payload: error.message || 'Profile update failed'
      });
      return false;
    }
  };

  // Change password function
  const changePassword = async (passwordData: ChangePasswordRequest): Promise<boolean> => {
    try {
      const response = await AuthService.changePassword(passwordData);

      if (response.success) {
        return true;
      } else {
        dispatch({
          type: 'AUTH_FAILURE',
          payload: response.message || 'Password change failed'
        });
        return false;
      }
    } catch (error: any) {
      dispatch({
        type: 'AUTH_FAILURE',
        payload: error.message || 'Password change failed'
      });
      return false;
    }
  };

  // Clear error function
  const clearError = (): void => {
    dispatch({ type: 'AUTH_CLEAR_ERROR' });
  };

  // Refresh user profile
  const refreshUserProfile = async (): Promise<void> => {
    try {
      const response = await AuthService.getProfile();
      if (response.success && response.data) {
        dispatch({
          type: 'AUTH_UPDATE_USER',
          payload: response.data
        });
      }
    } catch (error) {
      console.error('Failed to refresh user profile:', error);
    }
  };

  // Role checking functions
  const hasRole = (role: string): boolean => {
    return state.user?.role === role || false;
  };

  const hasAnyRole = (roles: string[]): boolean => {
    return state.user ? roles.includes(state.user.role) : false;
  };

  // Context value
  const contextValue: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    updateProfile,
    changePassword,
    clearError,
    refreshUserProfile,
    hasRole,
    hasAnyRole,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook to use auth context
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// HOC for protecting routes
interface WithAuthProps {
  requiredRoles?: string[];
  fallback?: ReactNode;
}

export function withAuth<P extends object>(
  Component: React.ComponentType<P>,
  options: WithAuthProps = {}
) {
  const { requiredRoles, fallback } = options;

  return function AuthenticatedComponent(props: P) {
    const { isAuthenticated, isLoading, hasAnyRole } = useAuth();

    if (isLoading) {
      return (
        <div style={{ 
          display: 'flex', 
          justifyContent: 'center', 
          alignItems: 'center', 
          height: '100vh' 
        }}>
          Loading...
        </div>
      );
    }

    if (!isAuthenticated) {
      return fallback || <div>Please log in to access this page.</div>;
    }

    if (requiredRoles && !hasAnyRole(requiredRoles)) {
      return <div>You do not have permission to access this page.</div>;
    }

    return <Component {...props} />;
  };
}

export default AuthContext;