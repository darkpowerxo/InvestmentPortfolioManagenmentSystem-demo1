// Portfolio analytics service for institutional investment management
// Service d'analyse de portefeuille pour la gestion d'investissements institutionnels
// Advanced portfolio analysis and risk assessment capabilities
// Capacités avancées d'analyse de portefeuille et d'évaluation des risques

using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Services;

namespace InvestmentPortfolioManager.Domain.Services
{
    /// <summary>
    /// Comprehensive portfolio analytics service
    /// Service d'analyse complète de portefeuille
    /// </summary>
    public class PortfolioAnalyticsService
    {
        #region Portfolio Performance Analysis / Analyse de performance de portefeuille

        /// <summary>
        /// Calculates comprehensive portfolio performance metrics
        /// Calcule les métriques complètes de performance de portefeuille
        /// </summary>
        public PortfolioPerformanceAnalysis CalculatePortfolioPerformance(Portfolio portfolio, 
                                                                          IEnumerable<MarketData> portfolioValues,
                                                                          IEnumerable<MarketData>? benchmarkValues = null)
        {
            if (portfolio == null)
                throw new ArgumentNullException(nameof(portfolio));

            var valuesList = portfolioValues?.ToList() ?? new List<MarketData>();
            var benchmarkList = benchmarkValues?.ToList() ?? new List<MarketData>();

            if (!valuesList.Any())
                return new PortfolioPerformanceAnalysis();

            // Calculate returns from portfolio values
            var returns = CalculateReturnsFromValues(valuesList);
            var benchmarkReturns = benchmarkList.Any() ? CalculateReturnsFromValues(benchmarkList) : null;

            var analysis = new PortfolioPerformanceAnalysis
            {
                PortfolioId = portfolio.Id,
                AnalysisDate = DateTime.UtcNow,
                AnalysisPeriodStart = valuesList.Min(v => v.Date),
                AnalysisPeriodEnd = valuesList.Max(v => v.Date)
            };

            // Basic return metrics
            analysis.TotalReturn = FinancialCalculations.CalculateTimeWeightedReturn(returns);
            analysis.AnnualizedReturn = FinancialCalculations.AnnualizeReturn(
                analysis.TotalReturn, 
                (analysis.AnalysisPeriodEnd - analysis.AnalysisPeriodStart).TotalDays / 365.25);

            // Risk metrics
            analysis.Volatility = FinancialCalculations.CalculateVolatility(returns, true);
            analysis.SharpeRatio = FinancialCalculations.CalculateSharpeRatio(analysis.AnnualizedReturn, analysis.Volatility);
            analysis.SortinoRatio = FinancialCalculations.CalculateSortinoRatio(returns);
            analysis.MaxDrawdown = FinancialCalculations.CalculateMaxDrawdown(valuesList.Select(v => v.ClosePrice));

            // Value at Risk calculations
            var currentValue = valuesList.LastOrDefault()?.ClosePrice ?? 0;
            analysis.VaR95 = FinancialCalculations.CalculateVaR(returns, currentValue, 0.95m);
            analysis.VaR99 = FinancialCalculations.CalculateVaR(returns, currentValue, 0.99m);
            analysis.ConditionalVaR95 = FinancialCalculations.CalculateConditionalVaR(returns, currentValue, 0.95m);

            // Market risk metrics (if benchmark provided)
            if (benchmarkReturns != null && benchmarkReturns.Any())
            {
                analysis.Beta = FinancialCalculations.CalculateBeta(returns, benchmarkReturns);
                analysis.Alpha = FinancialCalculations.CalculateAlpha(
                    analysis.AnnualizedReturn, 
                    FinancialCalculations.CalculateTimeWeightedReturn(benchmarkReturns), 
                    analysis.Beta ?? 1m);
                analysis.TrackingError = FinancialCalculations.CalculateTrackingError(returns, benchmarkReturns);
                analysis.InformationRatio = FinancialCalculations.CalculateInformationRatio(returns, benchmarkReturns);
            }

            return analysis;
        }

        /// <summary>
        /// Analyzes portfolio asset allocation and diversification
        /// Analyse l'allocation d'actifs et la diversification du portefeuille
        /// </summary>
        public AssetAllocationAnalysis AnalyzeAssetAllocation(Portfolio portfolio)
        {
            if (portfolio?.Holdings == null || !portfolio.Holdings.Any())
                return new AssetAllocationAnalysis();

            var analysis = new AssetAllocationAnalysis
            {
                PortfolioId = portfolio.Id,
                AnalysisDate = DateTime.UtcNow,
                TotalValue = portfolio.Holdings.Sum(h => h.MarketValue ?? 0)
            };

            // Asset class allocation
            var assetClassAllocations = portfolio.Holdings
                .GroupBy(h => h.Security.AssetClass)
                .Select(g => new AssetClassAllocation
                {
                    AssetClass = g.Key,
                    Value = g.Sum(h => h.MarketValue ?? 0),
                    Weight = g.Sum(h => h.MarketValue ?? 0) / analysis.TotalValue,
                    SecurityCount = g.Count()
                })
                .OrderByDescending(a => a.Weight)
                .ToList();

            analysis.AssetClassAllocations = assetClassAllocations;

            // Sector allocation
            var sectorAllocations = portfolio.Holdings
                .Where(h => h.Security.Sector != null)
                .GroupBy(h => h.Security.Sector)
                .Select(g => new SectorAllocation
                {
                    Sector = g.Key!,
                    Value = g.Sum(h => h.MarketValue ?? 0),
                    Weight = g.Sum(h => h.MarketValue ?? 0) / analysis.TotalValue,
                    SecurityCount = g.Count()
                })
                .OrderByDescending(s => s.Weight)
                .ToList();

            analysis.SectorAllocations = sectorAllocations;

            // Geographic allocation
            var geographicAllocations = portfolio.Holdings
                .Where(h => h.Security.Country != null)
                .GroupBy(h => h.Security.Country)
                .Select(g => new GeographicAllocation
                {
                    Country = g.Key!,
                    Value = g.Sum(h => h.MarketValue ?? 0),
                    Weight = g.Sum(h => h.MarketValue ?? 0) / analysis.TotalValue,
                    SecurityCount = g.Count()
                })
                .OrderByDescending(g => g.Weight)
                .ToList();

            analysis.GeographicAllocations = geographicAllocations;

            // Concentration analysis
            analysis.ConcentrationMetrics = CalculateConcentrationMetrics(portfolio.Holdings);

            return analysis;
        }

        /// <summary>
        /// Performs risk analysis for the portfolio
        /// Effectue une analyse de risque pour le portefeuille
        /// </summary>
        public RiskAnalysis PerformRiskAnalysis(Portfolio portfolio, IEnumerable<MarketData> historicalData)
        {
            if (portfolio?.Holdings == null || !portfolio.Holdings.Any())
                return new RiskAnalysis();

            var analysis = new RiskAnalysis
            {
                PortfolioId = portfolio.Id,
                AnalysisDate = DateTime.UtcNow,
                TotalValue = portfolio.Holdings.Sum(h => h.MarketValue ?? 0)
            };

            // Holdings-based risk metrics
            analysis.HoldingRiskMetrics = CalculateHoldingRiskMetrics(portfolio.Holdings);

            // Portfolio-level risk metrics
            if (historicalData != null && historicalData.Any())
            {
                var returns = CalculateReturnsFromValues(historicalData);
                analysis.PortfolioVolatility = FinancialCalculations.CalculateVolatility(returns, true);
                analysis.VaRDaily = FinancialCalculations.CalculateVaR(returns, analysis.TotalValue, 0.95m, false);
                analysis.VaRAnnual = FinancialCalculations.CalculateVaR(returns, analysis.TotalValue, 0.95m, true);
                analysis.ExpectedShortfall = FinancialCalculations.CalculateConditionalVaR(returns, analysis.TotalValue, 0.95m);
            }

            // Liquidity analysis
            analysis.LiquidityAnalysis = AnalyzeLiquidity(portfolio.Holdings);

            // Currency exposure
            analysis.CurrencyExposure = AnalyzeCurrencyExposure(portfolio.Holdings);

            return analysis;
        }

        #endregion

        #region Stress Testing / Tests de stress

        /// <summary>
        /// Performs stress testing scenarios on the portfolio
        /// Effectue des scénarios de tests de stress sur le portefeuille
        /// </summary>
        public StressTestResults PerformStressTest(Portfolio portfolio, IEnumerable<StressTestScenario> scenarios)
        {
            if (portfolio?.Holdings == null || !portfolio.Holdings.Any())
                return new StressTestResults();

            var results = new StressTestResults
            {
                PortfolioId = portfolio.Id,
                BaselineValue = portfolio.Holdings.Sum(h => h.MarketValue ?? 0),
                TestDate = DateTime.UtcNow,
                ScenarioResults = new List<StressTestScenarioResult>()
            };

            foreach (var scenario in scenarios ?? Enumerable.Empty<StressTestScenario>())
            {
                var scenarioResult = ExecuteStressScenario(portfolio, scenario);
                results.ScenarioResults.Add(scenarioResult);
            }

            // Calculate worst-case scenario
            if (results.ScenarioResults.Any())
            {
                results.WorstCaseScenario = results.ScenarioResults
                    .OrderBy(r => r.EstimatedValue)
                    .First();
            }

            return results;
        }

        private StressTestScenarioResult ExecuteStressScenario(Portfolio portfolio, StressTestScenario scenario)
        {
            var result = new StressTestScenarioResult
            {
                ScenarioId = scenario.Id,
                ScenarioName = scenario.Name,
                EstimatedValue = 0m,
                HoldingImpacts = new List<HoldingStressImpact>()
            };

            foreach (var holding in portfolio.Holdings)
            {
                var impact = CalculateHoldingStressImpact(holding, scenario);
                result.HoldingImpacts.Add(impact);
                result.EstimatedValue += impact.StressedValue;
            }

            result.ValueChange = result.EstimatedValue - portfolio.Holdings.Sum(h => h.MarketValue ?? 0);
            result.PercentageChange = result.ValueChange / portfolio.Holdings.Sum(h => h.MarketValue ?? 0);

            return result;
        }

        private HoldingStressImpact CalculateHoldingStressImpact(Holding holding, StressTestScenario scenario)
        {
            var baseValue = holding.MarketValue ?? 0;
            var stressedValue = baseValue;

            // Apply asset class specific stress factors
            var assetClassFactor = GetAssetClassStressFactor(holding.Security.AssetClass, scenario);
            stressedValue *= (1m + assetClassFactor);

            // Apply sector specific stress factors if applicable
            if (holding.Security.Sector != null)
            {
                var sectorFactor = GetSectorStressFactor(holding.Security.Sector, scenario);
                stressedValue *= (1m + sectorFactor);
            }

            // Apply currency stress if applicable
            if (holding.Security.Currency != holding.Portfolio.BaseCurrency)
            {
                var currencyFactor = GetCurrencyStressFactor(holding.Security.Currency, scenario);
                stressedValue *= (1m + currencyFactor);
            }

            return new HoldingStressImpact
            {
                HoldingId = holding.Id,
                SecuritySymbol = holding.Security.Symbol,
                BaseValue = baseValue,
                StressedValue = stressedValue,
                ValueChange = stressedValue - baseValue,
                PercentageChange = (stressedValue - baseValue) / baseValue
            };
        }

        #endregion

        #region Helper Methods / Méthodes d'aide

        private List<decimal> CalculateReturnsFromValues(IEnumerable<MarketData> values)
        {
            var valuesList = values.OrderBy(v => v.Date).ToList();
            var returns = new List<decimal>();

            for (int i = 1; i < valuesList.Count; i++)
            {
                var currentValue = valuesList[i].ClosePrice;
                var previousValue = valuesList[i - 1].ClosePrice;
                
                if (previousValue > 0)
                {
                    returns.Add(FinancialCalculations.CalculateSimpleReturn(previousValue, currentValue));
                }
            }

            return returns;
        }

        private ConcentrationMetrics CalculateConcentrationMetrics(IEnumerable<Holding> holdings)
        {
            var holdingsList = holdings.ToList();
            var totalValue = holdingsList.Sum(h => h.MarketValue ?? 0);
            var weights = holdingsList.Select(h => (h.MarketValue ?? 0) / totalValue).ToList();

            return new ConcentrationMetrics
            {
                LargestHoldingWeight = weights.Max(),
                Top5HoldingsWeight = weights.OrderByDescending(w => w).Take(5).Sum(),
                Top10HoldingsWeight = weights.OrderByDescending(w => w).Take(10).Sum(),
                HerfindahlIndex = weights.Sum(w => w * w), // Concentration index
                EffectiveNumberOfPositions = 1m / weights.Sum(w => w * w)
            };
        }

        private List<HoldingRiskMetric> CalculateHoldingRiskMetrics(IEnumerable<Holding> holdings)
        {
            return holdings.Select(h => new HoldingRiskMetric
            {
                HoldingId = h.Id,
                SecuritySymbol = h.Security.Symbol,
                Value = h.MarketValue ?? 0,
                Weight = (h.MarketValue ?? 0) / holdings.Sum(holding => holding.MarketValue ?? 0),
                EstimatedVolatility = h.Security.HistoricalVolatility ?? 0m,
                EstimatedBeta = h.Security.Beta ?? 1m,
                LiquidityRisk = AssessLiquidityRisk(h.Security),
                CurrencyRisk = h.Security.Currency.Code != h.Portfolio.BaseCurrency.Code
            }).ToList();
        }

        private LiquidityAnalysis AnalyzeLiquidity(IEnumerable<Holding> holdings)
        {
            var totalValue = holdings.Sum(h => h.MarketValue ?? 0);
            
            var liquidityBreakdown = holdings
                .GroupBy(h => AssessLiquidityCategory(h.Security))
                .Select(g => new LiquidityCategory
                {
                    Category = g.Key,
                    Value = g.Sum(h => h.MarketValue ?? 0),
                    Weight = g.Sum(h => h.MarketValue ?? 0) / totalValue,
                    SecurityCount = g.Count()
                })
                .ToList();

            return new LiquidityAnalysis
            {
                LiquidityBreakdown = liquidityBreakdown,
                AverageLiquidityScore = holdings.Average(h => GetLiquidityScore(h.Security)),
                EstimatedLiquidationTime = EstimateLiquidationTime(holdings)
            };
        }

        private CurrencyExposureAnalysis AnalyzeCurrencyExposure(IEnumerable<Holding> holdings)
        {
            var totalValue = holdings.Sum(h => h.MarketValue ?? 0);

            var exposures = holdings
                .GroupBy(h => h.Security.Currency)
                .Select(g => new CurrencyExposure
                {
                    Currency = g.Key,
                    Value = g.Sum(h => h.MarketValue ?? 0),
                    Weight = g.Sum(h => h.MarketValue ?? 0) / totalValue,
                    SecurityCount = g.Count()
                })
                .OrderByDescending(e => e.Weight)
                .ToList();

            return new CurrencyExposureAnalysis
            {
                CurrencyExposures = exposures,
                BaseCurrencyWeight = exposures.FirstOrDefault()?.Weight ?? 1m,
                ForeignCurrencyWeight = exposures.Skip(1).Sum(e => e.Weight),
                NumberOfCurrencies = exposures.Count
            };
        }

        // Stress testing helper methods
        private decimal GetAssetClassStressFactor(AssetClass assetClass, StressTestScenario scenario)
        {
            // This would typically be configured based on the scenario
            // For now, using example stress factors
            return assetClass.Code switch
            {
                "EQUITY" => scenario.Name.Contains("Market Crash") ? -0.30m : -0.10m,
                "BOND" => scenario.Name.Contains("Interest Rate") ? -0.15m : -0.05m,
                "COMMODITY" => scenario.Name.Contains("Inflation") ? 0.15m : -0.05m,
                "CASH" => 0m,
                _ => -0.05m
            };
        }

        private decimal GetSectorStressFactor(Sector sector, StressTestScenario scenario)
        {
            // Example sector-specific stress factors
            return sector.Code switch
            {
                "TECH" => scenario.Name.Contains("Tech Bubble") ? -0.40m : -0.05m,
                "FINANCE" => scenario.Name.Contains("Credit Crisis") ? -0.35m : -0.05m,
                "ENERGY" => scenario.Name.Contains("Oil Shock") ? -0.25m : -0.05m,
                _ => 0m
            };
        }

        private decimal GetCurrencyStressFactor(Currency currency, StressTestScenario scenario)
        {
            // Example currency stress factors
            if (scenario.Name.Contains("Currency Crisis"))
            {
                return currency.Code switch
                {
                    "USD" => 0.05m,
                    "EUR" => -0.10m,
                    "CAD" => -0.15m,
                    _ => -0.05m
                };
            }
            return 0m;
        }

        private string AssessLiquidityRisk(Security security)
        {
            // Simple liquidity risk assessment
            return security.SecurityType switch
            {
                SecurityType.Stock => "Medium",
                SecurityType.Bond => "Low",
                SecurityType.ETF => "High",
                SecurityType.MutualFund => "Medium",
                SecurityType.Alternative => "Low",
                _ => "Medium"
            };
        }

        private string AssessLiquidityCategory(Security security)
        {
            return security.SecurityType switch
            {
                SecurityType.Stock => "Liquid",
                SecurityType.ETF => "Highly Liquid",
                SecurityType.Bond => "Semi-Liquid",
                SecurityType.Alternative => "Illiquid",
                _ => "Semi-Liquid"
            };
        }

        private decimal GetLiquidityScore(Security security)
        {
            // Score from 0-10, where 10 is most liquid
            return security.SecurityType switch
            {
                SecurityType.ETF => 10m,
                SecurityType.Stock => 8m,
                SecurityType.Bond => 6m,
                SecurityType.MutualFund => 7m,
                SecurityType.Alternative => 3m,
                _ => 5m
            };
        }

        private int EstimateLiquidationTime(IEnumerable<Holding> holdings)
        {
            // Estimate liquidation time in days
            var maxTime = holdings.Max(h => h.Security.SecurityType switch
            {
                SecurityType.ETF => 1,
                SecurityType.Stock => 2,
                SecurityType.Bond => 5,
                SecurityType.MutualFund => 3,
                SecurityType.Alternative => 30,
                _ => 5
            });

            return maxTime;
        }

        #endregion
    }

    #region Analysis Result Classes / Classes de résultats d'analyse

    public class PortfolioPerformanceAnalysis
    {
        public Guid PortfolioId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public DateTime AnalysisPeriodStart { get; set; }
        public DateTime AnalysisPeriodEnd { get; set; }
        
        public decimal TotalReturn { get; set; }
        public decimal AnnualizedReturn { get; set; }
        public decimal Volatility { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal SortinoRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
        
        public decimal VaR95 { get; set; }
        public decimal VaR99 { get; set; }
        public decimal ConditionalVaR95 { get; set; }
        
        public decimal? Beta { get; set; }
        public decimal? Alpha { get; set; }
        public decimal? TrackingError { get; set; }
        public decimal? InformationRatio { get; set; }
    }

    public class AssetAllocationAnalysis
    {
        public Guid PortfolioId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public decimal TotalValue { get; set; }
        
        public List<AssetClassAllocation> AssetClassAllocations { get; set; } = new();
        public List<SectorAllocation> SectorAllocations { get; set; } = new();
        public List<GeographicAllocation> GeographicAllocations { get; set; } = new();
        public ConcentrationMetrics ConcentrationMetrics { get; set; } = new();
    }

    public class AssetClassAllocation
    {
        public required AssetClass AssetClass { get; set; }
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public int SecurityCount { get; set; }
    }

    public class SectorAllocation
    {
        public required Sector Sector { get; set; }
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public int SecurityCount { get; set; }
    }

    public class GeographicAllocation
    {
        public required Country Country { get; set; }
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public int SecurityCount { get; set; }
    }

    public class ConcentrationMetrics
    {
        public decimal LargestHoldingWeight { get; set; }
        public decimal Top5HoldingsWeight { get; set; }
        public decimal Top10HoldingsWeight { get; set; }
        public decimal HerfindahlIndex { get; set; }
        public decimal EffectiveNumberOfPositions { get; set; }
    }

    public class RiskAnalysis
    {
        public Guid PortfolioId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public decimal TotalValue { get; set; }
        
        public decimal PortfolioVolatility { get; set; }
        public decimal VaRDaily { get; set; }
        public decimal VaRAnnual { get; set; }
        public decimal ExpectedShortfall { get; set; }
        
        public List<HoldingRiskMetric> HoldingRiskMetrics { get; set; } = new();
        public LiquidityAnalysis LiquidityAnalysis { get; set; } = new();
        public CurrencyExposureAnalysis CurrencyExposure { get; set; } = new();
    }

    public class HoldingRiskMetric
    {
        public Guid HoldingId { get; set; }
        public string SecuritySymbol { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public decimal EstimatedVolatility { get; set; }
        public decimal EstimatedBeta { get; set; }
        public string LiquidityRisk { get; set; } = string.Empty;
        public bool CurrencyRisk { get; set; }
    }

    public class LiquidityAnalysis
    {
        public List<LiquidityCategory> LiquidityBreakdown { get; set; } = new();
        public decimal AverageLiquidityScore { get; set; }
        public int EstimatedLiquidationTime { get; set; }
    }

    public class LiquidityCategory
    {
        public string Category { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public int SecurityCount { get; set; }
    }

    public class CurrencyExposureAnalysis
    {
        public List<CurrencyExposure> CurrencyExposures { get; set; } = new();
        public decimal BaseCurrencyWeight { get; set; }
        public decimal ForeignCurrencyWeight { get; set; }
        public int NumberOfCurrencies { get; set; }
    }

    public class CurrencyExposure
    {
        public required Currency Currency { get; set; }
        public decimal Value { get; set; }
        public decimal Weight { get; set; }
        public int SecurityCount { get; set; }
    }

    public class StressTestResults
    {
        public Guid PortfolioId { get; set; }
        public DateTime TestDate { get; set; }
        public decimal BaselineValue { get; set; }
        public List<StressTestScenarioResult> ScenarioResults { get; set; } = new();
        public StressTestScenarioResult? WorstCaseScenario { get; set; }
    }

    public class StressTestScenarioResult
    {
        public Guid ScenarioId { get; set; }
        public string ScenarioName { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public decimal ValueChange { get; set; }
        public decimal PercentageChange { get; set; }
        public List<HoldingStressImpact> HoldingImpacts { get; set; } = new();
    }

    public class HoldingStressImpact
    {
        public Guid HoldingId { get; set; }
        public string SecuritySymbol { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal StressedValue { get; set; }
        public decimal ValueChange { get; set; }
        public decimal PercentageChange { get; set; }
    }

    #endregion
}