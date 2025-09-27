using InvestmentPortfolioManager.Application.Services.Contracts;

namespace InvestmentPortfolioManager.Application.Services;

/// <summary>
/// Implémentation des calculs financiers / Implementation of financial calculations
/// </summary>
public class FinancialCalculationsService : IFinancialCalculationsService
{
    public decimal CalculateTimeWeightedReturn(IEnumerable<(DateTime Date, decimal Value)> portfolioValues)
    {
        var sortedValues = portfolioValues.OrderBy(x => x.Date).ToList();
        if (sortedValues.Count < 2) return 0m;

        decimal totalReturn = 1m;
        for (int i = 1; i < sortedValues.Count; i++)
        {
            var periodReturn = (sortedValues[i].Value / sortedValues[i - 1].Value) - 1m;
            totalReturn *= (1m + periodReturn);
        }

        return totalReturn - 1m;
    }

    public decimal CalculateMoneyWeightedReturn(IEnumerable<(DateTime Date, decimal CashFlow)> cashFlows, 
        decimal initialValue, decimal finalValue)
    {
        // Implémentation simplifiée du taux de rendement interne (TRI) / Simplified IRR implementation
        var flows = cashFlows.OrderBy(x => x.Date).ToList();
        if (!flows.Any()) return 0m;

        // Utiliser la méthode de Newton-Raphson pour résoudre l'équation TRI / Use Newton-Raphson method to solve IRR equation
        decimal rate = 0.1m; // Estimation initiale / Initial guess
        decimal tolerance = 0.0001m;
        int maxIterations = 100;

        for (int i = 0; i < maxIterations; i++)
        {
            decimal npv = -initialValue;
            decimal npvDerivative = 0m;
            
            var startDate = flows.First().Date;
            
            foreach (var flow in flows)
            {
                var yearFraction = (decimal)(flow.Date - startDate).TotalDays / 365.25m;
                var discountFactor = (decimal)Math.Pow((double)(1m + rate), (double)yearFraction);
                
                npv += flow.CashFlow / discountFactor;
                npvDerivative -= flow.CashFlow * yearFraction / discountFactor / (1m + rate);
            }
            
            // Ajouter la valeur finale / Add final value
            var finalYearFraction = (decimal)(flows.Last().Date - startDate).TotalDays / 365.25m;
            var finalDiscountFactor = (decimal)Math.Pow((double)(1m + rate), (double)finalYearFraction);
            npv += finalValue / finalDiscountFactor;
            npvDerivative -= finalValue * finalYearFraction / finalDiscountFactor / (1m + rate);

            if (Math.Abs(npv) < tolerance) break;
            
            if (Math.Abs(npvDerivative) < tolerance) break; // Éviter la division par zéro / Avoid division by zero
            
            rate = rate - npv / npvDerivative;
        }

        return rate;
    }

    public decimal CalculateSharpeRatio(decimal portfolioReturn, decimal riskFreeRate, decimal volatility)
    {
        if (volatility == 0m) return 0m;
        return (portfolioReturn - riskFreeRate) / volatility;
    }

    public decimal CalculateSortinoRatio(IEnumerable<decimal> returns, decimal riskFreeRate)
    {
        var returnsList = returns.ToList();
        if (!returnsList.Any()) return 0m;

        var avgReturn = returnsList.Average();
        var downSideReturns = returnsList.Where(r => r < riskFreeRate).ToList();
        
        if (!downSideReturns.Any()) return decimal.MaxValue; // Pas de rendements négatifs / No negative returns
        
        var downSideVariance = downSideReturns.Select(r => (r - riskFreeRate) * (r - riskFreeRate)).Average();
        var downSideDeviation = (decimal)Math.Sqrt((double)downSideVariance);
        
        if (downSideDeviation == 0m) return 0m;
        return (avgReturn - riskFreeRate) / downSideDeviation;
    }

    public decimal CalculateBeta(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns)
    {
        var portfolioList = portfolioReturns.ToList();
        var benchmarkList = benchmarkReturns.ToList();
        
        if (portfolioList.Count != benchmarkList.Count || portfolioList.Count < 2) return 1m;

        var portfolioMean = portfolioList.Average();
        var benchmarkMean = benchmarkList.Average();
        
        decimal covariance = 0m;
        decimal benchmarkVariance = 0m;
        
        for (int i = 0; i < portfolioList.Count; i++)
        {
            var portfolioDiff = portfolioList[i] - portfolioMean;
            var benchmarkDiff = benchmarkList[i] - benchmarkMean;
            
            covariance += portfolioDiff * benchmarkDiff;
            benchmarkVariance += benchmarkDiff * benchmarkDiff;
        }
        
        covariance /= (portfolioList.Count - 1);
        benchmarkVariance /= (portfolioList.Count - 1);
        
        if (benchmarkVariance == 0m) return 1m;
        return covariance / benchmarkVariance;
    }

    public decimal CalculateAlpha(decimal portfolioReturn, decimal benchmarkReturn, decimal beta, decimal riskFreeRate)
    {
        return portfolioReturn - (riskFreeRate + beta * (benchmarkReturn - riskFreeRate));
    }

    public decimal CalculateVolatility(IEnumerable<decimal> returns, int periodsPerYear = 252)
    {
        var returnsList = returns.ToList();
        if (returnsList.Count < 2) return 0m;

        var mean = returnsList.Average();
        var variance = returnsList.Select(r => (r - mean) * (r - mean)).Average();
        var standardDeviation = (decimal)Math.Sqrt((double)variance);
        
        // Annualiser la volatilité / Annualize volatility
        return standardDeviation * (decimal)Math.Sqrt(periodsPerYear);
    }

    public decimal CalculateVaR(IEnumerable<decimal> returns, decimal confidenceLevel = 0.95m)
    {
        var sortedReturns = returns.OrderBy(x => x).ToList();
        if (!sortedReturns.Any()) return 0m;

        var index = (int)Math.Floor((double)((1m - confidenceLevel) * sortedReturns.Count));
        index = Math.Max(0, Math.Min(index, sortedReturns.Count - 1));
        
        return -sortedReturns[index]; // Retourner une valeur positive / Return positive value
    }

    public decimal CalculateMaxDrawdown(IEnumerable<decimal> portfolioValues)
    {
        var values = portfolioValues.ToList();
        if (values.Count < 2) return 0m;

        decimal maxDrawdown = 0m;
        decimal peak = values[0];

        foreach (var value in values)
        {
            if (value > peak)
            {
                peak = value;
            }
            else
            {
                var drawdown = (peak - value) / peak;
                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }
        }

        return maxDrawdown;
    }

    public decimal CalculateTrackingError(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns)
    {
        var portfolioList = portfolioReturns.ToList();
        var benchmarkList = benchmarkReturns.ToList();
        
        if (portfolioList.Count != benchmarkList.Count) return 0m;
        
        var differences = portfolioList.Zip(benchmarkList, (p, b) => p - b).ToList();
        return CalculateVolatility(differences);
    }

    public decimal CalculateInformationRatio(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> benchmarkReturns)
    {
        var portfolioList = portfolioReturns.ToList();
        var benchmarkList = benchmarkReturns.ToList();
        
        if (portfolioList.Count != benchmarkList.Count) return 0m;
        
        var differences = portfolioList.Zip(benchmarkList, (p, b) => p - b).ToList();
        var avgExcessReturn = differences.Average();
        var trackingError = CalculateVolatility(differences);
        
        if (trackingError == 0m) return 0m;
        return avgExcessReturn / trackingError;
    }

    public decimal CalculateCorrelation(IEnumerable<decimal> series1, IEnumerable<decimal> series2)
    {
        var list1 = series1.ToList();
        var list2 = series2.ToList();
        
        if (list1.Count != list2.Count || list1.Count < 2) return 0m;
        
        var mean1 = list1.Average();
        var mean2 = list2.Average();
        
        decimal numerator = 0m;
        decimal sumSquares1 = 0m;
        decimal sumSquares2 = 0m;
        
        for (int i = 0; i < list1.Count; i++)
        {
            var diff1 = list1[i] - mean1;
            var diff2 = list2[i] - mean2;
            
            numerator += diff1 * diff2;
            sumSquares1 += diff1 * diff1;
            sumSquares2 += diff2 * diff2;
        }
        
        var denominator = (decimal)Math.Sqrt((double)(sumSquares1 * sumSquares2));
        
        if (denominator == 0m) return 0m;
        return numerator / denominator;
    }

    public decimal[,] CalculateCorrelationMatrix(IEnumerable<IEnumerable<decimal>> returnSeries)
    {
        var series = returnSeries.Select(s => s.ToList()).ToList();
        var n = series.Count;
        var correlationMatrix = new decimal[n, n];
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    correlationMatrix[i, j] = 1m;
                }
                else
                {
                    correlationMatrix[i, j] = CalculateCorrelation(series[i], series[j]);
                }
            }
        }
        
        return correlationMatrix;
    }
}