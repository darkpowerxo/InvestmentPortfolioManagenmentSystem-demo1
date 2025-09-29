using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportPortfolioDataAsync(int portfolioId, DateTime asOfDate);
        Task<byte[]> ExportTransactionsAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportPerformanceMetricsAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportPositionsAsync(int portfolioId, DateTime asOfDate);
        Task<byte[]> ExportMultiPortfolioSummaryAsync(IEnumerable<int> portfolioIds, DateTime asOfDate);
        Task<byte[]> ExportRiskAnalysisAsync(int portfolioId, DateTime asOfDate);
    }
}