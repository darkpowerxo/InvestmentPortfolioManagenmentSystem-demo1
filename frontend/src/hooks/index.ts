import { useState, useEffect } from 'react';
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
  SearchResult 
} from '../types';

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