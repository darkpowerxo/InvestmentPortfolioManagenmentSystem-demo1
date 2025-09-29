export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  statusCode: number;
}

export interface SearchResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  department?: string;
  isActive: boolean;
  phoneNumber?: string;
  createdAt: string;
  lastLoginAt?: string;
}

// Authentication Types
export interface AuthUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  department?: string;
  phoneNumber?: string;
  preferences?: UserPreferences;
}

export interface UserPreferences {
  language: string;
  theme: 'light' | 'dark';
  dateFormat: string;
  currencyFormat: string;
  notifications: NotificationSettings;
}

export interface NotificationSettings {
  emailNotifications: boolean;
  systemAlerts: boolean;
  portfolioUpdates: boolean;
  marketNews: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: AuthUser;
  expiresAt: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: UserRole;
  department?: string;
  phoneNumber?: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: AuthUser | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  error: string | null;
}

export interface PasswordResetRequest {
  email: string;
}

export interface PasswordResetConfirm {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  department?: string;
  preferences?: UserPreferences;
}

export enum UserRole {
  PortfolioManager = 'PortfolioManager',
  Analyst = 'Analyst',
  RiskManager = 'RiskManager',
  Trader = 'Trader',
  Administrator = 'Administrator'
}

export interface Portfolio {
  id: number;
  name: string;
  description?: string;
  managerId: number;
  managerName?: string;
  type: PortfolioType;
  riskLevel: RiskLevel;
  benchmark?: string;
  createdAt: string;
  isActive: boolean;
  initialValue: number;
  currentValue: number;
  cashBalance: number;
  currency: string;
  targetEquityAllocation: number;
  targetBondAllocation: number;
  targetAlternativeAllocation: number;
  targetCashAllocation: number;
}

export enum PortfolioType {
  Equity = 'Equity',
  FixedIncome = 'FixedIncome',
  Balanced = 'Balanced',
  Alternative = 'Alternative',
  MoneyMarket = 'MoneyMarket'
}

export enum RiskLevel {
  Conservative = 'Conservative',
  Moderate = 'Moderate',
  Aggressive = 'Aggressive',
  VeryAggressive = 'VeryAggressive'
}

export interface Security {
  id: number;
  symbol: string;
  name: string;
  type: SecurityType;
  exchange: string;
  sector?: string;
  currency: string;
  currentPrice: number;
  isActive: boolean;
  country?: string;
  lastUpdated: string;
}

export enum SecurityType {
  Stock = 'Stock',
  Bond = 'Bond',
  ETF = 'ETF',
  MutualFund = 'MutualFund',
  Option = 'Option',
  Future = 'Future',
  Currency = 'Currency',
  Commodity = 'Commodity'
}

export interface Position {
  id: number;
  portfolioId: number;
  portfolioName?: string;
  securityId: number;
  security?: Security;
  quantity: number;
  averageCost: number;
  currentPrice?: number;
  marketValue: number;
  unrealizedGainLoss: number;
  unrealizedGainLossPercent: number;
  weight?: number;
  lastUpdated: string;
}

export interface Transaction {
  id: number;
  portfolioId: number;
  portfolioName?: string;
  securityId: number;
  security?: Security;
  type: TransactionType;
  quantity: number;
  price: number;
  grossAmount: number;
  commission: number;
  tax: number;
  netAmount: number;
  executedAt: string;
  orderId?: string;
  notes?: string;
}

export enum TransactionType {
  Buy = 'Buy',
  Sell = 'Sell',
  Dividend = 'Dividend',
  Interest = 'Interest',
  Split = 'Split',
  Merger = 'Merger'
}

export interface MarketData {
  id: number;
  securityId: number;
  symbol?: string;
  price: number;
  volume: number;
  high: number;
  low: number;
  previousClose: number;
  change: number;
  changePercent: number;
  timestamp: string;
}

export interface PortfolioSummary {
  id: number;
  name: string;
  currentValue: number;
  dayChange: number;
  dayChangePercent: number;
  totalReturn: number;
  totalReturnPercent: number;
  positionCount: number;
  cashBalance: number;
  assetAllocation: AssetAllocation;
}

export interface AssetAllocation {
  equityPercent: number;
  bondPercent: number;
  alternativePercent: number;
  cashPercent: number;
}

export interface CreatePortfolioRequest {
  name: string;
  description?: string;
  managerId: number;
  type: PortfolioType;
  riskLevel: RiskLevel;
  benchmark?: string;
  initialValue: number;
  currency?: string;
}

export interface UpdatePortfolioRequest extends CreatePortfolioRequest {
  isActive: boolean;
}

export interface CreatePositionRequest {
  portfolioId: number;
  securityId: number;
  quantity: number;
  averageCost: number;
}

export interface CreateTransactionRequest {
  portfolioId: number;
  securityId: number;
  type: TransactionType;
  quantity: number;
  price: number;
  commission?: number;
  tax?: number;
  notes?: string;
}