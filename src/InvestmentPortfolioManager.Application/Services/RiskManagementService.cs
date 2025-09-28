using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolioManager.Application.Services;

/// <summary>
/// Service de gestion des risques avec analyse de stress et corrélations / Risk management service with stress testing and correlation analysis
/// </summary>
public class RiskManagementService : IRiskManagementService
{
    private readonly IMarketDataService _marketDataService;
    private readonly IFinancialCalculationsService _financialCalculationsService;
    private readonly ILogger<RiskManagementService> _logger;

    // Mock data store for portfolios (in real implementation, would use repository)
    private readonly Dictionary<int, Portfolio> _portfolios = new();

    public RiskManagementService(
        IMarketDataService marketDataService,
        IFinancialCalculationsService financialCalculationsService,
        ILogger<RiskManagementService> logger)
    {
        _marketDataService = marketDataService;
        _financialCalculationsService = financialCalculationsService;
        _logger = logger;
        
        InitializeSamplePortfolios();
    }

    public async Task<PortfolioRiskMetricsDto> CalculatePortfolioRiskAsync(int portfolioId, int periodDays = 252)
    {
        _logger.LogInformation("Calculating risk metrics for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        // Get historical returns for the portfolio
        var returns = await GetPortfolioReturnsAsync(portfolio, periodDays);
        var benchmarkReturns = await GetBenchmarkReturnsAsync(periodDays);

        var portfolioValue = CalculatePortfolioValue(portfolio);
        var riskFreeRate = 0.025m; // 2.5% risk-free rate

        var avgReturn = returns.Average();
        var volatility = _financialCalculationsService.CalculateVolatility(returns, periodDays);
        var sharpeRatio = _financialCalculationsService.CalculateSharpeRatio(avgReturn, riskFreeRate, volatility);
        var sortinoRatio = _financialCalculationsService.CalculateSortinoRatio(returns, riskFreeRate);
        var beta = _financialCalculationsService.CalculateBeta(returns, benchmarkReturns);
        var alpha = _financialCalculationsService.CalculateAlpha(avgReturn, benchmarkReturns.Average(), beta, riskFreeRate);
        var maxDrawdown = _financialCalculationsService.CalculateMaxDrawdown(GetPortfolioValues(portfolio, periodDays));
        var var95 = _financialCalculationsService.CalculateVaR(returns, 0.95m);
        var var99 = _financialCalculationsService.CalculateVaR(returns, 0.99m);
        var es95 = CalculateExpectedShortfall(returns, 0.95m);
        var es99 = CalculateExpectedShortfall(returns, 0.99m);
        var trackingError = _financialCalculationsService.CalculateTrackingError(returns, benchmarkReturns);
        var informationRatio = _financialCalculationsService.CalculateInformationRatio(returns, benchmarkReturns);

        return new PortfolioRiskMetricsDto
        {
            Value = portfolioValue,
            Volatility = volatility,
            SharpeRatio = sharpeRatio,
            SortinoRatio = sortinoRatio,
            Beta = beta,
            Alpha = alpha,
            MaxDrawdown = maxDrawdown,
            VaR95 = var95,
            VaR99 = var99,
            ExpectedShortfall95 = es95,
            ExpectedShortfall99 = es99,
            TrackingError = trackingError,
            InformationRatio = informationRatio
        };
    }

    public async Task<StressTestResultDto> PerformStressTestAsync(int portfolioId, StressTestScenarioDto scenario)
    {
        _logger.LogInformation("Performing stress test {ScenarioName} for portfolio {PortfolioId}", scenario.Name, portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var originalValue = CalculatePortfolioValue(portfolio);
        var stressedValue = 0m;
        var positionImpacts = new Dictionary<string, decimal>();
        var sectorImpacts = new Dictionary<string, decimal>();

        foreach (var position in portfolio.Positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Security.Symbol);
            var originalPositionValue = position.Quantity * (currentPrice ?? 100m);

            // Apply market shocks
            var shock = 0m;
            if (scenario.MarketShocks.ContainsKey(position.Security.Symbol))
            {
                shock += scenario.MarketShocks[position.Security.Symbol];
            }

            // Apply sector shocks
            var sector = GetSecuritySector(position.Security.Symbol);
            if (scenario.SectorShocks.ContainsKey(sector))
            {
                shock += scenario.SectorShocks[sector];
            }

            // Apply volatility shock for options and derivatives
            if (position.Security.SecurityType == Domain.Enums.SecurityType.Option)
            {
                shock += scenario.VolatilityShock * 0.1m; // Options are more sensitive to volatility
            }

            var stressedPrice = (currentPrice ?? 100m) * (1 + shock);
            var stressedPositionValue = position.Quantity * stressedPrice;
            var positionImpact = stressedPositionValue - originalPositionValue;

            stressedValue += stressedPositionValue;
            positionImpacts[position.Security.Symbol] = positionImpact;

            // Aggregate sector impacts
            if (!sectorImpacts.ContainsKey(sector))
                sectorImpacts[sector] = 0m;
            sectorImpacts[sector] += positionImpact;
        }

        var absoluteImpact = stressedValue - originalValue;
        var percentageImpact = originalValue > 0 ? (absoluteImpact / originalValue) * 100 : 0m;

        return new StressTestResultDto
        {
            ScenarioName = scenario.Name,
            OriginalValue = originalValue,
            StressedValue = stressedValue,
            AbsoluteImpact = absoluteImpact,
            PercentageImpact = percentageImpact,
            PositionImpacts = positionImpacts,
            SectorImpacts = sectorImpacts
        };
    }

    public async Task<CorrelationAnalysisDto> AnalyzeCorrelationsAsync(int portfolioId, int periodDays = 252)
    {
        _logger.LogInformation("Analyzing correlations for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var symbols = portfolio.Positions.Select(p => p.Security.Symbol).ToArray();
        var returnsSeries = new List<IEnumerable<decimal>>();

        foreach (var symbol in symbols)
        {
            var returns = await GetSecurityReturnsAsync(symbol, periodDays);
            returnsSeries.Add(returns);
        }

        var correlationMatrix = _financialCalculationsService.CalculateCorrelationMatrix(returnsSeries);
        var correlations = new List<decimal>();
        var highCorrelationPairs = new List<HighCorrelationPairDto>();

        // Extract correlation values and identify high correlation pairs
        for (int i = 0; i < symbols.Length; i++)
        {
            for (int j = i + 1; j < symbols.Length; j++)
            {
                var correlation = correlationMatrix[i, j];
                correlations.Add(Math.Abs(correlation));

                if (Math.Abs(correlation) > 0.7m) // High correlation threshold
                {
                    highCorrelationPairs.Add(new HighCorrelationPairDto
                    {
                        Security1 = symbols[i],
                        Security2 = symbols[j],
                        Correlation = correlation
                    });
                }
            }
        }

        return new CorrelationAnalysisDto
        {
            CorrelationMatrix = correlationMatrix,
            SecuritySymbols = symbols,
            AverageCorrelation = correlations.Average(),
            MaxCorrelation = correlations.Max(),
            MinCorrelation = correlations.Min(),
            HighCorrelationPairs = highCorrelationPairs
        };
    }

    public async Task<ExposureAnalysisDto> AnalyzeExposureAsync(int portfolioId)
    {
        _logger.LogInformation("Analyzing exposure for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var totalValue = CalculatePortfolioValue(portfolio);
        var sectorExposure = new Dictionary<string, decimal>();
        var geographicExposure = new Dictionary<string, decimal>();
        var currencyExposure = new Dictionary<string, decimal>();
        var assetClassExposure = new Dictionary<string, decimal>();

        decimal equityExposure = 0m;
        decimal fixedIncomeExposure = 0m;
        decimal cashExposure = 0m;
        decimal alternativeExposure = 0m;

        foreach (var position in portfolio.Positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Security.Symbol);
            var positionValue = position.Quantity * (currentPrice ?? 100m);
            var weight = totalValue > 0 ? positionValue / totalValue : 0m;

            // Sector exposure
            var sector = GetSecuritySector(position.Security.Symbol);
            if (!sectorExposure.ContainsKey(sector))
                sectorExposure[sector] = 0m;
            sectorExposure[sector] += weight;

            // Geographic exposure
            var geography = GetSecurityGeography(position.Security.Symbol);
            if (!geographicExposure.ContainsKey(geography))
                geographicExposure[geography] = 0m;
            geographicExposure[geography] += weight;

            // Currency exposure
            var currency = GetSecurityCurrency(position.Security.Symbol);
            if (!currencyExposure.ContainsKey(currency))
                currencyExposure[currency] = 0m;
            currencyExposure[currency] += weight;

            // Asset class exposure
            var assetClass = GetAssetClass(position.Security.SecurityType);
            if (!assetClassExposure.ContainsKey(assetClass))
                assetClassExposure[assetClass] = 0m;
            assetClassExposure[assetClass] += weight;

            // High-level asset class aggregation
            switch (position.Security.SecurityType)
            {
                case Domain.Enums.SecurityType.Stock:
                    equityExposure += weight;
                    break;
                case Domain.Enums.SecurityType.ETF:
                    if (sector.Contains("Bond") || position.Security.Name.Contains("Bond"))
                        fixedIncomeExposure += weight;
                    else
                        equityExposure += weight; // Default ETFs to equity
                    break;
                case Domain.Enums.SecurityType.Bond:
                    fixedIncomeExposure += weight;
                    break;
                case Domain.Enums.SecurityType.Cash:
                    cashExposure += weight;
                    break;
                default:
                    alternativeExposure += weight;
                    break;
            }
        }

        return new ExposureAnalysisDto
        {
            SectorExposure = sectorExposure,
            GeographicExposure = geographicExposure,
            CurrencyExposure = currencyExposure,
            AssetClassExposure = assetClassExposure,
            EquityExposure = equityExposure,
            FixedIncomeExposure = fixedIncomeExposure,
            CashExposure = cashExposure,
            AlternativeExposure = alternativeExposure
        };
    }

    public async Task<ConcentrationRiskDto> AssessConcentrationRiskAsync(int portfolioId)
    {
        _logger.LogInformation("Assessing concentration risk for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var totalValue = CalculatePortfolioValue(portfolio);
        var concentrations = new List<ConcentrationDto>();

        foreach (var position in portfolio.Positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Security.Symbol);
            var positionValue = position.Quantity * (currentPrice ?? 100m);
            var weight = totalValue > 0 ? positionValue / totalValue : 0m;

            concentrations.Add(new ConcentrationDto
            {
                Symbol = position.Security.Symbol,
                Name = position.Security.Name,
                Weight = weight,
                Value = positionValue
            });
        }

        var sortedConcentrations = concentrations.OrderByDescending(c => c.Weight).ToList();
        var top5Concentration = sortedConcentrations.Take(5).Sum(c => c.Weight);
        var top10Concentration = sortedConcentrations.Take(10).Sum(c => c.Weight);

        // Calculate Herfindahl Index
        var herfindahlIndex = concentrations.Sum(c => c.Weight * c.Weight);

        // Determine risk level
        var riskLevel = ConcentrationRiskLevel.Low;
        var recommendations = new List<string>();

        if (top5Concentration > 0.5m)
        {
            riskLevel = ConcentrationRiskLevel.High;
            recommendations.Add("Consider diversifying the top 5 holdings which represent over 50% of the portfolio");
        }
        else if (top5Concentration > 0.35m)
        {
            riskLevel = ConcentrationRiskLevel.Moderate;
            recommendations.Add("Monitor concentration in top holdings");
        }

        if (herfindahlIndex > 0.15m)
        {
            riskLevel = ConcentrationRiskLevel.VeryHigh;
            recommendations.Add("High concentration detected. Consider rebalancing to improve diversification");
        }

        if (sortedConcentrations.FirstOrDefault()?.Weight > 0.15m)
        {
            recommendations.Add($"Single position {sortedConcentrations.First().Symbol} represents over 15% of portfolio");
        }

        return new ConcentrationRiskDto
        {
            HerfindahlIndex = herfindahlIndex,
            Top5Concentration = top5Concentration,
            Top10Concentration = top10Concentration,
            TopConcentrations = sortedConcentrations.Take(10),
            RiskLevel = riskLevel,
            Recommendations = recommendations.ToArray()
        };
    }

    public async Task<ScenarioAnalysisDto> RunScenarioAnalysisAsync(int portfolioId, IEnumerable<MarketScenarioDto> scenarios)
    {
        _logger.LogInformation("Running scenario analysis for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var results = new List<ScenarioResultDto>();
        var originalValue = CalculatePortfolioValue(portfolio);

        foreach (var scenario in scenarios)
        {
            var scenarioValue = await CalculateScenarioValue(portfolio, scenario);
            var scenarioReturn = originalValue > 0 ? ((scenarioValue - originalValue) / originalValue) : 0m;

            results.Add(new ScenarioResultDto
            {
                ScenarioName = scenario.Name,
                Probability = scenario.Probability,
                PortfolioReturn = scenarioReturn,
                PortfolioValue = scenarioValue
            });
        }

        var weightedReturns = results.Select(r => r.PortfolioReturn * r.Probability);
        var returns = results.Select(r => r.PortfolioReturn);

        return new ScenarioAnalysisDto
        {
            Results = results,
            WorstCaseScenario = returns.Min(),
            BestCaseScenario = returns.Max(),
            AverageScenario = weightedReturns.Sum()
        };
    }

    public async Task<VaRAnalysisDto> CalculateVaRAnalysisAsync(int portfolioId, decimal[] confidenceLevels, int periodDays = 252)
    {
        _logger.LogInformation("Calculating VaR analysis for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var returns = await GetPortfolioReturnsAsync(portfolio, periodDays);
        var varLevels = new Dictionary<decimal, decimal>();
        var expectedShortfall = new Dictionary<decimal, decimal>();

        foreach (var confidenceLevel in confidenceLevels)
        {
            var var = _financialCalculationsService.CalculateVaR(returns, confidenceLevel);
            var es = CalculateExpectedShortfall(returns, confidenceLevel);

            varLevels[confidenceLevel] = var;
            expectedShortfall[confidenceLevel] = es;
        }

        return new VaRAnalysisDto
        {
            VaRLevels = varLevels,
            ExpectedShortfall = expectedShortfall,
            Methodology = VaRMethodology.Historical,
            PeriodDays = periodDays
        };
    }

    public async Task<LiquidityAnalysisDto> AssessLiquidityAsync(int portfolioId)
    {
        _logger.LogInformation("Assessing liquidity for portfolio {PortfolioId}", portfolioId);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        var totalValue = CalculatePortfolioValue(portfolio);
        var liquidityBuckets = new List<LiquidityBucketDto>();
        decimal highLiquidityValue = 0m;
        decimal mediumLiquidityValue = 0m;
        decimal lowLiquidityValue = 0m;
        decimal totalDaysWeighted = 0m;

        foreach (var position in portfolio.Positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Security.Symbol);
            var positionValue = position.Quantity * (currentPrice ?? 100m);
            var liquidityDays = GetLiquidityDays(position.Security.Symbol, position.Security.SecurityType);

            totalDaysWeighted += liquidityDays * positionValue;

            if (liquidityDays <= 1)
                highLiquidityValue += positionValue;
            else if (liquidityDays <= 5)
                mediumLiquidityValue += positionValue;
            else
                lowLiquidityValue += positionValue;
        }

        var averageDays = totalValue > 0 ? totalDaysWeighted / totalValue : 0m;
        var highLiquidityPct = totalValue > 0 ? highLiquidityValue / totalValue : 0m;
        var mediumLiquidityPct = totalValue > 0 ? mediumLiquidityValue / totalValue : 0m;
        var lowLiquidityPct = totalValue > 0 ? lowLiquidityValue / totalValue : 0m;

        liquidityBuckets.Add(new LiquidityBucketDto { Name = "High (≤1 day)", Percentage = highLiquidityPct, Value = highLiquidityValue, DaysToLiquidate = 1 });
        liquidityBuckets.Add(new LiquidityBucketDto { Name = "Medium (2-5 days)", Percentage = mediumLiquidityPct, Value = mediumLiquidityValue, DaysToLiquidate = 3 });
        liquidityBuckets.Add(new LiquidityBucketDto { Name = "Low (>5 days)", Percentage = lowLiquidityPct, Value = lowLiquidityValue, DaysToLiquidate = 10 });

        var riskLevel = LiquidityRiskLevel.Low;
        if (lowLiquidityPct > 0.3m) riskLevel = LiquidityRiskLevel.VeryHigh;
        else if (lowLiquidityPct > 0.2m) riskLevel = LiquidityRiskLevel.High;
        else if (lowLiquidityPct > 0.1m) riskLevel = LiquidityRiskLevel.Moderate;

        return new LiquidityAnalysisDto
        {
            HighLiquidityPercentage = highLiquidityPct,
            MediumLiquidityPercentage = mediumLiquidityPct,
            LowLiquidityPercentage = lowLiquidityPct,
            AverageDaysToLiquidate = averageDays,
            LiquidityBuckets = liquidityBuckets,
            OverallRiskLevel = riskLevel
        };
    }

    public async Task<IEnumerable<RiskAlertDto>> GenerateRiskAlertsAsync(int portfolioId)
    {
        _logger.LogInformation("Generating risk alerts for portfolio {PortfolioId}", portfolioId);

        var alerts = new List<RiskAlertDto>();
        
        // Get risk metrics for analysis
        var riskMetrics = await CalculatePortfolioRiskAsync(portfolioId);
        var concentrationRisk = await AssessConcentrationRiskAsync(portfolioId);
        var liquidityAnalysis = await AssessLiquidityAsync(portfolioId);

        // Volatility alerts
        if (riskMetrics.Volatility > 0.25m)
        {
            alerts.Add(new RiskAlertDto
            {
                Type = "High Volatility",
                Message = $"Portfolio volatility is {riskMetrics.Volatility:P2}, above the 25% threshold",
                Severity = RiskSeverity.Warning,
                Threshold = 0.25m,
                CurrentValue = riskMetrics.Volatility,
                Recommendation = "Consider reducing exposure to volatile assets or increasing diversification"
            });
        }

        // VaR alerts
        if (riskMetrics.VaR95 > 0.05m)
        {
            alerts.Add(new RiskAlertDto
            {
                Type = "High VaR",
                Message = $"95% VaR is {riskMetrics.VaR95:P2}, indicating potential for significant losses",
                Severity = RiskSeverity.Critical,
                Threshold = 0.05m,
                CurrentValue = riskMetrics.VaR95,
                Recommendation = "Review portfolio composition and consider risk reduction strategies"
            });
        }

        // Concentration alerts
        if (concentrationRisk.RiskLevel >= ConcentrationRiskLevel.High)
        {
            alerts.Add(new RiskAlertDto
            {
                Type = "High Concentration",
                Message = $"Portfolio concentration risk is {concentrationRisk.RiskLevel}",
                Severity = RiskSeverity.Warning,
                Threshold = 0.35m,
                CurrentValue = concentrationRisk.Top5Concentration,
                Recommendation = "Consider rebalancing to improve diversification across holdings"
            });
        }

        // Liquidity alerts
        if (liquidityAnalysis.OverallRiskLevel >= LiquidityRiskLevel.High)
        {
            alerts.Add(new RiskAlertDto
            {
                Type = "Liquidity Risk",
                Message = $"Portfolio liquidity risk is {liquidityAnalysis.OverallRiskLevel}",
                Severity = RiskSeverity.Warning,
                Threshold = 0.2m,
                CurrentValue = liquidityAnalysis.LowLiquidityPercentage,
                Recommendation = "Consider increasing allocation to liquid assets for better portfolio flexibility"
            });
        }

        // Sharpe ratio alerts
        if (riskMetrics.SharpeRatio < 0.5m)
        {
            alerts.Add(new RiskAlertDto
            {
                Type = "Low Risk-Adjusted Return",
                Message = $"Sharpe ratio is {riskMetrics.SharpeRatio:F2}, indicating poor risk-adjusted performance",
                Severity = RiskSeverity.Info,
                Threshold = 0.5m,
                CurrentValue = riskMetrics.SharpeRatio,
                Recommendation = "Review portfolio strategy to improve risk-adjusted returns"
            });
        }

        return alerts;
    }

    public async Task<PerformanceAttributionDto> CalculatePerformanceAttributionAsync(int portfolioId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Calculating performance attribution for portfolio {PortfolioId} from {StartDate} to {EndDate}", 
            portfolioId, startDate, endDate);

        if (!_portfolios.TryGetValue(portfolioId, out var portfolio))
            throw new ArgumentException($"Portfolio {portfolioId} not found");

        // Simplified attribution analysis (in practice, would need historical weights and benchmark data)
        var totalReturn = 0.08m; // Mock 8% return
        var benchmarkReturn = 0.06m; // Mock 6% benchmark return
        var activeReturn = totalReturn - benchmarkReturn;

        // Simplified attribution effects
        var assetAllocationEffect = 0.01m; // 1% from asset allocation
        var securitySelectionEffect = 0.01m; // 1% from security selection
        var interactionEffect = 0.002m; // 0.2% interaction

        var sectorAttributions = new List<SectorAttributionDto>
        {
            new() { Sector = "Technology", AllocationEffect = 0.005m, SelectionEffect = 0.008m, TotalEffect = 0.013m },
            new() { Sector = "Financials", AllocationEffect = 0.003m, SelectionEffect = 0.002m, TotalEffect = 0.005m },
            new() { Sector = "Healthcare", AllocationEffect = 0.002m, SelectionEffect = -0.001m, TotalEffect = 0.001m }
        };

        return new PerformanceAttributionDto
        {
            TotalReturn = totalReturn,
            BenchmarkReturn = benchmarkReturn,
            ActiveReturn = activeReturn,
            AssetAllocationEffect = assetAllocationEffect,
            SecuritySelectionEffect = securitySelectionEffect,
            InteractionEffect = interactionEffect,
            SectorAttributions = sectorAttributions,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    #region Helper Methods

    private void InitializeSamplePortfolios()
    {
        // Create sample portfolio for testing
        var samplePortfolio = new Portfolio
        {
            Id = 1,
            Name = "Canadian Pension Fund Portfolio",
            PortfolioType = Domain.Enums.PortfolioType.Custom,
            BaseCurrency = "CAD",
            CreatedAt = DateTime.UtcNow.AddMonths(-12)
        };

        // Add sample positions
        samplePortfolio.Positions = new List<Position>
        {
            new() { Security = new Security { Symbol = "RY.TO", Name = "Royal Bank of Canada", SecurityType = Domain.Enums.SecurityType.Stock }, Quantity = 1000, AverageCost = 145.50m },
            new() { Security = new Security { Symbol = "SHOP.TO", Name = "Shopify Inc", SecurityType = Domain.Enums.SecurityType.Stock }, Quantity = 500, AverageCost = 185.25m },
            new() { Security = new Security { Symbol = "TDB902", Name = "TD Canadian Bond Index", SecurityType = Domain.Enums.SecurityType.ETF }, Quantity = 2000, AverageCost = 25.50m },
            new() { Security = new Security { Symbol = "VTI", Name = "Vanguard Total Stock Market", SecurityType = Domain.Enums.SecurityType.ETF }, Quantity = 800, AverageCost = 220.75m },
            new() { Security = new Security { Symbol = "GC001", Name = "Government of Canada Bond", SecurityType = Domain.Enums.SecurityType.Bond }, Quantity = 100, AverageCost = 1025.50m }
        };

        _portfolios[1] = samplePortfolio;
    }

    private decimal CalculatePortfolioValue(Portfolio portfolio)
    {
        return portfolio.Positions.Sum(p => p.Quantity * 150m); // Mock current price
    }

    private IEnumerable<decimal> GetPortfolioValues(Portfolio portfolio, int days)
    {
        var random = new Random(42); // Seed for reproducibility
        var baseValue = CalculatePortfolioValue(portfolio);
        
        for (int i = 0; i < days; i++)
        {
            var dailyReturn = (decimal)(random.NextDouble() - 0.5) * 0.04m; // ±2% daily variation
            baseValue *= (1 + dailyReturn);
            yield return baseValue;
        }
    }

    private async Task<IEnumerable<decimal>> GetPortfolioReturnsAsync(Portfolio portfolio, int days)
    {
        var random = new Random(42); // Seed for reproducibility
        var returns = new List<decimal>();
        
        for (int i = 0; i < days; i++)
        {
            var dailyReturn = (decimal)(random.NextDouble() - 0.5) * 0.04m; // ±2% daily variation
            returns.Add(dailyReturn);
        }
        
        return returns;
    }

    private async Task<IEnumerable<decimal>> GetBenchmarkReturnsAsync(int days)
    {
        var random = new Random(24); // Different seed for benchmark
        var returns = new List<decimal>();
        
        for (int i = 0; i < days; i++)
        {
            var dailyReturn = (decimal)(random.NextDouble() - 0.5) * 0.03m; // ±1.5% daily variation (less volatile than portfolio)
            returns.Add(dailyReturn);
        }
        
        return returns;
    }

    private async Task<IEnumerable<decimal>> GetSecurityReturnsAsync(string symbol, int days)
    {
        var random = new Random(symbol.GetHashCode()); // Consistent seed per symbol
        var returns = new List<decimal>();
        
        for (int i = 0; i < days; i++)
        {
            var dailyReturn = (decimal)(random.NextDouble() - 0.5) * 0.05m; // ±2.5% daily variation
            returns.Add(dailyReturn);
        }
        
        return returns;
    }

    private async Task<decimal> CalculateScenarioValue(Portfolio portfolio, MarketScenarioDto scenario)
    {
        decimal scenarioValue = 0m;
        
        foreach (var position in portfolio.Positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Security.Symbol);
            var sector = GetSecuritySector(position.Security.Symbol);
            
            var returnMultiplier = 1m;
            
            // Apply general market return based on asset type
            if (position.Security.SecurityType == Domain.Enums.SecurityType.Stock || 
                (position.Security.SecurityType == Domain.Enums.SecurityType.ETF && sector.Contains("Equity")))
            {
                returnMultiplier += scenario.EquityReturn;
            }
            else if (position.Security.SecurityType == Domain.Enums.SecurityType.Bond ||
                     (position.Security.SecurityType == Domain.Enums.SecurityType.ETF && sector.Contains("Bond")))
            {
                returnMultiplier += scenario.BondReturn;
            }
            
            // Apply sector-specific returns
            if (scenario.SectorReturns.ContainsKey(sector))
            {
                returnMultiplier += scenario.SectorReturns[sector];
            }
            
            var scenarioPrice = (currentPrice ?? 100m) * returnMultiplier;
            scenarioValue += position.Quantity * scenarioPrice;
        }
        
        return scenarioValue;
    }

    private decimal CalculateExpectedShortfall(IEnumerable<decimal> returns, decimal confidenceLevel)
    {
        var sortedReturns = returns.OrderBy(x => x).ToList();
        var varIndex = (int)Math.Floor((1 - confidenceLevel) * sortedReturns.Count);
        var tailReturns = sortedReturns.Take(varIndex + 1);
        
        return tailReturns.Any() ? -tailReturns.Average() : 0m;
    }

    private string GetSecuritySector(string symbol)
    {
        return symbol switch
        {
            "RY.TO" or "TD.TO" or "BNS.TO" => "Financials",
            "SHOP.TO" or "MSFT" or "AAPL" => "Technology",
            "JNJ" or "PFE" => "Healthcare",
            "TDB902" or "GC001" => "Fixed Income",
            "VTI" => "Broad Market ETF",
            _ => "Other"
        };
    }

    private string GetSecurityGeography(string symbol)
    {
        return symbol.EndsWith(".TO") ? "Canada" : "United States";
    }

    private string GetSecurityCurrency(string symbol)
    {
        return symbol.EndsWith(".TO") ? "CAD" : "USD";
    }

    private string GetAssetClass(Domain.Enums.SecurityType type)
    {
        return type switch
        {
            Domain.Enums.SecurityType.Stock => "Equity",
            Domain.Enums.SecurityType.Bond => "Fixed Income",
            Domain.Enums.SecurityType.ETF => "Fund",
            Domain.Enums.SecurityType.Cash => "Cash",
            Domain.Enums.SecurityType.Option => "Derivative",
            Domain.Enums.SecurityType.Future => "Derivative",
            _ => "Other"
        };
    }

    private int GetLiquidityDays(string symbol, Domain.Enums.SecurityType type)
    {
        return type switch
        {
            Domain.Enums.SecurityType.Cash => 0,
            Domain.Enums.SecurityType.ETF when symbol.Contains("VTI") => 1,
            Domain.Enums.SecurityType.Stock when symbol.EndsWith(".TO") => 1,
            Domain.Enums.SecurityType.Stock => 1,
            Domain.Enums.SecurityType.Bond when symbol.StartsWith("GC") => 2,
            Domain.Enums.SecurityType.Bond => 5,
            Domain.Enums.SecurityType.Option => 1,
            Domain.Enums.SecurityType.Future => 1,
            _ => 3
        };
    }

    #endregion
}