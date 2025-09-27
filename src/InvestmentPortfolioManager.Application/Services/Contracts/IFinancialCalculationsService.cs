namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour les calculs financiers / Service for financial calculations
/// </summary>
public interface IFinancialCalculationsService
{
    /// <summary>
    /// Calcule le rendement pondéré dans le temps / Calculate time-weighted return
    /// </summary>
    decimal CalculateTimeWeightedReturn(IEnumerable<(DateTime Date, decimal Value)> portfolioValues);
    
    /// <summary>
    /// Calcule le rendement pondéré par l'argent / Calculate money-weighted return (IRR)
    /// </summary>
    decimal CalculateMoneyWeightedReturn(IEnumerable<(DateTime Date, decimal CashFlow)> cashFlows, decimal initialValue, decimal finalValue);
    
    /// <summary>
    /// Calcule le ratio de Sharpe / Calculate Sharpe ratio
    /// </summary>
    decimal CalculateSharpeRatio(decimal portfolioReturn, decimal riskFreeRate, decimal volatility);
    
    /// <summary>
    /// Calcule le ratio de Sortino / Calculate Sortino ratio
    /// </summary>
    decimal CalculateSortinoRatio(IEnumerable<decimal> returns, decimal riskFreeRate);
    
    /// <summary>
    /// Calcule le bêta par rapport à un indice de référence / Calculate beta against benchmark
    /// </summary>
    decimal CalculateBeta(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns);
    
    /// <summary>
    /// Calcule l'alpha par rapport à un indice de référence / Calculate alpha against benchmark
    /// </summary>
    decimal CalculateAlpha(decimal portfolioReturn, decimal benchmarkReturn, decimal beta, decimal riskFreeRate);
    
    /// <summary>
    /// Calcule la volatilité (écart-type annualisé) / Calculate volatility (annualized standard deviation)
    /// </summary>
    decimal CalculateVolatility(IEnumerable<decimal> returns, int periodsPerYear = 252);
    
    /// <summary>
    /// Calcule la Value at Risk (VaR) / Calculate Value at Risk
    /// </summary>
    decimal CalculateVaR(IEnumerable<decimal> returns, decimal confidenceLevel = 0.95m);
    
    /// <summary>
    /// Calcule le drawdown maximum / Calculate maximum drawdown
    /// </summary>
    decimal CalculateMaxDrawdown(IEnumerable<decimal> portfolioValues);
    
    /// <summary>
    /// Calcule l'erreur de suivi / Calculate tracking error
    /// </summary>
    decimal CalculateTrackingError(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns);
    
    /// <summary>
    /// Calcule le ratio d'information / Calculate information ratio
    /// </summary>
    decimal CalculateInformationRatio(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns);
    
    /// <summary>
    /// Calcule la corrélation entre deux séries de rendements / Calculate correlation between two return series
    /// </summary>
    decimal CalculateCorrelation(IEnumerable<decimal> series1, IEnumerable<decimal> series2);
    
    /// <summary>
    /// Calcule la matrice de corrélation / Calculate correlation matrix
    /// </summary>
    decimal[,] CalculateCorrelationMatrix(IEnumerable<IEnumerable<decimal>> returnSeries);
}