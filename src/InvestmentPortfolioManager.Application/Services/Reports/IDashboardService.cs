using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public interface IDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(int? portfolioId = null);
        Task<PerformanceDashboard> GetPerformanceDashboardAsync(int portfolioId, int months = 12);
        Task<RiskDashboard> GetRiskDashboardAsync(int portfolioId);
        Task<AllocationDashboard> GetAllocationDashboardAsync(int portfolioId);
        Task<TransactionsDashboard> GetTransactionsDashboardAsync(int portfolioId, int days = 30);
        Task<MultiPortfolioDashboard> GetMultiPortfolioDashboardAsync(IEnumerable<int> portfolioIds);
    }

    public class DashboardSummary
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal DailyChange { get; set; }
        public decimal DailyChangePercent { get; set; }
        public int ActivePortfolios { get; set; }
        public int TotalSecurities { get; set; }
        public List<TopPerformer> TopPerformers { get; set; } = new();
        public List<TopPerformer> BottomPerformers { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
    }

    public class PerformanceDashboard
    {
        public string PortfolioName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal YearToDateReturn { get; set; }
        public decimal OneYearReturn { get; set; }
        public decimal Volatility { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
        public List<PerformanceDataPoint> PerformanceHistory { get; set; } = new();
        public List<BenchmarkComparison> BenchmarkComparisons { get; set; } = new();
        public List<ReturnBreakdown> ReturnBreakdown { get; set; } = new();
    }

    public class RiskDashboard
    {
        public string PortfolioName { get; set; } = string.Empty;
        public decimal Beta { get; set; }
        public decimal Alpha { get; set; }
        public decimal Volatility { get; set; }
        public decimal VaR95 { get; set; }
        public decimal VaR99 { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal SortinoRatio { get; set; }
        public List<RiskMetricHistory> RiskHistory { get; set; } = new();
        public List<ConcentrationRisk> ConcentrationRisks { get; set; } = new();
        public List<SectorRisk> SectorRisks { get; set; } = new();
    }

    public class AllocationDashboard
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<AssetAllocation> AssetAllocations { get; set; } = new();
        public List<SectorAllocation> SectorAllocations { get; set; } = new();
        public List<GeographicAllocation> GeographicAllocations { get; set; } = new();
        public List<AllocationTarget> AllocationTargets { get; set; } = new();
        public List<AllocationDeviation> AllocationDeviations { get; set; } = new();
    }

    public class TransactionsDashboard
    {
        public string PortfolioName { get; set; } = string.Empty;
        public decimal TotalTransactionValue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageTransactionSize { get; set; }
        public List<TransactionSummary> TransactionsByType { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        public List<DailyTransactionVolume> DailyVolume { get; set; } = new();
        public List<SecurityTransactionSummary> TopTradedSecurities { get; set; } = new();
    }

    public class MultiPortfolioDashboard
    {
        public decimal TotalValue { get; set; }
        public decimal WeightedReturn { get; set; }
        public decimal WeightedVolatility { get; set; }
        public List<PortfolioSummary> PortfolioSummaries { get; set; } = new();
        public List<AssetAllocation> ConsolidatedAllocations { get; set; } = new();
        public List<PerformanceComparison> PerformanceComparisons { get; set; } = new();
        public List<RiskComparison> RiskComparisons { get; set; } = new();
    }

    // Supporting classes
    public class TopPerformer
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Return { get; set; }
        public decimal Value { get; set; }
    }

    public class RecentTransaction
    {
        public DateTime Date { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }

    public class PerformanceDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal Return { get; set; }
        public decimal CumulativeReturn { get; set; }
    }

    public class BenchmarkComparison
    {
        public string BenchmarkName { get; set; } = string.Empty;
        public decimal PortfolioReturn { get; set; }
        public decimal BenchmarkReturn { get; set; }
        public decimal Alpha { get; set; }
        public decimal Beta { get; set; }
    }

    public class ReturnBreakdown
    {
        public string Component { get; set; } = string.Empty;
        public decimal Contribution { get; set; }
        public decimal Weight { get; set; }
    }

    public class RiskMetricHistory
    {
        public DateTime Date { get; set; }
        public decimal Volatility { get; set; }
        public decimal Beta { get; set; }
        public decimal VaR95 { get; set; }
        public decimal MaxDrawdown { get; set; }
    }

    public class ConcentrationRisk
    {
        public string Type { get; set; } = string.Empty;
        public decimal Concentration { get; set; }
        public decimal Threshold { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class SectorRisk
    {
        public string Sector { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Volatility { get; set; }
        public decimal Beta { get; set; }
        public decimal RiskContribution { get; set; }
    }

    public class AssetAllocation
    {
        public string AssetClass { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
        public decimal Target { get; set; }
        public decimal Deviation { get; set; }
    }

    public class SectorAllocation
    {
        public string Sector { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
        public int Holdings { get; set; }
    }

    public class GeographicAllocation
    {
        public string Region { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
    }

    public class AllocationTarget
    {
        public string Category { get; set; } = string.Empty;
        public decimal Target { get; set; }
        public decimal Actual { get; set; }
        public decimal Difference { get; set; }
    }

    public class AllocationDeviation
    {
        public string Category { get; set; } = string.Empty;
        public decimal Deviation { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TransactionSummary
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageSize { get; set; }
    }

    public class DailyTransactionVolume
    {
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
        public int Count { get; set; }
    }

    public class SecurityTransactionSummary
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalVolume { get; set; }
    }

    public class PortfolioSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Return { get; set; }
        public decimal Volatility { get; set; }
        public int RiskLevel { get; set; }
    }

    public class PerformanceComparison
    {
        public string PortfolioName { get; set; } = string.Empty;
        public decimal OneMonth { get; set; }
        public decimal ThreeMonth { get; set; }
        public decimal YearToDate { get; set; }
        public decimal OneYear { get; set; }
    }

    public class RiskComparison
    {
        public string PortfolioName { get; set; } = string.Empty;
        public decimal Volatility { get; set; }
        public decimal Beta { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
    }
}