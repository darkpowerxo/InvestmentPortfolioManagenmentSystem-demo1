// Market data service for real-time and historical market information
// Service de données de marché pour les informations de marché en temps réel et historiques
// Technical indicators and market data processing capabilities
// Capacités d'indicateurs techniques et de traitement des données de marché

using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Domain.Services
{
    /// <summary>
    /// Market data processing and technical analysis service
    /// Service de traitement des données de marché et d'analyse technique
    /// </summary>
    public class MarketDataService
    {
        #region Price Data Processing / Traitement des données de prix

        /// <summary>
        /// Calculates simple moving average for a given period
        /// Calcule la moyenne mobile simple pour une période donnée
        /// </summary>
        public List<TechnicalIndicatorValue> CalculateSimpleMovingAverage(IEnumerable<MarketData> prices, int period)
        {
            if (prices == null || period <= 0)
                return new List<TechnicalIndicatorValue>();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var results = new List<TechnicalIndicatorValue>();

            for (int i = period - 1; i < pricesList.Count; i++)
            {
                var periodPrices = pricesList.Skip(i - period + 1).Take(period);
                var average = periodPrices.Average(p => p.ClosePrice);

                results.Add(new TechnicalIndicatorValue
                {
                    Date = pricesList[i].Date,
                    Value = average,
                    IndicatorName = $"SMA_{period}"
                });
            }

            return results;
        }

        /// <summary>
        /// Calculates exponential moving average
        /// Calcule la moyenne mobile exponentielle
        /// </summary>
        public List<TechnicalIndicatorValue> CalculateExponentialMovingAverage(IEnumerable<MarketData> prices, int period)
        {
            if (prices == null || period <= 0)
                return new List<TechnicalIndicatorValue>();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var results = new List<TechnicalIndicatorValue>();
            
            if (pricesList.Count < period)
                return results;

            var multiplier = 2m / (period + 1);
            
            // Initialize with first SMA value
            var initialSMA = pricesList.Take(period).Average(p => p.ClosePrice);
            decimal previousEMA = initialSMA;

            results.Add(new TechnicalIndicatorValue
            {
                Date = pricesList[period - 1].Date,
                Value = initialSMA,
                IndicatorName = $"EMA_{period}"
            });

            // Calculate EMA for remaining periods
            for (int i = period; i < pricesList.Count; i++)
            {
                var currentEMA = (pricesList[i].ClosePrice * multiplier) + (previousEMA * (1 - multiplier));
                
                results.Add(new TechnicalIndicatorValue
                {
                    Date = pricesList[i].Date,
                    Value = currentEMA,
                    IndicatorName = $"EMA_{period}"
                });

                previousEMA = currentEMA;
            }

            return results;
        }

        /// <summary>
        /// Calculates Relative Strength Index (RSI)
        /// Calcule l'indice de force relative (RSI)
        /// </summary>
        public List<TechnicalIndicatorValue> CalculateRSI(IEnumerable<MarketData> prices, int period = 14)
        {
            if (prices == null || period <= 0)
                return new List<TechnicalIndicatorValue>();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var results = new List<TechnicalIndicatorValue>();

            if (pricesList.Count < period + 1)
                return results;

            // Calculate price changes
            var priceChanges = new List<decimal>();
            for (int i = 1; i < pricesList.Count; i++)
            {
                priceChanges.Add(pricesList[i].ClosePrice - pricesList[i - 1].ClosePrice);
            }

            // Calculate initial averages
            var initialGains = priceChanges.Take(period).Where(c => c > 0).ToList();
            var initialLosses = priceChanges.Take(period).Where(c => c < 0).Select(Math.Abs).ToList();

            var avgGain = initialGains.Any() ? initialGains.Average() : 0m;
            var avgLoss = initialLosses.Any() ? initialLosses.Average() : 0m;

            // Calculate RSI for each subsequent period
            for (int i = period; i < priceChanges.Count; i++)
            {
                var change = priceChanges[i];
                
                // Update averages using Wilder's smoothing
                if (change > 0)
                {
                    avgGain = ((avgGain * (period - 1)) + change) / period;
                    avgLoss = (avgLoss * (period - 1)) / period;
                }
                else if (change < 0)
                {
                    avgGain = (avgGain * (period - 1)) / period;
                    avgLoss = ((avgLoss * (period - 1)) + Math.Abs(change)) / period;
                }
                else
                {
                    avgGain = (avgGain * (period - 1)) / period;
                    avgLoss = (avgLoss * (period - 1)) / period;
                }

                var rs = avgLoss == 0 ? 100m : avgGain / avgLoss;
                var rsi = 100m - (100m / (1m + rs));

                results.Add(new TechnicalIndicatorValue
                {
                    Date = pricesList[i + 1].Date,
                    Value = rsi,
                    IndicatorName = $"RSI_{period}"
                });
            }

            return results;
        }

        /// <summary>
        /// Calculates MACD (Moving Average Convergence Divergence)
        /// Calcule le MACD (convergence et divergence des moyennes mobiles)
        /// </summary>
        public MACDResult CalculateMACD(IEnumerable<MarketData> prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            if (prices == null)
                return new MACDResult();

            var fastEMA = CalculateExponentialMovingAverage(prices, fastPeriod);
            var slowEMA = CalculateExponentialMovingAverage(prices, slowPeriod);

            var macdLine = new List<TechnicalIndicatorValue>();
            
            // Calculate MACD line (Fast EMA - Slow EMA)
            foreach (var fastValue in fastEMA)
            {
                var slowValue = slowEMA.FirstOrDefault(s => s.Date == fastValue.Date);
                if (slowValue != null)
                {
                    macdLine.Add(new TechnicalIndicatorValue
                    {
                        Date = fastValue.Date,
                        Value = fastValue.Value - slowValue.Value,
                        IndicatorName = "MACD_Line"
                    });
                }
            }

            // Calculate signal line (EMA of MACD line)
            var macdPrices = macdLine.Select(m => new MarketData
            {
                Date = m.Date,
                ClosePrice = m.Value
            });

            var signalLine = CalculateExponentialMovingAverage(macdPrices, signalPeriod);
            signalLine.ForEach(s => s.IndicatorName = "MACD_Signal");

            // Calculate histogram (MACD - Signal)
            var histogram = new List<TechnicalIndicatorValue>();
            foreach (var macdValue in macdLine)
            {
                var signalValue = signalLine.FirstOrDefault(s => s.Date == macdValue.Date);
                if (signalValue != null)
                {
                    histogram.Add(new TechnicalIndicatorValue
                    {
                        Date = macdValue.Date,
                        Value = macdValue.Value - signalValue.Value,
                        IndicatorName = "MACD_Histogram"
                    });
                }
            }

            return new MACDResult
            {
                MACDLine = macdLine,
                SignalLine = signalLine,
                Histogram = histogram
            };
        }

        /// <summary>
        /// Calculates Bollinger Bands
        /// Calcule les bandes de Bollinger
        /// </summary>
        public BollingerBandsResult CalculateBollingerBands(IEnumerable<MarketData> prices, int period = 20, decimal standardDeviations = 2m)
        {
            if (prices == null || period <= 0)
                return new BollingerBandsResult();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var sma = CalculateSimpleMovingAverage(prices, period);
            
            var upperBand = new List<TechnicalIndicatorValue>();
            var lowerBand = new List<TechnicalIndicatorValue>();

            for (int i = period - 1; i < pricesList.Count; i++)
            {
                var periodPrices = pricesList.Skip(i - period + 1).Take(period).Select(p => p.ClosePrice).ToList();
                var mean = periodPrices.Average();
                var variance = periodPrices.Sum(p => (p - mean) * (p - mean)) / period;
                var stdDev = (decimal)Math.Sqrt((double)variance);

                var date = pricesList[i].Date;
                var smaValue = sma.FirstOrDefault(s => s.Date == date)?.Value ?? mean;

                upperBand.Add(new TechnicalIndicatorValue
                {
                    Date = date,
                    Value = smaValue + (standardDeviations * stdDev),
                    IndicatorName = "BB_Upper"
                });

                lowerBand.Add(new TechnicalIndicatorValue
                {
                    Date = date,
                    Value = smaValue - (standardDeviations * stdDev),
                    IndicatorName = "BB_Lower"
                });
            }

            return new BollingerBandsResult
            {
                MiddleBand = sma,
                UpperBand = upperBand,
                LowerBand = lowerBand
            };
        }

        /// <summary>
        /// Calculates support and resistance levels
        /// Calcule les niveaux de support et de résistance
        /// </summary>
        public SupportResistanceResult CalculateSupportResistance(IEnumerable<MarketData> prices, int lookbackPeriod = 20)
        {
            if (prices == null || lookbackPeriod <= 0)
                return new SupportResistanceResult();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var supportLevels = new List<decimal>();
            var resistanceLevels = new List<decimal>();

            for (int i = lookbackPeriod; i < pricesList.Count - lookbackPeriod; i++)
            {
                var currentPrice = pricesList[i];
                var surroundingPrices = pricesList.Skip(i - lookbackPeriod).Take(lookbackPeriod * 2 + 1).ToList();

                // Check for local minima (support)
                var isLocalMin = surroundingPrices.All(p => p.LowPrice >= currentPrice.LowPrice);
                if (isLocalMin)
                {
                    supportLevels.Add(currentPrice.LowPrice);
                }

                // Check for local maxima (resistance)
                var isLocalMax = surroundingPrices.All(p => p.HighPrice <= currentPrice.HighPrice);
                if (isLocalMax)
                {
                    resistanceLevels.Add(currentPrice.HighPrice);
                }
            }

            // Cluster similar levels
            var clusteredSupport = ClusterPriceLevels(supportLevels, 0.02m); // 2% clustering threshold
            var clusteredResistance = ClusterPriceLevels(resistanceLevels, 0.02m);

            return new SupportResistanceResult
            {
                SupportLevels = clusteredSupport.OrderByDescending(s => s).Take(5).ToList(),
                ResistanceLevels = clusteredResistance.OrderBy(r => r).Take(5).ToList()
            };
        }

        #endregion

        #region Volatility Calculations / Calculs de volatilité

        /// <summary>
        /// Calculates historical volatility
        /// Calcule la volatilité historique
        /// </summary>
        public decimal CalculateHistoricalVolatility(IEnumerable<MarketData> prices, int period = 30, bool annualized = true)
        {
            if (prices == null || period <= 0)
                return 0m;

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            if (pricesList.Count < 2)
                return 0m;

            // Calculate daily returns
            var returns = new List<decimal>();
            for (int i = 1; i < pricesList.Count; i++)
            {
                var dailyReturn = (decimal)Math.Log((double)(pricesList[i].ClosePrice / pricesList[i - 1].ClosePrice));
                returns.Add(dailyReturn);
            }

            // Take only the specified period
            var periodReturns = returns.TakeLast(period).ToList();
            if (periodReturns.Count < 2)
                return 0m;

            // Calculate standard deviation
            var mean = periodReturns.Average();
            var variance = periodReturns.Sum(r => (r - mean) * (r - mean)) / (periodReturns.Count - 1);
            var volatility = (decimal)Math.Sqrt((double)variance);

            // Annualize if requested
            return annualized ? volatility * (decimal)Math.Sqrt(252) : volatility;
        }

        /// <summary>
        /// Calculates Average True Range (ATR)
        /// Calcule la plage réelle moyenne (ATR)
        /// </summary>
        public List<TechnicalIndicatorValue> CalculateATR(IEnumerable<MarketData> prices, int period = 14)
        {
            if (prices == null || period <= 0)
                return new List<TechnicalIndicatorValue>();

            var pricesList = prices.OrderBy(p => p.Date).ToList();
            var results = new List<TechnicalIndicatorValue>();

            if (pricesList.Count < 2)
                return results;

            var trueRanges = new List<decimal>();

            // Calculate True Range for each period
            for (int i = 1; i < pricesList.Count; i++)
            {
                var current = pricesList[i];
                var previous = pricesList[i - 1];

                var tr1 = current.HighPrice - current.LowPrice;
                var tr2 = Math.Abs(current.HighPrice - previous.ClosePrice);
                var tr3 = Math.Abs(current.LowPrice - previous.ClosePrice);

                var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
                trueRanges.Add(trueRange);
            }

            // Calculate ATR using Wilder's smoothing
            if (trueRanges.Count >= period)
            {
                var initialATR = trueRanges.Take(period).Average();
                decimal previousATR = initialATR;

                results.Add(new TechnicalIndicatorValue
                {
                    Date = pricesList[period].Date,
                    Value = initialATR,
                    IndicatorName = $"ATR_{period}"
                });

                for (int i = period; i < trueRanges.Count; i++)
                {
                    var currentATR = ((previousATR * (period - 1)) + trueRanges[i]) / period;
                    
                    results.Add(new TechnicalIndicatorValue
                    {
                        Date = pricesList[i + 1].Date,
                        Value = currentATR,
                        IndicatorName = $"ATR_{period}"
                    });

                    previousATR = currentATR;
                }
            }

            return results;
        }

        #endregion

        #region Market Data Quality / Qualité des données de marché

        /// <summary>
        /// Validates market data quality and identifies issues
        /// Valide la qualité des données de marché et identifie les problèmes
        /// </summary>
        public MarketDataQualityReport ValidateMarketData(IEnumerable<MarketData> marketData)
        {
            if (marketData == null)
                return new MarketDataQualityReport { IsValid = false };

            var dataList = marketData.OrderBy(m => m.Date).ToList();
            var report = new MarketDataQualityReport
            {
                TotalRecords = dataList.Count,
                DateRange = dataList.Any() ? new DateRange 
                { 
                    StartDate = dataList.First().Date, 
                    EndDate = dataList.Last().Date 
                } : null,
                Issues = new List<DataQualityIssue>()
            };

            // Check for missing dates
            if (dataList.Count > 1)
            {
                var expectedDays = (dataList.Last().Date - dataList.First().Date).Days + 1;
                var actualDays = dataList.Count;
                var missingDays = expectedDays - actualDays;

                if (missingDays > 0)
                {
                    report.Issues.Add(new DataQualityIssue
                    {
                        IssueType = "Missing Data",
                        Description = $"{missingDays} missing trading days detected",
                        Severity = "Medium"
                    });
                }
            }

            // Check for price anomalies
            for (int i = 1; i < dataList.Count; i++)
            {
                var current = dataList[i];
                var previous = dataList[i - 1];

                // Check for extreme price movements (>50% in one day)
                var priceChange = Math.Abs((current.ClosePrice - previous.ClosePrice) / previous.ClosePrice);
                if (priceChange > 0.5m)
                {
                    report.Issues.Add(new DataQualityIssue
                    {
                        IssueType = "Price Anomaly",
                        Description = $"Extreme price movement on {current.Date:yyyy-MM-dd}: {priceChange:P2}",
                        Severity = "High"
                    });
                }

                // Check for invalid OHLC relationships
                if (current.HighPrice < current.LowPrice ||
                    current.ClosePrice > current.HighPrice ||
                    current.ClosePrice < current.LowPrice ||
                    current.OpenPrice > current.HighPrice ||
                    current.OpenPrice < current.LowPrice)
                {
                    report.Issues.Add(new DataQualityIssue
                    {
                        IssueType = "Invalid OHLC",
                        Description = $"Invalid OHLC relationship on {current.Date:yyyy-MM-dd}",
                        Severity = "High"
                    });
                }

                // Check for zero or negative values
                if (current.OpenPrice <= 0 || current.HighPrice <= 0 ||
                    current.LowPrice <= 0 || current.ClosePrice <= 0)
                {
                    report.Issues.Add(new DataQualityIssue
                    {
                        IssueType = "Invalid Price",
                        Description = $"Zero or negative price detected on {current.Date:yyyy-MM-dd}",
                        Severity = "High"
                    });
                }
            }

            report.IsValid = !report.Issues.Any(i => i.Severity == "High");
            report.QualityScore = CalculateQualityScore(report);

            return report;
        }

        #endregion

        #region Helper Methods / Méthodes d'aide

        private List<decimal> ClusterPriceLevels(List<decimal> levels, decimal threshold)
        {
            if (!levels.Any())
                return new List<decimal>();

            var sortedLevels = levels.OrderBy(l => l).ToList();
            var clustered = new List<decimal>();
            var currentCluster = new List<decimal> { sortedLevels[0] };

            for (int i = 1; i < sortedLevels.Count; i++)
            {
                var percentDiff = Math.Abs(sortedLevels[i] - currentCluster.Last()) / currentCluster.Last();
                
                if (percentDiff <= threshold)
                {
                    currentCluster.Add(sortedLevels[i]);
                }
                else
                {
                    // Close current cluster and start new one
                    clustered.Add(currentCluster.Average());
                    currentCluster = new List<decimal> { sortedLevels[i] };
                }
            }

            // Add the last cluster
            if (currentCluster.Any())
            {
                clustered.Add(currentCluster.Average());
            }

            return clustered;
        }

        private decimal CalculateQualityScore(MarketDataQualityReport report)
        {
            if (report.TotalRecords == 0)
                return 0m;

            var highIssues = report.Issues.Count(i => i.Severity == "High");
            var mediumIssues = report.Issues.Count(i => i.Severity == "Medium");
            var lowIssues = report.Issues.Count(i => i.Severity == "Low");

            // Score based on issue severity and frequency
            var scoreReduction = (highIssues * 0.3m) + (mediumIssues * 0.1m) + (lowIssues * 0.05m);
            var qualityScore = Math.Max(0m, 1m - scoreReduction);

            return Math.Min(1m, qualityScore);
        }

        #endregion
    }

    #region Technical Indicator Result Classes / Classes de résultats d'indicateurs techniques

    public class TechnicalIndicatorValue
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string IndicatorName { get; set; }
    }

    public class MACDResult
    {
        public List<TechnicalIndicatorValue> MACDLine { get; set; } = new();
        public List<TechnicalIndicatorValue> SignalLine { get; set; } = new();
        public List<TechnicalIndicatorValue> Histogram { get; set; } = new();
    }

    public class BollingerBandsResult
    {
        public List<TechnicalIndicatorValue> UpperBand { get; set; } = new();
        public List<TechnicalIndicatorValue> MiddleBand { get; set; } = new();
        public List<TechnicalIndicatorValue> LowerBand { get; set; } = new();
    }

    public class SupportResistanceResult
    {
        public List<decimal> SupportLevels { get; set; } = new();
        public List<decimal> ResistanceLevels { get; set; } = new();
    }

    public class MarketDataQualityReport
    {
        public bool IsValid { get; set; }
        public int TotalRecords { get; set; }
        public DateRange DateRange { get; set; }
        public decimal QualityScore { get; set; }
        public List<DataQualityIssue> Issues { get; set; } = new();
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DataQualityIssue
    {
        public string IssueType { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; } // High, Medium, Low
    }

    #endregion
}