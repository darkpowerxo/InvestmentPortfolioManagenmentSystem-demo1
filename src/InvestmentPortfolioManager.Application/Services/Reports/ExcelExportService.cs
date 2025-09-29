using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Application.Services.Contracts;
using Microsoft.Extensions.Localization;
using System.Drawing;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IPositionService _positionService;
        private readonly ITransactionService _transactionService;
        private readonly IPerformanceMetricsService _performanceService;
        private readonly IStringLocalizer<ExcelExportService> _localizer;

        public ExcelExportService(
            IPortfolioService portfolioService,
            IPositionService positionService,
            ITransactionService transactionService,
            IPerformanceMetricsService performanceService,
            IStringLocalizer<ExcelExportService> localizer)
        {
            _portfolioService = portfolioService;
            _positionService = positionService;
            _transactionService = transactionService;
            _performanceService = performanceService;
            _localizer = localizer;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> ExportPortfolioDataAsync(int portfolioId, DateTime asOfDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var recentTransactions = await _transactionService.GetRecentTransactionsAsync(portfolioId, 50);
            var performanceMetrics = await _performanceService.GetPerformanceMetricsAsync(portfolioId, asOfDate);

            using var package = new ExcelPackage();

            // Create worksheets
            CreatePortfolioSummaryWorksheet(package, portfolio, positions, performanceMetrics, asOfDate);
            CreatePositionsWorksheet(package, positions);
            CreateTransactionsWorksheet(package, recentTransactions);
            
            if (performanceMetrics != null)
            {
                CreatePerformanceWorksheet(package, performanceMetrics);
            }

            CreateChartsWorksheet(package, positions, portfolio);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportTransactionsAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var transactions = await _transactionService.GetTransactionsByPortfolioAsync(portfolioId, startDate, endDate);

            using var package = new ExcelPackage();
            CreateTransactionsWorksheet(package, transactions);
            CreateTransactionSummaryWorksheet(package, transactions, startDate, endDate);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportPerformanceMetricsAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var performanceHistory = await _performanceService.GetPerformanceHistoryAsync(portfolioId, startDate, endDate);

            using var package = new ExcelPackage();
            CreatePerformanceHistoryWorksheet(package, performanceHistory);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportPositionsAsync(int portfolioId, DateTime asOfDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);

            using var package = new ExcelPackage();
            CreatePositionsWorksheet(package, positions);
            CreateSectorAllocationWorksheet(package, positions);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportMultiPortfolioSummaryAsync(IEnumerable<int> portfolioIds, DateTime asOfDate)
        {
            var portfolios = new List<Portfolio>();
            var allPositions = new List<Position>();

            foreach (var id in portfolioIds)
            {
                var portfolio = await _portfolioService.GetByIdAsync(id);
                if (portfolio != null)
                {
                    portfolios.Add(portfolio);
                    var positions = await _positionService.GetPortfolioPositionsAsync(id);
                    allPositions.AddRange(positions);
                }
            }

            using var package = new ExcelPackage();
            CreateMultiPortfolioSummaryWorksheet(package, portfolios, asOfDate);
            CreateConsolidatedPositionsWorksheet(package, allPositions);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportRiskAnalysisAsync(int portfolioId, DateTime asOfDate)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var performanceMetrics = await _performanceService.GetPerformanceMetricsAsync(portfolioId, asOfDate);

            using var package = new ExcelPackage();
            CreateRiskAnalysisWorksheet(package, positions, performanceMetrics);
            CreateSectorAllocationWorksheet(package, positions);

            return package.GetAsByteArray();
        }

        private void CreatePortfolioSummaryWorksheet(ExcelPackage package, Portfolio portfolio, 
            IEnumerable<Position> positions, PerformanceMetric? performanceMetrics, DateTime asOfDate)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["PortfolioSummary"]);

            // Header
            worksheet.Cells["A1"].Value = "CDPQ - Caisse de dépôt et placement du Québec";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1:F1"].Merge = true;

            worksheet.Cells["A2"].Value = _localizer["PortfolioSummary"];
            worksheet.Cells["A2"].Style.Font.Size = 14;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2:F2"].Merge = true;

            // Portfolio Information
            var row = 4;
            AddKeyValuePair(worksheet, ref row, _localizer["PortfolioName"], portfolio.Name);
            AddKeyValuePair(worksheet, ref row, _localizer["Manager"], $"Manager {portfolio.ManagerId}");
            AddKeyValuePair(worksheet, ref row, _localizer["PortfolioType"], portfolio.Type.ToString());
            AddKeyValuePair(worksheet, ref row, _localizer["RiskLevel"], portfolio.RiskLevel.ToString());
            AddKeyValuePair(worksheet, ref row, _localizer["BaseCurrency"], portfolio.BaseCurrency);
            AddKeyValuePair(worksheet, ref row, _localizer["AsOfDate"], asOfDate.ToString("yyyy-MM-dd"));

            row++;

            // Portfolio Metrics
            var totalValue = positions.Sum(p => p.MarketValue);
            var equityValue = positions.Where(p => p.Security?.Type == SecurityType.Equity).Sum(p => p.MarketValue);
            var bondValue = positions.Where(p => p.Security?.Type == SecurityType.Bond).Sum(p => p.MarketValue);

            AddKeyValuePair(worksheet, ref row, _localizer["TotalValue"], FormatCurrency(totalValue, portfolio.BaseCurrency));
            AddKeyValuePair(worksheet, ref row, _localizer["EquityAllocation"], $"{(totalValue > 0 ? equityValue / totalValue * 100 : 0):F1}%");
            AddKeyValuePair(worksheet, ref row, _localizer["BondAllocation"], $"{(totalValue > 0 ? bondValue / totalValue * 100 : 0):F1}%");

            if (performanceMetrics != null)
            {
                row++;
                AddKeyValuePair(worksheet, ref row, _localizer["YTDReturn"], $"{performanceMetrics.YearToDateReturn:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["Volatility"], $"{performanceMetrics.Volatility:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["SharpeRatio"], $"{performanceMetrics.SharpeRatio:F2}");
            }

            // Format the worksheet
            worksheet.Cells["A:F"].AutoFitColumns();
            ApplyHeaderStyle(worksheet.Cells["A1:F2"]);
        }

        private void CreatePositionsWorksheet(ExcelPackage package, IEnumerable<Position> positions)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["Holdings"]);

            // Headers
            var headers = new[] {
                _localizer["Security"],
                _localizer["Symbol"],
                _localizer["Sector"],
                _localizer["Quantity"],
                _localizer["AveragePrice"],
                _localizer["CurrentPrice"],
                _localizer["MarketValue"],
                _localizer["Allocation%"],
                _localizer["UnrealizedGL"],
                _localizer["UnrealizedGL%"]
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Data
            var totalValue = positions.Sum(p => p.MarketValue);
            var row = 2;

            foreach (var position in positions.OrderByDescending(p => p.MarketValue))
            {
                worksheet.Cells[row, 1].Value = position.Security?.Name ?? "Unknown";
                worksheet.Cells[row, 2].Value = position.Security?.Symbol ?? "N/A";
                worksheet.Cells[row, 3].Value = position.Security?.Sector ?? "N/A";
                worksheet.Cells[row, 4].Value = position.Quantity;
                worksheet.Cells[row, 5].Value = position.AveragePrice;
                worksheet.Cells[row, 6].Value = position.CurrentPrice;
                worksheet.Cells[row, 7].Value = position.MarketValue;
                worksheet.Cells[row, 8].Value = totalValue > 0 ? position.MarketValue / totalValue : 0;
                worksheet.Cells[row, 9].Value = position.UnrealizedGainLoss;
                worksheet.Cells[row, 10].Value = position.UnrealizedGainLossPercent / 100;

                // Format percentage columns
                worksheet.Cells[row, 8].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 10].Style.Numberformat.Format = "0.00%";

                // Format currency columns
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 9].Style.Numberformat.Format = "$#,##0.00";

                row++;
            }

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, headers.Length]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreateTransactionsWorksheet(ExcelPackage package, IEnumerable<Transaction> transactions)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["Transactions"]);

            // Headers
            var headers = new[] {
                _localizer["Date"],
                _localizer["Security"],
                _localizer["Type"],
                _localizer["Quantity"],
                _localizer["Price"],
                _localizer["Amount"],
                _localizer["Commission"],
                _localizer["Currency"]
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Data
            var row = 2;
            foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
            {
                worksheet.Cells[row, 1].Value = transaction.TransactionDate;
                worksheet.Cells[row, 2].Value = transaction.Security?.Symbol ?? "N/A";
                worksheet.Cells[row, 3].Value = transaction.TransactionType.ToString();
                worksheet.Cells[row, 4].Value = transaction.Quantity;
                worksheet.Cells[row, 5].Value = transaction.Price;
                worksheet.Cells[row, 6].Value = transaction.NetAmount;
                worksheet.Cells[row, 7].Value = transaction.Commission;
                worksheet.Cells[row, 8].Value = transaction.Currency;

                // Format date column
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd";

                // Format currency columns
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

                row++;
            }

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, headers.Length]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreatePerformanceWorksheet(ExcelPackage package, PerformanceMetric performanceMetrics)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["Performance"]);

            var row = 1;
            AddKeyValuePair(worksheet, ref row, _localizer["TotalReturn"], $"{performanceMetrics.TotalReturn:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["YearToDateReturn"], $"{performanceMetrics.YearToDateReturn:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["OneYearReturn"], $"{performanceMetrics.OneYearReturn:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["ThreeYearReturn"], $"{performanceMetrics.ThreeYearReturn:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["FiveYearReturn"], $"{performanceMetrics.FiveYearReturn:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["SinceInceptionReturn"], $"{performanceMetrics.SinceInceptionReturn:P2}");

            row++;
            AddKeyValuePair(worksheet, ref row, _localizer["Volatility"], $"{performanceMetrics.Volatility:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["SharpeRatio"], $"{performanceMetrics.SharpeRatio:F2}");
            AddKeyValuePair(worksheet, ref row, _localizer["SortinoRatio"], $"{performanceMetrics.SortinoRatio:F2}");
            AddKeyValuePair(worksheet, ref row, _localizer["Beta"], $"{performanceMetrics.Beta:F2}");
            AddKeyValuePair(worksheet, ref row, _localizer["Alpha"], $"{performanceMetrics.Alpha:F2}");

            row++;
            AddKeyValuePair(worksheet, ref row, _localizer["MaxDrawdown"], $"{performanceMetrics.MaxDrawdown:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["VaR95"], $"{performanceMetrics.VaR95:P2}");
            AddKeyValuePair(worksheet, ref row, _localizer["VaR99"], $"{performanceMetrics.VaR99:P2}");

            worksheet.Cells.AutoFitColumns();
        }

        private void CreateChartsWorksheet(ExcelPackage package, IEnumerable<Position> positions, Portfolio portfolio)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["Charts"]);

            // Sector allocation data
            var sectorGroups = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other")
                .Select(g => new { Sector = g.Key, Value = g.Sum(p => p.MarketValue) })
                .OrderByDescending(g => g.Value)
                .Take(10)
                .ToList();

            if (sectorGroups.Any())
            {
                // Create data for pie chart
                worksheet.Cells["A1"].Value = _localizer["Sector"];
                worksheet.Cells["B1"].Value = _localizer["Value"];

                for (int i = 0; i < sectorGroups.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = sectorGroups[i].Sector;
                    worksheet.Cells[i + 2, 2].Value = sectorGroups[i].Value;
                }

                // Create pie chart
                var chart = worksheet.Drawings.AddChart("SectorAllocation", eChartType.Pie) as ExcelPieChart;
                chart.Title.Text = _localizer["SectorAllocation"];
                chart.SetPosition(1, 0, 4, 0);
                chart.SetSize(600, 400);

                var series = chart.Series.Add(worksheet.Cells[2, 2, sectorGroups.Count + 1, 2], 
                                              worksheet.Cells[2, 1, sectorGroups.Count + 1, 1]);
                series.Header = _localizer["SectorAllocation"];

                chart.DataLabel.ShowPercent = true;
                chart.DataLabel.ShowCategory = true;
            }

            worksheet.Cells.AutoFitColumns();
        }

        private void CreateTransactionSummaryWorksheet(ExcelPackage package, IEnumerable<Transaction> transactions, 
            DateTime startDate, DateTime endDate)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["TransactionSummary"]);

            // Summary by type
            var buyTransactions = transactions.Where(t => t.TransactionType == TransactionType.Buy);
            var sellTransactions = transactions.Where(t => t.TransactionType == TransactionType.Sell);
            var dividendTransactions = transactions.Where(t => t.TransactionType == TransactionType.Dividend);

            var row = 1;
            worksheet.Cells[row, 1].Value = _localizer["TransactionSummary"];
            worksheet.Cells[row, 1].Style.Font.Size = 14;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 4].Merge = true;

            row += 2;
            worksheet.Cells[row, 1].Value = _localizer["Period"];
            worksheet.Cells[row, 2].Value = $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}";

            row += 2;

            // Headers
            worksheet.Cells[row, 1].Value = _localizer["TransactionType"];
            worksheet.Cells[row, 2].Value = _localizer["Count"];
            worksheet.Cells[row, 3].Value = _localizer["TotalAmount"];
            worksheet.Cells[row, 4].Value = _localizer["Commission"];

            ApplyHeaderStyle(worksheet.Cells[row, 1, row, 4]);
            row++;

            // Data
            AddTransactionSummaryRow(worksheet, ref row, _localizer["Purchases"], buyTransactions);
            AddTransactionSummaryRow(worksheet, ref row, _localizer["Sales"], sellTransactions);
            AddTransactionSummaryRow(worksheet, ref row, _localizer["Dividends"], dividendTransactions);

            worksheet.Cells.AutoFitColumns();
        }

        private void CreatePerformanceHistoryWorksheet(ExcelPackage package, IEnumerable<PerformanceMetric> performanceHistory)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["PerformanceHistory"]);

            // Headers
            var headers = new[] {
                _localizer["Date"],
                _localizer["TotalReturn"],
                _localizer["MonthlyReturn"],
                _localizer["YTDReturn"],
                _localizer["Volatility"],
                _localizer["SharpeRatio"],
                _localizer["MaxDrawdown"]
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Data
            var row = 2;
            foreach (var metric in performanceHistory.OrderBy(p => p.CalculationDate))
            {
                worksheet.Cells[row, 1].Value = metric.CalculationDate;
                worksheet.Cells[row, 2].Value = metric.TotalReturn;
                worksheet.Cells[row, 3].Value = metric.MonthlyReturn;
                worksheet.Cells[row, 4].Value = metric.YearToDateReturn;
                worksheet.Cells[row, 5].Value = metric.Volatility;
                worksheet.Cells[row, 6].Value = metric.SharpeRatio;
                worksheet.Cells[row, 7].Value = metric.MaxDrawdown;

                // Format date column
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd";

                // Format percentage columns
                for (int col = 2; col <= 5; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "0.00%";
                }
                worksheet.Cells[row, 7].Style.Numberformat.Format = "0.00%";

                row++;
            }

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, headers.Length]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreateSectorAllocationWorksheet(ExcelPackage package, IEnumerable<Position> positions)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["SectorAllocation"]);

            var sectorGroups = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other")
                .Select(g => new { Sector = g.Key, Value = g.Sum(p => p.MarketValue), Count = g.Count() })
                .OrderByDescending(g => g.Value);

            // Headers
            worksheet.Cells[1, 1].Value = _localizer["Sector"];
            worksheet.Cells[1, 2].Value = _localizer["Value"];
            worksheet.Cells[1, 3].Value = _localizer["Allocation%"];
            worksheet.Cells[1, 4].Value = _localizer["Holdings"];

            var totalValue = sectorGroups.Sum(g => g.Value);
            var row = 2;

            foreach (var group in sectorGroups)
            {
                worksheet.Cells[row, 1].Value = group.Sector;
                worksheet.Cells[row, 2].Value = group.Value;
                worksheet.Cells[row, 3].Value = totalValue > 0 ? group.Value / totalValue : 0;
                worksheet.Cells[row, 4].Value = group.Count;

                // Format currency and percentage
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 3].Style.Numberformat.Format = "0.00%";

                row++;
            }

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, 4]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreateMultiPortfolioSummaryWorksheet(ExcelPackage package, IEnumerable<Portfolio> portfolios, DateTime asOfDate)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["MultiPortfolioSummary"]);

            // Headers
            worksheet.Cells[1, 1].Value = _localizer["Portfolio"];
            worksheet.Cells[1, 2].Value = _localizer["Type"];
            worksheet.Cells[1, 3].Value = _localizer["CurrentValue"];
            worksheet.Cells[1, 4].Value = _localizer["RiskLevel"];
            worksheet.Cells[1, 5].Value = _localizer["Currency"];

            var row = 2;
            foreach (var portfolio in portfolios.OrderBy(p => p.Name))
            {
                worksheet.Cells[row, 1].Value = portfolio.Name;
                worksheet.Cells[row, 2].Value = portfolio.Type.ToString();
                worksheet.Cells[row, 3].Value = portfolio.CurrentValue;
                worksheet.Cells[row, 4].Value = portfolio.RiskLevel;
                worksheet.Cells[row, 5].Value = portfolio.BaseCurrency;

                // Format currency
                worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";

                row++;
            }

            // Add totals
            worksheet.Cells[row, 1].Value = _localizer["Total"];
            worksheet.Cells[row, 3].Value = portfolios.Sum(p => p.CurrentValue);
            worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
            ApplyHeaderStyle(worksheet.Cells[row, 1, row, 5]);

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, 5]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreateConsolidatedPositionsWorksheet(ExcelPackage package, IEnumerable<Position> positions)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["ConsolidatedPositions"]);

            // Group by security across portfolios
            var consolidatedPositions = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Symbol)
                .Select(g => new
                {
                    Symbol = g.Key,
                    Name = g.First().Security!.Name,
                    Sector = g.First().Security!.Sector,
                    TotalQuantity = g.Sum(p => p.Quantity),
                    WeightedAvgPrice = g.Sum(p => p.AveragePrice * p.Quantity) / g.Sum(p => p.Quantity),
                    CurrentPrice = g.First().CurrentPrice,
                    TotalMarketValue = g.Sum(p => p.MarketValue),
                    PortfolioCount = g.Count()
                })
                .OrderByDescending(p => p.TotalMarketValue);

            // Headers
            var headers = new[] {
                _localizer["Symbol"],
                _localizer["Security"],
                _localizer["Sector"],
                _localizer["TotalQuantity"],
                _localizer["WeightedAvgPrice"],
                _localizer["CurrentPrice"],
                _localizer["TotalMarketValue"],
                _localizer["Portfolios"]
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            var row = 2;
            foreach (var position in consolidatedPositions)
            {
                worksheet.Cells[row, 1].Value = position.Symbol;
                worksheet.Cells[row, 2].Value = position.Name;
                worksheet.Cells[row, 3].Value = position.Sector;
                worksheet.Cells[row, 4].Value = position.TotalQuantity;
                worksheet.Cells[row, 5].Value = position.WeightedAvgPrice;
                worksheet.Cells[row, 6].Value = position.CurrentPrice;
                worksheet.Cells[row, 7].Value = position.TotalMarketValue;
                worksheet.Cells[row, 8].Value = position.PortfolioCount;

                // Format currency columns
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

                row++;
            }

            // Format headers and auto-fit columns
            ApplyHeaderStyle(worksheet.Cells[1, 1, 1, headers.Length]);
            worksheet.Cells.AutoFitColumns();
        }

        private void CreateRiskAnalysisWorksheet(ExcelPackage package, IEnumerable<Position> positions, 
            PerformanceMetric? performanceMetrics)
        {
            var worksheet = package.Workbook.Worksheets.Add(_localizer["RiskAnalysis"]);

            var row = 1;
            worksheet.Cells[row, 1].Value = _localizer["RiskAnalysis"];
            worksheet.Cells[row, 1].Style.Font.Size = 14;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 4].Merge = true;

            row += 2;

            if (performanceMetrics != null)
            {
                AddKeyValuePair(worksheet, ref row, _localizer["Volatility"], $"{performanceMetrics.Volatility:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["Beta"], $"{performanceMetrics.Beta:F2}");
                AddKeyValuePair(worksheet, ref row, _localizer["VaR95"], $"{performanceMetrics.VaR95:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["VaR99"], $"{performanceMetrics.VaR99:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["MaxDrawdown"], $"{performanceMetrics.MaxDrawdown:P2}");
                AddKeyValuePair(worksheet, ref row, _localizer["SharpeRatio"], $"{performanceMetrics.SharpeRatio:F2}");
                AddKeyValuePair(worksheet, ref row, _localizer["SortinoRatio"], $"{performanceMetrics.SortinoRatio:F2}");
            }

            // Risk concentration analysis
            row += 2;
            worksheet.Cells[row, 1].Value = _localizer["ConcentrationRisk"];
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            var totalValue = positions.Sum(p => p.MarketValue);
            var top10Holdings = positions.OrderByDescending(p => p.MarketValue).Take(10);
            var top10Concentration = top10Holdings.Sum(p => p.MarketValue) / totalValue;

            AddKeyValuePair(worksheet, ref row, _localizer["Top10Concentration"], $"{top10Concentration:P1}");

            worksheet.Cells.AutoFitColumns();
        }

        private void AddKeyValuePair(ExcelWorksheet worksheet, ref int row, string key, string value)
        {
            worksheet.Cells[row, 1].Value = key;
            worksheet.Cells[row, 2].Value = value;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;
        }

        private void AddTransactionSummaryRow(ExcelWorksheet worksheet, ref int row, string type, 
            IEnumerable<Transaction> transactions)
        {
            worksheet.Cells[row, 1].Value = type;
            worksheet.Cells[row, 2].Value = transactions.Count();
            worksheet.Cells[row, 3].Value = transactions.Sum(t => t.NetAmount);
            worksheet.Cells[row, 4].Value = transactions.Sum(t => t.Commission);

            // Format currency columns
            worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
            worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";

            row++;
        }

        private void ApplyHeaderStyle(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private string FormatCurrency(decimal amount, string currency)
        {
            return $"{amount:C} {currency}";
        }
    }
}