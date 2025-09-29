using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InvestmentPortfolioManager.Application.Services.Reports;

namespace InvestmentPortfolioManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IPdfReportService _pdfReportService;

        public ReportsController(IPdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        /// <summary>
        /// Generate portfolio statement PDF
        /// </summary>
        [HttpGet("portfolio/{portfolioId}/statement")]
        public async Task<IActionResult> GeneratePortfolioStatement(
            int portfolioId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;

                var pdfBytes = await _pdfReportService.GeneratePortfolioStatementAsync(portfolioId, start, end);
                
                return File(pdfBytes, "application/pdf", $"portfolio_statement_{portfolioId}_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating portfolio statement: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate performance report PDF
        /// </summary>
        [HttpGet("portfolio/{portfolioId}/performance")]
        public async Task<IActionResult> GeneratePerformanceReport(
            int portfolioId,
            [FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var date = asOfDate ?? DateTime.Now;
                var pdfBytes = await _pdfReportService.GeneratePerformanceReportAsync(portfolioId, date);
                
                return File(pdfBytes, "application/pdf", $"performance_report_{portfolioId}_{date:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating performance report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate transaction summary PDF
        /// </summary>
        [HttpGet("portfolio/{portfolioId}/transactions")]
        public async Task<IActionResult> GenerateTransactionSummary(
            int portfolioId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddMonths(-3);
                var end = endDate ?? DateTime.Now;

                var pdfBytes = await _pdfReportService.GenerateTransactionSummaryAsync(portfolioId, start, end);
                
                return File(pdfBytes, "application/pdf", $"transaction_summary_{portfolioId}_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating transaction summary: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate risk analysis report PDF
        /// </summary>
        [HttpGet("portfolio/{portfolioId}/risk-analysis")]
        public async Task<IActionResult> GenerateRiskAnalysisReport(
            int portfolioId,
            [FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var date = asOfDate ?? DateTime.Now;
                var pdfBytes = await _pdfReportService.GenerateRiskAnalysisReportAsync(portfolioId, date);
                
                return File(pdfBytes, "application/pdf", $"risk_analysis_{portfolioId}_{date:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating risk analysis report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate multi-portfolio summary PDF
        /// </summary>
        [HttpPost("multi-portfolio-summary")]
        public async Task<IActionResult> GenerateMultiPortfolioSummary(
            [FromBody] MultiPortfolioSummaryRequest request)
        {
            try
            {
                if (request.PortfolioIds == null || !request.PortfolioIds.Any())
                {
                    return BadRequest("Portfolio IDs are required");
                }

                var date = request.AsOfDate ?? DateTime.Now;
                var pdfBytes = await _pdfReportService.GenerateMultiPortfolioSummaryAsync(request.PortfolioIds, date);
                
                return File(pdfBytes, "application/pdf", $"multi_portfolio_summary_{date:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating multi-portfolio summary: {ex.Message}");
            }
        }
    }

    public class MultiPortfolioSummaryRequest
    {
        public IEnumerable<int> PortfolioIds { get; set; } = new List<int>();
        public DateTime? AsOfDate { get; set; }
    }
}