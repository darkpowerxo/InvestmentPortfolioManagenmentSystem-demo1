namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour la gestion des données de marché / Service for market data management
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Obtenir le prix actuel d'un titre / Get current price of a security
    /// </summary>
    Task<decimal?> GetCurrentPriceAsync(string symbol);
    
    /// <summary>
    /// Obtenir les données de marché en temps réel / Get real-time market data
    /// </summary>
    Task<MarketDataDto> GetRealTimeDataAsync(string symbol);
    
    /// <summary>
    /// Obtenir les données historiques / Get historical market data
    /// </summary>
    Task<IEnumerable<HistoricalPriceDto>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Obtenir les données OHLCV / Get OHLCV data
    /// </summary>
    Task<IEnumerable<OhlcvDataDto>> GetOhlcvDataAsync(string symbol, DateTime startDate, DateTime endDate, TimeInterval interval);
    
    /// <summary>
    /// Calculer les indicateurs techniques / Calculate technical indicators
    /// </summary>
    Task<TechnicalIndicatorsDto> GetTechnicalIndicatorsAsync(string symbol, int period = 20);
    
    /// <summary>
    /// Démarrer le flux de données en temps réel / Start real-time data feed
    /// </summary>
    Task StartRealTimeFeedAsync(IEnumerable<string> symbols);
    
    /// <summary>
    /// Arrêter le flux de données en temps réel / Stop real-time data feed
    /// </summary>
    Task StopRealTimeFeedAsync();
    
    /// <summary>
    /// Valider la qualité des données / Validate data quality
    /// </summary>
    Task<DataQualityReportDto> ValidateDataQualityAsync(string symbol, DateTime date);
    
    /// <summary>
    /// Synchroniser les données de marché / Synchronize market data
    /// </summary>
    Task SynchronizeMarketDataAsync();
}

/// <summary>
/// Intervalles de temps pour les données OHLCV / Time intervals for OHLCV data
/// </summary>
public enum TimeInterval
{
    OneMinute,
    FiveMinutes,
    FifteenMinutes,
    ThirtyMinutes,
    OneHour,
    FourHours,
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// DTO pour les données de marché en temps réel / DTO for real-time market data
/// </summary>
public class MarketDataDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public long Volume { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal High52Week { get; set; }
    public decimal Low52Week { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Currency { get; set; } = "CAD";
}

/// <summary>
/// DTO pour les données historiques / DTO for historical data
/// </summary>
public class HistoricalPriceDto
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public decimal AdjustedPrice { get; set; }
}

/// <summary>
/// DTO pour les données OHLCV / DTO for OHLCV data
/// </summary>
public class OhlcvDataDto
{
    public DateTime DateTime { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
}

/// <summary>
/// DTO pour les indicateurs techniques / DTO for technical indicators
/// </summary>
public class TechnicalIndicatorsDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal SimpleMovingAverage { get; set; }
    public decimal ExponentialMovingAverage { get; set; }
    public decimal RelativeStrengthIndex { get; set; }
    public decimal BollingerBandUpper { get; set; }
    public decimal BollingerBandLower { get; set; }
    public decimal BollingerBandMiddle { get; set; }
    public decimal MACD { get; set; }
    public decimal MACDSignal { get; set; }
    public decimal MACDHistogram { get; set; }
    public decimal Volatility { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO pour le rapport de qualité des données / DTO for data quality report
/// </summary>
public class DataQualityReportDto
{
    public string Symbol { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsComplete { get; set; }
    public bool HasOutliers { get; set; }
    public bool HasGaps { get; set; }
    public decimal CompletionRate { get; set; }
    public IEnumerable<string> Issues { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
}