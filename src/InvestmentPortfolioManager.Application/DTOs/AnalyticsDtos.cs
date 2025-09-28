namespace InvestmentPortfolioManager.Application.DTOs
{
    // Risk Analysis DTOs
    public record RiskMetricsDto(
        decimal Value,
        decimal DailyVaR95,
        decimal DailyVaR99,
        decimal ExpectedShortfall,
        decimal Volatility,
        decimal SharpeRatio,
        decimal MaxDrawdown,
        decimal Beta,
        decimal TrackingError,
        DateTime CalculationDate
    );

    public record StressTestResultDto(
        string ScenarioName,
        decimal PortfolioValue,
        decimal PnL,
        decimal PnLPercent,
        Dictionary<string, decimal> SectorImpacts,
        DateTime CalculationDate
    );

    public record CorrelationMatrixDto(
        Dictionary<string, Dictionary<string, decimal>> CorrelationMatrix,
        DateTime CalculationDate
    );

    public record ConcentrationRiskDto(
        decimal HerfindahlIndex,
        List<ConcentrationDto> TopConcentrations,
        decimal SingleSecurityLimit,
        decimal SectorLimit,
        List<string> ViolatingPositions,
        DateTime CalculationDate
    );

    public record ConcentrationDto(
        string SecuritySymbol,
        string SecurityName,
        decimal Weight,
        bool IsViolating
    );

    public record LiquidityAnalysisDto(
        decimal TotalValue,
        decimal LiquidValue,
        decimal LiquidityRatio,
        List<LiquidityBucketDto> LiquidityBuckets,
        int DaysToLiquidate,
        DateTime CalculationDate
    );

    public record LiquidityBucketDto(
        string BucketName,
        decimal Value,
        decimal Percentage,
        int EstimatedDaysToSell
    );

    public record RiskAlertDto(
        int Id,
        string AlertType,
        string Message,
        string Severity,
        Dictionary<string, object> Metadata,
        DateTime CreatedAt,
        bool IsActive
    );

    // Analytics DTOs
    public record PortfolioAnalyticsDto(
        PortfolioSummaryDto Portfolio,
        RiskMetricsDto RiskMetrics,
        List<PositionSummaryDto> TopPositions,
        List<SectorAllocationDto> SectorAllocation,
        List<AssetAllocationDto> AssetAllocation,
        List<PerformanceHistoryDto> PerformanceHistory,
        DateTime LastUpdated
    );

    public record SectorAllocationDto(
        string Sector,
        decimal Value,
        decimal Weight,
        decimal Return,
        int PositionCount
    );

    public record AssetAllocationDto(
        string AssetClass,
        decimal Value,
        decimal Weight,
        decimal Return,
        int PositionCount
    );

    public record PerformanceHistoryDto(
        DateTime Date,
        decimal Value,
        decimal Return,
        decimal CumulativeReturn,
        decimal Benchmark
    );

    public record MarketOverviewDto(
        List<MarketIndexDto> Indices,
        List<TopMoverDto> TopGainers,
        List<TopMoverDto> TopLosers,
        List<SectorPerformanceDto> SectorPerformance,
        DateTime LastUpdated
    );

    public record MarketIndexDto(
        string Name,
        string Symbol,
        decimal Value,
        decimal Change,
        decimal ChangePercent,
        DateTime LastUpdated
    );

    public record TopMoverDto(
        string Symbol,
        string Name,
        decimal Price,
        decimal Change,
        decimal ChangePercent,
        long Volume
    );

    public record SectorPerformanceDto(
        string Sector,
        decimal Return,
        decimal AverageVolume,
        int SecurityCount,
        DateTime Date
    );

    // Search and Filter DTOs
    public record SearchResultDto<T>(
        List<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        bool HasNextPage,
        bool HasPreviousPage
    );

    public record FilterCriteriaDto(
        string? SearchTerm,
        DateTime? StartDate,
        DateTime? EndDate,
        List<string>? Sectors,
        List<string>? AssetTypes,
        decimal? MinValue,
        decimal? MaxValue,
        string? SortBy,
        bool SortDescending,
        int PageNumber,
        int PageSize
    );

    // Response DTOs
    public record ApiResponseDto<T>(
        T Data,
        bool Success,
        string? Message,
        List<string>? Errors,
        DateTime Timestamp
    );

    public record ValidationErrorDto(
        string Field,
        string Message,
        object? AttemptedValue
    );
}