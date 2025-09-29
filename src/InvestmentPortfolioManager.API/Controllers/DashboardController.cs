using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Application.Services.Reports;

namespace InvestmentPortfolioManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard summary with key metrics
        /// </summary>
        /// <param name="portfolioId">Optional portfolio ID to filter by specific portfolio</param>
        /// <returns>Dashboard summary with total assets, returns, and top performers</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] int? portfolioId = null)
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync(portfolioId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get performance dashboard with detailed performance metrics
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <param name="months">Number of months of performance history (default: 12)</param>
        /// <returns>Performance dashboard with history, benchmarks, and return breakdown</returns>
        [HttpGet("performance/{portfolioId}")]
        public async Task<IActionResult> GetPerformanceDashboard(int portfolioId, [FromQuery] int months = 12)
        {
            try
            {
                var dashboard = await _dashboardService.GetPerformanceDashboardAsync(portfolioId, months);
                return Ok(dashboard);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get risk dashboard with risk metrics and analysis
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>Risk dashboard with volatility, VaR, and concentration risks</returns>
        [HttpGet("risk/{portfolioId}")]
        public async Task<IActionResult> GetRiskDashboard(int portfolioId)
        {
            try
            {
                var dashboard = await _dashboardService.GetRiskDashboardAsync(portfolioId);
                return Ok(dashboard);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get allocation dashboard with asset, sector, and geographic allocations
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>Allocation dashboard with current vs target allocations</returns>
        [HttpGet("allocation/{portfolioId}")]
        public async Task<IActionResult> GetAllocationDashboard(int portfolioId)
        {
            try
            {
                var dashboard = await _dashboardService.GetAllocationDashboardAsync(portfolioId);
                return Ok(dashboard);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get transactions dashboard with transaction analytics
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <param name="days">Number of days of transaction history (default: 30)</param>
        /// <returns>Transactions dashboard with volume, types, and trading activity</returns>
        [HttpGet("transactions/{portfolioId}")]
        public async Task<IActionResult> GetTransactionsDashboard(int portfolioId, [FromQuery] int days = 30)
        {
            try
            {
                var dashboard = await _dashboardService.GetTransactionsDashboardAsync(portfolioId, days);
                return Ok(dashboard);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get multi-portfolio dashboard comparing multiple portfolios
        /// </summary>
        /// <param name="portfolioIds">Comma-separated list of portfolio IDs</param>
        /// <returns>Multi-portfolio dashboard with consolidated metrics and comparisons</returns>
        [HttpGet("multi-portfolio")]
        public async Task<IActionResult> GetMultiPortfolioDashboard([FromQuery] string portfolioIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(portfolioIds))
                {
                    return BadRequest(new { error = "Portfolio IDs are required" });
                }

                var ids = portfolioIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (!ids.Any())
                {
                    return BadRequest(new { error = "At least one valid portfolio ID is required" });
                }

                var dashboard = await _dashboardService.GetMultiPortfolioDashboardAsync(ids);
                return Ok(dashboard);
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "Invalid portfolio ID format" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get dashboard data for all active portfolios
        /// </summary>
        /// <returns>Combined dashboard view for all active portfolios</returns>
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get performance comparison data for multiple portfolios
        /// </summary>
        /// <param name="portfolioIds">Comma-separated list of portfolio IDs</param>
        /// <param name="months">Number of months for performance comparison (default: 12)</param>
        /// <returns>Performance comparison data for the specified portfolios</returns>
        [HttpGet("performance-comparison")]
        public async Task<IActionResult> GetPerformanceComparison([FromQuery] string portfolioIds, [FromQuery] int months = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(portfolioIds))
                {
                    return BadRequest(new { error = "Portfolio IDs are required" });
                }

                var ids = portfolioIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (!ids.Any())
                {
                    return BadRequest(new { error = "At least one valid portfolio ID is required" });
                }

                var multiPortfolioDashboard = await _dashboardService.GetMultiPortfolioDashboardAsync(ids);
                
                // Extract performance comparison data
                var result = new
                {
                    TotalValue = multiPortfolioDashboard.TotalValue,
                    WeightedReturn = multiPortfolioDashboard.WeightedReturn,
                    WeightedVolatility = multiPortfolioDashboard.WeightedVolatility,
                    Portfolios = multiPortfolioDashboard.PortfolioSummaries,
                    PerformanceComparisons = multiPortfolioDashboard.PerformanceComparisons,
                    RiskComparisons = multiPortfolioDashboard.RiskComparisons
                };

                return Ok(result);
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "Invalid portfolio ID format" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get consolidated risk metrics across multiple portfolios
        /// </summary>
        /// <param name="portfolioIds">Comma-separated list of portfolio IDs</param>
        /// <returns>Consolidated risk analysis for the specified portfolios</returns>
        [HttpGet("risk-overview")]
        public async Task<IActionResult> GetRiskOverview([FromQuery] string portfolioIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(portfolioIds))
                {
                    return BadRequest(new { error = "Portfolio IDs are required" });
                }

                var ids = portfolioIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (!ids.Any())
                {
                    return BadRequest(new { error = "At least one valid portfolio ID is required" });
                }

                var multiPortfolioDashboard = await _dashboardService.GetMultiPortfolioDashboardAsync(ids);
                
                // Extract risk overview data
                var result = new
                {
                    WeightedVolatility = multiPortfolioDashboard.WeightedVolatility,
                    ConsolidatedAllocations = multiPortfolioDashboard.ConsolidatedAllocations,
                    RiskComparisons = multiPortfolioDashboard.RiskComparisons,
                    PortfolioRiskLevels = multiPortfolioDashboard.PortfolioSummaries.Select(p => new 
                    { 
                        p.Name, 
                        p.RiskLevel, 
                        p.Volatility 
                    })
                };

                return Ok(result);
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "Invalid portfolio ID format" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}