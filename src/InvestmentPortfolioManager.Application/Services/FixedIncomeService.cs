using InvestmentPortfolioManager.Application.Services.Contracts;

namespace InvestmentPortfolioManager.Application.Services;

/// <summary>
/// Service de calculs pour les instruments à revenu fixe (obligations) / Fixed income instruments calculations service
/// </summary>
public class FixedIncomeService : IFixedIncomeService
{
    public decimal CalculateBondPrice(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        if (yearsToMaturity <= 0) return faceValue;
        
        var periodicCoupon = faceValue * couponRate / couponFrequency;
        var periodicYield = yieldToMaturity / couponFrequency;
        var totalPeriods = (int)(yearsToMaturity * couponFrequency);
        
        decimal presentValueCoupons = 0m;
        decimal presentValuePrincipal = 0m;
        
        // Calculer la valeur présente des coupons / Calculate present value of coupons
        for (int i = 1; i <= totalPeriods; i++)
        {
            presentValueCoupons += periodicCoupon / (decimal)Math.Pow(1 + (double)periodicYield, i);
        }
        
        // Calculer la valeur présente du principal / Calculate present value of principal
        presentValuePrincipal = faceValue / (decimal)Math.Pow(1 + (double)periodicYield, totalPeriods);
        
        return presentValueCoupons + presentValuePrincipal;
    }

    public decimal CalculateYieldToMaturity(decimal currentPrice, decimal faceValue, decimal couponRate, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        // Utiliser la méthode de Newton-Raphson pour résoudre le rendement / Use Newton-Raphson method to solve for yield
        decimal yield = couponRate; // Estimation initiale / Initial guess
        decimal tolerance = 0.0001m;
        int maxIterations = 100;
        
        for (int i = 0; i < maxIterations; i++)
        {
            var price = CalculateBondPrice(faceValue, couponRate, yield, yearsToMaturity, couponFrequency);
            var priceDiff = price - currentPrice;
            
            if (Math.Abs(priceDiff) < tolerance) break;
            
            // Calculer la dérivée (duration modifiée approximative) / Calculate derivative (approximate modified duration)
            var yieldBump = 0.0001m;
            var priceUp = CalculateBondPrice(faceValue, couponRate, yield + yieldBump, yearsToMaturity, couponFrequency);
            var priceDown = CalculateBondPrice(faceValue, couponRate, yield - yieldBump, yearsToMaturity, couponFrequency);
            var derivative = (priceUp - priceDown) / (2 * yieldBump);
            
            if (Math.Abs(derivative) < tolerance) break; // Éviter la division par zéro / Avoid division by zero
            
            yield = yield - priceDiff / derivative;
            yield = Math.Max(0.0001m, yield); // S'assurer que le rendement reste positif / Ensure yield stays positive
        }
        
        return yield;
    }

    public decimal CalculateModifiedDuration(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        var macaulayDuration = CalculateMacaulayDuration(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);
        var periodicYield = yieldToMaturity / couponFrequency;
        
        return macaulayDuration / (1 + periodicYield);
    }

    public decimal CalculateConvexity(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        var periodicCoupon = faceValue * couponRate / couponFrequency;
        var periodicYield = yieldToMaturity / couponFrequency;
        var totalPeriods = (int)(yearsToMaturity * couponFrequency);
        var bondPrice = CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);
        
        decimal convexity = 0m;
        
        // Calculer la convexité des coupons / Calculate convexity of coupons
        for (int i = 1; i <= totalPeriods; i++)
        {
            var cashFlow = periodicCoupon;
            var discountFactor = (decimal)Math.Pow(1 + (double)periodicYield, -i);
            var presentValue = cashFlow * discountFactor;
            
            convexity += presentValue * i * (i + 1);
        }
        
        // Ajouter la convexité du principal / Add convexity of principal
        var principalPV = faceValue / (decimal)Math.Pow(1 + (double)periodicYield, totalPeriods);
        convexity += principalPV * totalPeriods * (totalPeriods + 1);
        
        // Normaliser par le prix de l'obligation et le carré du rendement / Normalize by bond price and square of yield
        convexity = convexity / (bondPrice * (decimal)Math.Pow(1 + (double)periodicYield, 2));
        
        // Ajuster pour les paiements annuels / Adjust for annual payments
        return convexity / (couponFrequency * couponFrequency);
    }

    public decimal CalculateMacaulayDuration(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        var periodicCoupon = faceValue * couponRate / couponFrequency;
        var periodicYield = yieldToMaturity / couponFrequency;
        var totalPeriods = (int)(yearsToMaturity * couponFrequency);
        var bondPrice = CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);
        
        decimal weightedTime = 0m;
        
        // Calculer le temps pondéré des coupons / Calculate weighted time of coupons
        for (int i = 1; i <= totalPeriods; i++)
        {
            var cashFlow = periodicCoupon;
            var discountFactor = (decimal)Math.Pow(1 + (double)periodicYield, -i);
            var presentValue = cashFlow * discountFactor;
            var timeInYears = (decimal)i / couponFrequency;
            
            weightedTime += presentValue * timeInYears;
        }
        
        // Ajouter le temps pondéré du principal / Add weighted time of principal
        var principalPV = faceValue / (decimal)Math.Pow(1 + (double)periodicYield, totalPeriods);
        var principalTimeInYears = (decimal)totalPeriods / couponFrequency;
        weightedTime += principalPV * principalTimeInYears;
        
        return weightedTime / bondPrice;
    }

    public decimal CalculateDollarDuration(decimal faceValue, decimal couponRate, decimal yieldToMaturity, 
        decimal yearsToMaturity, int couponFrequency = 2)
    {
        var modifiedDuration = CalculateModifiedDuration(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);
        var bondPrice = CalculateBondPrice(faceValue, couponRate, yieldToMaturity, yearsToMaturity, couponFrequency);
        
        return modifiedDuration * bondPrice;
    }

    public decimal CalculateAccruedInterest(decimal faceValue, decimal couponRate, DateTime lastCouponDate, 
        DateTime settlementDate, int paymentsPerYear = 2)
    {
        var periodicCoupon = faceValue * couponRate / paymentsPerYear;
        var daysBetweenCoupons = 365.25 / paymentsPerYear; // Approximation / Approximation
        var daysSinceLastCoupon = (settlementDate - lastCouponDate).TotalDays;
        
        return periodicCoupon * (decimal)(daysSinceLastCoupon / daysBetweenCoupons);
    }

    public decimal CalculateCurrentYield(decimal annualCouponPayment, decimal currentPrice)
    {
        return annualCouponPayment / currentPrice;
    }

    public decimal CalculatePriceSensitivity(decimal duration, decimal rateChange)
    {
        // Sensibilité approximative du prix = -Duration × Changement de taux / Approximate price sensitivity = -Duration × Rate change
        return -duration * rateChange;
    }

    public decimal CalculateSpreadOverBenchmark(decimal bondYield, decimal benchmarkYield)
    {
        return bondYield - benchmarkYield;
    }

    public decimal CalculateZSpread(decimal bondPrice, decimal faceValue, decimal couponRate, 
        decimal yearsToMaturity, IEnumerable<(int Period, decimal SpotRate)> spotRates, int couponFrequency = 2)
    {
        // Utiliser la méthode itérative pour trouver le Z-spread / Use iterative method to find Z-spread
        decimal zSpread = 0.01m; // Estimation initiale 1% / Initial guess 1%
        decimal tolerance = 0.0001m;
        int maxIterations = 100;
        
        var spotRatesList = spotRates.OrderBy(x => x.Period).ToList();
        
        for (int i = 0; i < maxIterations; i++)
        {
            var calculatedPrice = CalculatePriceWithZSpread(faceValue, couponRate, yearsToMaturity, 
                spotRatesList, zSpread, couponFrequency);
            var priceDiff = calculatedPrice - bondPrice;
            
            if (Math.Abs(priceDiff) < tolerance) break;
            
            // Calculer la dérivée numérique / Calculate numerical derivative
            var spreadBump = 0.0001m;
            var priceUp = CalculatePriceWithZSpread(faceValue, couponRate, yearsToMaturity, 
                spotRatesList, zSpread + spreadBump, couponFrequency);
            var priceDown = CalculatePriceWithZSpread(faceValue, couponRate, yearsToMaturity, 
                spotRatesList, zSpread - spreadBump, couponFrequency);
            var derivative = (priceUp - priceDown) / (2 * spreadBump);
            
            if (Math.Abs(derivative) < tolerance) break;
            
            zSpread = zSpread - priceDiff / derivative;
        }
        
        return zSpread;
    }

    public decimal CalculateOptionAdjustedSpread(decimal bondPrice, decimal faceValue, decimal couponRate, 
        decimal yearsToMaturity, IEnumerable<(int Period, decimal SpotRate)> spotRates, 
        decimal optionValue, int couponFrequency = 2)
    {
        // Le OAS est le Z-spread ajusté pour la valeur de l'option / OAS is Z-spread adjusted for option value
        var adjustedPrice = bondPrice + optionValue; // Pour une obligation callable / For callable bond
        return CalculateZSpread(adjustedPrice, faceValue, couponRate, yearsToMaturity, spotRates, couponFrequency);
    }

    #region Méthodes utilitaires / Utility Methods

    private decimal CalculatePriceWithZSpread(decimal faceValue, decimal couponRate, decimal yearsToMaturity,
        IList<(int Period, decimal SpotRate)> spotRates, decimal zSpread, int couponFrequency)
    {
        var periodicCoupon = faceValue * couponRate / couponFrequency;
        var totalPeriods = (int)(yearsToMaturity * couponFrequency);
        
        decimal price = 0m;
        
        // Calculer la valeur présente de chaque flux de trésorerie / Calculate present value of each cash flow
        for (int i = 1; i <= totalPeriods; i++)
        {
            var cashFlow = (i == totalPeriods) ? periodicCoupon + faceValue : periodicCoupon;
            var spotRate = InterpolateSpotRate(spotRates, i);
            var discountRate = (spotRate + zSpread) / couponFrequency;
            
            price += cashFlow / (decimal)Math.Pow(1 + (double)discountRate, i);
        }
        
        return price;
    }

    private decimal InterpolateSpotRate(IList<(int Period, decimal SpotRate)> spotRates, int period)
    {
        // Interpolation linéaire simple / Simple linear interpolation
        var lower = spotRates.Where(x => x.Period <= period).LastOrDefault();
        var upper = spotRates.Where(x => x.Period >= period).FirstOrDefault();
        
        if (lower.Period == period) return lower.SpotRate;
        if (upper.Period == period) return upper.SpotRate;
        if (lower.Period == 0 && upper.Period == 0) return spotRates.First().SpotRate;
        
        // Interpolation linéaire / Linear interpolation
        var weight = (decimal)(period - lower.Period) / (upper.Period - lower.Period);
        return lower.SpotRate + weight * (upper.SpotRate - lower.SpotRate);
    }

    #endregion
}