using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Domain.Services;
using InvestmentPortfolioManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Portfolio management API controller
    /// Contrôleur API de gestion de portefeuille
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PortfoliosController : ControllerBase
    {
        private readonly PortfolioAnalyticsService _analyticsService;
        private readonly ILogger<PortfoliosController> _logger;

        public PortfoliosController(
            PortfolioAnalyticsService analyticsService,
            ILogger<PortfoliosController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get portfolio performance analysis
        /// Obtenir l'analyse de performance du portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="startDate">Analysis start date / Date de début d'analyse</param>
        /// <param name="endDate">Analysis end date / Date de fin d'analyse</param>
        /// <returns>Portfolio performance metrics / Métriques de performance du portefeuille</returns>
        [HttpGet("{portfolioId}/performance")]
        [ProducesResponseType(typeof(PortfolioPerformanceResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPortfolioPerformance(
            [FromRoute] Guid portfolioId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting performance for portfolio {PortfolioId}", portfolioId);

                var start = startDate ?? DateTime.Today.AddYears(-1);
                var end = endDate ?? DateTime.Today;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date / La date de début doit être antérieure à la date de fin");
                }

                // Create mock portfolio for demonstration
                var portfolio = CreateMockPortfolio(portfolioId);
                var holdings = CreateMockHoldings(portfolioId);

                var performance = await _analyticsService.CalculatePortfolioPerformanceAsync(
                    portfolio, holdings, start, end);

                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio performance for {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get portfolio asset allocation analysis
        /// Obtenir l'analyse de répartition d'actifs du portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <returns>Asset allocation breakdown / Répartition des actifs</returns>
        [HttpGet("{portfolioId}/allocation")]
        [ProducesResponseType(typeof(AssetAllocationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetAllocation([FromRoute] Guid portfolioId)
        {
            try
            {
                _logger.LogInformation("Getting asset allocation for portfolio {PortfolioId}", portfolioId);

                var holdings = CreateMockHoldings(portfolioId);
                var allocation = await _analyticsService.AnalyzeAssetAllocationAsync(holdings);

                return Ok(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset allocation for {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get portfolio risk analysis
        /// Obtenir l'analyse de risque du portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="confidenceLevel">VaR confidence level (default 95%) / Niveau de confiance VaR (défaut 95%)</param>
        /// <returns>Risk metrics / Métriques de risque</returns>
        [HttpGet("{portfolioId}/risk")]
        [ProducesResponseType(typeof(RiskAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRiskAnalysis(
            [FromRoute] Guid portfolioId,
            [FromQuery, Range(90, 99)] decimal confidenceLevel = 95)
        {
            try
            {
                _logger.LogInformation("Getting risk analysis for portfolio {PortfolioId}", portfolioId);

                var portfolio = CreateMockPortfolio(portfolioId);
                var holdings = CreateMockHoldings(portfolioId);

                var riskAnalysis = await _analyticsService.AnalyzePortfolioRiskAsync(
                    portfolio, holdings, confidenceLevel / 100);

                return Ok(riskAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk analysis for {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Perform stress testing on portfolio
        /// Effectuer des tests de stress sur le portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="request">Stress test scenarios / Scénarios de test de stress</param>
        /// <returns>Stress test results / Résultats du test de stress</returns>
        [HttpPost("{portfolioId}/stress-test")]
        [ProducesResponseType(typeof(StressTestResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PerformStressTest(
            [FromRoute] Guid portfolioId,
            [FromBody] StressTestRequest request)
        {
            try
            {
                _logger.LogInformation("Performing stress test for portfolio {PortfolioId}", portfolioId);

                if (request?.Scenarios == null || !request.Scenarios.Any())
                {
                    return BadRequest("At least one stress test scenario is required / Au moins un scénario de test de stress est requis");
                }

                var portfolio = CreateMockPortfolio(portfolioId);
                var holdings = CreateMockHoldings(portfolioId);

                var stressTestResult = await _analyticsService.PerformStressTestAsync(
                    portfolio, holdings, request.Scenarios);

                return Ok(stressTestResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing stress test for {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get portfolio summary information
        /// Obtenir les informations de synthèse du portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <returns>Portfolio summary / Résumé du portefeuille</returns>
        [HttpGet("{portfolioId}")]
        [ProducesResponseType(typeof(PortfolioSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPortfolioSummary([FromRoute] Guid portfolioId)
        {
            try
            {
                _logger.LogInformation("Getting summary for portfolio {PortfolioId}", portfolioId);

                var portfolio = CreateMockPortfolio(portfolioId);
                var holdings = CreateMockHoldings(portfolioId);

                var summary = new PortfolioSummary
                {
                    Id = portfolio.Id,
                    Name = portfolio.Name,
                    NameFR = portfolio.NameFR,
                    TotalValue = holdings.Sum(h => h.MarketValue),
                    HoldingsCount = holdings.Count,
                    LastUpdated = DateTime.UtcNow,
                    Currency = "CAD"
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio summary for {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        #region Mock Data Helpers

        private Portfolio CreateMockPortfolio(Guid portfolioId)
        {
            // In production, this would come from database
            return new Portfolio(
                "CDPQ Sample Portfolio",
                "Portefeuille échantillon CDPQ",
                Guid.NewGuid()) // ClientId
            {
                Id = portfolioId
            };
        }

        private List<Holding> CreateMockHoldings(Guid portfolioId)
        {
            // In production, this would come from database
            var holdings = new List<Holding>();
            
            // Mock some typical CDPQ-style holdings
            var securities = new[]
            {
                new { Symbol = "SHOP.TO", Name = "Shopify Inc", NameFR = "Shopify Inc", Price = 85.50m, Shares = 1000 },
                new { Symbol = "RY.TO", Name = "Royal Bank of Canada", NameFR = "Banque Royale du Canada", Price = 145.75m, Shares = 500 },
                new { Symbol = "CNR.TO", Name = "Canadian National Railway", NameFR = "Chemin de fer Canadien National", Price = 168.25m, Shares = 300 },
                new { Symbol = "GOOGL", Name = "Alphabet Inc", NameFR = "Alphabet Inc", Price = 2750.00m, Shares = 50 },
                new { Symbol = "MSFT", Name = "Microsoft Corporation", NameFR = "Microsoft Corporation", Price = 415.50m, Shares = 100 }
            };

            foreach (var sec in securities)
            {
                var securityId = Guid.NewGuid();
                var holding = new Holding(portfolioId, securityId, sec.Shares, sec.Price);
                holdings.Add(holding);
            }

            return holdings;
        }

        #endregion
    }

    #region DTOs

    public class StressTestRequest
    {
        [Required]
        public List<StressTestScenarioDto> Scenarios { get; set; } = new();
    }

    public class StressTestScenarioDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string NameFR { get; set; } = string.Empty;
        
        [Required]
        public Dictionary<string, decimal> MarketShocks { get; set; } = new();
    }

    public class PortfolioSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameFR { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public int HoldingsCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    #endregion
}