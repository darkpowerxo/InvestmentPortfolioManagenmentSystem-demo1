using InvestmentPortfolioManager.Application.Services;
using Xunit;

namespace InvestmentPortfolioManager.Tests.Application.Services;

/// <summary>
/// Tests unitaires pour les calculs d'obligations / Unit tests for fixed income calculations
/// </summary>
public class FixedIncomeServiceTests
{
    private readonly FixedIncomeService _service;

    public FixedIncomeServiceTests()
    {
        _service = new FixedIncomeService();
    }

    [Fact]
    public void CalculateBondPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m; // 5%
        decimal yieldToMaturity = 0.04m; // 4%
        decimal yearsToMaturity = 10m;
        int couponFrequency = 2; // Semi-annuel / Semi-annual

        // Act
        var result = _service.CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);

        // Assert
        Assert.True(result > faceValue); // Prix supérieur à la valeur nominale car rendement < coupon / Price above face value as yield < coupon
        Assert.True(result < 1200m); // Prix raisonnable / Reasonable price
    }

    [Fact]
    public void CalculateBondPrice_YieldEqualsCoupon_ShouldEqualFaceValue()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m;
        decimal yieldToMaturity = 0.05m; // Même que le coupon / Same as coupon
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        Assert.True(Math.Abs(result - faceValue) < 1m); // Prix proche de la valeur nominale / Price close to face value
    }

    [Fact]
    public void CalculateYieldToMaturity_ShouldConverge()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.06m;
        decimal knownYield = 0.05m;
        decimal yearsToMaturity = 5m;
        
        // Calculer le prix avec un rendement connu / Calculate price with known yield
        var bondPrice = _service.CalculateBondPrice(faceValue, couponRate, knownYield, yearsToMaturity);

        // Act - Récupérer le rendement / Retrieve yield
        var result = _service.CalculateYieldToMaturity(bondPrice, faceValue, couponRate, yearsToMaturity);

        // Assert
        Assert.True(Math.Abs(result - knownYield) < 0.001m); // Convergence vers le rendement original / Convergence to original yield
    }

    [Fact]
    public void CalculateModifiedDuration_ShouldBePositive()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m;
        decimal yieldToMaturity = 0.04m;
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateModifiedDuration(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        Assert.True(result > 0m && result < yearsToMaturity); // Duration positive et inférieure à la maturité / Positive duration less than maturity
    }

    [Fact]
    public void CalculateMacaulayDuration_ShouldBePositive()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m;
        decimal yieldToMaturity = 0.04m;
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateMacaulayDuration(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        Assert.True(result > 0m && result <= yearsToMaturity); // Duration Macaulay positive / Positive Macaulay duration
    }

    [Fact]
    public void CalculateConvexity_ShouldBePositive()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m;
        decimal yieldToMaturity = 0.04m;
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateConvexity(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        Assert.True(result > 0m); // Convexité toujours positive / Convexity always positive
    }

    [Fact]
    public void CalculateCurrentYield_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal annualCouponPayment = 50m; // 5% de 1000 / 5% of 1000
        decimal currentPrice = 950m;

        // Act
        var result = _service.CalculateCurrentYield(annualCouponPayment, currentPrice);

        // Assert
        decimal expectedYield = 50m / 950m; // ≈ 5.26%
        Assert.True(Math.Abs(result - expectedYield) < 0.001m);
    }

    [Fact]
    public void CalculatePriceSensitivity_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal duration = 5.5m;
        decimal rateChange = 0.01m; // Hausse de 1% / 1% increase

        // Act
        var result = _service.CalculatePriceSensitivity(duration, rateChange);

        // Assert
        decimal expectedSensitivity = -5.5m * 0.01m; // -5.5%
        Assert.Equal(expectedSensitivity, result, 4);
    }

    [Fact]
    public void CalculateAccruedInterest_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.06m; // 6%
        var lastCouponDate = new DateTime(2024, 1, 1);
        var settlementDate = new DateTime(2024, 4, 1); // 3 mois après / 3 months later
        int paymentsPerYear = 2;

        // Act
        var result = _service.CalculateAccruedInterest(faceValue, couponRate, lastCouponDate, settlementDate, paymentsPerYear);

        // Assert
        Assert.True(result > 0m && result < 30m); // Intérêts courus raisonnables / Reasonable accrued interest
    }

    [Fact]
    public void CalculateDollarDuration_ShouldBePositive()
    {
        // Arrange
        decimal faceValue = 1000m;
        decimal couponRate = 0.05m;
        decimal yieldToMaturity = 0.04m;
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateDollarDuration(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        Assert.True(result > 0m); // Dollar duration toujours positive / Dollar duration always positive
    }

    [Fact]
    public void ZeroCouponBond_ShouldCalculateCorrectly()
    {
        // Arrange - Obligation zéro coupon / Zero coupon bond
        decimal faceValue = 1000m;
        decimal couponRate = 0m; // Pas de coupons / No coupons
        decimal yieldToMaturity = 0.05m;
        decimal yearsToMaturity = 10m;

        // Act
        var result = _service.CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity);

        // Assert
        decimal expectedPrice = faceValue / (decimal)Math.Pow(1.025, 20); // Semi-annual compounding: (1 + 0.05/2)^20 ≈ 610.27
        Assert.True(Math.Abs(result - expectedPrice) < 5m); // More tolerance for zero coupon bonds
    }
}