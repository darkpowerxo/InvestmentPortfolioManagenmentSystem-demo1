using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Application.Services.Contracts;
using Microsoft.Extensions.Localization;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public class PdfReportService : IPdfReportService
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IPositionService _positionService;
        private readonly ITransactionService _transactionService;
        private readonly IPerformanceMetricsService _performanceService;
        private readonly IStringLocalizer<PdfReportService> _localizer;

        public PdfReportService(
            IPortfolioService portfolioService,
            IPositionService positionService,
            ITransactionService transactionService,
            IPerformanceMetricsService performanceService,
            IStringLocalizer<PdfReportService> localizer)
        {
            _portfolioService = portfolioService;
            _positionService = positionService;
            _transactionService = transactionService;
            _performanceService = performanceService;
            _localizer = localizer;
        }

        public async Task<byte[]> GeneratePortfolioStatementAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var transactions = await _transactionService.GetTransactionsByPortfolioAsync(portfolioId, startDate, endDate);

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add CDPQ header and logo section
            AddCdpqHeader(document);

            // Add title
            document.Add(new Paragraph(_localizer["PortfolioStatement"])
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add portfolio information
            AddPortfolioInformation(document, portfolio, startDate, endDate);

            // Add portfolio summary
            AddPortfolioSummary(document, portfolio, positions);

            // Add positions table
            AddPositionsTable(document, positions);

            // Add transactions table
            AddTransactionsTable(document, transactions);

            // Add footer
            AddCdpqFooter(document);

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GeneratePerformanceReportAsync(int portfolioId, DateTime asOfDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var performanceMetrics = await _performanceService.GetPerformanceMetricsAsync(portfolioId, asOfDate);

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add CDPQ header
            AddCdpqHeader(document);

            // Add title
            document.Add(new Paragraph(_localizer["PerformanceReport"])
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add portfolio information
            AddPortfolioInformation(document, portfolio, asOfDate, asOfDate);

            // Add performance metrics
            AddPerformanceMetrics(document, performanceMetrics);

            // Add footer
            AddCdpqFooter(document);

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateTransactionSummaryAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var transactions = await _transactionService.GetTransactionsByPortfolioAsync(portfolioId, startDate, endDate);

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add CDPQ header
            AddCdpqHeader(document);

            // Add title
            document.Add(new Paragraph(_localizer["TransactionSummary"])
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add portfolio information
            AddPortfolioInformation(document, portfolio, startDate, endDate);

            // Add transaction summary
            AddTransactionSummary(document, transactions);

            // Add transactions table
            AddTransactionsTable(document, transactions);

            // Add footer
            AddCdpqFooter(document);

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateRiskAnalysisReportAsync(int portfolioId, DateTime asOfDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var performanceMetrics = await _performanceService.GetPerformanceMetricsAsync(portfolioId, asOfDate);

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add CDPQ header
            AddCdpqHeader(document);

            // Add title
            document.Add(new Paragraph(_localizer["RiskAnalysisReport"])
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add portfolio information
            AddPortfolioInformation(document, portfolio, asOfDate, asOfDate);

            // Add risk metrics
            AddRiskMetrics(document, performanceMetrics);

            // Add sector allocation
            AddSectorAllocation(document, positions);

            // Add footer
            AddCdpqFooter(document);

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateMultiPortfolioSummaryAsync(IEnumerable<int> portfolioIds, DateTime asOfDate)
        {
            var portfolios = new List<Portfolio>();
            foreach (var id in portfolioIds)
            {
                var portfolio = await _portfolioService.GetByIdAsync(id);
                if (portfolio != null)
                    portfolios.Add(portfolio);
            }

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add CDPQ header
            AddCdpqHeader(document);

            // Add title
            document.Add(new Paragraph(_localizer["MultiPortfolioSummary"])
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add summary table
            AddMultiPortfolioSummaryTable(document, portfolios, asOfDate);

            // Add footer
            AddCdpqFooter(document);

            document.Close();
            return stream.ToArray();
        }

        private void AddCdpqHeader(Document document)
        {
            var headerTable = new Table(2).UseAllAvailableWidth();
            
            // Left side - CDPQ logo/title
            var cdpqCell = new Cell()
                .Add(new Paragraph("CDPQ")
                    .SetFontSize(24)
                    .SetBold()
                    .SetFontColor(ColorConstants.BLUE))
                .Add(new Paragraph(_localizer["CaisseDeDepotEtPlacement"])
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY))
                .SetBorder(Border.NO_BORDER);

            // Right side - Date and report info
            var dateCell = new Cell()
                .Add(new Paragraph($"{_localizer["GeneratedOn"]}: {DateTime.Now:yyyy-MM-dd HH:mm}")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.TOP);

            headerTable.AddCell(cdpqCell);
            headerTable.AddCell(dateCell);

            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(20));
        }

        private void AddCdpqFooter(Document document)
        {
            var footerTable = new Table(1).UseAllAvailableWidth();
            var footerCell = new Cell()
                .Add(new Paragraph(_localizer["ConfidentialDocument"])
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY))
                .Add(new Paragraph(_localizer["CdpqAddress"])
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY))
                .SetBorder(Border.NO_BORDER)
                .SetMarginTop(20);

            footerTable.AddCell(footerCell);
            document.Add(footerTable);
        }

        private void AddPortfolioInformation(Document document, Portfolio portfolio, DateTime startDate, DateTime endDate)
        {
            var infoTable = new Table(2).UseAllAvailableWidth();
            
            // Portfolio details
            infoTable.AddCell(CreateInfoCell(_localizer["PortfolioName"], portfolio.Name));
            infoTable.AddCell(CreateInfoCell(_localizer["PortfolioType"], portfolio.Type.ToString()));
            infoTable.AddCell(CreateInfoCell(_localizer["Manager"], $"Manager {portfolio.ManagerId}"));
            infoTable.AddCell(CreateInfoCell(_localizer["RiskLevel"], portfolio.RiskLevel.ToString()));
            infoTable.AddCell(CreateInfoCell(_localizer["BaseCurrency"], portfolio.BaseCurrency));
            infoTable.AddCell(CreateInfoCell(_localizer["CurrentValue"], FormatCurrency(portfolio.CurrentValue, portfolio.BaseCurrency)));
            
            if (startDate != endDate)
            {
                infoTable.AddCell(CreateInfoCell(_localizer["ReportPeriod"], $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}"));
            }
            else
            {
                infoTable.AddCell(CreateInfoCell(_localizer["AsOfDate"], startDate.ToString("yyyy-MM-dd")));
            }

            document.Add(infoTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private Cell CreateInfoCell(string label, string value)
        {
            return new Cell()
                .Add(new Paragraph($"{label}: {value}")
                    .SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3);
        }

        private void AddPortfolioSummary(Document document, Portfolio portfolio, IEnumerable<Position> positions)
        {
            document.Add(new Paragraph(_localizer["PortfolioSummary"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var summaryTable = new Table(4).UseAllAvailableWidth();
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Metric"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Value"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Allocation"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Target"]));

            var totalValue = positions.Sum(p => p.MarketValue);
            var equityValue = positions.Where(p => p.Security?.SecurityType == SecurityType.Equity).Sum(p => p.MarketValue);
            var bondValue = positions.Where(p => p.Security?.SecurityType == SecurityType.Bond).Sum(p => p.MarketValue);

            summaryTable.AddCell(CreateDataCell(_localizer["TotalValue"]));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(totalValue, portfolio.BaseCurrency)));
            summaryTable.AddCell(CreateDataCell("100.0%"));
            summaryTable.AddCell(CreateDataCell("100.0%"));

            summaryTable.AddCell(CreateDataCell(_localizer["Equities"]));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(equityValue, portfolio.BaseCurrency)));
            summaryTable.AddCell(CreateDataCell($"{(totalValue > 0 ? equityValue / totalValue * 100 : 0):F1}%"));
            summaryTable.AddCell(CreateDataCell($"{portfolio.TargetEquityAllocation:F1}%"));

            summaryTable.AddCell(CreateDataCell(_localizer["Bonds"]));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(bondValue, portfolio.BaseCurrency)));
            summaryTable.AddCell(CreateDataCell($"{(totalValue > 0 ? bondValue / totalValue * 100 : 0):F1}%"));
            summaryTable.AddCell(CreateDataCell($"{portfolio.TargetBondAllocation:F1}%"));

            document.Add(summaryTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddPositionsTable(Document document, IEnumerable<Position> positions)
        {
            if (!positions.Any()) return;

            document.Add(new Paragraph(_localizer["Holdings"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var positionsTable = new Table(6).UseAllAvailableWidth();
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Security"]));
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Symbol"]));
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Quantity"]));
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["AveragePrice"]));
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["CurrentPrice"]));
            positionsTable.AddHeaderCell(CreateHeaderCell(_localizer["MarketValue"]));

            foreach (var position in positions.OrderByDescending(p => p.MarketValue))
            {
                positionsTable.AddCell(CreateDataCell(position.Security?.Name ?? "Unknown"));
                positionsTable.AddCell(CreateDataCell(position.Security?.Symbol ?? "N/A"));
                positionsTable.AddCell(CreateDataCell(position.Quantity.ToString("N0")));
                positionsTable.AddCell(CreateDataCell(FormatCurrency(position.AveragePrice, position.Portfolio?.BaseCurrency ?? "CAD")));
                positionsTable.AddCell(CreateDataCell(FormatCurrency(position.CurrentPrice, position.Portfolio?.BaseCurrency ?? "CAD")));
                positionsTable.AddCell(CreateDataCell(FormatCurrency(position.MarketValue, position.Portfolio?.BaseCurrency ?? "CAD")));
            }

            document.Add(positionsTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddTransactionsTable(Document document, IEnumerable<Transaction> transactions)
        {
            if (!transactions.Any()) return;

            document.Add(new Paragraph(_localizer["RecentTransactions"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var transactionsTable = new Table(6).UseAllAvailableWidth();
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Date"]));
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Security"]));
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Type"]));
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Quantity"]));
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Price"]));
            transactionsTable.AddHeaderCell(CreateHeaderCell(_localizer["Amount"]));

            foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate).Take(20))
            {
                transactionsTable.AddCell(CreateDataCell(transaction.TransactionDate.ToString("yyyy-MM-dd")));
                transactionsTable.AddCell(CreateDataCell(transaction.Security?.Symbol ?? "N/A"));
                transactionsTable.AddCell(CreateDataCell(transaction.TransactionType.ToString()));
                transactionsTable.AddCell(CreateDataCell(transaction.Quantity.ToString("N0")));
                transactionsTable.AddCell(CreateDataCell(FormatCurrency(transaction.Price, transaction.Currency)));
                transactionsTable.AddCell(CreateDataCell(FormatCurrency(transaction.NetAmount, transaction.Currency)));
            }

            document.Add(transactionsTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddPerformanceMetrics(Document document, PerformanceMetric? metrics)
        {
            if (metrics == null) return;

            document.Add(new Paragraph(_localizer["PerformanceMetrics"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var metricsTable = new Table(2).UseAllAvailableWidth();
            
            metricsTable.AddCell(CreateInfoCell(_localizer["TotalReturn"], $"{metrics.TotalReturn:P2}"));
            metricsTable.AddCell(CreateInfoCell(_localizer["YearToDateReturn"], $"{metrics.YearToDateReturn:P2}"));
            metricsTable.AddCell(CreateInfoCell(_localizer["OneYearReturn"], $"{metrics.OneYearReturn:P2}"));
            metricsTable.AddCell(CreateInfoCell(_localizer["SharpeRatio"], $"{metrics.SharpeRatio:F2}"));
            metricsTable.AddCell(CreateInfoCell(_localizer["Volatility"], $"{metrics.Volatility:P2}"));
            metricsTable.AddCell(CreateInfoCell(_localizer["MaxDrawdown"], $"{metrics.MaxDrawdown:P2}"));

            document.Add(metricsTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddTransactionSummary(Document document, IEnumerable<Transaction> transactions)
        {
            var buyTransactions = transactions.Where(t => t.TransactionType == TransactionType.Buy);
            var sellTransactions = transactions.Where(t => t.TransactionType == TransactionType.Sell);
            var dividendTransactions = transactions.Where(t => t.TransactionType == TransactionType.Dividend);

            document.Add(new Paragraph(_localizer["TransactionSummary"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var summaryTable = new Table(3).UseAllAvailableWidth();
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["TransactionType"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Count"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["TotalAmount"]));

            summaryTable.AddCell(CreateDataCell(_localizer["Purchases"]));
            summaryTable.AddCell(CreateDataCell(buyTransactions.Count().ToString()));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(buyTransactions.Sum(t => t.NetAmount), "CAD")));

            summaryTable.AddCell(CreateDataCell(_localizer["Sales"]));
            summaryTable.AddCell(CreateDataCell(sellTransactions.Count().ToString()));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(sellTransactions.Sum(t => t.NetAmount), "CAD")));

            summaryTable.AddCell(CreateDataCell(_localizer["Dividends"]));
            summaryTable.AddCell(CreateDataCell(dividendTransactions.Count().ToString()));
            summaryTable.AddCell(CreateDataCell(FormatCurrency(dividendTransactions.Sum(t => t.NetAmount), "CAD")));

            document.Add(summaryTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddRiskMetrics(Document document, PerformanceMetrics? metrics)
        {
            if (metrics == null) return;

            document.Add(new Paragraph(_localizer["RiskMetrics"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var riskTable = new Table(2).UseAllAvailableWidth();
            
            riskTable.AddCell(CreateInfoCell(_localizer["Volatility"], $"{metrics.Volatility:P2}"));
            riskTable.AddCell(CreateInfoCell(_localizer["Beta"], $"{metrics.Beta:F2}"));
            riskTable.AddCell(CreateInfoCell(_localizer["VaR95"], $"{metrics.VaR95:P2}"));
            riskTable.AddCell(CreateInfoCell(_localizer["VaR99"], $"{metrics.VaR99:P2}"));
            riskTable.AddCell(CreateInfoCell(_localizer["MaxDrawdown"], $"{metrics.MaxDrawdown:P2}"));
            riskTable.AddCell(CreateInfoCell(_localizer["SharpeRatio"], $"{metrics.SharpeRatio:F2}"));

            document.Add(riskTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddSectorAllocation(Document document, IEnumerable<Position> positions)
        {
            var sectorGroups = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other")
                .Select(g => new { Sector = g.Key, Value = g.Sum(p => p.MarketValue) })
                .OrderByDescending(g => g.Value);

            if (!sectorGroups.Any()) return;

            document.Add(new Paragraph(_localizer["SectorAllocation"])
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10));

            var sectorTable = new Table(3).UseAllAvailableWidth();
            sectorTable.AddHeaderCell(CreateHeaderCell(_localizer["Sector"]));
            sectorTable.AddHeaderCell(CreateHeaderCell(_localizer["Value"]));
            sectorTable.AddHeaderCell(CreateHeaderCell(_localizer["Allocation"]));

            var totalValue = sectorGroups.Sum(g => g.Value);
            foreach (var group in sectorGroups)
            {
                sectorTable.AddCell(CreateDataCell(group.Sector));
                sectorTable.AddCell(CreateDataCell(FormatCurrency(group.Value, "CAD")));
                sectorTable.AddCell(CreateDataCell($"{(totalValue > 0 ? group.Value / totalValue * 100 : 0):F1}%"));
            }

            document.Add(sectorTable);
            document.Add(new Paragraph().SetMarginBottom(15));
        }

        private void AddMultiPortfolioSummaryTable(Document document, IEnumerable<Portfolio> portfolios, DateTime asOfDate)
        {
            var summaryTable = new Table(5).UseAllAvailableWidth();
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Portfolio"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["Type"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["CurrentValue"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["RiskLevel"]));
            summaryTable.AddHeaderCell(CreateHeaderCell(_localizer["LastUpdated"]));

            foreach (var portfolio in portfolios.OrderBy(p => p.Name))
            {
                summaryTable.AddCell(CreateDataCell(portfolio.Name));
                summaryTable.AddCell(CreateDataCell(portfolio.Type.ToString()));
                summaryTable.AddCell(CreateDataCell(FormatCurrency(portfolio.CurrentValue, portfolio.BaseCurrency)));
                summaryTable.AddCell(CreateDataCell(portfolio.RiskLevel.ToString()));
                summaryTable.AddCell(CreateDataCell(portfolio.UpdatedAt?.ToString("yyyy-MM-dd") ?? "N/A"));
            }

            // Add totals
            var totalValue = portfolios.Sum(p => p.CurrentValue);
            summaryTable.AddCell(CreateHeaderCell(_localizer["Total"]));
            summaryTable.AddCell(CreateDataCell(""));
            summaryTable.AddCell(CreateHeaderCell(FormatCurrency(totalValue, "CAD")));
            summaryTable.AddCell(CreateDataCell(""));
            summaryTable.AddCell(CreateDataCell(""));

            document.Add(summaryTable);
        }

        private Cell CreateHeaderCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFontSize(10)
                    .SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
        }

        private Cell CreateDataCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFontSize(9))
                .SetPadding(3);
        }

        private string FormatCurrency(decimal amount, string currency)
        {
            return $"{amount:C} {currency}";
        }
    }
}