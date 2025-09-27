using InvestmentPortfolioManager.Application.Services.Contracts;

namespace InvestmentPortfolioManager.Application.Services;

/// <summary>
/// Service de tarification d'options utilisant le modèle Black-Scholes et autres méthodes / Option pricing service using Black-Scholes model and other methods
/// </summary>
public class OptionPricingService : IOptionPricingService
{
    public decimal CalculateBlackScholesPrice(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility, bool isCall = true)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        var d2 = d1 - volatility * (decimal)Math.Sqrt((double)timeToExpiry);
        
        var Nd1 = CumulativeNormalDistribution(d1);
        var Nd2 = CumulativeNormalDistribution(d2);
        var NnegD1 = CumulativeNormalDistribution(-d1);
        var NnegD2 = CumulativeNormalDistribution(-d2);
        
        var discountFactor = (decimal)Math.Exp(-(double)riskFreeRate * (double)timeToExpiry);
        
        if (isCall)
        {
            return spotPrice * Nd1 - strikePrice * discountFactor * Nd2;
        }
        else
        {
            return strikePrice * discountFactor * NnegD2 - spotPrice * NnegD1;
        }
    }

    public decimal CalculateDelta(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility, bool isCall = true)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        
        if (isCall)
        {
            return CumulativeNormalDistribution(d1);
        }
        else
        {
            return CumulativeNormalDistribution(d1) - 1m;
        }
    }

    public decimal CalculateGamma(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        var phi = StandardNormalProbabilityDensity(d1);
        
        return phi / (spotPrice * volatility * (decimal)Math.Sqrt((double)timeToExpiry));
    }

    public decimal CalculateTheta(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility, bool isCall = true)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        var d2 = d1 - volatility * (decimal)Math.Sqrt((double)timeToExpiry);
        
        var phi_d1 = StandardNormalProbabilityDensity(d1);
        var Nd2 = CumulativeNormalDistribution(d2);
        var NnegD2 = CumulativeNormalDistribution(-d2);
        
        var discountFactor = (decimal)Math.Exp(-(double)riskFreeRate * (double)timeToExpiry);
        var sqrtT = (decimal)Math.Sqrt((double)timeToExpiry);
        
        var term1 = -spotPrice * phi_d1 * volatility / (2m * sqrtT);
        
        if (isCall)
        {
            var term2 = -riskFreeRate * strikePrice * discountFactor * Nd2;
            return (term1 + term2) / 365m; // Convertir en thêta quotidien / Convert to daily theta
        }
        else
        {
            var term2 = riskFreeRate * strikePrice * discountFactor * NnegD2;
            return (term1 + term2) / 365m; // Convertir en thêta quotidien / Convert to daily theta
        }
    }

    public decimal CalculateVega(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        var phi_d1 = StandardNormalProbabilityDensity(d1);
        
        return spotPrice * phi_d1 * (decimal)Math.Sqrt((double)timeToExpiry) / 100m; // Divisé par 100 pour obtenir la sensibilité à 1% / Divided by 100 for 1% sensitivity
    }

    public decimal CalculateRho(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility, bool isCall = true)
    {
        var d1 = CalculateD1(spotPrice, strikePrice, timeToExpiry, riskFreeRate, volatility);
        var d2 = d1 - volatility * (decimal)Math.Sqrt((double)timeToExpiry);
        
        var discountFactor = (decimal)Math.Exp(-(double)riskFreeRate * (double)timeToExpiry);
        
        if (isCall)
        {
            var Nd2 = CumulativeNormalDistribution(d2);
            return strikePrice * timeToExpiry * discountFactor * Nd2 / 100m; // Divisé par 100 pour obtenir la sensibilité à 1% / Divided by 100 for 1% sensitivity
        }
        else
        {
            var NnegD2 = CumulativeNormalDistribution(-d2);
            return -strikePrice * timeToExpiry * discountFactor * NnegD2 / 100m; // Divisé par 100 pour obtenir la sensibilité à 1% / Divided by 100 for 1% sensitivity
        }
    }

    public decimal CalculateImpliedVolatility(decimal optionPrice, decimal spotPrice, decimal strikePrice, 
        decimal timeToExpiry, decimal riskFreeRate, bool isCall = true)
    {
        // Utiliser la méthode de Newton-Raphson pour résoudre la volatilité implicite / Use Newton-Raphson method to solve for implied volatility
        decimal vol = 0.3m; // Estimation initiale / Initial guess
        decimal tolerance = 0.0001m;
        int maxIterations = 100;
        
        for (int i = 0; i < maxIterations; i++)
        {
            var price = CalculateBlackScholesPrice(spotPrice, strikePrice, timeToExpiry, riskFreeRate, vol, isCall);
            var vega = CalculateVega(spotPrice, strikePrice, timeToExpiry, riskFreeRate, vol) * 100m; // Multiplier par 100 car vega est divisé par 100 / Multiply by 100 as vega is divided by 100
            
            var priceDiff = price - optionPrice;
            
            if (Math.Abs(priceDiff) < tolerance) break;
            
            if (Math.Abs(vega) < tolerance) break; // Éviter la division par zéro / Avoid division by zero
            
            vol = vol - priceDiff / vega;
            
            // S'assurer que la volatilité reste positive / Ensure volatility stays positive
            vol = Math.Max(0.001m, vol);
        }
        
        return vol;
    }

    public decimal CalculateBinomialPrice(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility, int steps, bool isCall = true, bool isAmerican = false)
    {
        var dt = timeToExpiry / steps;
        var u = (decimal)Math.Exp((double)volatility * Math.Sqrt((double)dt)); // Facteur de montée / Up factor
        var d = 1m / u; // Facteur de descente / Down factor
        var p = ((decimal)Math.Exp((double)riskFreeRate * (double)dt) - d) / (u - d); // Probabilité neutre au risque / Risk-neutral probability
        var discount = (decimal)Math.Exp(-(double)riskFreeRate * (double)dt);
        
        // Créer l'arbre des prix / Create price tree
        var prices = new decimal[steps + 1];
        for (int i = 0; i <= steps; i++)
        {
            prices[i] = spotPrice * (decimal)Math.Pow((double)u, steps - i) * (decimal)Math.Pow((double)d, i);
        }
        
        // Calculer les valeurs d'option à l'échéance / Calculate option values at maturity
        var optionValues = new decimal[steps + 1];
        for (int i = 0; i <= steps; i++)
        {
            if (isCall)
            {
                optionValues[i] = Math.Max(0, prices[i] - strikePrice);
            }
            else
            {
                optionValues[i] = Math.Max(0, strikePrice - prices[i]);
            }
        }
        
        // Remonter l'arbre / Walk backwards through the tree
        for (int step = steps - 1; step >= 0; step--)
        {
            for (int i = 0; i <= step; i++)
            {
                // Valeur de continuation / Continuation value
                var continuationValue = discount * (p * optionValues[i] + (1 - p) * optionValues[i + 1]);
                
                if (isAmerican)
                {
                    // Valeur d'exercice anticipé / Early exercise value
                    var currentPrice = spotPrice * (decimal)Math.Pow((double)u, step - i) * (decimal)Math.Pow((double)d, i);
                    decimal exerciseValue;
                    
                    if (isCall)
                        exerciseValue = Math.Max(0, currentPrice - strikePrice);
                    else
                        exerciseValue = Math.Max(0, strikePrice - currentPrice);
                    
                    optionValues[i] = Math.Max(continuationValue, exerciseValue);
                }
                else
                {
                    optionValues[i] = continuationValue;
                }
            }
        }
        
        return optionValues[0];
    }

    #region Méthodes utilitaires / Utility Methods

    private decimal CalculateD1(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
        decimal riskFreeRate, decimal volatility)
    {
        var numerator = (decimal)Math.Log((double)(spotPrice / strikePrice)) + 
                       (riskFreeRate + 0.5m * volatility * volatility) * timeToExpiry;
        var denominator = volatility * (decimal)Math.Sqrt((double)timeToExpiry);
        
        return numerator / denominator;
    }

    private decimal CumulativeNormalDistribution(decimal x)
    {
        // Approximation d'Abramowitz et Stegun / Abramowitz and Stegun approximation
        const decimal a1 = 0.254829592m;
        const decimal a2 = -0.284496736m;
        const decimal a3 = 1.421413741m;
        const decimal a4 = -1.453152027m;
        const decimal a5 = 1.061405429m;
        const decimal p = 0.3275911m;

        var sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);

        var t = 1m / (1m + p * x);
        var y = 1m - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * 
                     (decimal)Math.Exp(-(double)(x * x) / 2.0) / (decimal)Math.Sqrt(2.0 * Math.PI);

        return 0.5m * (1m + sign * y);
    }

    private decimal StandardNormalProbabilityDensity(decimal x)
    {
        return (decimal)(1 / Math.Sqrt(2 * Math.PI)) * (decimal)Math.Exp(-(double)(x * x) / 2.0);
    }

    #endregion
}