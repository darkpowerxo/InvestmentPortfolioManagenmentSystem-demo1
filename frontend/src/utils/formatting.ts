import { format, parseISO } from 'date-fns';

// Currency formatting
export const formatCurrency = (amount: number, currency = 'CAD'): string => {
  return new Intl.NumberFormat('en-CA', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
};

// Large number formatting (for portfolio values)
export const formatLargeNumber = (num: number): string => {
  if (num >= 1000000000) {
    return `$${(num / 1000000000).toFixed(1)}B`;
  } else if (num >= 1000000) {
    return `$${(num / 1000000).toFixed(1)}M`;
  } else if (num >= 1000) {
    return `$${(num / 1000).toFixed(0)}K`;
  }
  return formatCurrency(num);
};

// Percentage formatting
export const formatPercentage = (value: number, decimals = 2): string => {
  return `${value.toFixed(decimals)}%`;
};

// Number formatting with thousand separators
export const formatNumber = (num: number, decimals = 0): string => {
  return new Intl.NumberFormat('en-CA', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(num);
};

// Date formatting
export const formatDate = (dateString: string): string => {
  return format(parseISO(dateString), 'MMM dd, yyyy');
};

export const formatDateTime = (dateString: string): string => {
  return format(parseISO(dateString), 'MMM dd, yyyy HH:mm');
};

export const formatTime = (dateString: string): string => {
  return format(parseISO(dateString), 'HH:mm:ss');
};

// Color utilities for gains/losses
export const getGainLossColor = (value: number): string => {
  if (value > 0) return '#4caf50'; // Green for gains
  if (value < 0) return '#f44336'; // Red for losses
  return '#757575'; // Gray for neutral
};

// Risk level colors
export const getRiskLevelColor = (riskLevel: string): string => {
  switch (riskLevel.toLowerCase()) {
    case 'conservative': return '#4caf50';
    case 'moderate': return '#ff9800';
    case 'aggressive': return '#f44336';
    case 'veryaggressive': return '#d32f2f';
    default: return '#757575';
  }
};

// Portfolio type colors
export const getPortfolioTypeColor = (type: string): string => {
  switch (type.toLowerCase()) {
    case 'equity': return '#2196f3';
    case 'fixedincome': return '#4caf50';
    case 'balanced': return '#ff9800';
    case 'alternative': return '#9c27b0';
    case 'moneymarket': return '#607d8b';
    default: return '#757575';
  }
};

// Security type colors
export const getSecurityTypeColor = (type: string): string => {
  switch (type.toLowerCase()) {
    case 'stock': return '#2196f3';
    case 'bond': return '#4caf50';
    case 'etf': return '#ff9800';
    case 'mutualfund': return '#9c27b0';
    case 'option': return '#f44336';
    case 'future': return '#795548';
    case 'currency': return '#607d8b';
    case 'commodity': return '#ffc107';
    default: return '#757575';
  }
};

// Calculate portfolio allocation percentages
export const calculateAllocation = (positions: any[], totalValue: number) => {
  if (!positions || totalValue === 0) return [];
  
  return positions.map(position => ({
    ...position,
    allocation: (position.marketValue / totalValue) * 100
  }));
};

// Sort data by various criteria
export const sortBy = <T>(data: T[], key: keyof T, direction: 'asc' | 'desc' = 'asc'): T[] => {
  return [...data].sort((a, b) => {
    const aVal = a[key];
    const bVal = b[key];
    
    if (aVal === bVal) return 0;
    
    const result = aVal < bVal ? -1 : 1;
    return direction === 'asc' ? result : -result;
  });
};

// Filter data by search term
export const filterBySearch = <T>(data: T[], searchTerm: string, keys: (keyof T)[]): T[] => {
  if (!searchTerm) return data;
  
  const lowercaseSearch = searchTerm.toLowerCase();
  return data.filter(item =>
    keys.some(key => {
      const value = item[key];
      return value && String(value).toLowerCase().includes(lowercaseSearch);
    })
  );
};

// Generate random color for charts
export const generateColors = (count: number): string[] => {
  const baseColors = [
    '#2196f3', '#4caf50', '#ff9800', '#f44336', '#9c27b0',
    '#607d8b', '#795548', '#ffc107', '#e91e63', '#00bcd4'
  ];
  
  const colors = [];
  for (let i = 0; i < count; i++) {
    colors.push(baseColors[i % baseColors.length]);
  }
  return colors;
};

// Debounce function for search inputs
export const debounce = <T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void => {
  let timeout: NodeJS.Timeout;
  return (...args: Parameters<T>) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
};

// Validate email
export const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

// Calculate days between dates
export const daysBetween = (date1: string, date2: string): number => {
  const d1 = parseISO(date1);
  const d2 = parseISO(date2);
  const diffTime = Math.abs(d2.getTime() - d1.getTime());
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
};