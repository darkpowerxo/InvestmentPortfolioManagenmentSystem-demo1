using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour l'analyse et la gestion des risques / Service for risk analysis and management
/// </summary>
public interface IRiskManagementService
{
    /// <summary>
    /// Calculer les métriques de risque pour un portefeuille / Calculate risk metrics for a portfolio
    /// </summary>
    Task<PortfolioRiskMetricsDto> CalculatePortfolioRiskAsync(int portfolioId, int periodDays = 252);
    
    /// <summary>
    /// Effectuer un test de stress sur le portefeuille / Perform stress test on portfolio
    /// </summary>
    Task<StressTestResultDto> PerformStressTestAsync(int portfolioId, StressTestScenarioDto scenario);
    
    /// <summary>
    /// Analyser les corrélations entre les positions / Analyze correlations between positions
    /// </summary>
    Task<CorrelationAnalysisDto> AnalyzeCorrelationsAsync(int portfolioId, int periodDays = 252);
    
    /// <summary>
    /// Calculer l'exposition par secteur et géographie / Calculate sector and geographic exposure
    /// </summary>
    Task<ExposureAnalysisDto> AnalyzeExposureAsync(int portfolioId);
    
    /// <summary>
    /// Évaluer la concentration du portefeuille / Assess portfolio concentration
    /// </summary>
    Task<ConcentrationRiskDto> AssessConcentrationRiskAsync(int portfolioId);
    
    /// <summary>
    /// Simuler des scénarios de marché / Simulate market scenarios
    /// </summary>
    Task<ScenarioAnalysisDto> RunScenarioAnalysisAsync(int portfolioId, IEnumerable<MarketScenarioDto> scenarios);
    
    /// <summary>
    /// Calculer la Value at Risk (VaR) et Expected Shortfall / Calculate VaR and Expected Shortfall
    /// </summary>
    Task<VaRAnalysisDto> CalculateVaRAnalysisAsync(int portfolioId, decimal[] confidenceLevels, int periodDays = 252);
    
    /// <summary>
    /// Évaluer la liquidité du portefeuille / Assess portfolio liquidity
    /// </summary>
    Task<LiquidityAnalysisDto> AssessLiquidityAsync(int portfolioId);
    
    /// <summary>
    /// Générer des alertes de risque / Generate risk alerts
    /// </summary>
    Task<IEnumerable<RiskAlertDto>> GenerateRiskAlertsAsync(int portfolioId);
    
    /// <summary>
    /// Calculer l'attribution de performance / Calculate performance attribution
    /// </summary>
    Task<PerformanceAttributionDto> CalculatePerformanceAttributionAsync(int portfolioId, DateTime startDate, DateTime endDate);
}

#region DTOs

/// <summary>
/// Métriques de risque du portefeuille / Portfolio risk metrics
/// </summary>
public record PortfolioRiskMetricsDto
{
    public decimal Value { get; init; }
    public decimal Volatility { get; init; }
    public decimal SharpeRatio { get; init; }
    public decimal SortinoRatio { get; init; }
    public decimal Beta { get; init; }
    public decimal Alpha { get; init; }
    public decimal MaxDrawdown { get; init; }
    public decimal VaR95 { get; init; }
    public decimal VaR99 { get; init; }
    public decimal ExpectedShortfall95 { get; init; }
    public decimal ExpectedShortfall99 { get; init; }
    public decimal TrackingError { get; init; }
    public decimal InformationRatio { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Scénario de test de stress / Stress test scenario
/// </summary>
public record StressTestScenarioDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, decimal> MarketShocks { get; init; } = new();
    public Dictionary<string, decimal> SectorShocks { get; init; } = new();
    public decimal InterestRateShock { get; init; }
    public decimal CurrencyShock { get; init; }
    public decimal VolatilityShock { get; init; }
}

/// <summary>
/// Résultat du test de stress / Stress test result
/// </summary>
public record StressTestResultDto
{
    public string ScenarioName { get; init; } = string.Empty;
    public decimal OriginalValue { get; init; }
    public decimal StressedValue { get; init; }
    public decimal AbsoluteImpact { get; init; }
    public decimal PercentageImpact { get; init; }
    public Dictionary<string, decimal> PositionImpacts { get; init; } = new();
    public Dictionary<string, decimal> SectorImpacts { get; init; } = new();
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Analyse des corrélations / Correlation analysis
/// </summary>
public record CorrelationAnalysisDto
{
    public decimal[,] CorrelationMatrix { get; init; } = new decimal[0,0];
    public string[] SecuritySymbols { get; init; } = Array.Empty<string>();
    public decimal AverageCorrelation { get; init; }
    public decimal MaxCorrelation { get; init; }
    public decimal MinCorrelation { get; init; }
    public IEnumerable<HighCorrelationPairDto> HighCorrelationPairs { get; init; } = new List<HighCorrelationPairDto>();
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Paire de titres hautement corrélés / Highly correlated security pair
/// </summary>
public record HighCorrelationPairDto
{
    public string Security1 { get; init; } = string.Empty;
    public string Security2 { get; init; } = string.Empty;
    public decimal Correlation { get; init; }
}

/// <summary>
/// Analyse d'exposition / Exposure analysis
/// </summary>
public record ExposureAnalysisDto
{
    public Dictionary<string, decimal> SectorExposure { get; init; } = new();
    public Dictionary<string, decimal> GeographicExposure { get; init; } = new();
    public Dictionary<string, decimal> CurrencyExposure { get; init; } = new();
    public Dictionary<string, decimal> AssetClassExposure { get; init; } = new();
    public decimal EquityExposure { get; init; }
    public decimal FixedIncomeExposure { get; init; }
    public decimal CashExposure { get; init; }
    public decimal AlternativeExposure { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Risque de concentration / Concentration risk
/// </summary>
public record ConcentrationRiskDto
{
    public decimal HerfindahlIndex { get; init; }
    public decimal Top5Concentration { get; init; }
    public decimal Top10Concentration { get; init; }
    public IEnumerable<ConcentrationDto> TopConcentrations { get; init; } = new List<ConcentrationDto>();
    public ConcentrationRiskLevel RiskLevel { get; init; }
    public string[] Recommendations { get; init; } = Array.Empty<string>();
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Concentration individuelle / Individual concentration
/// </summary>
public record ConcentrationDto
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public decimal Value { get; init; }
}

/// <summary>
/// Niveau de risque de concentration / Concentration risk level
/// </summary>
public enum ConcentrationRiskLevel
{
    Low = 1,
    Moderate = 2,
    High = 3,
    VeryHigh = 4
}

/// <summary>
/// Analyse de scénarios / Scenario analysis
/// </summary>
public record ScenarioAnalysisDto
{
    public IEnumerable<ScenarioResultDto> Results { get; init; } = new List<ScenarioResultDto>();
    public decimal WorstCaseScenario { get; init; }
    public decimal BestCaseScenario { get; init; }
    public decimal AverageScenario { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Résultat de scénario / Scenario result
/// </summary>
public record ScenarioResultDto
{
    public string ScenarioName { get; init; } = string.Empty;
    public decimal Probability { get; init; }
    public decimal PortfolioReturn { get; init; }
    public decimal PortfolioValue { get; init; }
}

/// <summary>
/// Scénario de marché / Market scenario
/// </summary>
public record MarketScenarioDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Probability { get; init; }
    public decimal EquityReturn { get; init; }
    public decimal BondReturn { get; init; }
    public decimal InterestRateChange { get; init; }
    public decimal InflationRate { get; init; }
    public Dictionary<string, decimal> SectorReturns { get; init; } = new();
}

/// <summary>
/// Analyse VaR / VaR analysis
/// </summary>
public record VaRAnalysisDto
{
    public Dictionary<decimal, decimal> VaRLevels { get; init; } = new(); // Confidence Level -> VaR
    public Dictionary<decimal, decimal> ExpectedShortfall { get; init; } = new(); // Confidence Level -> ES
    public VaRMethodology Methodology { get; init; }
    public int PeriodDays { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Méthodologie VaR / VaR methodology
/// </summary>
public enum VaRMethodology
{
    Historical = 1,
    Parametric = 2,
    MonteCarlo = 3
}

/// <summary>
/// Analyse de liquidité / Liquidity analysis
/// </summary>
public record LiquidityAnalysisDto
{
    public decimal HighLiquidityPercentage { get; init; }
    public decimal MediumLiquidityPercentage { get; init; }
    public decimal LowLiquidityPercentage { get; init; }
    public decimal AverageDaysToLiquidate { get; init; }
    public IEnumerable<LiquidityBucketDto> LiquidityBuckets { get; init; } = new List<LiquidityBucketDto>();
    public LiquidityRiskLevel OverallRiskLevel { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Seau de liquidité / Liquidity bucket
/// </summary>
public record LiquidityBucketDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
    public decimal Value { get; init; }
    public int DaysToLiquidate { get; init; }
}

/// <summary>
/// Niveau de risque de liquidité / Liquidity risk level
/// </summary>
public enum LiquidityRiskLevel
{
    Low = 1,
    Moderate = 2,
    High = 3,
    VeryHigh = 4
}

/// <summary>
/// Alerte de risque / Risk alert
/// </summary>
public record RiskAlertDto
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public RiskSeverity Severity { get; init; }
    public decimal Threshold { get; init; }
    public decimal CurrentValue { get; init; }
    public string Recommendation { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Sévérité du risque / Risk severity
/// </summary>
public enum RiskSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

/// <summary>
/// Attribution de performance / Performance attribution
/// </summary>
public record PerformanceAttributionDto
{
    public decimal TotalReturn { get; init; }
    public decimal BenchmarkReturn { get; init; }
    public decimal ActiveReturn { get; init; }
    public decimal AssetAllocationEffect { get; init; }
    public decimal SecuritySelectionEffect { get; init; }
    public decimal InteractionEffect { get; init; }
    public IEnumerable<SectorAttributionDto> SectorAttributions { get; init; } = new List<SectorAttributionDto>();
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Attribution par secteur / Sector attribution
/// </summary>
public record SectorAttributionDto
{
    public string Sector { get; init; } = string.Empty;
    public decimal AllocationEffect { get; init; }
    public decimal SelectionEffect { get; init; }
    public decimal TotalEffect { get; init; }
}

#endregion