using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;

/// <summary>
/// Interface de base pour tous les dépôts / Base interface for all repositories
/// </summary>
/// <typeparam name="T">Type d'entité / Entity type</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<int> CountAsync();
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Interface pour le dépôt des utilisateurs / Interface for user repository
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

/// <summary>
/// Interface pour le dépôt des portefeuilles / Interface for portfolio repository
/// </summary>
public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<IEnumerable<Portfolio>> GetPortfoliosByManagerIdAsync(int managerId);
    Task<IEnumerable<Portfolio>> GetPortfoliosByTypeAsync(Domain.Enums.PortfolioType portfolioType);
    Task<IEnumerable<Portfolio>> GetActivePortfoliosAsync();
    Task<Portfolio?> GetPortfolioWithPositionsAsync(int portfolioId);
    Task<Portfolio?> GetPortfolioWithTransactionsAsync(int portfolioId);
    Task<decimal> GetTotalAssetsUnderManagementAsync();
    Task<IEnumerable<Portfolio>> GetPortfoliosByRiskLevelAsync(Domain.Enums.RiskLevel riskLevel);
}

/// <summary>
/// Interface pour le dépôt des titres / Interface for security repository
/// </summary>
public interface ISecurityRepository : IRepository<Security>
{
    Task<Security?> GetBySymbolAsync(string symbol);
    Task<IEnumerable<Security>> GetSecuritiesByTypeAsync(Domain.Enums.SecurityType securityType);
    Task<IEnumerable<Security>> GetSecuritiesBySectorAsync(string sector);
    Task<IEnumerable<Security>> GetSecuritiesByExchangeAsync(string exchange);
    Task<IEnumerable<Security>> GetActiveSecuritiesAsync();
    Task<IEnumerable<Security>> SearchSecuritiesAsync(string searchTerm);
    Task<IEnumerable<Security>> GetSecuritiesByCountryAsync(string country);
    Task<IEnumerable<Security>> GetSecuritiesByCurrencyAsync(string currency);
}

/// <summary>
/// Interface pour le dépôt des positions / Interface for position repository
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    Task<IEnumerable<Position>> GetPositionsByPortfolioIdAsync(int portfolioId);
    Task<IEnumerable<Position>> GetPositionsBySecurityIdAsync(int securityId);
    Task<Position?> GetPositionAsync(int portfolioId, int securityId);
    Task<IEnumerable<Position>> GetActivePositionsAsync();
    Task<decimal> GetTotalPositionValueAsync(int portfolioId);
    Task<IEnumerable<Position>> GetPositionsAboveThresholdAsync(decimal minValue);
    Task<IEnumerable<Position>> GetConcentratedPositionsAsync(int portfolioId, decimal concentrationThreshold);
}

/// <summary>
/// Interface pour le dépôt des transactions / Interface for transaction repository
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetTransactionsByPortfolioIdAsync(int portfolioId);
    Task<IEnumerable<Transaction>> GetTransactionsBySecurityIdAsync(int securityId);
    Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(Domain.Enums.TransactionType transactionType);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetTransactionsByPortfolioAndDateRangeAsync(int portfolioId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalTransactionVolumeAsync(int portfolioId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetDividendTransactionsAsync(int portfolioId);
    Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int portfolioId, int count = 10);
}

/// <summary>
/// Interface pour le dépôt des données de marché / Interface for market data repository
/// </summary>
public interface IMarketDataRepository : IRepository<MarketData>
{
    Task<IEnumerable<MarketData>> GetMarketDataBySecurityIdAsync(int securityId);
    Task<IEnumerable<MarketData>> GetMarketDataByDateRangeAsync(int securityId, DateTime startDate, DateTime endDate);
    Task<MarketData?> GetLatestMarketDataAsync(int securityId);
    Task<IEnumerable<MarketData>> GetLatestMarketDataForSecuritiesAsync(IEnumerable<int> securityIds);
    Task<decimal?> GetLatestPriceAsync(int securityId);
    Task<IEnumerable<MarketData>> GetHistoricalDataAsync(int securityId, int days);
    Task<Dictionary<int, decimal?>> GetLatestPricesAsync(IEnumerable<int> securityIds);
    Task<IEnumerable<MarketData>> GetDailyDataAsync(int securityId, DateTime date);
}

/// <summary>
/// Interface pour le dépôt des métriques de performance / Interface for performance metrics repository
/// </summary>
public interface IPerformanceMetricRepository : IRepository<PerformanceMetric>
{
    Task<IEnumerable<PerformanceMetric>> GetMetricsByPortfolioIdAsync(int portfolioId);
    Task<IEnumerable<PerformanceMetric>> GetMetricsByDateRangeAsync(int portfolioId, DateTime startDate, DateTime endDate);
    Task<PerformanceMetric?> GetLatestMetricsAsync(int portfolioId);
    Task<IEnumerable<PerformanceMetric>> GetMetricsByTypeAsync(string metricType);
    Task<decimal?> GetLatestReturnAsync(int portfolioId);
    Task<IEnumerable<PerformanceMetric>> GetBenchmarkMetricsAsync(int portfolioId);
}

/// <summary>
/// Interface pour le dépôt des ordres de transaction / Interface for trade order repository
/// </summary>
public interface ITradeOrderRepository : IRepository<TradeOrder>
{
    Task<IEnumerable<TradeOrder>> GetOrdersByPortfolioIdAsync(int portfolioId);
    Task<IEnumerable<TradeOrder>> GetOrdersByStatusAsync(Domain.Enums.OrderStatus status);
    Task<IEnumerable<TradeOrder>> GetPendingOrdersAsync();
    Task<IEnumerable<TradeOrder>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TradeOrder>> GetOrdersBySecurityIdAsync(int securityId);
    Task<TradeOrder?> GetOrderWithDetailsAsync(int orderId);
    Task<IEnumerable<TradeOrder>> GetRecentOrdersAsync(int portfolioId, int count = 10);
    Task<decimal> GetTotalOrderValueAsync(int portfolioId, Domain.Enums.OrderStatus status);
}

/// <summary>
/// Interface pour l'unité de travail / Interface for unit of work pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPortfolioRepository Portfolios { get; }
    ISecurityRepository Securities { get; }
    IPositionRepository Positions { get; }
    ITransactionRepository Transactions { get; }
    IMarketDataRepository MarketData { get; }
    IPerformanceMetricRepository PerformanceMetrics { get; }
    ITradeOrderRepository TradeOrders { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}