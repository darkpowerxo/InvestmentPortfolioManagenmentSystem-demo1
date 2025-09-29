import axios, { AxiosResponse } from 'axios';
import {
  ApiResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  AuthUser,
  PasswordResetRequest,
  PasswordResetConfirm,
  ChangePasswordRequest,
  UpdateProfileRequest
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5189/api';

// Create a separate axios instance for auth to avoid circular dependencies
const authClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Token management utilities
export const tokenManager = {
  getToken: (): string | null => {
    return localStorage.getItem('auth_token');
  },

  getRefreshToken: (): string | null => {
    return localStorage.getItem('refresh_token');
  },

  setTokens: (token: string, refreshToken: string): void => {
    localStorage.setItem('auth_token', token);
    localStorage.setItem('refresh_token', refreshToken);
  },

  clearTokens: (): void => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_data');
  },

  setUser: (user: AuthUser): void => {
    localStorage.setItem('user_data', JSON.stringify(user));
  },

  getUser: (): AuthUser | null => {
    const userData = localStorage.getItem('user_data');
    return userData ? JSON.parse(userData) : null;
  },

  isTokenExpired: (token: string): boolean => {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
};

// Add request interceptor to include auth token
authClient.interceptors.request.use(
  (config) => {
    const token = tokenManager.getToken();
    if (token && !tokenManager.isTokenExpired(token)) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Add language header based on user preferences
    const user = tokenManager.getUser();
    if (user?.preferences?.language) {
      config.headers['Accept-Language'] = user.preferences.language;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle token refresh
authClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = tokenManager.getRefreshToken();
        if (refreshToken) {
          const response = await authClient.post('/auth/refresh', {
            refreshToken
          });

          const { token, refreshToken: newRefreshToken } = response.data.data;
          tokenManager.setTokens(token, newRefreshToken);

          originalRequest.headers.Authorization = `Bearer ${token}`;
          return authClient(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, logout user
        tokenManager.clearTokens();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export class AuthService {
  /**
   * Authenticate user with email and password
   */
  static async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    try {
      const response: AxiosResponse<ApiResponse<LoginResponse>> = await authClient.post(
        '/auth/login',
        credentials
      );

      if (response.data.success && response.data.data) {
        const { token, refreshToken, user } = response.data.data;
        tokenManager.setTokens(token, refreshToken);
        tokenManager.setUser(user);
      }

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Login failed',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Register a new user
   */
  static async register(userData: RegisterRequest): Promise<ApiResponse<AuthUser>> {
    try {
      const response: AxiosResponse<ApiResponse<AuthUser>> = await authClient.post(
        '/auth/register',
        userData
      );

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Registration failed',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Logout current user
   */
  static async logout(): Promise<void> {
    try {
      const refreshToken = tokenManager.getRefreshToken();
      if (refreshToken) {
        await authClient.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      // Even if logout fails on server, clear local tokens
      console.warn('Logout request failed:', error);
    } finally {
      tokenManager.clearTokens();
    }
  }

  /**
   * Refresh authentication token
   */
  static async refreshToken(): Promise<ApiResponse<{ token: string; refreshToken: string }>> {
    try {
      const refreshToken = tokenManager.getRefreshToken();
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response: AxiosResponse<ApiResponse<{ token: string; refreshToken: string }>> = 
        await authClient.post('/auth/refresh', { refreshToken });

      if (response.data.success && response.data.data) {
        const { token, refreshToken: newRefreshToken } = response.data.data;
        tokenManager.setTokens(token, newRefreshToken);
      }

      return response.data;
    } catch (error: any) {
      tokenManager.clearTokens();
      return {
        success: false,
        message: error.response?.data?.message || 'Token refresh failed',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Get current user profile
   */
  static async getProfile(): Promise<ApiResponse<AuthUser>> {
    try {
      const response: AxiosResponse<ApiResponse<AuthUser>> = await authClient.get('/auth/profile');
      
      if (response.data.success && response.data.data) {
        tokenManager.setUser(response.data.data);
      }

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to get profile',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Update user profile
   */
  static async updateProfile(profileData: UpdateProfileRequest): Promise<ApiResponse<AuthUser>> {
    try {
      const response: AxiosResponse<ApiResponse<AuthUser>> = await authClient.put(
        '/auth/profile',
        profileData
      );

      if (response.data.success && response.data.data) {
        tokenManager.setUser(response.data.data);
      }

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to update profile',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Change user password
   */
  static async changePassword(passwordData: ChangePasswordRequest): Promise<ApiResponse<void>> {
    try {
      const response: AxiosResponse<ApiResponse<void>> = await authClient.post(
        '/auth/change-password',
        passwordData
      );

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to change password',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Request password reset
   */
  static async requestPasswordReset(resetData: PasswordResetRequest): Promise<ApiResponse<void>> {
    try {
      const response: AxiosResponse<ApiResponse<void>> = await authClient.post(
        '/auth/forgot-password',
        resetData
      );

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to request password reset',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Confirm password reset with token
   */
  static async confirmPasswordReset(resetData: PasswordResetConfirm): Promise<ApiResponse<void>> {
    try {
      const response: AxiosResponse<ApiResponse<void>> = await authClient.post(
        '/auth/reset-password',
        resetData
      );

      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to reset password',
        statusCode: error.response?.status || 500
      };
    }
  }

  /**
   * Validate current authentication status
   */
  static isAuthenticated(): boolean {
    const token = tokenManager.getToken();
    const user = tokenManager.getUser();
    
    return !!(token && user && !tokenManager.isTokenExpired(token));
  }

  /**
   * Get current authenticated user
   */
  static getCurrentUser(): AuthUser | null {
    return this.isAuthenticated() ? tokenManager.getUser() : null;
  }

  /**
   * Check if user has specific role
   */
  static hasRole(requiredRole: string): boolean {
    const user = this.getCurrentUser();
    return user?.role === requiredRole;
  }

  /**
   * Check if user has any of the specified roles
   */
  static hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    return user ? roles.includes(user.role) : false;
  }
}

export default AuthService;