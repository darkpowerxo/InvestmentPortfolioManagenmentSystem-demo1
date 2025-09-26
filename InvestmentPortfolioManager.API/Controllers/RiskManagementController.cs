using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Domain.Services;
using InvestmentPortfolioManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Risk management API controller
    /// Contrôleur API de gestion des risques
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RiskManagementController : ControllerBase
    {
        private readonly PortfolioAnalyticsService _analyticsService;
        private readonly FinancialCalculations _financialCalculations;
        private readonly ILogger<RiskManagementController> _logger;

        public RiskManagementController(
            PortfolioAnalyticsService analyticsService,
            FinancialCalculations financialCalculations,
            ILogger<RiskManagementController> logger)
        {
            _analyticsService = analyticsService;
            _financialCalculations = financialCalculations;
            _logger = logger;
        }

        /// <summary>
        /// Calculate Value at Risk (VaR) for a portfolio
        /// Calculer la valeur à risque (VaR) d'un portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="confidenceLevel">Confidence level (default 95%) / Niveau de confiance (défaut 95%)</param>
        /// <param name="timeHorizon">Time horizon in days (default 1) / Horizon temporel en jours (défaut 1)</param>
        /// <returns>VaR calculation results / Résultats du calcul VaR</returns>
        [HttpGet("portfolios/{portfolioId}/var")]
        [ProducesResponseType(typeof(VaRResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CalculateVaR(
            [FromRoute] Guid portfolioId,
            [FromQuery, Range(90, 99)] decimal confidenceLevel = 95,
            [FromQuery, Range(1, 250)] int timeHorizon = 1)
        {
            try
            {
                _logger.LogInformation("Calculating VaR for portfolio {PortfolioId} with {ConfidenceLevel}% confidence", 
                    portfolioId, confidenceLevel);

                // Generate mock historical returns for demonstration
                var historicalReturns = GenerateMockReturns(250); // 1 year of daily returns
                var portfolioValue = 1000000m; // Mock portfolio value

                var confidenceDecimal = confidenceLevel / 100m;
                var parametricVaR = _financialCalculations.CalculateParametricVaR(
                    historicalReturns, portfolioValue, confidenceDecimal, timeHorizon);
                
                var historicalVaR = _financialCalculations.CalculateHistoricalVaR(
                    historicalReturns, portfolioValue, confidenceDecimal, timeHorizon);

                var result = new VaRResult
                {
                    PortfolioId = portfolioId,
                    PortfolioValue = portfolioValue,
                    ConfidenceLevel = confidenceLevel,
                    TimeHorizon = timeHorizon,
                    ParametricVaR = parametricVaR,
                    HistoricalVaR = historicalVaR,
                    CalculationDate = DateTime.UtcNow,
                    Currency = "CAD"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating VaR for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Perform comprehensive risk analysis
        /// Effectuer une analyse de risque complète
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <returns>Comprehensive risk metrics / Métriques de risque complètes</returns>
        [HttpGet("portfolios/{portfolioId}/comprehensive")]
        [ProducesResponseType(typeof(ComprehensiveRiskResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComprehensiveRiskAnalysis([FromRoute] Guid portfolioId)
        {
            try
            {
                _logger.LogInformation("Performing comprehensive risk analysis for portfolio {PortfolioId}", portfolioId);

                var portfolio = CreateMockPortfolio(portfolioId);
                var holdings = CreateMockHoldings(portfolioId);

                var riskAnalysis = await _analyticsService.AnalyzePortfolioRiskAsync(portfolio, holdings, 0.95m);

                // Generate additional risk metrics
                var historicalReturns = GenerateMockReturns(252);
                var portfolioValue = holdings.Sum(h => h.MarketValue);
                
                var result = new ComprehensiveRiskResult
                {
                    PortfolioId = portfolioId,
                    PortfolioValue = portfolioValue,
                    
                    // Risk metrics from analytics service
                    Volatility = riskAnalysis.Volatility,
                    SharpeRatio = riskAnalysis.SharpeRatio,
                    SortinoRatio = riskAnalysis.SortinoRatio,
                    MaxDrawdown = riskAnalysis.MaxDrawdown,
                    VaR95 = riskAnalysis.VaR,
                    CVaR95 = riskAnalysis.ConditionalVaR,
                    
                    // Additional calculated metrics
                    Beta = CalculatePortfolioBeta(holdings),
                    Alpha = CalculatePortfolioAlpha(holdings, historicalReturns),
                    TrackingError = _financialCalculations.CalculateTrackingError(historicalReturns, GenerateBenchmarkReturns(252)),
                    InformationRatio = CalculateInformationRatio(historicalReturns),
                    
                    // Concentration metrics
                    ConcentrationRisk = CalculateConcentrationRisk(holdings),
                    LargestPosition = holdings.Max(h => h.MarketValue) / portfolioValue * 100,
                    Top10Concentration = holdings.OrderByDescending(h => h.MarketValue).Take(10).Sum(h => h.MarketValue) / portfolioValue * 100,
                    
                    CalculationDate = DateTime.UtcNow,
                    Currency = "CAD"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing comprehensive risk analysis for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Generate correlation matrix for portfolio holdings
        /// Générer la matrice de corrélation des positions du portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="period">Analysis period in days (default 252) / Période d'analyse en jours (défaut 252)</param>
        /// <returns>Correlation matrix / Matrice de corrélation</returns>
        [HttpGet("portfolios/{portfolioId}/correlation-matrix")]
        [ProducesResponseType(typeof(CorrelationMatrixResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCorrelationMatrix(
            [FromRoute] Guid portfolioId,
            [FromQuery, Range(30, 1000)] int period = 252)
        {
            try
            {
                _logger.LogInformation("Generating correlation matrix for portfolio {PortfolioId}", portfolioId);

                var holdings = CreateMockHoldings(portfolioId);
                var correlationMatrix = GenerateMockCorrelationMatrix(holdings);

                var result = new CorrelationMatrixResult
                {
                    PortfolioId = portfolioId,
                    Period = period,
                    Securities = holdings.Select(h => new SecurityInfo
                    {
                        Id = h.SecurityId,
                        Symbol = GetSecuritySymbol(h.SecurityId),
                        Weight = h.MarketValue / holdings.Sum(x => x.MarketValue) * 100
                    }).ToList(),
                    CorrelationMatrix = correlationMatrix,
                    CalculationDate = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating correlation matrix for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Analyze risk concentration by different dimensions
        /// Analyser la concentration des risques par différentes dimensions
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <returns>Risk concentration analysis / Analyse de concentration des risques</returns>
        [HttpGet("portfolios/{portfolioId}/concentration")]
        [ProducesResponseType(typeof(ConcentrationAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConcentrationAnalysis([FromRoute] Guid portfolioId)
        {
            try
            {
                _logger.LogInformation("Analyzing risk concentration for portfolio {PortfolioId}", portfolioId);

                var holdings = CreateMockHoldings(portfolioId);
                var totalValue = holdings.Sum(h => h.MarketValue);

                var result = new ConcentrationAnalysisResult
                {
                    PortfolioId = portfolioId,
                    TotalValue = totalValue,
                    
                    // Security concentration
                    SecurityConcentration = holdings.Select(h => new ConcentrationItem
                    {
                        Name = GetSecuritySymbol(h.SecurityId),
                        Value = h.MarketValue,
                        Percentage = h.MarketValue / totalValue * 100
                    }).OrderByDescending(x => x.Percentage).ToList(),
                    
                    // Mock sector concentration
                    SectorConcentration = GenerateMockSectorConcentration(totalValue),
                    
                    // Mock geographic concentration
                    GeographicConcentration = GenerateMockGeographicConcentration(totalValue),
                    
                    // Mock currency concentration
                    CurrencyConcentration = new List<ConcentrationItem>
                    {
                        new ConcentrationItem { Name = "CAD", Value = totalValue * 0.7m, Percentage = 70 },
                        new ConcentrationItem { Name = "USD", Value = totalValue * 0.25m, Percentage = 25 },
                        new ConcentrationItem { Name = "EUR", Value = totalValue * 0.05m, Percentage = 5 }
                    },
                    
                    // Risk metrics
                    HerfindahlIndex = CalculateHerfindahlIndex(holdings),
                    EffectiveSecurities = CalculateEffectiveNumberOfSecurities(holdings),
                    
                    CalculationDate = DateTime.UtcNow,
                    Currency = "CAD"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing concentration for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Perform liquidity risk analysis
        /// Effectuer une analyse du risque de liquidité
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <returns>Liquidity risk metrics / Métriques de risque de liquidité</returns>
        [HttpGet("portfolios/{portfolioId}/liquidity")]
        [ProducesResponseType(typeof(LiquidityRiskResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLiquidityRiskAnalysis([FromRoute] Guid portfolioId)
        {
            try
            {
                _logger.LogInformation("Analyzing liquidity risk for portfolio {PortfolioId}", portfolioId);

                var holdings = CreateMockHoldings(portfolioId);
                var totalValue = holdings.Sum(h => h.MarketValue);

                var result = new LiquidityRiskResult
                {
                    PortfolioId = portfolioId,
                    TotalValue = totalValue,
                    
                    // Mock liquidity categories
                    HighLiquidity = totalValue * 0.6m, // 60% in highly liquid securities
                    MediumLiquidity = totalValue * 0.3m, // 30% in medium liquidity
                    LowLiquidity = totalValue * 0.1m, // 10% in low liquidity
                    
                    // Liquidity metrics
                    AverageDailyVolume = CalculateAverageDailyVolume(holdings),
                    LiquidityScore = CalculateLiquidityScore(holdings),
                    DaysToLiquidate = CalculateDaysToLiquidate(holdings),
                    
                    // Market impact estimates
                    EstimatedMarketImpact1Day = 0.5m, // 50 basis points
                    EstimatedMarketImpact5Days = 0.2m, // 20 basis points
                    EstimatedMarketImpact20Days = 0.1m, // 10 basis points
                    
                    CalculationDate = DateTime.UtcNow,
                    Currency = "CAD"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing liquidity risk for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        #region Mock Data Helpers

        private Portfolio CreateMockPortfolio(Guid portfolioId)
        {
            return new Portfolio("CDPQ Risk Analysis Portfolio", "Portefeuille d'analyse de risque CDPQ", Guid.NewGuid())
            {
                Id = portfolioId
            };
        }

        private List<Holding> CreateMockHoldings(Guid portfolioId)
        {
            var holdings = new List<Holding>();
            var securities = new[]
            {
                new { Symbol = "SHOP.TO", Shares = 1000, Price = 85.50m },
                new { Symbol = "RY.TO", Shares = 500, Price = 145.75m },
                new { Symbol = "CNR.TO", Shares = 300, Price = 168.25m },
                new { Symbol = "GOOGL", Shares = 50, Price = 2750.00m },
                new { Symbol = "MSFT", Shares = 100, Price = 415.50m }
            };

            foreach (var sec in securities)
            {
                var holding = new Holding(portfolioId, Guid.NewGuid(), sec.Shares, sec.Price);
                holdings.Add(holding);
            }

            return holdings;
        }

        private List<decimal> GenerateMockReturns(int count)
        {
            var random = new Random(42);
            var returns = new List<decimal>();
            
            for (int i = 0; i < count; i++)
            {
                // Generate normally distributed returns with some volatility
                var u1 = 1.0 - random.NextDouble();
                var u2 = 1.0 - random.NextDouble();
                var normalReturn = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                returns.Add((decimal)(normalReturn * 0.015)); // 1.5% daily volatility
            }
            
            return returns;
        }

        private List<decimal> GenerateBenchmarkReturns(int count)
        {
            var random = new Random(123);
            var returns = new List<decimal>();
            
            for (int i = 0; i < count; i++)
            {
                var u1 = 1.0 - random.NextDouble();
                var u2 = 1.0 - random.NextDouble();
                var normalReturn = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                returns.Add((decimal)(normalReturn * 0.01)); // Lower volatility for benchmark
            }
            
            return returns;
        }

        private decimal CalculatePortfolioBeta(List<Holding> holdings)
        {
            // Mock beta calculation - weighted average of individual security betas
            var mockBetas = new[] { 1.85m, 1.15m, 0.95m, 1.25m, 0.92m };
            var totalValue = holdings.Sum(h => h.MarketValue);
            
            decimal weightedBeta = 0;
            for (int i = 0; i < holdings.Count && i < mockBetas.Length; i++)
            {
                var weight = holdings[i].MarketValue / totalValue;
                weightedBeta += weight * mockBetas[i];
            }
            
            return Math.Round(weightedBeta, 2);
        }

        private decimal CalculatePortfolioAlpha(List<Holding> holdings, List<decimal> returns)
        {
            // Mock alpha calculation
            var portfolioReturn = returns.Average();
            var riskFreeRate = 0.02m / 252; // 2% annual risk-free rate
            var beta = CalculatePortfolioBeta(holdings);
            var marketReturn = 0.08m / 252; // 8% annual market return
            
            return (portfolioReturn - riskFreeRate) - beta * (marketReturn - riskFreeRate);
        }

        private decimal CalculateInformationRatio(List<decimal> returns)
        {
            var benchmarkReturns = GenerateBenchmarkReturns(returns.Count);
            var excessReturns = returns.Zip(benchmarkReturns, (r, b) => r - b).ToList();
            
            if (!excessReturns.Any()) return 0;
            
            var avgExcessReturn = excessReturns.Average();
            var trackingError = _financialCalculations.CalculateVolatility(excessReturns);
            
            return trackingError != 0 ? avgExcessReturn / trackingError : 0;
        }

        private decimal CalculateConcentrationRisk(List<Holding> holdings)
        {
            var totalValue = holdings.Sum(h => h.MarketValue);
            var weights = holdings.Select(h => h.MarketValue / totalValue).ToList();
            
            // Herfindahl-Hirschman Index
            return weights.Sum(w => w * w);
        }

        private decimal CalculateHerfindahlIndex(List<Holding> holdings)
        {
            var totalValue = holdings.Sum(h => h.MarketValue);
            var weights = holdings.Select(h => h.MarketValue / totalValue).ToList();
            return weights.Sum(w => w * w) * 10000; // Scaled to 0-10000
        }

        private decimal CalculateEffectiveNumberOfSecurities(List<Holding> holdings)
        {
            var hhi = CalculateHerfindahlIndex(holdings) / 10000;
            return hhi > 0 ? 1 / hhi : 0;
        }

        private decimal[,] GenerateMockCorrelationMatrix(List<Holding> holdings)
        {
            var count = holdings.Count;
            var matrix = new decimal[count, count];
            var random = new Random(42);
            
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 1.0m;
                    }
                    else if (i < j)
                    {
                        // Generate correlation between -0.5 and 0.8
                        var correlation = (decimal)(random.NextDouble() * 1.3 - 0.5);
                        correlation = Math.Max(-0.5m, Math.Min(0.8m, correlation));
                        matrix[i, j] = matrix[j, i] = Math.Round(correlation, 3);
                    }
                }
            }
            
            return matrix;
        }

        private List<ConcentrationItem> GenerateMockSectorConcentration(decimal totalValue)
        {
            return new List<ConcentrationItem>
            {
                new ConcentrationItem { Name = "Technology", Value = totalValue * 0.35m, Percentage = 35 },
                new ConcentrationItem { Name = "Financial Services", Value = totalValue * 0.25m, Percentage = 25 },
                new ConcentrationItem { Name = "Transportation", Value = totalValue * 0.15m, Percentage = 15 },
                new ConcentrationItem { Name = "Consumer Discretionary", Value = totalValue * 0.15m, Percentage = 15 },
                new ConcentrationItem { Name = "Other", Value = totalValue * 0.10m, Percentage = 10 }
            };
        }

        private List<ConcentrationItem> GenerateMockGeographicConcentration(decimal totalValue)
        {
            return new List<ConcentrationItem>
            {
                new ConcentrationItem { Name = "Canada", Value = totalValue * 0.45m, Percentage = 45 },
                new ConcentrationItem { Name = "United States", Value = totalValue * 0.40m, Percentage = 40 },
                new ConcentrationItem { Name = "Europe", Value = totalValue * 0.10m, Percentage = 10 },
                new ConcentrationItem { Name = "Asia Pacific", Value = totalValue * 0.05m, Percentage = 5 }
            };
        }

        private decimal CalculateAverageDailyVolume(List<Holding> holdings)
        {
            // Mock calculation - in production, this would use actual trading volume data
            return holdings.Sum(h => h.MarketValue) * 0.02m; // Assume 2% daily turnover
        }

        private decimal CalculateLiquidityScore(List<Holding> holdings)
        {
            // Mock liquidity score from 0-100 (100 = most liquid)
            return 75.5m; // Assume reasonably liquid portfolio
        }

        private decimal CalculateDaysToLiquidate(List<Holding> holdings)
        {
            // Mock calculation - days to liquidate entire portfolio at 20% of daily volume
            return 3.5m; // Assume 3.5 days to liquidate
        }

        private string GetSecuritySymbol(Guid securityId)
        {
            var symbols = new[] { "SHOP.TO", "RY.TO", "CNR.TO", "GOOGL", "MSFT" };
            var hash = Math.Abs(securityId.GetHashCode());
            return symbols[hash % symbols.Length];
        }

        #endregion
    }

    #region DTOs

    public class VaRResult
    {
        public Guid PortfolioId { get; set; }
        public decimal PortfolioValue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public int TimeHorizon { get; set; }
        public decimal ParametricVaR { get; set; }
        public decimal HistoricalVaR { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class ComprehensiveRiskResult
    {
        public Guid PortfolioId { get; set; }
        public decimal PortfolioValue { get; set; }
        public decimal Volatility { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal SortinoRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal VaR95 { get; set; }
        public decimal CVaR95 { get; set; }
        public decimal Beta { get; set; }
        public decimal Alpha { get; set; }
        public decimal TrackingError { get; set; }
        public decimal InformationRatio { get; set; }
        public decimal ConcentrationRisk { get; set; }
        public decimal LargestPosition { get; set; }
        public decimal Top10Concentration { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class CorrelationMatrixResult
    {
        public Guid PortfolioId { get; set; }
        public int Period { get; set; }
        public List<SecurityInfo> Securities { get; set; } = new();
        public decimal[,] CorrelationMatrix { get; set; } = new decimal[0,0];
        public DateTime CalculationDate { get; set; }
    }

    public class ConcentrationAnalysisResult
    {
        public Guid PortfolioId { get; set; }
        public decimal TotalValue { get; set; }
        public List<ConcentrationItem> SecurityConcentration { get; set; } = new();
        public List<ConcentrationItem> SectorConcentration { get; set; } = new();
        public List<ConcentrationItem> GeographicConcentration { get; set; } = new();
        public List<ConcentrationItem> CurrencyConcentration { get; set; } = new();
        public decimal HerfindahlIndex { get; set; }
        public decimal EffectiveSecurities { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class LiquidityRiskResult
    {
        public Guid PortfolioId { get; set; }
        public decimal TotalValue { get; set; }
        public decimal HighLiquidity { get; set; }
        public decimal MediumLiquidity { get; set; }
        public decimal LowLiquidity { get; set; }
        public decimal AverageDailyVolume { get; set; }
        public decimal LiquidityScore { get; set; }
        public decimal DaysToLiquidate { get; set; }
        public decimal EstimatedMarketImpact1Day { get; set; }
        public decimal EstimatedMarketImpact5Days { get; set; }
        public decimal EstimatedMarketImpact20Days { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class SecurityInfo
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class ConcentrationItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }

    #endregion
}