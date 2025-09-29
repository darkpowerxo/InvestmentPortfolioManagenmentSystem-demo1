using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Application.Services
{
    public class PerformanceMetricsService : IPerformanceMetricsService
    {
        private readonly PortfolioDbContext _context;

        public PerformanceMetricsService(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<PerformanceMetric> CreateAsync(PerformanceMetric metrics)
        {
            _context.PerformanceMetrics.Add(metrics);
            await _context.SaveChangesAsync();
            return metrics;
        }

        public async Task<PerformanceMetric?> GetByIdAsync(int id)
        {
            return await _context.PerformanceMetrics
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PerformanceMetric>> GetAllAsync()
        {
            return await _context.PerformanceMetrics
                .Include(p => p.Portfolio)
                .OrderByDescending(p => p.CalculationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceMetric>> GetByPortfolioIdAsync(int portfolioId)
        {
            return await _context.PerformanceMetrics
                .Where(p => p.PortfolioId == portfolioId)
                .OrderByDescending(p => p.CalculationDate)
                .ToListAsync();
        }

        public async Task<PerformanceMetric?> GetLatestByPortfolioIdAsync(int portfolioId)
        {
            return await _context.PerformanceMetrics
                .Where(p => p.PortfolioId == portfolioId)
                .OrderByDescending(p => p.CalculationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PerformanceMetric>> GetByDateRangeAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            return await _context.PerformanceMetrics
                .Where(p => p.PortfolioId == portfolioId &&
                           p.CalculationDate >= startDate &&
                           p.CalculationDate <= endDate)
                .OrderBy(p => p.CalculationDate)
                .ToListAsync();
        }

        public async Task<PerformanceMetric> UpdateAsync(PerformanceMetric metrics)
        {
            _context.PerformanceMetrics.Update(metrics);
            await _context.SaveChangesAsync();
            return metrics;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var metrics = await _context.PerformanceMetrics.FindAsync(id);
            if (metrics == null)
                return false;

            _context.PerformanceMetrics.Remove(metrics);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PerformanceMetrics.AnyAsync(p => p.Id == id);
        }

        public async Task<PerformanceMetric?> GetPerformanceMetricsAsync(int portfolioId, DateTime asOfDate)
        {
            return await _context.PerformanceMetrics
                .Where(p => p.PortfolioId == portfolioId && p.CalculationDate <= asOfDate)
                .OrderByDescending(p => p.CalculationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PerformanceMetric>> GetPerformanceHistoryAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            return await _context.PerformanceMetrics
                .Where(p => p.PortfolioId == portfolioId &&
                           p.CalculationDate >= startDate &&
                           p.CalculationDate <= endDate)
                .OrderBy(p => p.CalculationDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateMonthlyReturnAsync(int portfolioId, DateTime month)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var startMetrics = await GetPerformanceMetricsAsync(portfolioId, startOfMonth);
            var endMetrics = await GetPerformanceMetricsAsync(portfolioId, endOfMonth);

            if (startMetrics == null || endMetrics == null)
                return 0;

            var startValue = startMetrics.TotalReturn + 1;
            var endValue = endMetrics.TotalReturn + 1;

            return startValue != 0 ? (endValue - startValue) / startValue : 0;
        }

        public async Task<decimal> CalculateYearToDateReturnAsync(int portfolioId, DateTime asOfDate)
        {
            var startOfYear = new DateTime(asOfDate.Year, 1, 1);
            
            var startMetrics = await GetPerformanceMetricsAsync(portfolioId, startOfYear);
            var currentMetrics = await GetPerformanceMetricsAsync(portfolioId, asOfDate);

            if (startMetrics == null || currentMetrics == null)
                return 0;

            var startValue = startMetrics.TotalReturn + 1;
            var currentValue = currentMetrics.TotalReturn + 1;

            return startValue != 0 ? (currentValue - startValue) / startValue : 0;
        }

        public async Task<decimal> CalculateVolatilityAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var performanceHistory = await GetPerformanceHistoryAsync(portfolioId, startDate, endDate);
            var returns = performanceHistory.Select(p => p.MonthlyReturn).ToList();

            if (returns.Count < 2)
                return 0;

            var mean = returns.Average();
            var variance = returns.Sum(r => (r - mean) * (r - mean)) / (returns.Count - 1);
            
            return (decimal)Math.Sqrt((double)variance);
        }

        public async Task<decimal> CalculateSharpeRatioAsync(int portfolioId, DateTime startDate, DateTime endDate, decimal riskFreeRate = 0.02m)
        {
            var performanceHistory = await GetPerformanceHistoryAsync(portfolioId, startDate, endDate);
            
            if (!performanceHistory.Any())
                return 0;

            var totalReturn = performanceHistory.OrderByDescending(p => p.CalculationDate).First().TotalReturn;
            var volatility = await CalculateVolatilityAsync(portfolioId, startDate, endDate);

            return volatility != 0 ? (totalReturn - riskFreeRate) / volatility : 0;
        }

        public async Task<decimal> CalculateMaxDrawdownAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var performanceHistory = await GetPerformanceHistoryAsync(portfolioId, startDate, endDate);
            
            if (!performanceHistory.Any())
                return 0;

            var peak = 0m;
            var maxDrawdown = 0m;

            foreach (var metrics in performanceHistory.OrderBy(p => p.CalculationDate))
            {
                var currentValue = 1 + metrics.TotalReturn;
                
                if (currentValue > peak)
                    peak = currentValue;
                
                var drawdown = (peak - currentValue) / peak;
                
                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }

            return maxDrawdown;
        }

        public async Task<PerformanceMetric> CalculatePerformanceMetricsAsync(int portfolioId, DateTime calculationDate)
        {
            // This would typically involve complex calculations based on portfolio positions and transactions
            // For now, we'll create a basic implementation
            
            var portfolio = await _context.Portfolios
                .Include(p => p.Positions)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);

            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var totalValue = portfolio.CurrentValue;
            var costBasis = portfolio.Positions.Sum(p => p.Quantity * p.AverageCost);
            var totalReturn = costBasis != 0 ? (totalValue - costBasis) / costBasis : 0;

            // Calculate monthly return (simplified - would need previous month's data)
            var previousMonth = calculationDate.AddMonths(-1);
            var previousMetrics = await GetPerformanceMetricsAsync(portfolioId, previousMonth);
            var monthlyReturn = 0m;
            
            if (previousMetrics != null)
            {
                var previousValue = 1 + previousMetrics.TotalReturn;
                var currentValue = 1 + totalReturn;
                monthlyReturn = previousValue != 0 ? (currentValue - previousValue) / previousValue : 0;
            }

            var metrics = new PerformanceMetric
            {
                PortfolioId = portfolioId,
                CalculationDate = calculationDate,
                TotalReturn = totalReturn,
                MonthlyReturn = monthlyReturn,
                YearToDateReturn = await CalculateYearToDateReturnAsync(portfolioId, calculationDate),
                OneYearReturn = totalReturn, // Simplified
                Volatility = await CalculateVolatilityAsync(portfolioId, calculationDate.AddYears(-1), calculationDate),
                SharpeRatio = await CalculateSharpeRatioAsync(portfolioId, calculationDate.AddYears(-1), calculationDate),
                MaxDrawdown = await CalculateMaxDrawdownAsync(portfolioId, calculationDate.AddYears(-1), calculationDate),
                Alpha = 0, // Placeholder - would need benchmark data
                Beta = 1, // Placeholder - would need benchmark data
                SortinoRatio = 0, // Placeholder - would need downside deviation calculation
                VaR95 = totalValue * -0.05m, // Simplified 5% VaR
                VaR99 = totalValue * -0.10m, // Simplified 10% VaR
                CreatedAt = DateTime.UtcNow
            };

            return await CreateAsync(metrics);
        }
    }
}