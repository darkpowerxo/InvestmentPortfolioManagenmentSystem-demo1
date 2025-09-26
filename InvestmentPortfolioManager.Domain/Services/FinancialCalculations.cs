// Financial calculations engine for institutional investment management
// Moteur de calculs financiers pour la gestion d'investissements institutionnels
// Comprehensive implementation of financial formulas used in portfolio management
// Implémentation complète de formules financières utilisées en gestion de portefeuille

using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentPortfolioManager.Domain.Services
{
    /// <summary>
    /// Core financial calculations service for portfolio analytics
    /// Service des calculs financiers de base pour l'analyse de portefeuille
    /// </summary>
    public static class FinancialCalculations
    {
        private const decimal RiskFreeRate = 0.025m; // 2.5% default risk-free rate / Taux sans risque par défaut
        private const int TradingDaysPerYear = 252;
        
        #region Portfolio Returns / Rendements de portefeuille

        /// <summary>
        /// Calculates simple return between two values
        /// Calcule le rendement simple entre deux valeurs
        /// </summary>
        /// <param name="initialValue">Initial portfolio value / Valeur initiale du portefeuille</param>
        /// <param name="finalValue">Final portfolio value / Valeur finale du portefeuille</param>
        /// <returns>Simple return as decimal / Rendement simple en décimal</returns>
        public static decimal CalculateSimpleReturn(decimal initialValue, decimal finalValue)
        {
            if (initialValue <= 0)
                throw new ArgumentException("Initial value must be positive / La valeur initiale doit être positive");

            return (finalValue - initialValue) / initialValue;
        }

        /// <summary>
        /// Calculates logarithmic return between two values
        /// Calcule le rendement logarithmique entre deux valeurs
        /// </summary>
        public static decimal CalculateLogReturn(decimal initialValue, decimal finalValue)
        {
            if (initialValue <= 0 || finalValue <= 0)
                throw new ArgumentException("Values must be positive for log return / Les valeurs doivent être positives pour le rendement logarithmique");

            return (decimal)Math.Log((double)(finalValue / initialValue));
        }

        /// <summary>
        /// Calculates time-weighted return for a portfolio
        /// Calcule le rendement pondéré dans le temps pour un portefeuille
        /// </summary>
        /// <param name="periodReturns">List of period returns / Liste des rendements de période</param>
        /// <returns>Compound time-weighted return / Rendement composé pondéré dans le temps</returns>
        public static decimal CalculateTimeWeightedReturn(IEnumerable<decimal> periodReturns)
        {
            if (periodReturns == null || !periodReturns.Any())
                return 0m;

            decimal cumulativeReturn = 1m;
            foreach (var periodReturn in periodReturns)
            {
                cumulativeReturn *= (1m + periodReturn);
            }

            return cumulativeReturn - 1m;
        }

        /// <summary>
        /// Calculates money-weighted return (IRR) for cash flows
        /// Calcule le rendement pondéré par l'argent (TRI) pour les flux de trésorerie
        /// </summary>
        /// <param name="cashFlows">Cash flows including initial investment (negative) and final value / Flux de trésorerie incluant l'investissement initial (négatif) et la valeur finale</param>
        /// <param name="dates">Corresponding dates for cash flows / Dates correspondantes pour les flux de trésorerie</param>
        /// <returns>Internal rate of return / Taux de rendement interne</returns>
        public static decimal CalculateMoneyWeightedReturn(IList<decimal> cashFlows, IList<DateTime> dates)
        {
            if (cashFlows == null || dates == null || cashFlows.Count != dates.Count || cashFlows.Count < 2)
                throw new ArgumentException("Invalid cash flows or dates / Flux de trésorerie ou dates invalides");

            // Newton-Raphson method for IRR calculation
            // Méthode de Newton-Raphson pour le calcul du TRI
            decimal guess = 0.1m; // 10% initial guess
            const int maxIterations = 100;
            const decimal tolerance = 0.0001m;

            for (int i = 0; i < maxIterations; i++)
            {
                decimal npv = CalculateNPV(cashFlows, dates, guess);
                decimal derivative = CalculateNPVDerivative(cashFlows, dates, guess);

                if (Math.Abs(derivative) < tolerance)
                    break;

                decimal newGuess = guess - (npv / derivative);
                
                if (Math.Abs(newGuess - guess) < tolerance)
                    return newGuess;

                guess = newGuess;
            }

            return guess;
        }

        /// <summary>
        /// Calculates annualized return from total return and time period
        /// Calcule le rendement annualisé à partir du rendement total et de la période
        /// </summary>
        public static decimal AnnualizeReturn(decimal totalReturn, double years)
        {
            if (years <= 0)
                throw new ArgumentException("Years must be positive / Les années doivent être positives");

            return (decimal)Math.Pow((double)(1m + totalReturn), 1.0 / years) - 1m;
        }

        #endregion

        #region Risk Metrics / Métriques de risque

        /// <summary>
        /// Calculates portfolio volatility (standard deviation of returns)
        /// Calcule la volatilité du portefeuille (écart-type des rendements)
        /// </summary>
        public static decimal CalculateVolatility(IEnumerable<decimal> returns, bool annualized = true)
        {
            if (returns == null || !returns.Any())
                return 0m;

            var returnsList = returns.ToList();
            if (returnsList.Count < 2)
                return 0m;

            var mean = returnsList.Average();
            var sumSquaredDeviations = returnsList.Sum(r => (r - mean) * (r - mean));
            var variance = sumSquaredDeviations / (returnsList.Count - 1); // Sample variance
            var volatility = (decimal)Math.Sqrt((double)variance);

            return annualized ? volatility * (decimal)Math.Sqrt(TradingDaysPerYear) : volatility;
        }

        /// <summary>
        /// Calculates Sharpe ratio
        /// Calcule le ratio de Sharpe
        /// </summary>
        public static decimal CalculateSharpeRatio(decimal portfolioReturn, decimal portfolioVolatility, 
                                                  decimal riskFreeRate = RiskFreeRate)
        {
            if (portfolioVolatility == 0)
                return 0m;

            return (portfolioReturn - riskFreeRate) / portfolioVolatility;
        }

        /// <summary>
        /// Calculates Sortino ratio (downside deviation based)
        /// Calcule le ratio de Sortino (basé sur l'écart-type de baisse)
        /// </summary>
        public static decimal CalculateSortinoRatio(IEnumerable<decimal> returns, decimal targetReturn = 0m)
        {
            if (returns == null || !returns.Any())
                return 0m;

            var returnsList = returns.ToList();
            var averageReturn = returnsList.Average();
            
            // Calculate downside deviation / Calculer l'écart-type de baisse
            var downsideReturns = returnsList.Where(r => r < targetReturn).ToList();
            if (!downsideReturns.Any())
                return decimal.MaxValue; // No downside risk

            var downsideVariance = downsideReturns.Sum(r => (r - targetReturn) * (r - targetReturn)) / downsideReturns.Count;
            var downsideDeviation = (decimal)Math.Sqrt((double)downsideVariance);

            return downsideDeviation == 0 ? 0m : (averageReturn - targetReturn) / downsideDeviation;
        }

        /// <summary>
        /// Calculates Value at Risk using historical simulation method
        /// Calcule la valeur à risque en utilisant la méthode de simulation historique
        /// </summary>
        public static decimal CalculateVaR(IEnumerable<decimal> returns, decimal portfolioValue, 
                                          decimal confidenceLevel = 0.95m, bool annualized = false)
        {
            if (returns == null || !returns.Any())
                return 0m;

            var sortedReturns = returns.OrderBy(r => r).ToList();
            var index = (int)Math.Floor((double)sortedReturns.Count * (double)(1m - confidenceLevel));
            index = Math.Max(0, Math.Min(index, sortedReturns.Count - 1));

            var varReturn = sortedReturns[index];
            var var = portfolioValue * Math.Abs(varReturn);

            return annualized ? var * (decimal)Math.Sqrt(TradingDaysPerYear) : var;
        }

        /// <summary>
        /// Calculates Conditional Value at Risk (Expected Shortfall)
        /// Calcule la valeur à risque conditionnelle (déficit attendu)
        /// </summary>
        public static decimal CalculateConditionalVaR(IEnumerable<decimal> returns, decimal portfolioValue, 
                                                     decimal confidenceLevel = 0.95m)
        {
            if (returns == null || !returns.Any())
                return 0m;

            var sortedReturns = returns.OrderBy(r => r).ToList();
            var cutoffIndex = (int)Math.Floor((double)sortedReturns.Count * (double)(1m - confidenceLevel));
            
            if (cutoffIndex <= 0)
                return CalculateVaR(returns, portfolioValue, confidenceLevel);

            var tailReturns = sortedReturns.Take(cutoffIndex);
            var expectedTailReturn = tailReturns.Average();

            return portfolioValue * Math.Abs(expectedTailReturn);
        }

        /// <summary>
        /// Calculates maximum drawdown from a series of portfolio values
        /// Calcule le drawdown maximum à partir d'une série de valeurs de portefeuille
        /// </summary>
        public static decimal CalculateMaxDrawdown(IEnumerable<decimal> portfolioValues)
        {
            if (portfolioValues == null || !portfolioValues.Any())
                return 0m;

            var values = portfolioValues.ToList();
            decimal maxDrawdown = 0m;
            decimal peak = values[0];

            foreach (var value in values)
            {
                if (value > peak)
                    peak = value;

                var drawdown = (peak - value) / peak;
                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }

            return maxDrawdown;
        }

        #endregion

        #region Market Risk Metrics / Métriques de risque de marché

        /// <summary>
        /// Calculates Beta (systematic risk relative to market)
        /// Calcule le Beta (risque systématique par rapport au marché)
        /// </summary>
        public static decimal CalculateBeta(IEnumerable<decimal> portfolioReturns, IEnumerable<decimal> marketReturns)
        {
            if (portfolioReturns == null || marketReturns == null)
                return 0m;

            var portfolioList = portfolioReturns.ToList();
            var marketList = marketReturns.ToList();

            if (portfolioList.Count != marketList.Count || portfolioList.Count < 2)
                return 0m;

            var marketVariance = CalculateVariance(marketList);
            if (marketVariance == 0)
                return 0m;

            var covariance = CalculateCovariance(portfolioList, marketList);
            return covariance / marketVariance;
        }

        /// <summary>
        /// Calculates Alpha (excess return over expected return based on Beta)
        /// Calcule l'Alpha (rendement excédentaire par rapport au rendement attendu basé sur le Beta)
        /// </summary>
        public static decimal CalculateAlpha(decimal portfolioReturn, decimal marketReturn, decimal beta, 
                                           decimal riskFreeRate = RiskFreeRate)
        {
            var expectedReturn = riskFreeRate + beta * (marketReturn - riskFreeRate);
            return portfolioReturn - expectedReturn;
        }

        /// <summary>
        /// Calculates tracking error (standard deviation of active returns)
        /// Calcule l'erreur de suivi (écart-type des rendements actifs)
        /// </summary>
        public static decimal CalculateTrackingError(IEnumerable<decimal> portfolioReturns, 
                                                    IEnumerable<decimal> benchmarkReturns, bool annualized = true)
        {
            if (portfolioReturns == null || benchmarkReturns == null)
                return 0m;

            var portfolioList = portfolioReturns.ToList();
            var benchmarkList = benchmarkReturns.ToList();

            if (portfolioList.Count != benchmarkList.Count)
                return 0m;

            var activeReturns = portfolioList.Zip(benchmarkList, (p, b) => p - b);
            return CalculateVolatility(activeReturns, annualized);
        }

        /// <summary>
        /// Calculates Information Ratio
        /// Calcule le ratio d'information
        /// </summary>
        public static decimal CalculateInformationRatio(IEnumerable<decimal> portfolioReturns, 
                                                       IEnumerable<decimal> benchmarkReturns)
        {
            if (portfolioReturns == null || benchmarkReturns == null)
                return 0m;

            var portfolioList = portfolioReturns.ToList();
            var benchmarkList = benchmarkReturns.ToList();

            if (portfolioList.Count != benchmarkList.Count)
                return 0m;

            var activeReturns = portfolioList.Zip(benchmarkList, (p, b) => p - b).ToList();
            var averageActiveReturn = activeReturns.Average();
            var trackingError = CalculateVolatility(activeReturns, false);

            return trackingError == 0 ? 0m : averageActiveReturn / trackingError;
        }

        #endregion

        #region Fixed Income Calculations / Calculs de revenu fixe

        /// <summary>
        /// Calculates bond price given yield
        /// Calcule le prix d'une obligation en fonction du rendement
        /// </summary>
        public static decimal CalculateBondPrice(decimal faceValue, decimal couponRate, decimal yield, 
                                                double yearsToMaturity, int paymentsPerYear = 2)
        {
            if (yearsToMaturity <= 0 || paymentsPerYear <= 0)
                return faceValue;

            var periodsToMaturity = (int)(yearsToMaturity * paymentsPerYear);
            var periodicCoupon = faceValue * couponRate / paymentsPerYear;
            var periodicYield = yield / paymentsPerYear;

            decimal presentValue = 0m;

            // Present value of coupon payments / Valeur présente des paiements de coupon
            for (int t = 1; t <= periodsToMaturity; t++)
            {
                presentValue += periodicCoupon / (decimal)Math.Pow((double)(1m + periodicYield), t);
            }

            // Present value of face value / Valeur présente de la valeur nominale
            presentValue += faceValue / (decimal)Math.Pow((double)(1m + periodicYield), periodsToMaturity);

            return presentValue;
        }

        /// <summary>
        /// Calculates Macaulay duration
        /// Calcule la duration de Macaulay
        /// </summary>
        public static decimal CalculateMacaulayDuration(decimal faceValue, decimal couponRate, decimal yield, 
                                                       double yearsToMaturity, int paymentsPerYear = 2)
        {
            if (yearsToMaturity <= 0 || paymentsPerYear <= 0)
                return 0m;

            var periodsToMaturity = (int)(yearsToMaturity * paymentsPerYear);
            var periodicCoupon = faceValue * couponRate / paymentsPerYear;
            var periodicYield = yield / paymentsPerYear;
            var bondPrice = CalculateBondPrice(faceValue, couponRate, yield, yearsToMaturity, paymentsPerYear);

            if (bondPrice == 0)
                return 0m;

            decimal weightedCashFlows = 0m;

            // Weighted present value of coupon payments
            for (int t = 1; t <= periodsToMaturity; t++)
            {
                var cashFlow = periodicCoupon;
                if (t == periodsToMaturity)
                    cashFlow += faceValue; // Add face value to last payment

                var presentValue = cashFlow / (decimal)Math.Pow((double)(1m + periodicYield), t);
                weightedCashFlows += presentValue * t / paymentsPerYear;
            }

            return weightedCashFlows / bondPrice;
        }

        /// <summary>
        /// Calculates modified duration
        /// Calcule la duration modifiée
        /// </summary>
        public static decimal CalculateModifiedDuration(decimal macaulayDuration, decimal yield, int paymentsPerYear = 2)
        {
            return macaulayDuration / (1m + yield / paymentsPerYear);
        }

        /// <summary>
        /// Calculates convexity for bonds
        /// Calcule la convexité pour les obligations
        /// </summary>
        public static decimal CalculateConvexity(decimal faceValue, decimal couponRate, decimal yield, 
                                                double yearsToMaturity, int paymentsPerYear = 2)
        {
            if (yearsToMaturity <= 0 || paymentsPerYear <= 0)
                return 0m;

            var periodsToMaturity = (int)(yearsToMaturity * paymentsPerYear);
            var periodicCoupon = faceValue * couponRate / paymentsPerYear;
            var periodicYield = yield / paymentsPerYear;
            var bondPrice = CalculateBondPrice(faceValue, couponRate, yield, yearsToMaturity, paymentsPerYear);

            if (bondPrice == 0)
                return 0m;

            decimal convexitySum = 0m;

            for (int t = 1; t <= periodsToMaturity; t++)
            {
                var cashFlow = periodicCoupon;
                if (t == periodsToMaturity)
                    cashFlow += faceValue;

                var presentValue = cashFlow / (decimal)Math.Pow((double)(1m + periodicYield), t);
                convexitySum += presentValue * t * (t + 1);
            }

            return convexitySum / (bondPrice * (decimal)Math.Pow((double)(1m + periodicYield), 2) * paymentsPerYear * paymentsPerYear);
        }

        #endregion

        #region Option Pricing / Évaluation d'options

        /// <summary>
        /// Calculates Black-Scholes option price
        /// Calcule le prix d'option Black-Scholes
        /// </summary>
        public static decimal CalculateBlackScholesPrice(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
                                                        decimal riskFreeRate, decimal volatility, bool isCall = true)
        {
            if (timeToExpiry <= 0 || volatility <= 0 || spotPrice <= 0 || strikePrice <= 0)
                return 0m;

            var d1 = (Math.Log((double)(spotPrice / strikePrice)) + 
                     (double)(riskFreeRate + volatility * volatility / 2m) * (double)timeToExpiry) / 
                     ((double)volatility * Math.Sqrt((double)timeToExpiry));

            var d2 = d1 - (double)volatility * Math.Sqrt((double)timeToExpiry);

            if (isCall)
            {
                return spotPrice * (decimal)NormalCDF(d1) - 
                       strikePrice * (decimal)Math.Exp((double)(-riskFreeRate * timeToExpiry)) * (decimal)NormalCDF(d2);
            }
            else
            {
                return strikePrice * (decimal)Math.Exp((double)(-riskFreeRate * timeToExpiry)) * (decimal)NormalCDF(-d2) - 
                       spotPrice * (decimal)NormalCDF(-d1);
            }
        }

        /// <summary>
        /// Calculates option delta (price sensitivity to underlying)
        /// Calcule le delta d'option (sensibilité du prix au sous-jacent)
        /// </summary>
        public static decimal CalculateOptionDelta(decimal spotPrice, decimal strikePrice, decimal timeToExpiry, 
                                                  decimal riskFreeRate, decimal volatility, bool isCall = true)
        {
            if (timeToExpiry <= 0 || volatility <= 0)
                return 0m;

            var d1 = (Math.Log((double)(spotPrice / strikePrice)) + 
                     (double)(riskFreeRate + volatility * volatility / 2m) * (double)timeToExpiry) / 
                     ((double)volatility * Math.Sqrt((double)timeToExpiry));

            return isCall ? (decimal)NormalCDF(d1) : (decimal)NormalCDF(d1) - 1m;
        }

        #endregion

        #region Helper Methods / Méthodes d'aide

        private static decimal CalculateNPV(IList<decimal> cashFlows, IList<DateTime> dates, decimal rate)
        {
            decimal npv = 0m;
            var baseDate = dates[0];

            for (int i = 0; i < cashFlows.Count; i++)
            {
                var years = (decimal)(dates[i] - baseDate).TotalDays / 365.25m;
                npv += cashFlows[i] / (decimal)Math.Pow((double)(1m + rate), (double)years);
            }

            return npv;
        }

        private static decimal CalculateNPVDerivative(IList<decimal> cashFlows, IList<DateTime> dates, decimal rate)
        {
            decimal derivative = 0m;
            var baseDate = dates[0];

            for (int i = 0; i < cashFlows.Count; i++)
            {
                var years = (decimal)(dates[i] - baseDate).TotalDays / 365.25m;
                derivative -= years * cashFlows[i] / (decimal)Math.Pow((double)(1m + rate), (double)years + 1);
            }

            return derivative;
        }

        private static decimal CalculateVariance(IList<decimal> values)
        {
            if (values.Count < 2)
                return 0m;

            var mean = values.Average();
            var sumSquaredDeviations = values.Sum(v => (v - mean) * (v - mean));
            return sumSquaredDeviations / (values.Count - 1);
        }

        private static decimal CalculateCovariance(IList<decimal> x, IList<decimal> y)
        {
            if (x.Count != y.Count || x.Count < 2)
                return 0m;

            var meanX = x.Average();
            var meanY = y.Average();
            var covariance = 0m;

            for (int i = 0; i < x.Count; i++)
            {
                covariance += (x[i] - meanX) * (y[i] - meanY);
            }

            return covariance / (x.Count - 1);
        }

        /// <summary>
        /// Cumulative normal distribution function
        /// Fonction de distribution normale cumulative
        /// </summary>
        private static double NormalCDF(double x)
        {
            // Approximation using error function
            // Approximation utilisant la fonction d'erreur
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }

        /// <summary>
        /// Error function approximation
        /// Approximation de la fonction d'erreur
        /// </summary>
        private static double Erf(double x)
        {
            // Abramowitz and Stegun approximation
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            var sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            var t = 1.0 / (1.0 + p * x);
            var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        #endregion
    }
}