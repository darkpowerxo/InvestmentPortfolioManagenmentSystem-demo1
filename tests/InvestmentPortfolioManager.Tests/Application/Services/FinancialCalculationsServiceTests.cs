using InvestmentPortfolioManager.Application.Services;
using Xunit;

namespace InvestmentPortfolioManager.Tests.Application.Services;

/// <summary>
/// Tests unitaires pour les calculs financiers / Unit tests for financial calculations
/// </summary>
public class FinancialCalculationsServiceTests
{
    private readonly FinancialCalculationsService _service;

    public FinancialCalculationsServiceTests()
    {
        _service = new FinancialCalculationsService();
    }

    [Fact]
    public void CalculateTimeWeightedReturn_ShouldCalculateCorrectly()
    {
        // Arrange
        var portfolioValues = new List<(DateTime Date, decimal Value)>
        {
            (new DateTime(2023, 1, 1), 100000m),
            (new DateTime(2023, 6, 1), 105000m),
            (new DateTime(2023, 12, 31), 110000m)
        };

        // Act
        var result = _service.CalculateTimeWeightedReturn(portfolioValues);

        // Assert
        Assert.True(result > 0.09m && result < 0.11m); // Environ 10% de rendement / Around 10% return
    }

    [Fact]
    public void CalculateSharpeRatio_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal portfolioReturn = 0.12m; // 12%
        decimal riskFreeRate = 0.03m; // 3%
        decimal volatility = 0.15m; // 15%

        // Act
        var result = _service.CalculateSharpeRatio(portfolioReturn, riskFreeRate, volatility);

        // Assert
        Assert.Equal(0.6m, result, 2); // (12% - 3%) / 15% = 0.6
    }

    [Fact]
    public void CalculateBeta_ShouldCalculateCorrectly()
    {
        // Arrange
        var portfolioReturns = new[] { 0.10m, 0.05m, 0.15m, -0.05m, 0.08m };
        var benchmarkReturns = new[] { 0.08m, 0.04m, 0.12m, -0.03m, 0.06m };

        // Act
        var result = _service.CalculateBeta(portfolioReturns, benchmarkReturns);

        // Assert
        Assert.True(result > 0.5m && result < 1.5m); // Beta raisonnable / Reasonable beta
    }

    [Fact]
    public void CalculateVolatility_ShouldCalculateCorrectly()
    {
        // Arrange
        var returns = new[] { 0.10m, 0.05m, 0.15m, -0.05m, 0.08m, 0.12m, 0.03m, 0.07m };

        // Act
        var result = _service.CalculateVolatility(returns, 252);

        // Assert
        Assert.True(result > 0m); // La volatilité doit être positive / Volatility should be positive
    }

    [Fact]
    public void CalculateVaR_ShouldCalculateCorrectly()
    {
        // Arrange
        var returns = new[] { 0.10m, 0.05m, 0.15m, -0.10m, 0.08m, -0.05m, 0.03m, 0.07m, -0.02m, 0.12m };
        decimal confidenceLevel = 0.95m;

        // Act
        var result = _service.CalculateVaR(returns, confidenceLevel);

        // Assert
        Assert.True(result >= 0m); // VaR doit être positive (perte) / VaR should be positive (loss)
    }

    [Fact]
    public void CalculateMaxDrawdown_ShouldCalculateCorrectly()
    {
        // Arrange
        var portfolioValues = new[] { 100000m, 105000m, 98000m, 102000m, 110000m, 95000m, 108000m };

        // Act
        var result = _service.CalculateMaxDrawdown(portfolioValues);

        // Assert
        Assert.True(result > 0m && result < 1m); // Drawdown entre 0 et 100% / Drawdown between 0 and 100%
    }

    [Fact]
    public void CalculateCorrelation_ShouldCalculateCorrectly()
    {
        // Arrange
        var series1 = new[] { 1m, 2m, 3m, 4m, 5m };
        var series2 = new[] { 2m, 4m, 6m, 8m, 10m }; // Parfaitement corrélées / Perfectly correlated

        // Act
        var result = _service.CalculateCorrelation(series1, series2);

        // Assert
        Assert.True(Math.Abs(result - 1m) < 0.01m); // Corrélation proche de 1 / Correlation close to 1
    }

    [Fact]
    public void CalculateAlpha_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal portfolioReturn = 0.15m;
        decimal benchmarkReturn = 0.10m;
        decimal beta = 1.2m;
        decimal riskFreeRate = 0.03m;

        // Act
        var result = _service.CalculateAlpha(portfolioReturn, benchmarkReturn, beta, riskFreeRate);

        // Assert
        // Alpha = 15% - (3% + 1.2 * (10% - 3%)) = 15% - (3% + 8.4%) = 3.6%
        Assert.True(Math.Abs(result - 0.036m) < 0.001m);
    }
}