import axios from 'axios';
import { 
  ApiResponse, 
  SearchResult, 
  User, 
  Portfolio, 
  Security, 
  Position, 
  Transaction,
  MarketData,
  PortfolioSummary,
  CreatePortfolioRequest,
  UpdatePortfolioRequest,
  CreatePositionRequest,
  CreateTransactionRequest
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7001/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Users API
export const usersApi = {
  getUsers: (searchTerm?: string, role?: string, pageNumber = 1, pageSize = 20) => 
    apiClient.get<ApiResponse<SearchResult<User>>>('/users', {
      params: { searchTerm, role, pageNumber, pageSize }
    }),
  
  getUser: (id: number) => 
    apiClient.get<ApiResponse<User>>(`/users/${id}`),
  
  createUser: (user: Omit<User, 'id' | 'createdAt' | 'lastLoginAt'>) => 
    apiClient.post<ApiResponse<User>>('/users', user),
  
  updateUser: (id: number, user: Partial<User>) => 
    apiClient.put<ApiResponse<User>>(`/users/${id}`, user),
  
  deleteUser: (id: number) => 
    apiClient.delete<ApiResponse<object>>(`/users/${id}`)
};

// Portfolios API
export const portfoliosApi = {
  getPortfolios: (searchTerm?: string, managerId?: number, type?: string, pageNumber = 1, pageSize = 20) => 
    apiClient.get<ApiResponse<SearchResult<Portfolio>>>('/portfolios', {
      params: { searchTerm, managerId, type, pageNumber, pageSize }
    }),
  
  getPortfolio: (id: number) => 
    apiClient.get<ApiResponse<Portfolio>>(`/portfolios/${id}`),
  
  getPortfolioSummary: (id: number) => 
    apiClient.get<ApiResponse<PortfolioSummary>>(`/portfolios/${id}/summary`),
  
  getPortfoliosByManager: (managerId: number) => 
    apiClient.get<ApiResponse<Portfolio[]>>(`/portfolios/by-manager/${managerId}`),
  
  getPortfoliosByType: (type: string) => 
    apiClient.get<ApiResponse<Portfolio[]>>(`/portfolios/by-type/${type}`),
  
  getLargePortfolios: (threshold = 1000000) => 
    apiClient.get<ApiResponse<Portfolio[]>>('/portfolios/large', {
      params: { threshold }
    }),
  
  getPortfoliosWithLowCash: (threshold = 10000) => 
    apiClient.get<ApiResponse<Portfolio[]>>('/portfolios/low-cash', {
      params: { threshold }
    }),
  
  createPortfolio: (portfolio: CreatePortfolioRequest) => 
    apiClient.post<ApiResponse<Portfolio>>('/portfolios', portfolio),
  
  updatePortfolio: (id: number, portfolio: UpdatePortfolioRequest) => 
    apiClient.put<ApiResponse<Portfolio>>(`/portfolios/${id}`, portfolio),
  
  deletePortfolio: (id: number) => 
    apiClient.delete<ApiResponse<object>>(`/portfolios/${id}`)
};

// Securities API
export const securitiesApi = {
  getSecurities: (searchTerm?: string, type?: string, exchange?: string, sector?: string, pageNumber = 1, pageSize = 20) => 
    apiClient.get<ApiResponse<SearchResult<Security>>>('/securities', {
      params: { searchTerm, type, exchange, sector, pageNumber, pageSize }
    }),
  
  getSecurity: (id: number) => 
    apiClient.get<ApiResponse<Security>>(`/securities/${id}`),
  
  getSecuritiesByExchange: (exchange: string) => 
    apiClient.get<ApiResponse<Security[]>>(`/securities/by-exchange/${exchange}`),
  
  getSecuritiesBySector: (sector: string) => 
    apiClient.get<ApiResponse<Security[]>>(`/securities/by-sector/${sector}`),
  
  getSecuritiesByType: (type: string) => 
    apiClient.get<ApiResponse<Security[]>>(`/securities/by-type/${type}`),
  
  getHighPricedSecurities: (threshold = 100) => 
    apiClient.get<ApiResponse<Security[]>>('/securities/high-priced', {
      params: { threshold }
    }),
  
  createSecurity: (security: Omit<Security, 'id' | 'lastUpdated'>) => 
    apiClient.post<ApiResponse<Security>>('/securities', security),
  
  updateSecurity: (id: number, security: Partial<Security>) => 
    apiClient.put<ApiResponse<Security>>(`/securities/${id}`, security),
  
  deleteSecurity: (id: number) => 
    apiClient.delete<ApiResponse<object>>(`/securities/${id}`)
};

// Positions API
export const positionsApi = {
  getPositions: (portfolioId?: number, securityId?: number, pageNumber = 1, pageSize = 20) => 
    apiClient.get<ApiResponse<SearchResult<Position>>>('/positions', {
      params: { portfolioId, securityId, pageNumber, pageSize }
    }),
  
  getPosition: (id: number) => 
    apiClient.get<ApiResponse<Position>>(`/positions/${id}`),
  
  getPositionsByPortfolio: (portfolioId: number) => 
    apiClient.get<ApiResponse<Position[]>>(`/positions/by-portfolio/${portfolioId}`),
  
  getPositionsBySecurity: (securityId: number) => 
    apiClient.get<ApiResponse<Position[]>>(`/positions/by-security/${securityId}`),
  
  getTopPositions: (count = 10) => 
    apiClient.get<ApiResponse<Position[]>>('/positions/top', {
      params: { count }
    }),
  
  createPosition: (position: CreatePositionRequest) => 
    apiClient.post<ApiResponse<Position>>('/positions', position),
  
  updatePosition: (id: number, position: Partial<Position>) => 
    apiClient.put<ApiResponse<Position>>(`/positions/${id}`, position),
  
  deletePosition: (id: number) => 
    apiClient.delete<ApiResponse<object>>(`/positions/${id}`)
};

// Transactions API
export const transactionsApi = {
  getTransactions: (portfolioId?: number, securityId?: number, type?: string, pageNumber = 1, pageSize = 20) => 
    apiClient.get<ApiResponse<SearchResult<Transaction>>>('/transactions', {
      params: { portfolioId, securityId, type, pageNumber, pageSize }
    }),
  
  getTransaction: (id: number) => 
    apiClient.get<ApiResponse<Transaction>>(`/transactions/${id}`),
  
  getTransactionsByPortfolio: (portfolioId: number) => 
    apiClient.get<ApiResponse<Transaction[]>>(`/transactions/by-portfolio/${portfolioId}`),
  
  getTransactionsBySecurity: (securityId: number) => 
    apiClient.get<ApiResponse<Transaction[]>>(`/transactions/by-security/${securityId}`),
  
  getTransactionsByType: (type: string) => 
    apiClient.get<ApiResponse<Transaction[]>>(`/transactions/by-type/${type}`),
  
  getTransactionsByDateRange: (startDate: string, endDate: string) => 
    apiClient.get<ApiResponse<Transaction[]>>('/transactions/by-date-range', {
      params: { startDate, endDate }
    }),
  
  getLargeTransactions: (threshold = 100000) => 
    apiClient.get<ApiResponse<Transaction[]>>('/transactions/large', {
      params: { threshold }
    }),
  
  getRecentTransactions: (days = 30, portfolioId?: number) => 
    apiClient.get<ApiResponse<Transaction[]>>('/transactions/recent', {
      params: { days, portfolioId }
    }),
  
  createTransaction: (transaction: CreateTransactionRequest) => 
    apiClient.post<ApiResponse<Transaction>>('/transactions', transaction),
  
  updateTransaction: (id: number, transaction: Partial<Transaction>) => 
    apiClient.put<ApiResponse<Transaction>>(`/transactions/${id}`, transaction),
  
  deleteTransaction: (id: number) => 
    apiClient.delete<ApiResponse<object>>(`/transactions/${id}`)
};

// Market Data API
export const marketDataApi = {
  getLatestPrices: (symbols: string[]) => 
    apiClient.get<ApiResponse<MarketData[]>>('/marketdata/latest', {
      params: { symbols: symbols.join(',') }
    }),
  
  getHistoricalData: (symbol: string, days: number) => 
    apiClient.get<ApiResponse<MarketData[]>>(`/marketdata/${symbol}/history`, {
      params: { days }
    }),
  
  getCurrentPrice: (symbol: string) => 
    apiClient.get<ApiResponse<number>>(`/marketdata/${symbol}/price`)
};

export default apiClient;