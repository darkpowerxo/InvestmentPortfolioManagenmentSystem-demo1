using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Contracts
{
    public interface IPerformanceMetricsService
    {
        Task<PerformanceMetric?> GetPerformanceMetricsAsync(int portfolioId, DateTime calculationDate);
        Task<IEnumerable<PerformanceMetric>> GetPerformanceHistoryAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<PerformanceMetric> CalculatePerformanceMetricsAsync(int portfolioId, DateTime calculationDate);
        Task<PerformanceMetric> CreateAsync(PerformanceMetric metrics);
        Task<PerformanceMetric> UpdateAsync(PerformanceMetric metrics);
    }
}