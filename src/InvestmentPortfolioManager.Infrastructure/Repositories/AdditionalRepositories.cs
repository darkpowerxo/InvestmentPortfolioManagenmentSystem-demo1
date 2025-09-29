using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Infrastructure.Data;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;

namespace InvestmentPortfolioManager.Infrastructure.Repositories;

/// <summary>
/// Dépôt pour les transactions / Repository for transactions
/// </summary>
public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByPortfolioIdAsync(int portfolioId)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Where(t => t.PortfolioId == portfolioId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsBySecurityIdAsync(int securityId)
    {
        return await _dbSet
            .Include(t => t.Portfolio)
            .Where(t => t.SecurityId == securityId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(Domain.Enums.TransactionType transactionType)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Include(t => t.Portfolio)
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Include(t => t.Portfolio)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByPortfolioAndDateRangeAsync(int portfolioId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Where(t => t.PortfolioId == portfolioId && 
                       t.TransactionDate >= startDate && 
                       t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalTransactionVolumeAsync(int portfolioId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.PortfolioId == portfolioId && 
                       t.TransactionDate >= startDate && 
                       t.TransactionDate <= endDate &&
                       (t.TransactionType == Domain.Enums.TransactionType.Buy || 
                        t.TransactionType == Domain.Enums.TransactionType.Sell))
            .SumAsync(t => Math.Abs(t.Quantity * t.Price));
    }

    public async Task<IEnumerable<Transaction>> GetDividendTransactionsAsync(int portfolioId)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Where(t => t.PortfolioId == portfolioId && 
                       (t.TransactionType == Domain.Enums.TransactionType.Dividend ||
                        t.TransactionType == Domain.Enums.TransactionType.Interest))
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int portfolioId, int count = 10)
    {
        return await _dbSet
            .Include(t => t.Security)
            .Where(t => t.PortfolioId == portfolioId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les données de marché / Repository for market data
/// </summary>
public class MarketDataRepository : Repository<MarketData>, IMarketDataRepository
{
    public MarketDataRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MarketData>> GetMarketDataBySecurityIdAsync(int securityId)
    {
        return await _dbSet
            .Where(md => md.SecurityId == securityId)
            .OrderByDescending(md => md.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<MarketData>> GetMarketDataByDateRangeAsync(int securityId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(md => md.SecurityId == securityId && 
                        md.Date >= startDate && 
                        md.Date <= endDate)
            .OrderBy(md => md.Date)
            .ToListAsync();
    }

    public async Task<MarketData?> GetLatestMarketDataAsync(int securityId)
    {
        return await _dbSet
            .Where(md => md.SecurityId == securityId)
            .OrderByDescending(md => md.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MarketData>> GetLatestMarketDataForSecuritiesAsync(IEnumerable<int> securityIds)
    {
        var result = new List<MarketData>();
        
        foreach (var securityId in securityIds)
        {
            var latest = await GetLatestMarketDataAsync(securityId);
            if (latest != null)
                result.Add(latest);
        }
        
        return result;
    }

    public async Task<decimal?> GetLatestPriceAsync(int securityId)
    {
        var latestData = await GetLatestMarketDataAsync(securityId);
        return latestData?.ClosePrice;
    }

    public async Task<IEnumerable<MarketData>> GetHistoricalDataAsync(int securityId, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(md => md.SecurityId == securityId && md.Date >= startDate)
            .OrderBy(md => md.Date)
            .ToListAsync();
    }

    public async Task<Dictionary<int, decimal?>> GetLatestPricesAsync(IEnumerable<int> securityIds)
    {
        var result = new Dictionary<int, decimal?>();
        
        foreach (var securityId in securityIds)
        {
            result[securityId] = await GetLatestPriceAsync(securityId);
        }
        
        return result;
    }

    public async Task<IEnumerable<MarketData>> GetDailyDataAsync(int securityId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        
        return await _dbSet
            .Where(md => md.SecurityId == securityId && 
                        md.Date >= startOfDay && 
                        md.Date < endOfDay)
            .OrderBy(md => md.Date)
            .ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les métriques de performance / Repository for performance metrics
/// </summary>
public class PerformanceMetricRepository : Repository<PerformanceMetric>, IPerformanceMetricRepository
{
    public PerformanceMetricRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByPortfolioIdAsync(int portfolioId)
    {
        return await _dbSet
            .Where(pm => pm.PortfolioId == portfolioId)
            .OrderByDescending(pm => pm.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByDateRangeAsync(int portfolioId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(pm => pm.PortfolioId == portfolioId && 
                        pm.Date >= startDate && 
                        pm.Date <= endDate)
            .OrderBy(pm => pm.Date)
            .ToListAsync();
    }

    public async Task<PerformanceMetric?> GetLatestMetricsAsync(int portfolioId)
    {
        return await _dbSet
            .Where(pm => pm.PortfolioId == portfolioId)
            .OrderByDescending(pm => pm.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByTypeAsync(string metricType)
    {
        return await _dbSet
            .Where(pm => pm.MetricType == metricType)
            .OrderByDescending(pm => pm.Date)
            .ToListAsync();
    }

    public async Task<decimal?> GetLatestReturnAsync(int portfolioId)
    {
        var latest = await GetLatestMetricsAsync(portfolioId);
        return latest?.Value;
    }

    public async Task<IEnumerable<PerformanceMetric>> GetBenchmarkMetricsAsync(int portfolioId)
    {
        return await _dbSet
            .Where(pm => pm.PortfolioId == portfolioId && pm.MetricType.Contains("Benchmark"))
            .OrderByDescending(pm => pm.Date)
            .ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les ordres de transaction / Repository for trade orders
/// </summary>
public class TradeOrderRepository : Repository<TradeOrder>, ITradeOrderRepository
{
    public TradeOrderRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TradeOrder>> GetOrdersByPortfolioIdAsync(int portfolioId)
    {
        return await _dbSet
            .Include(o => o.Security)
            .Where(o => o.PortfolioId == portfolioId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TradeOrder>> GetOrdersByStatusAsync(Domain.Enums.OrderStatus status)
    {
        return await _dbSet
            .Include(o => o.Security)
            .Include(o => o.Portfolio)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TradeOrder>> GetPendingOrdersAsync()
    {
        return await GetOrdersByStatusAsync(Domain.Enums.OrderStatus.Pending);
    }

    public async Task<IEnumerable<TradeOrder>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(o => o.Security)
            .Include(o => o.Portfolio)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TradeOrder>> GetOrdersBySecurityIdAsync(int securityId)
    {
        return await _dbSet
            .Include(o => o.Portfolio)
            .Where(o => o.SecurityId == securityId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<TradeOrder?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Security)
            .Include(o => o.Portfolio)
            .ThenInclude(p => p.Manager)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<TradeOrder>> GetRecentOrdersAsync(int portfolioId, int count = 10)
    {
        return await _dbSet
            .Include(o => o.Security)
            .Where(o => o.PortfolioId == portfolioId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalOrderValueAsync(int portfolioId, Domain.Enums.OrderStatus status)
    {
        return await _dbSet
            .Where(o => o.PortfolioId == portfolioId && o.Status == status)
            .SumAsync(o => o.Quantity * (o.LimitPrice ?? o.EstimatedPrice ?? 0));
    }
}