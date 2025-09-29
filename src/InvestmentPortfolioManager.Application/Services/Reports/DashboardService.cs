using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Application.Services.Contracts;
using Microsoft.Extensions.Localization;

namespace InvestmentPortfolioManager.Application.Services.Reports
{
    public class DashboardService : IDashboardService
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IPositionService _positionService;
        private readonly ITransactionService _transactionService;
        private readonly IPerformanceMetricsService _performanceService;
        private readonly IStringLocalizer<DashboardService> _localizer;

        public DashboardService(
            IPortfolioService portfolioService,
            IPositionService positionService,
            ITransactionService transactionService,
            IPerformanceMetricsService performanceService,
            IStringLocalizer<DashboardService> localizer)
        {
            _portfolioService = portfolioService;
            _positionService = positionService;
            _transactionService = transactionService;
            _performanceService = performanceService;
            _localizer = localizer;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(int? portfolioId = null)
        {
            var portfolios = portfolioId.HasValue 
                ? new[] { await _portfolioService.GetByIdAsync(portfolioId.Value) }.Where(p => p != null).Cast<Portfolio>() 
                : await _portfolioService.GetAllAsync();

            var totalAssets = portfolios.Sum(p => p.CurrentValue);
            var activePortfolios = portfolios.Count(p => p.IsActive);

            var allPositions = new List<Position>();
            var allTransactions = new List<Transaction>();

            foreach (var portfolio in portfolios)
            {
                var positions = await _positionService.GetPortfolioPositionsAsync(portfolio.Id);
                var recentTransactions = await _transactionService.GetRecentTransactionsAsync(portfolio.Id, 10);
                
                allPositions.AddRange(positions);
                allTransactions.AddRange(recentTransactions);
            }

            var totalSecurities = allPositions.Select(p => p.SecurityId).Distinct().Count();

            // Calculate top and bottom performers
            var topPerformers = allPositions
                .Where(p => p.UnrealizedGainLossPercent != 0)
                .OrderByDescending(p => p.UnrealizedGainLossPercent)
                .Take(5)
                .Select(p => new TopPerformer
                {
                    Symbol = p.Security?.Symbol ?? "N/A",
                    Name = p.Security?.Name ?? "Unknown",
                    Return = p.UnrealizedGainLossPercent / 100,
                    Value = p.MarketValue
                })
                .ToList();

            var bottomPerformers = allPositions
                .Where(p => p.UnrealizedGainLossPercent != 0)
                .OrderBy(p => p.UnrealizedGainLossPercent)
                .Take(5)
                .Select(p => new TopPerformer
                {
                    Symbol = p.Security?.Symbol ?? "N/A",
                    Name = p.Security?.Name ?? "Unknown",
                    Return = p.UnrealizedGainLossPercent / 100,
                    Value = p.MarketValue
                })
                .ToList();

            var recentTransactionsSummary = allTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .Select(t => new RecentTransaction
                {
                    Date = t.TransactionDate,
                    Symbol = t.Security?.Symbol ?? "N/A",
                    Type = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    Price = t.Price,
                    Amount = t.NetAmount
                })
                .ToList();

            // Calculate daily change (simplified - would need historical data in real scenario)
            var dailyChange = totalAssets * 0.01m; // Placeholder
            var dailyChangePercent = 0.01m; // Placeholder

            return new DashboardSummary
            {
                TotalAssets = totalAssets,
                TotalReturn = allPositions.Sum(p => p.UnrealizedGainLoss) / totalAssets,
                DailyChange = dailyChange,
                DailyChangePercent = dailyChangePercent,
                ActivePortfolios = activePortfolios,
                TotalSecurities = totalSecurities,
                TopPerformers = topPerformers,
                BottomPerformers = bottomPerformers,
                RecentTransactions = recentTransactionsSummary
            };
        }

        public async Task<PerformanceDashboard> GetPerformanceDashboardAsync(int portfolioId, int months = 12)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-months);

            var performanceHistory = await _performanceService.GetPerformanceHistoryAsync(portfolioId, startDate, endDate);
            var latestMetrics = performanceHistory.OrderByDescending(p => p.CalculationDate).FirstOrDefault();

            var performanceDataPoints = performanceHistory
                .OrderBy(p => p.CalculationDate)
                .Select(p => new PerformanceDataPoint
                {
                    Date = p.CalculationDate,
                    Value = portfolio.CurrentValue * (1 + p.TotalReturn),
                    Return = p.MonthlyReturn,
                    CumulativeReturn = p.TotalReturn
                })
                .ToList();

            // Benchmark comparisons (simplified)
            var benchmarkComparisons = new List<BenchmarkComparison>
            {
                new BenchmarkComparison
                {
                    BenchmarkName = "S&P 500",
                    PortfolioReturn = latestMetrics?.TotalReturn ?? 0,
                    BenchmarkReturn = 0.12m, // Placeholder
                    Alpha = latestMetrics?.Alpha ?? 0,
                    Beta = latestMetrics?.Beta ?? 1
                }
            };

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var returnBreakdown = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other")
                .Select(g => new ReturnBreakdown
                {
                    Component = g.Key,
                    Contribution = g.Sum(p => p.UnrealizedGainLoss) / portfolio.CurrentValue,
                    Weight = g.Sum(p => p.MarketValue) / portfolio.CurrentValue
                })
                .OrderByDescending(r => r.Contribution)
                .Take(10)
                .ToList();

            return new PerformanceDashboard
            {
                PortfolioName = portfolio.Name,
                CurrentValue = portfolio.CurrentValue,
                TotalReturn = latestMetrics?.TotalReturn ?? 0,
                YearToDateReturn = latestMetrics?.YearToDateReturn ?? 0,
                OneYearReturn = latestMetrics?.OneYearReturn ?? 0,
                Volatility = latestMetrics?.Volatility ?? 0,
                SharpeRatio = latestMetrics?.SharpeRatio ?? 0,
                MaxDrawdown = latestMetrics?.MaxDrawdown ?? 0,
                PerformanceHistory = performanceDataPoints,
                BenchmarkComparisons = benchmarkComparisons,
                ReturnBreakdown = returnBreakdown
            };
        }

        public async Task<RiskDashboard> GetRiskDashboardAsync(int portfolioId)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-12);

            var performanceHistory = await _performanceService.GetPerformanceHistoryAsync(portfolioId, startDate, endDate);
            var latestMetrics = performanceHistory.OrderByDescending(p => p.CalculationDate).FirstOrDefault();
            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);

            var riskHistory = performanceHistory
                .OrderBy(p => p.CalculationDate)
                .Select(p => new RiskMetricHistory
                {
                    Date = p.CalculationDate,
                    Volatility = p.Volatility,
                    Beta = p.Beta,
                    VaR95 = p.VaR95,
                    MaxDrawdown = p.MaxDrawdown
                })
                .ToList();

            // Concentration risks
            var totalValue = positions.Sum(p => p.MarketValue);
            var concentrationRisks = new List<ConcentrationRisk>();

            // Top 10 holdings concentration
            var top10Value = positions.OrderByDescending(p => p.MarketValue).Take(10).Sum(p => p.MarketValue);
            concentrationRisks.Add(new ConcentrationRisk
            {
                Type = "Top 10 Holdings",
                Concentration = totalValue > 0 ? top10Value / totalValue : 0,
                Threshold = 0.6m,
                RiskLevel = (totalValue > 0 && top10Value / totalValue > 0.6m) ? "High" : "Medium"
            });

            // Sector concentration
            var sectorGroups = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other");

            var sectorRisks = sectorGroups
                .Select(g => new SectorRisk
                {
                    Sector = g.Key,
                    Weight = totalValue > 0 ? g.Sum(p => p.MarketValue) / totalValue : 0,
                    Volatility = 0.15m, // Placeholder - would calculate from historical data
                    Beta = 1.0m, // Placeholder
                    RiskContribution = 0.10m // Placeholder
                })
                .OrderByDescending(s => s.Weight)
                .Take(10)
                .ToList();

            return new RiskDashboard
            {
                PortfolioName = portfolio.Name,
                Beta = latestMetrics?.Beta ?? 1,
                Alpha = latestMetrics?.Alpha ?? 0,
                Volatility = latestMetrics?.Volatility ?? 0,
                VaR95 = latestMetrics?.VaR95 ?? 0,
                VaR99 = latestMetrics?.VaR99 ?? 0,
                MaxDrawdown = latestMetrics?.MaxDrawdown ?? 0,
                SharpeRatio = latestMetrics?.SharpeRatio ?? 0,
                SortinoRatio = latestMetrics?.SortinoRatio ?? 0,
                RiskHistory = riskHistory,
                ConcentrationRisks = concentrationRisks,
                SectorRisks = sectorRisks
            };
        }

        public async Task<AllocationDashboard> GetAllocationDashboardAsync(int portfolioId)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var positions = await _positionService.GetPortfolioPositionsAsync(portfolioId);
            var totalValue = positions.Sum(p => p.MarketValue);

            // Asset allocation by security type
            var assetAllocations = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Type)
                .Select(g => new AssetAllocation
                {
                    AssetClass = g.Key.ToString(),
                    Weight = totalValue > 0 ? g.Sum(p => p.MarketValue) / totalValue : 0,
                    Value = g.Sum(p => p.MarketValue),
                    Target = GetTargetAllocation(g.Key, portfolio),
                    Deviation = 0 // Will be calculated
                })
                .ToList();

            // Calculate deviations
            foreach (var allocation in assetAllocations)
            {
                allocation.Deviation = allocation.Weight - allocation.Target;
            }

            // Sector allocation
            var sectorAllocations = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Sector ?? "Other")
                .Select(g => new SectorAllocation
                {
                    Sector = g.Key,
                    Weight = totalValue > 0 ? g.Sum(p => p.MarketValue) / totalValue : 0,
                    Value = g.Sum(p => p.MarketValue),
                    Holdings = g.Count()
                })
                .OrderByDescending(s => s.Weight)
                .ToList();

            // Geographic allocation
            var geographicAllocations = positions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Region ?? "Unknown")
                .Select(g => new GeographicAllocation
                {
                    Region = g.Key,
                    Weight = totalValue > 0 ? g.Sum(p => p.MarketValue) / totalValue : 0,
                    Value = g.Sum(p => p.MarketValue)
                })
                .OrderByDescending(g => g.Weight)
                .ToList();

            // Allocation targets vs actual
            var allocationTargets = new List<AllocationTarget>
            {
                new AllocationTarget
                {
                    Category = "Equity",
                    Target = portfolio.TargetEquityAllocation / 100,
                    Actual = assetAllocations.Where(a => a.AssetClass == "Stock" || a.AssetClass == "ETF").Sum(a => a.Weight),
                    Difference = 0 // Will be calculated
                },
                new AllocationTarget
                {
                    Category = "Bond",
                    Target = portfolio.TargetBondAllocation / 100,
                    Actual = assetAllocations.FirstOrDefault(a => a.AssetClass == "Bond")?.Weight ?? 0,
                    Difference = 0 // Will be calculated
                },
                new AllocationTarget
                {
                    Category = "Cash",
                    Target = portfolio.TargetCashAllocation / 100,
                    Actual = portfolio.CashBalance / totalValue,
                    Difference = 0 // Will be calculated
                }
            };

            foreach (var target in allocationTargets)
            {
                target.Difference = target.Actual - target.Target;
            }

            var allocationDeviations = allocationTargets
                .Where(t => Math.Abs(t.Difference) > 0.02m) // 2% threshold
                .Select(t => new AllocationDeviation
                {
                    Category = t.Category,
                    Deviation = t.Difference,
                    Status = Math.Abs(t.Difference) > 0.05m ? "High" : "Medium"
                })
                .ToList();

            return new AllocationDashboard
            {
                PortfolioName = portfolio.Name,
                AssetAllocations = assetAllocations,
                SectorAllocations = sectorAllocations,
                GeographicAllocations = geographicAllocations,
                AllocationTargets = allocationTargets,
                AllocationDeviations = allocationDeviations
            };
        }

        public async Task<TransactionsDashboard> GetTransactionsDashboardAsync(int portfolioId, int days = 30)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");

            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-days);

            var transactions = await _transactionService.GetTransactionsByPortfolioAsync(portfolioId, startDate, endDate);

            var totalTransactionValue = transactions.Sum(t => Math.Abs(t.NetAmount));
            var transactionCount = transactions.Count();
            var averageTransactionSize = transactionCount > 0 ? totalTransactionValue / transactionCount : 0;

            // Transactions by type
            var transactionsByType = transactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new TransactionSummary
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    TotalValue = g.Sum(t => Math.Abs(t.NetAmount)),
                    AverageSize = g.Count() > 0 ? g.Sum(t => Math.Abs(t.NetAmount)) / g.Count() : 0
                })
                .OrderByDescending(t => t.TotalValue)
                .ToList();

            // Recent transactions
            var recentTransactions = transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .Select(t => new RecentTransaction
                {
                    Date = t.TransactionDate,
                    Symbol = t.Security?.Symbol ?? "N/A",
                    Type = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    Price = t.Price,
                    Amount = t.NetAmount
                })
                .ToList();

            // Daily transaction volume
            var dailyVolume = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new DailyTransactionVolume
                {
                    Date = g.Key,
                    Volume = g.Sum(t => Math.Abs(t.NetAmount)),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Top traded securities
            var topTradedSecurities = transactions
                .Where(t => t.Security != null)
                .GroupBy(t => t.Security!)
                .Select(g => new SecurityTransactionSummary
                {
                    Symbol = g.Key.Symbol,
                    Name = g.Key.Name,
                    TransactionCount = g.Count(),
                    TotalVolume = g.Sum(t => Math.Abs(t.NetAmount))
                })
                .OrderByDescending(s => s.TotalVolume)
                .Take(10)
                .ToList();

            return new TransactionsDashboard
            {
                PortfolioName = portfolio.Name,
                TotalTransactionValue = totalTransactionValue,
                TransactionCount = transactionCount,
                AverageTransactionSize = averageTransactionSize,
                TransactionsByType = transactionsByType,
                RecentTransactions = recentTransactions,
                DailyVolume = dailyVolume,
                TopTradedSecurities = topTradedSecurities
            };
        }

        public async Task<MultiPortfolioDashboard> GetMultiPortfolioDashboardAsync(IEnumerable<int> portfolioIds)
        {
            var portfolios = new List<Portfolio>();
            var allPositions = new List<Position>();
            var portfolioSummaries = new List<PortfolioSummary>();

            foreach (var id in portfolioIds)
            {
                var portfolio = await _portfolioService.GetByIdAsync(id);
                if (portfolio != null)
                {
                    portfolios.Add(portfolio);
                    var positions = await _positionService.GetPortfolioPositionsAsync(id);
                    allPositions.AddRange(positions);

                    var endDate = DateTime.Now;
                    var performanceMetrics = await _performanceService.GetPerformanceMetricsAsync(id, endDate);

                    portfolioSummaries.Add(new PortfolioSummary
                    {
                        Id = portfolio.Id,
                        Name = portfolio.Name,
                        Type = portfolio.Type.ToString(),
                        Value = portfolio.CurrentValue,
                        Return = performanceMetrics?.TotalReturn ?? 0,
                        Volatility = performanceMetrics?.Volatility ?? 0,
                        RiskLevel = (int)portfolio.RiskLevel
                    });
                }
            }

            var totalValue = portfolios.Sum(p => p.CurrentValue);
            var weightedReturn = portfolios.Sum(p => (p.CurrentValue / totalValue) * 
                (portfolioSummaries.FirstOrDefault(s => s.Id == p.Id)?.Return ?? 0));
            var weightedVolatility = Math.Sqrt(portfolios.Sum(p => Math.Pow((double)((p.CurrentValue / totalValue) * 
                (portfolioSummaries.FirstOrDefault(s => s.Id == p.Id)?.Volatility ?? 0)), 2)));

            // Consolidated allocations
            var consolidatedAllocations = allPositions
                .Where(p => p.Security != null)
                .GroupBy(p => p.Security!.Type)
                .Select(g => new AssetAllocation
                {
                    AssetClass = g.Key.ToString(),
                    Weight = totalValue > 0 ? g.Sum(p => p.MarketValue) / totalValue : 0,
                    Value = g.Sum(p => p.MarketValue),
                    Target = 0, // Would need to be calculated based on composite targets
                    Deviation = 0
                })
                .ToList();

            // Performance comparisons
            var performanceComparisons = portfolioSummaries
                .Select(p => new PerformanceComparison
                {
                    PortfolioName = p.Name,
                    OneMonth = p.Return * 0.08m, // Placeholder calculations
                    ThreeMonth = p.Return * 0.25m,
                    YearToDate = p.Return * 0.75m,
                    OneYear = p.Return
                })
                .ToList();

            // Risk comparisons
            var riskComparisons = portfolioSummaries
                .Select(p => new RiskComparison
                {
                    PortfolioName = p.Name,
                    Volatility = p.Volatility,
                    Beta = 1.0m, // Placeholder
                    SharpeRatio = p.Return / (p.Volatility == 0 ? 1 : p.Volatility),
                    MaxDrawdown = -0.05m // Placeholder
                })
                .ToList();

            return new MultiPortfolioDashboard
            {
                TotalValue = totalValue,
                WeightedReturn = weightedReturn,
                WeightedVolatility = (decimal)weightedVolatility,
                PortfolioSummaries = portfolioSummaries,
                ConsolidatedAllocations = consolidatedAllocations,
                PerformanceComparisons = performanceComparisons,
                RiskComparisons = riskComparisons
            };
        }

        private decimal GetTargetAllocation(SecurityType securityType, Portfolio portfolio)
        {
            return securityType switch
            {
                SecurityType.Stock => portfolio.TargetEquityAllocation / 100,
                SecurityType.ETF => portfolio.TargetEquityAllocation / 100,
                SecurityType.Bond => portfolio.TargetBondAllocation / 100,
                SecurityType.Cash => portfolio.TargetCashAllocation / 100,
                SecurityType.PrivateEquity => portfolio.TargetAlternativeAllocation / 100,
                SecurityType.HedgeFund => portfolio.TargetAlternativeAllocation / 100,
                SecurityType.REIT => portfolio.TargetAlternativeAllocation / 100,
                _ => 0
            };
        }
    }
}