using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public interface IPdfReportService
    {
        Task<byte[]> GeneratePortfolioStatementAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<byte[]> GeneratePerformanceReportAsync(int portfolioId, DateTime asOfDate);
        Task<byte[]> GenerateTransactionSummaryAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateRiskAnalysisReportAsync(int portfolioId, DateTime asOfDate);
        Task<byte[]> GenerateMultiPortfolioSummaryAsync(IEnumerable<int> portfolioIds, DateTime asOfDate);
    }
}