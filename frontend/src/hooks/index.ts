import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { 
  portfoliosApi, 
  securitiesApi, 
  positionsApi, 
  transactionsApi, 
  usersApi 
} from '../services/api';
import { 
  Portfolio, 
  Security, 
  Position, 
  Transaction, 
  User, 
  PortfolioSummary,
  SearchResult,
  LoginRequest,
  RegisterRequest,
  PasswordResetRequest,
  PasswordResetConfirm
} from '../types';
import { useAuth } from '../contexts/AuthContext';
import AuthService from '../services/authService';

// Generic hook for API data fetching
export const useApiData = <T>(
  apiCall: () => Promise<any>,
  dependencies: any[] = []
) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await apiCall();
        setData(response.data.data);
      } catch (err: any) {
        setError(err.response?.data?.message || err.message || 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencies]);

  const refetch = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiCall();
      setData(response.data.data);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return { data, loading, error, refetch };
};

// Portfolios hooks
export const usePortfolios = (searchTerm?: string, managerId?: number, type?: string) => {
  return useApiData<SearchResult<Portfolio>>(
    () => portfoliosApi.getPortfolios(searchTerm, managerId, type),
    [searchTerm, managerId, type]
  );
};

export const usePortfolio = (id: number) => {
  return useApiData<Portfolio>(
    () => portfoliosApi.getPortfolio(id),
    [id]
  );
};

export const usePortfolioSummary = (id: number) => {
  return useApiData<PortfolioSummary>(
    () => portfoliosApi.getPortfolioSummary(id),
    [id]
  );
};

export const usePortfoliosByManager = (managerId: number) => {
  return useApiData<Portfolio[]>(
    () => portfoliosApi.getPortfoliosByManager(managerId),
    [managerId]
  );
};

// Securities hooks
export const useSecurities = (searchTerm?: string, type?: string, exchange?: string, sector?: string) => {
  return useApiData<SearchResult<Security>>(
    () => securitiesApi.getSecurities(searchTerm, type, exchange, sector),
    [searchTerm, type, exchange, sector]
  );
};

export const useSecurity = (id: number) => {
  return useApiData<Security>(
    () => securitiesApi.getSecurity(id),
    [id]
  );
};

// Positions hooks
export const usePositions = (portfolioId?: number, securityId?: number) => {
  return useApiData<SearchResult<Position>>(
    () => positionsApi.getPositions(portfolioId, securityId),
    [portfolioId, securityId]
  );
};

export const usePositionsByPortfolio = (portfolioId: number) => {
  return useApiData<Position[]>(
    () => positionsApi.getPositionsByPortfolio(portfolioId),
    [portfolioId]
  );
};

export const useTopPositions = (count: number = 10) => {
  return useApiData<Position[]>(
    () => positionsApi.getTopPositions(count),
    [count]
  );
};

// Transactions hooks
export const useTransactions = (portfolioId?: number, securityId?: number, type?: string) => {
  return useApiData<SearchResult<Transaction>>(
    () => transactionsApi.getTransactions(portfolioId, securityId, type),
    [portfolioId, securityId, type]
  );
};

export const useTransactionsByPortfolio = (portfolioId: number) => {
  return useApiData<Transaction[]>(
    () => transactionsApi.getTransactionsByPortfolio(portfolioId),
    [portfolioId]
  );
};

export const useRecentTransactions = (days: number = 30, portfolioId?: number) => {
  return useApiData<Transaction[]>(
    () => transactionsApi.getRecentTransactions(days, portfolioId),
    [days, portfolioId]
  );
};

export const useLargeTransactions = (threshold: number = 100000) => {
  return useApiData<Transaction[]>(
    () => transactionsApi.getLargeTransactions(threshold),
    [threshold]
  );
};

// Users hooks
export const useUsers = (searchTerm?: string, role?: string) => {
  return useApiData<SearchResult<User>>(
    () => usersApi.getUsers(searchTerm, role),
    [searchTerm, role]
  );
};

export const useUser = (id: number) => {
  return useApiData<User>(
    () => usersApi.getUser(id),
    [id]
  );
};

// Hook for managing form state
export const useFormState = <T>(initialState: T) => {
  const [formData, setFormData] = useState<T>(initialState);
  const [errors, setErrors] = useState<Partial<Record<keyof T, string>>>({});
  const [touched, setTouched] = useState<Partial<Record<keyof T, boolean>>>({});

  const updateField = (field: keyof T, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setTouched(prev => ({ ...prev, [field]: true }));
    
    // Clear error when field is updated
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const setFieldError = (field: keyof T, error: string) => {
    setErrors(prev => ({ ...prev, [field]: error }));
  };

  const clearErrors = () => {
    setErrors({});
  };

  const resetForm = () => {
    setFormData(initialState);
    setErrors({});
    setTouched({});
  };

  return {
    formData,
    errors,
    touched,
    updateField,
    setFieldError,
    clearErrors,
    resetForm,
    setFormData
  };
};

// Hook for pagination
export const usePagination = (initialPage = 1, initialPageSize = 20) => {
  const [currentPage, setCurrentPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);

  const goToPage = (page: number) => {
    setCurrentPage(page);
  };

  const nextPage = () => {
    setCurrentPage(prev => prev + 1);
  };

  const prevPage = () => {
    setCurrentPage(prev => Math.max(1, prev - 1));
  };

  const changePageSize = (size: number) => {
    setPageSize(size);
    setCurrentPage(1); // Reset to first page when changing page size
  };

  return {
    currentPage,
    pageSize,
    goToPage,
    nextPage,
    prevPage,
    changePageSize
  };
};

// Hook for local storage
export const useLocalStorage = <T>(key: string, initialValue: T) => {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(`Error reading localStorage key "${key}":`, error);
      return initialValue;
    }
  });

  const setValue = (value: T | ((val: T) => T)) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value;
      setStoredValue(valueToStore);
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      console.error(`Error setting localStorage key "${key}":`, error);
    }
  };

  return [storedValue, setValue] as const;
};

// Authentication Hooks

/**
 * Hook for handling login form and state
 */
export const useLogin = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogin = useCallback(async (credentials: LoginRequest) => {
    setIsLoading(true);
    setError(null);

    try {
      const success = await login(credentials);
      
      if (success) {
        // Redirect to intended page or dashboard
        const from = (location.state as any)?.from?.pathname || '/dashboard';
        navigate(from, { replace: true });
        return true;
      } else {
        setError('Invalid email or password');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Login failed');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [login, navigate, location.state]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    login: handleLogin,
    isLoading,
    error,
    clearError
  };
};

/**
 * Hook for handling registration form and state
 */
export const useRegister = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleRegister = useCallback(async (userData: RegisterRequest) => {
    setIsLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const success = await register(userData);
      
      if (success) {
        setSuccess(true);
        // Redirect to dashboard after successful registration
        setTimeout(() => {
          navigate('/dashboard', { replace: true });
        }, 1000);
        return true;
      } else {
        setError('Registration failed. Please check your information.');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Registration failed');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [register, navigate]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const clearSuccess = useCallback(() => {
    setSuccess(false);
  }, []);

  return {
    register: handleRegister,
    isLoading,
    error,
    success,
    clearError,
    clearSuccess
  };
};

/**
 * Hook for handling password reset functionality
 */
export const usePasswordReset = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const requestReset = useCallback(async (data: PasswordResetRequest) => {
    setIsLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const response = await AuthService.requestPasswordReset(data);
      
      if (response.success) {
        setSuccess(true);
        return true;
      } else {
        setError(response.message || 'Password reset request failed');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Password reset request failed');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const confirmReset = useCallback(async (data: PasswordResetConfirm) => {
    setIsLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const response = await AuthService.confirmPasswordReset(data);
      
      if (response.success) {
        setSuccess(true);
        return true;
      } else {
        setError(response.message || 'Password reset failed');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Password reset failed');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const clearSuccess = useCallback(() => {
    setSuccess(false);
  }, []);

  return {
    requestReset,
    confirmReset,
    isLoading,
    error,
    success,
    clearError,
    clearSuccess
  };
};

/**
 * Hook for protected routes - redirects to login if not authenticated
 */
export const useProtectedRoute = (requiredRoles?: string[]) => {
  const { isAuthenticated, isLoading, hasAnyRole } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        // Redirect to login with return URL
        navigate('/login', { 
          state: { from: location },
          replace: true 
        });
      } else if (requiredRoles && !hasAnyRole(requiredRoles)) {
        // Redirect to unauthorized page or dashboard
        navigate('/unauthorized', { replace: true });
      }
    }
  }, [isAuthenticated, isLoading, hasAnyRole, requiredRoles, navigate, location]);

  return {
    isAuthenticated,
    isLoading,
    hasPermission: !requiredRoles || hasAnyRole(requiredRoles)
  };
};

/**
 * Hook for logout functionality
 */
export const useLogout = () => {
  const [isLoading, setIsLoading] = useState(false);
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = useCallback(async () => {
    setIsLoading(true);
    
    try {
      await logout();
      navigate('/login', { replace: true });
    } catch (error) {
      console.error('Logout error:', error);
      // Still navigate to login even if logout fails
      navigate('/login', { replace: true });
    } finally {
      setIsLoading(false);
    }
  }, [logout, navigate]);

  return {
    logout: handleLogout,
    isLoading
  };
};

/**
 * Hook for user profile management
 */
export const useProfile = () => {
  const { user, updateProfile, changePassword, refreshUserProfile } = useAuth();
  const [isUpdating, setIsUpdating] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleUpdateProfile = useCallback(async (profileData: any) => {
    setIsUpdating(true);
    setError(null);
    setSuccess(null);

    try {
      const success = await updateProfile(profileData);
      
      if (success) {
        setSuccess('Profile updated successfully');
        return true;
      } else {
        setError('Failed to update profile');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Failed to update profile');
      return false;
    } finally {
      setIsUpdating(false);
    }
  }, [updateProfile]);

  const handleChangePassword = useCallback(async (passwordData: any) => {
    setIsChangingPassword(true);
    setError(null);
    setSuccess(null);

    try {
      const success = await changePassword(passwordData);
      
      if (success) {
        setSuccess('Password changed successfully');
        return true;
      } else {
        setError('Failed to change password');
        return false;
      }
    } catch (err: any) {
      setError(err.message || 'Failed to change password');
      return false;
    } finally {
      setIsChangingPassword(false);
    }
  }, [changePassword]);

  const clearMessages = useCallback(() => {
    setError(null);
    setSuccess(null);
  }, []);

  return {
    user,
    updateProfile: handleUpdateProfile,
    changePassword: handleChangePassword,
    refreshProfile: refreshUserProfile,
    isUpdating,
    isChangingPassword,
    error,
    success,
    clearMessages
  };
};