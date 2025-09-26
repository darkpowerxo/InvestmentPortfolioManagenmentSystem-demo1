using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Domain.Services;
using InvestmentPortfolioManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Securities management API controller
    /// Contrôleur API de gestion des titres
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SecuritiesController : ControllerBase
    {
        private readonly MarketDataService _marketDataService;
        private readonly ILogger<SecuritiesController> _logger;

        public SecuritiesController(
            MarketDataService marketDataService,
            ILogger<SecuritiesController> logger)
        {
            _marketDataService = marketDataService;
            _logger = logger;
        }

        /// <summary>
        /// Search securities by symbol or name
        /// Rechercher des titres par symbole ou nom
        /// </summary>
        /// <param name="query">Search query / Requête de recherche</param>
        /// <param name="securityType">Filter by security type / Filtrer par type de titre</param>
        /// <param name="limit">Maximum results (default 20) / Résultats maximum (défaut 20)</param>
        /// <returns>List of matching securities / Liste des titres correspondants</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<SecuritySummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchSecurities(
            [FromQuery, Required] string query,
            [FromQuery] SecurityType? securityType = null,
            [FromQuery, Range(1, 100)] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest("Query must be at least 2 characters / La requête doit contenir au moins 2 caractères");
                }

                _logger.LogInformation("Searching securities with query: {Query}", query);

                // Mock search results - in production, this would query the database
                var mockResults = CreateMockSecurities()
                    .Where(s => s.Symbol.Contains(query.ToUpper()) || 
                               s.NameEN.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                               s.NameFR.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Where(s => securityType == null || s.SecurityType == securityType)
                    .Take(limit)
                    .Select(s => new SecuritySummary
                    {
                        Id = s.Id,
                        Symbol = s.Symbol,
                        NameEN = s.NameEN,
                        NameFR = s.NameFR,
                        SecurityType = s.SecurityType,
                        Exchange = s.Exchange,
                        Currency = "CAD"
                    })
                    .ToList();

                return Ok(mockResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching securities with query: {Query}", query);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get detailed security information
        /// Obtenir les informations détaillées du titre
        /// </summary>
        /// <param name="securityId">Security ID / ID du titre</param>
        /// <returns>Detailed security information / Informations détaillées du titre</returns>
        [HttpGet("{securityId}")]
        [ProducesResponseType(typeof(SecurityDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSecurityDetail([FromRoute] Guid securityId)
        {
            try
            {
                _logger.LogInformation("Getting security detail for {SecurityId}", securityId);

                var security = CreateMockSecurities().FirstOrDefault(s => s.Id == securityId);
                if (security == null)
                {
                    return NotFound($"Security not found / Titre non trouvé: {securityId}");
                }

                var detail = new SecurityDetail
                {
                    Id = security.Id,
                    Symbol = security.Symbol,
                    ISIN = security.ISIN,
                    CUSIP = security.CUSIP,
                    NameEN = security.NameEN,
                    NameFR = security.NameFR,
                    SecurityType = security.SecurityType,
                    Exchange = security.Exchange,
                    Currency = "CAD",
                    
                    // Mock market data
                    CurrentPrice = GenerateRandomPrice(security.Symbol),
                    PreviousClose = GenerateRandomPrice(security.Symbol) * 0.98m,
                    DayHigh = GenerateRandomPrice(security.Symbol) * 1.02m,
                    DayLow = GenerateRandomPrice(security.Symbol) * 0.97m,
                    Volume = Random.Shared.Next(10000, 1000000),
                    
                    // Risk metrics
                    Beta = security.Beta,
                    HistoricalVolatility = security.HistoricalVolatility,
                    DividendYield = security.DividendYield,
                    
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security detail for {SecurityId}", securityId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get technical analysis for a security
        /// Obtenir l'analyse technique d'un titre
        /// </summary>
        /// <param name="securityId">Security ID / ID du titre</param>
        /// <param name="period">Analysis period in days (default 30) / Période d'analyse en jours (défaut 30)</param>
        /// <returns>Technical analysis indicators / Indicateurs d'analyse technique</returns>
        [HttpGet("{securityId}/technical-analysis")]
        [ProducesResponseType(typeof(TechnicalAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTechnicalAnalysis(
            [FromRoute] Guid securityId,
            [FromQuery, Range(10, 365)] int period = 30)
        {
            try
            {
                _logger.LogInformation("Getting technical analysis for security {SecurityId}", securityId);

                // Generate mock price history
                var priceHistory = GenerateMockPriceHistory(period);
                
                var analysis = await _marketDataService.CalculateTechnicalIndicatorsAsync(priceHistory);

                var result = new TechnicalAnalysisResult
                {
                    SecurityId = securityId,
                    Period = period,
                    SMA20 = analysis.SMA,
                    EMA20 = analysis.EMA,
                    RSI = analysis.RSI,
                    MACD = analysis.MACD,
                    MACDSignal = analysis.MACDSignal,
                    BollingerUpper = analysis.BollingerUpper,
                    BollingerLower = analysis.BollingerLower,
                    ATR = analysis.ATR,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting technical analysis for {SecurityId}", securityId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get historical price data for a security
        /// Obtenir les données de prix historiques d'un titre
        /// </summary>
        /// <param name="securityId">Security ID / ID du titre</param>
        /// <param name="startDate">Start date / Date de début</param>
        /// <param name="endDate">End date / Date de fin</param>
        /// <returns>Historical price data / Données de prix historiques</returns>
        [HttpGet("{securityId}/price-history")]
        [ProducesResponseType(typeof(List<PriceDataPoint>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPriceHistory(
            [FromRoute] Guid securityId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-3);
                var end = endDate ?? DateTime.Today;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date / La date de début doit être antérieure à la date de fin");
                }

                if ((end - start).TotalDays > 365)
                {
                    return BadRequest("Date range cannot exceed 365 days / La plage de dates ne peut pas dépasser 365 jours");
                }

                _logger.LogInformation("Getting price history for security {SecurityId} from {Start} to {End}", 
                    securityId, start, end);

                var days = (int)(end - start).TotalDays;
                var priceHistory = GenerateMockPriceHistory(days, start);

                return Ok(priceHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price history for {SecurityId}", securityId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        #region Mock Data Helpers

        private List<Security> CreateMockSecurities()
        {
            var securities = new List<Security>();
            
            var mockData = new[]
            {
                new { Symbol = "SHOP.TO", NameEN = "Shopify Inc", NameFR = "Shopify Inc", Type = SecurityType.Stock, Exchange = "TSX", ISIN = "CA82509L1076", Beta = 1.85m, Vol = 0.45m, Div = 0.0m },
                new { Symbol = "RY.TO", NameEN = "Royal Bank of Canada", NameFR = "Banque Royale du Canada", Type = SecurityType.Stock, Exchange = "TSX", ISIN = "CA7800871021", Beta = 1.15m, Vol = 0.18m, Div = 4.2m },
                new { Symbol = "CNR.TO", NameEN = "Canadian National Railway", NameFR = "Chemin de fer Canadien National", Type = SecurityType.Stock, Exchange = "TSX", ISIN = "CA1363751027", Beta = 0.95m, Vol = 0.22m, Div = 2.8m },
                new { Symbol = "GOOGL", NameEN = "Alphabet Inc Class A", NameFR = "Alphabet Inc Classe A", Type = SecurityType.Stock, Exchange = "NASDAQ", ISIN = "US02079K3059", Beta = 1.25m, Vol = 0.28m, Div = 0.0m },
                new { Symbol = "MSFT", NameEN = "Microsoft Corporation", NameFR = "Microsoft Corporation", Type = SecurityType.Stock, Exchange = "NASDAQ", ISIN = "US5949181045", Beta = 0.92m, Vol = 0.24m, Div = 2.1m },
                new { Symbol = "VTI", NameEN = "Vanguard Total Stock Market ETF", NameFR = "FNB Vanguard Total Stock Market", Type = SecurityType.ETF, Exchange = "NYSE", ISIN = "US9229087690", Beta = 1.00m, Vol = 0.16m, Div = 1.8m },
                new { Symbol = "GOC.TO", NameEN = "Government of Canada Bond", NameFR = "Obligation du Gouvernement du Canada", Type = SecurityType.Bond, Exchange = "TSX", ISIN = "CA135087K296", Beta = 0.05m, Vol = 0.08m, Div = 2.5m }
            };

            foreach (var data in mockData)
            {
                var security = new Security(
                    data.Symbol,
                    data.NameEN,
                    data.NameFR,
                    data.Type,
                    Guid.NewGuid(), // AssetClassId
                    Guid.NewGuid()  // CurrencyId
                )
                {
                    Id = Guid.NewGuid(),
                    Exchange = data.Exchange
                };

                security.UpdateIdentifiers(data.ISIN);
                security.UpdateEquityInfo(data.Div, data.Beta);
                security.UpdateHistoricalVolatility(data.Vol);

                securities.Add(security);
            }

            return securities;
        }

        private decimal GenerateRandomPrice(string symbol)
        {
            // Generate consistent prices based on symbol hash
            var hash = symbol.GetHashCode();
            var random = new Random(Math.Abs(hash));
            return (decimal)(random.NextDouble() * 500 + 10); // Price between $10 and $510
        }

        private List<PriceDataPoint> GenerateMockPriceHistory(int days, DateTime? startDate = null)
        {
            var start = startDate ?? DateTime.Today.AddDays(-days);
            var priceHistory = new List<PriceDataPoint>();
            var random = new Random();
            
            var basePrice = 100m;
            var currentPrice = basePrice;

            for (int i = 0; i < days; i++)
            {
                var date = start.AddDays(i);
                
                // Generate realistic price movement
                var change = (decimal)(random.NextDouble() - 0.5) * 0.05m; // ±2.5% daily change
                currentPrice *= (1 + change);
                
                var dayHigh = currentPrice * (1 + (decimal)random.NextDouble() * 0.02m);
                var dayLow = currentPrice * (1 - (decimal)random.NextDouble() * 0.02m);
                var volume = random.Next(50000, 500000);

                priceHistory.Add(new PriceDataPoint
                {
                    Date = date,
                    Open = currentPrice,
                    High = dayHigh,
                    Low = dayLow,
                    Close = currentPrice,
                    Volume = volume
                });
            }

            return priceHistory;
        }

        #endregion
    }

    #region DTOs

    public class SecuritySummary
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string NameEN { get; set; } = string.Empty;
        public string NameFR { get; set; } = string.Empty;
        public SecurityType SecurityType { get; set; }
        public string Exchange { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
    }

    public class SecurityDetail : SecuritySummary
    {
        public string? ISIN { get; set; }
        public string? CUSIP { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayLow { get; set; }
        public int Volume { get; set; }
        public decimal? Beta { get; set; }
        public decimal? HistoricalVolatility { get; set; }
        public decimal? DividendYield { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TechnicalAnalysisResult
    {
        public Guid SecurityId { get; set; }
        public int Period { get; set; }
        public decimal? SMA20 { get; set; }
        public decimal? EMA20 { get; set; }
        public decimal? RSI { get; set; }
        public decimal? MACD { get; set; }
        public decimal? MACDSignal { get; set; }
        public decimal? BollingerUpper { get; set; }
        public decimal? BollingerLower { get; set; }
        public decimal? ATR { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PriceDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public int Volume { get; set; }
    }

    #endregion
}