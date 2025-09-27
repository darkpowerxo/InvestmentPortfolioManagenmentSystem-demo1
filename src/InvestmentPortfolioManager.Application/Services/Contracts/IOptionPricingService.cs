namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour le calcul des prix d'options / Service for option pricing calculations
/// </summary>
public interface IOptionPricingService
{
    /// <summary>
    /// Calcule le prix d'une option européenne avec le modèle Black-Scholes / Calculate European option price using Black-Scholes model
    /// </summary>
    /// <param name="spotPrice">Prix actuel de l'actif sous-jacent / Current underlying asset price</param>
    /// <param name="strikePrice">Prix d'exercice / Strike price</param>
    /// <param name="timeToMaturity">Temps jusqu'à l'échéance (en années) / Time to maturity (in years)</param>
    /// <param name="riskFreeRate">Taux sans risque / Risk-free rate</param>
    /// <param name="volatility">Volatilité / Volatility</param>
    /// <param name="isCall">True pour call, false pour put / True for call, false for put</param>
    /// <returns>Prix de l'option / Option price</returns>
    decimal CalculateBlackScholesPrice(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility, bool isCall);
    
    /// <summary>
    /// Calcule le delta d'une option / Calculate option delta
    /// </summary>
    decimal CalculateDelta(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility, bool isCall);
    
    /// <summary>
    /// Calcule le gamma d'une option / Calculate option gamma
    /// </summary>
    decimal CalculateGamma(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility);
    
    /// <summary>
    /// Calcule le theta d'une option / Calculate option theta
    /// </summary>
    decimal CalculateTheta(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility, bool isCall);
    
    /// <summary>
    /// Calcule le vega d'une option / Calculate option vega
    /// </summary>
    decimal CalculateVega(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility);
    
    /// <summary>
    /// Calcule le rho d'une option / Calculate option rho
    /// </summary>
    decimal CalculateRho(decimal spotPrice, decimal strikePrice, decimal timeToMaturity, 
        decimal riskFreeRate, decimal volatility, bool isCall);
}