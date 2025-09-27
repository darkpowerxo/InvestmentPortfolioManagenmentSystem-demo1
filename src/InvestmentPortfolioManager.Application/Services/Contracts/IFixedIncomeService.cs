namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour les calculs d'obligations / Service for fixed income calculations
/// </summary>
public interface IFixedIncomeService
{
    /// <summary>
    /// Calcule le prix d'une obligation / Calculate bond price
    /// </summary>
    /// <param name="faceValue">Valeur nominale / Face value</param>
    /// <param name="couponRate">Taux de coupon annuel / Annual coupon rate</param>
    /// <param name="yieldToMaturity">Rendement à l'échéance / Yield to maturity</param>
    /// <param name="yearsToMaturity">Années jusqu'à l'échéance / Years to maturity</param>
    /// <param name="couponFrequency">Fréquence des coupons par an / Coupon payments per year</param>
    /// <returns>Prix de l'obligation / Bond price</returns>
    decimal CalculateBondPrice(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2);
    
    /// <summary>
    /// Calcule la durée modifiée d'une obligation / Calculate modified duration
    /// </summary>
    decimal CalculateModifiedDuration(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2);
    
    /// <summary>
    /// Calcule la durée de Macaulay / Calculate Macaulay duration
    /// </summary>
    decimal CalculateMacaulayDuration(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2);
    
    /// <summary>
    /// Calcule la convexité d'une obligation / Calculate bond convexity
    /// </summary>
    decimal CalculateConvexity(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2);
    
    /// <summary>
    /// Calcule le rendement à l'échéance / Calculate yield to maturity
    /// </summary>
    decimal CalculateYieldToMaturity(decimal currentPrice, decimal faceValue, decimal couponRate, 
        decimal yearsToMaturity, int couponFrequency = 2);
    
    /// <summary>
    /// Calcule le rendement courant / Calculate current yield
    /// </summary>
    decimal CalculateCurrentYield(decimal annualCouponPayment, decimal currentPrice);
    
    /// <summary>
    /// Calcule la sensibilité du prix aux changements de taux / Calculate price sensitivity to rate changes
    /// </summary>
    decimal CalculatePriceSensitivity(decimal duration, decimal rateChange);
}