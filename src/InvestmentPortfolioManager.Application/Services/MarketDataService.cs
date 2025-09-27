using InvestmentPortfolioManager.Application.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolioManager.Application.Services;

/// <summary>
/// Service de simulation des données de marché / Market data simulation service
/// Simule des données réalistes pour les titres canadiens / Simulates realistic data for Canadian securities
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly ILogger<MarketDataService> _logger;
    private readonly Dictionary<string, MarketDataDto> _currentPrices;
    private readonly Dictionary<string, List<HistoricalPriceDto>> _historicalData;
    private readonly Random _random;

    private readonly HashSet<string> _subscribedSymbols;
    private bool _isRealTimeFeedActive;

    // Titres canadiens de référence / Reference Canadian securities
    private readonly Dictionary<string, (decimal BasePrice, decimal Volatility, string Name)> _securityData = new()
    {
        { "RY.TO", (146.50m, 0.18m, "Royal Bank of Canada") },
        { "TD.TO", (87.25m, 0.20m, "Toronto-Dominion Bank") },
        { "SHOP.TO", (95.30m, 0.35m, "Shopify Inc.") },
        { "CNR.TO", (164.80m, 0.15m, "Canadian National Railway") },
        { "WEED.TO", (8.45m, 0.45m, "Canopy Growth Corporation") },
        { "AC.TO", (18.90m, 0.40m, "Air Canada") },
        { "SU.TO", (56.75m, 0.25m, "Suncor Energy Inc.") },
        { "ENB.TO", (54.20m, 0.16m, "Enbridge Inc.") },
        { "BAM.A.TO", (71.40m, 0.22m, "Brookfield Asset Management") },
        { "XIU.TO", (32.45m, 0.12m, "iShares Core S&P Total Canadian Stock Market ETF") }
    };

    public MarketDataService(ILogger<MarketDataService> logger)
    {
        _logger = logger;
        _random = new Random();
        _currentPrices = new Dictionary<string, MarketDataDto>();
        _historicalData = new Dictionary<string, List<HistoricalPriceDto>>();
        _subscribedSymbols = new HashSet<string>();
        
        InitializeMarketData();
    }

    public async Task<decimal?> GetCurrentPriceAsync(string symbol)
    {
        await Task.Delay(10); // Simuler latence réseau / Simulate network latency
        
        if (_currentPrices.TryGetValue(symbol, out var marketData))
        {
            return marketData.Price;
        }
        
        // Générer un prix si le symbole n'existe pas / Generate price if symbol doesn't exist
        if (_securityData.TryGetValue(symbol, out var securityInfo))
        {
            var price = GenerateRealisticPrice(securityInfo.BasePrice, securityInfo.Volatility);
            return price;
        }
        
        return null;
    }

    public async Task<MarketDataDto> GetRealTimeDataAsync(string symbol)
    {
        await Task.Delay(15); // Simuler latence / Simulate latency
        
        if (!_currentPrices.TryGetValue(symbol, out var marketData))
        {
            marketData = await GenerateMarketDataAsync(symbol);
            _currentPrices[symbol] = marketData;
        }
        
        // Mettre à jour avec de petites variations / Update with small variations
        UpdateMarketDataWithVariation(marketData);
        
        _logger.LogDebug("Retrieved real-time data for {Symbol}: {Price}", symbol, marketData.Price);
        return marketData;
    }

    public async Task<IEnumerable<HistoricalPriceDto>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        await Task.Delay(50); // Simuler temps de traitement / Simulate processing time
        
        if (!_historicalData.TryGetValue(symbol, out var data))
        {
            data = await GenerateHistoricalDataAsync(symbol, startDate, endDate);
            _historicalData[symbol] = data;
        }
        
        return data.Where(d => d.Date >= startDate && d.Date <= endDate)
                  .OrderBy(d => d.Date);
    }

    public async Task<IEnumerable<OhlcvDataDto>> GetOhlcvDataAsync(string symbol, DateTime startDate, DateTime endDate, TimeInterval interval)
    {
        await Task.Delay(75); // Simuler temps de traitement / Simulate processing time
        
        var historicalData = await GetHistoricalDataAsync(symbol, startDate, endDate);
        return ConvertToOhlcv(historicalData, interval);
    }

    public async Task<TechnicalIndicatorsDto> GetTechnicalIndicatorsAsync(string symbol, int period = 20)
    {
        await Task.Delay(30); // Simuler calculs / Simulate calculations
        
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-period * 2); // Plus de données pour les calculs / More data for calculations
        var historicalData = await GetHistoricalDataAsync(symbol, startDate, endDate);
        var prices = historicalData.Select(h => h.Price).ToList();
        
        return CalculateTechnicalIndicators(symbol, prices, period);
    }

    public async Task StartRealTimeFeedAsync(IEnumerable<string> symbols)
    {
        await Task.Delay(100); // Simuler initialisation / Simulate initialization
        
        foreach (var symbol in symbols)
        {
            _subscribedSymbols.Add(symbol);
            if (!_currentPrices.ContainsKey(symbol))
            {
                _currentPrices[symbol] = await GenerateMarketDataAsync(symbol);
            }
        }
        
        _isRealTimeFeedActive = true;
        _logger.LogInformation("Started real-time feed for {Count} symbols", _subscribedSymbols.Count);
        
        // Simuler des mises à jour périodiques / Simulate periodic updates
        _ = Task.Run(async () =>
        {
            while (_isRealTimeFeedActive)
            {
                UpdateAllSubscribedSymbols();
                await Task.Delay(5000); // Mise à jour toutes les 5 secondes / Update every 5 seconds
            }
        });
    }

    public async Task StopRealTimeFeedAsync()
    {
        await Task.Delay(10);
        _isRealTimeFeedActive = false;
        _subscribedSymbols.Clear();
        _logger.LogInformation("Stopped real-time feed");
    }

    public async Task<DataQualityReportDto> ValidateDataQualityAsync(string symbol, DateTime date)
    {
        await Task.Delay(25); // Simuler validation / Simulate validation
        
        var report = new DataQualityReportDto
        {
            Symbol = symbol,
            Date = date,
            IsComplete = true,
            HasOutliers = false,
            HasGaps = false,
            CompletionRate = 0.98m,
            Issues = new List<string>(),
            Warnings = new List<string> { "Minor price gap detected at market open" }
        };
        
        // Simuler quelques problèmes de qualité occasionnels / Simulate occasional quality issues
        if (_random.NextDouble() < 0.1) // 10% de chance / 10% chance
        {
            report.HasOutliers = true;
            report.CompletionRate = 0.95m;
            ((List<string>)report.Issues).Add("Unusual price spike detected");
        }
        
        return report;
    }

    public async Task SynchronizeMarketDataAsync()
    {
        await Task.Delay(200); // Simuler synchronisation / Simulate synchronization
        
        _logger.LogInformation("Synchronizing market data for {Count} securities", _securityData.Count);
        
        // Mettre à jour tous les prix / Update all prices
        foreach (var symbol in _securityData.Keys)
        {
            if (_currentPrices.ContainsKey(symbol))
            {
                UpdateMarketDataWithVariation(_currentPrices[symbol]);
            }
        }
        
        _logger.LogInformation("Market data synchronization completed");
    }

    #region Méthodes privées / Private Methods

    private void InitializeMarketData()
    {
        foreach (var (symbol, (basePrice, volatility, name)) in _securityData)
        {
            _currentPrices[symbol] = new MarketDataDto
            {
                Symbol = symbol,
                Price = GenerateRealisticPrice(basePrice, volatility),
                Change = 0m,
                ChangePercent = 0m,
                Volume = GenerateRealisticVolume(),
                Bid = 0m,
                Ask = 0m,
                High52Week = basePrice * 1.25m,
                Low52Week = basePrice * 0.75m,
                LastUpdated = DateTime.UtcNow,
                Currency = "CAD"
            };
        }
    }

    private async Task<MarketDataDto> GenerateMarketDataAsync(string symbol)
    {
        await Task.Yield();
        
        if (!_securityData.TryGetValue(symbol, out var securityInfo))
        {
            // Valeurs par défaut pour symboles inconnus / Default values for unknown symbols
            securityInfo = (100m, 0.20m, "Unknown Security");
        }
        
        var price = GenerateRealisticPrice(securityInfo.BasePrice, securityInfo.Volatility);
        var change = (price - securityInfo.BasePrice);
        var changePercent = change / securityInfo.BasePrice * 100m;
        
        return new MarketDataDto
        {
            Symbol = symbol,
            Price = price,
            Change = change,
            ChangePercent = changePercent,
            Volume = GenerateRealisticVolume(),
            Bid = price * 0.999m,
            Ask = price * 1.001m,
            High52Week = securityInfo.BasePrice * 1.25m,
            Low52Week = securityInfo.BasePrice * 0.75m,
            LastUpdated = DateTime.UtcNow,
            Currency = "CAD"
        };
    }

    private async Task<List<HistoricalPriceDto>> GenerateHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        await Task.Yield();
        
        if (!_securityData.TryGetValue(symbol, out var securityInfo))
        {
            securityInfo = (100m, 0.20m, "Unknown Security");
        }
        
        var data = new List<HistoricalPriceDto>();
        var currentPrice = securityInfo.BasePrice;
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            // Ignorer les week-ends / Skip weekends
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
            // Simuler le rendement quotidien avec volatilité / Simulate daily return with volatility
            var dailyReturn = GenerateGaussianRandom() * (double)securityInfo.Volatility / Math.Sqrt(252);
            currentPrice *= (1m + (decimal)dailyReturn);                data.Add(new HistoricalPriceDto
                {
                    Date = currentDate,
                    Price = Math.Round(currentPrice, 2),
                    Volume = GenerateRealisticVolume(),
                    AdjustedPrice = Math.Round(currentPrice, 2) // Simplifié / Simplified
                });
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        return data;
    }

    private decimal GenerateRealisticPrice(decimal basePrice, decimal volatility)
    {
        // Utiliser un mouvement brownien géométrique simplifié / Use simplified geometric Brownian motion
        var randomReturn = GenerateGaussianRandom() * (double)volatility * 0.1;
        var price = basePrice * (1m + (decimal)randomReturn);
        return Math.Round(Math.Max(price, 0.01m), 2); // Prix minimum de 1 cent / Minimum price of 1 cent
    }

    private long GenerateRealisticVolume()
    {
        // Volume entre 100K et 5M / Volume between 100K and 5M
        return (long)(_random.NextDouble() * 4900000 + 100000);
    }

    private double GenerateGaussianRandom()
    {
        // Box-Muller transform pour distribution normale / Box-Muller transform for normal distribution
        static double NextGaussian(Random random)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
        
        return NextGaussian(_random);
    }

    private void UpdateMarketDataWithVariation(MarketDataDto marketData)
    {
        if (!_securityData.TryGetValue(marketData.Symbol, out var securityInfo))
            return;
        
        var oldPrice = marketData.Price;
        var variation = GenerateGaussianRandom() * (double)securityInfo.Volatility * 0.01;
        marketData.Price = Math.Round(Math.Max(oldPrice * (1m + (decimal)variation), 0.01m), 2);
        marketData.Change = marketData.Price - securityInfo.BasePrice;
        marketData.ChangePercent = marketData.Change / securityInfo.BasePrice * 100m;
        marketData.Volume = GenerateRealisticVolume();
        marketData.Bid = marketData.Price * 0.999m;
        marketData.Ask = marketData.Price * 1.001m;
        marketData.LastUpdated = DateTime.UtcNow;
    }

    private void UpdateAllSubscribedSymbols()
    {
        foreach (var symbol in _subscribedSymbols)
        {
            if (_currentPrices.TryGetValue(symbol, out var marketData))
            {
                UpdateMarketDataWithVariation(marketData);
            }
        }
    }

    private IEnumerable<OhlcvDataDto> ConvertToOhlcv(IEnumerable<HistoricalPriceDto> historicalData, TimeInterval interval)
    {
        var data = historicalData.OrderBy(h => h.Date).ToList();
        var ohlcvData = new List<OhlcvDataDto>();
        
        // Simplification: regrouper par jour pour tous les intervalles / Simplification: group by day for all intervals
        foreach (var dayData in data)
        {
            var basePrice = dayData.Price;
            var dailyVolatility = 0.02m; // 2% volatilité intrajournalière / 2% intraday volatility
            
            var open = basePrice * (1m + (decimal)(GenerateGaussianRandom() * (double)dailyVolatility / 2));
            var high = Math.Max(Math.Max(basePrice, (decimal)open), basePrice * (1m + dailyVolatility));
            var low = Math.Min(Math.Min(basePrice, (decimal)open), basePrice * (1m - dailyVolatility));
            
            ohlcvData.Add(new OhlcvDataDto
            {
                DateTime = dayData.Date,
                Open = Math.Round((decimal)open, 2),
                High = Math.Round(high, 2),
                Low = Math.Round(low, 2),
                Close = dayData.Price,
                Volume = dayData.Volume
            });
        }
        
        return ohlcvData;
    }

    private TechnicalIndicatorsDto CalculateTechnicalIndicators(string symbol, List<decimal> prices, int period)
    {
        if (prices.Count < period)
        {
            return new TechnicalIndicatorsDto
            {
                Symbol = symbol,
                CalculatedAt = DateTime.UtcNow
            };
        }
        
        var recentPrices = prices.TakeLast(period).ToList();
        var sma = recentPrices.Average();
        
        // EMA avec facteur de lissage / EMA with smoothing factor
        var multiplier = 2m / (period + 1);
        var ema = recentPrices.First();
        foreach (var price in recentPrices.Skip(1))
        {
            ema = (price * multiplier) + (ema * (1m - multiplier));
        }
        
        // RSI simplifié / Simplified RSI
        var gains = new List<decimal>();
        var losses = new List<decimal>();
        
        for (int i = 1; i < recentPrices.Count; i++)
        {
            var change = recentPrices[i] - recentPrices[i - 1];
            if (change > 0)
                gains.Add(change);
            else
                losses.Add(Math.Abs(change));
        }
        
        var avgGain = gains.Any() ? gains.Average() : 0m;
        var avgLoss = losses.Any() ? losses.Average() : 0.01m; // Éviter division par zéro / Avoid division by zero
        var rs = avgGain / avgLoss;
        var rsi = 100m - (100m / (1m + rs));
        
        // Bandes de Bollinger / Bollinger Bands
        var standardDeviation = CalculateStandardDeviation(recentPrices, sma);
        var upperBand = sma + (2m * standardDeviation);
        var lowerBand = sma - (2m * standardDeviation);
        
        // MACD simplifié / Simplified MACD
        var ema12 = CalculateEma(prices, 12);
        var ema26 = CalculateEma(prices, 26);
        var macd = ema12 - ema26;
        var macdSignal = CalculateEma(new List<decimal> { macd }, 9); // Simplifié / Simplified
        var macdHistogram = macd - macdSignal;
        
        // Volatilité (écart-type annualisé) / Volatility (annualized standard deviation)
        var volatility = standardDeviation * (decimal)Math.Sqrt(252);
        
        return new TechnicalIndicatorsDto
        {
            Symbol = symbol,
            SimpleMovingAverage = Math.Round(sma, 2),
            ExponentialMovingAverage = Math.Round(ema, 2),
            RelativeStrengthIndex = Math.Round(rsi, 2),
            BollingerBandUpper = Math.Round(upperBand, 2),
            BollingerBandLower = Math.Round(lowerBand, 2),
            BollingerBandMiddle = Math.Round(sma, 2),
            MACD = Math.Round(macd, 4),
            MACDSignal = Math.Round(macdSignal, 4),
            MACDHistogram = Math.Round(macdHistogram, 4),
            Volatility = Math.Round(volatility, 4),
            CalculatedAt = DateTime.UtcNow
        };
    }

    private decimal CalculateStandardDeviation(List<decimal> values, decimal mean)
    {
        var variance = values.Select(v => (v - mean) * (v - mean)).Average();
        return (decimal)Math.Sqrt((double)variance);
    }

    private decimal CalculateEma(List<decimal> prices, int period)
    {
        if (prices.Count < period) return prices.LastOrDefault();
        
        var multiplier = 2m / (period + 1);
        var ema = prices.Take(period).Average(); // SMA initiale / Initial SMA
        
        foreach (var price in prices.Skip(period))
        {
            ema = (price * multiplier) + (ema * (1m - multiplier));
        }
        
        return ema;
    }

    #endregion
}