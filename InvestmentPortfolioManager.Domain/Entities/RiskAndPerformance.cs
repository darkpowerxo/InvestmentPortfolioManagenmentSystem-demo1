// Risk management and performance tracking entities
// Entités de gestion des risques et de suivi de performance
// Comprehensive risk metrics and performance analytics for institutional portfolios
// Métriques de risque complètes et analyses de performance pour portefeuilles institutionnels

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Risk metrics entity for portfolio risk measurement
    /// Entité métriques de risque pour la mesure du risque de portefeuille
    /// </summary>
    public class RiskMetric : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        [Required]
        public decimal TotalValue { get; private set; }

        // Value at Risk metrics / Métriques de valeur à risque
        public decimal? ValueAtRisk95 { get; private set; }    // VaR à 95%
        public decimal? ValueAtRisk99 { get; private set; }    // VaR à 99%
        public decimal? ConditionalVaR95 { get; private set; } // CVaR à 95%

        // Volatility and risk-adjusted returns / Volatilité et rendements ajustés au risque
        public decimal? VolatilityAnnualized { get; private set; }
        public decimal? SharpeRatio { get; private set; }
        public decimal? SortinoRatio { get; private set; }
        public decimal? MaxDrawdown { get; private set; }

        // Market risk metrics / Métriques de risque de marché
        public decimal? Beta { get; private set; }
        public decimal? Alpha { get; private set; }
        public decimal? TrackingError { get; private set; }
        public decimal? InformationRatio { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;

        protected RiskMetric() { } // For EF Core

        public RiskMetric(Guid portfolioId, DateTime date, decimal totalValue)
        {
            PortfolioId = portfolioId;
            Date = date.Date;
            TotalValue = totalValue;

            if (totalValue < 0)
                throw new ArgumentException("Total value cannot be negative / La valeur totale ne peut pas être négative");
        }

        /// <summary>
        /// Updates VaR metrics
        /// Met à jour les métriques VaR
        /// </summary>
        public void UpdateVaRMetrics(decimal? var95, decimal? var99, decimal? cvar95)
        {
            ValueAtRisk95 = var95;
            ValueAtRisk99 = var99;
            ConditionalVaR95 = cvar95;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates volatility and risk-adjusted return metrics
        /// Met à jour les métriques de volatilité et rendements ajustés au risque
        /// </summary>
        public void UpdateVolatilityMetrics(decimal? volatility, decimal? sharpe, decimal? sortino, decimal? maxDrawdown)
        {
            VolatilityAnnualized = volatility;
            SharpeRatio = sharpe;
            SortinoRatio = sortino;
            MaxDrawdown = maxDrawdown;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates market risk metrics relative to benchmark
        /// Met à jour les métriques de risque de marché par rapport à l'indice de référence
        /// </summary>
        public void UpdateMarketRiskMetrics(decimal? beta, decimal? alpha, decimal? trackingError, decimal? informationRatio)
        {
            Beta = beta;
            Alpha = alpha;
            TrackingError = trackingError;
            InformationRatio = informationRatio;
            UpdateTimestamp();
        }

        /// <summary>
        /// Gets risk level based on VaR
        /// Obtient le niveau de risque basé sur la VaR
        /// </summary>
        public RiskLevel GetRiskLevel()
        {
            if (!ValueAtRisk95.HasValue || TotalValue == 0)
                return RiskLevel.Unknown;

            var varPercentage = Math.Abs(ValueAtRisk95.Value) / TotalValue;

            return varPercentage switch
            {
                < 0.02m => RiskLevel.Low,        // < 2%
                < 0.05m => RiskLevel.Moderate,   // 2-5%
                < 0.10m => RiskLevel.High,       // 5-10%
                _ => RiskLevel.VeryHigh          // > 10%
            };
        }

        /// <summary>
        /// Checks if metrics indicate acceptable risk levels
        /// Vérifie si les métriques indiquent des niveaux de risque acceptables
        /// </summary>
        public bool IsWithinRiskTolerance(decimal maxVarPercentage = 0.05m, decimal minSharpeRatio = 0.5m)
        {
            var varCheck = !ValueAtRisk95.HasValue || TotalValue == 0 || 
                          (Math.Abs(ValueAtRisk95.Value) / TotalValue) <= maxVarPercentage;

            var sharpeCheck = !SharpeRatio.HasValue || SharpeRatio.Value >= minSharpeRatio;

            return varCheck && sharpeCheck;
        }
    }

    /// <summary>
    /// Risk level enumeration
    /// Énumération du niveau de risque
    /// </summary>
    public enum RiskLevel
    {
        Unknown,    // Inconnu
        Low,        // Faible
        Moderate,   // Modéré
        High,       // Élevé
        VeryHigh    // Très élevé
    }

    /// <summary>
    /// Portfolio performance entity for tracking returns
    /// Entité performance de portefeuille pour le suivi des rendements
    /// </summary>
    public class PortfolioPerformance : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        // Portfolio returns / Rendements du portefeuille
        public decimal? TotalReturn1D { get; private set; }
        public decimal? TotalReturn1W { get; private set; }
        public decimal? TotalReturn1M { get; private set; }
        public decimal? TotalReturn3M { get; private set; }
        public decimal? TotalReturn6M { get; private set; }
        public decimal? TotalReturn1Y { get; private set; }
        public decimal? TotalReturnYTD { get; private set; }
        public decimal? TotalReturnInception { get; private set; }

        // Benchmark returns / Rendements de l'indice de référence
        public decimal? BenchmarkReturn1D { get; private set; }
        public decimal? BenchmarkReturn1W { get; private set; }
        public decimal? BenchmarkReturn1M { get; private set; }
        public decimal? BenchmarkReturn3M { get; private set; }
        public decimal? BenchmarkReturn6M { get; private set; }
        public decimal? BenchmarkReturn1Y { get; private set; }
        public decimal? BenchmarkReturnYTD { get; private set; }
        public decimal? BenchmarkReturnInception { get; private set; }

        // Active returns (Portfolio - Benchmark) / Rendements actifs (Portefeuille - Référence)
        public decimal? ActiveReturn1D { get; private set; }
        public decimal? ActiveReturn1W { get; private set; }
        public decimal? ActiveReturn1M { get; private set; }
        public decimal? ActiveReturn3M { get; private set; }
        public decimal? ActiveReturn6M { get; private set; }
        public decimal? ActiveReturn1Y { get; private set; }
        public decimal? ActiveReturnYTD { get; private set; }
        public decimal? ActiveReturnInception { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;

        protected PortfolioPerformance() { } // For EF Core

        public PortfolioPerformance(Guid portfolioId, DateTime date)
        {
            PortfolioId = portfolioId;
            Date = date.Date;
        }

        /// <summary>
        /// Updates portfolio returns for all periods
        /// Met à jour les rendements du portefeuille pour toutes les périodes
        /// </summary>
        public void UpdatePortfolioReturns(decimal? return1D, decimal? return1W, decimal? return1M,
                                          decimal? return3M, decimal? return6M, decimal? return1Y,
                                          decimal? returnYTD, decimal? returnInception)
        {
            TotalReturn1D = return1D;
            TotalReturn1W = return1W;
            TotalReturn1M = return1M;
            TotalReturn3M = return3M;
            TotalReturn6M = return6M;
            TotalReturn1Y = return1Y;
            TotalReturnYTD = returnYTD;
            TotalReturnInception = returnInception;
            
            CalculateActiveReturns();
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates benchmark returns for all periods
        /// Met à jour les rendements de l'indice de référence pour toutes les périodes
        /// </summary>
        public void UpdateBenchmarkReturns(decimal? return1D, decimal? return1W, decimal? return1M,
                                          decimal? return3M, decimal? return6M, decimal? return1Y,
                                          decimal? returnYTD, decimal? returnInception)
        {
            BenchmarkReturn1D = return1D;
            BenchmarkReturn1W = return1W;
            BenchmarkReturn1M = return1M;
            BenchmarkReturn3M = return3M;
            BenchmarkReturn6M = return6M;
            BenchmarkReturn1Y = return1Y;
            BenchmarkReturnYTD = returnYTD;
            BenchmarkReturnInception = returnInception;
            
            CalculateActiveReturns();
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates active returns (portfolio minus benchmark)
        /// Calcule les rendements actifs (portefeuille moins référence)
        /// </summary>
        private void CalculateActiveReturns()
        {
            ActiveReturn1D = CalculateActiveDifference(TotalReturn1D, BenchmarkReturn1D);
            ActiveReturn1W = CalculateActiveDifference(TotalReturn1W, BenchmarkReturn1W);
            ActiveReturn1M = CalculateActiveDifference(TotalReturn1M, BenchmarkReturn1M);
            ActiveReturn3M = CalculateActiveDifference(TotalReturn3M, BenchmarkReturn3M);
            ActiveReturn6M = CalculateActiveDifference(TotalReturn6M, BenchmarkReturn6M);
            ActiveReturn1Y = CalculateActiveDifference(TotalReturn1Y, BenchmarkReturn1Y);
            ActiveReturnYTD = CalculateActiveDifference(TotalReturnYTD, BenchmarkReturnYTD);
            ActiveReturnInception = CalculateActiveDifference(TotalReturnInception, BenchmarkReturnInception);
        }

        /// <summary>
        /// Helper method to calculate active return difference
        /// Méthode d'aide pour calculer la différence de rendement actif
        /// </summary>
        private decimal? CalculateActiveDifference(decimal? portfolioReturn, decimal? benchmarkReturn)
        {
            if (!portfolioReturn.HasValue || !benchmarkReturn.HasValue)
                return null;

            return portfolioReturn.Value - benchmarkReturn.Value;
        }

        /// <summary>
        /// Gets performance summary for display
        /// Obtient un résumé de performance pour l'affichage
        /// </summary>
        public PerformanceSummary GetPerformanceSummary()
        {
            return new PerformanceSummary
            {
                Date = Date,
                Return1Y = TotalReturn1Y,
                Return3Y = null, // Would need historical data / Nécessiterait des données historiques
                ReturnInception = TotalReturnInception,
                ActiveReturn1Y = ActiveReturn1Y,
                ActiveReturnInception = ActiveReturnInception,
                OutperformingBenchmark1Y = ActiveReturn1Y > 0,
                OutperformingBenchmarkInception = ActiveReturnInception > 0
            };
        }

        /// <summary>
        /// Checks if portfolio is outperforming benchmark
        /// Vérifie si le portefeuille surperforme l'indice de référence
        /// </summary>
        public bool IsOutperformingBenchmark(PerformancePeriod period = PerformancePeriod.OneYear)
        {
            var activeReturn = period switch
            {
                PerformancePeriod.OneDay => ActiveReturn1D,
                PerformancePeriod.OneWeek => ActiveReturn1W,
                PerformancePeriod.OneMonth => ActiveReturn1M,
                PerformancePeriod.ThreeMonths => ActiveReturn3M,
                PerformancePeriod.SixMonths => ActiveReturn6M,
                PerformancePeriod.OneYear => ActiveReturn1Y,
                PerformancePeriod.YearToDate => ActiveReturnYTD,
                PerformancePeriod.Inception => ActiveReturnInception,
                _ => null
            };

            return activeReturn.HasValue && activeReturn.Value > 0;
        }
    }

    /// <summary>
    /// Performance period enumeration
    /// Énumération des périodes de performance
    /// </summary>
    public enum PerformancePeriod
    {
        OneDay,         // 1 jour
        OneWeek,        // 1 semaine
        OneMonth,       // 1 mois
        ThreeMonths,    // 3 mois
        SixMonths,      // 6 mois
        OneYear,        // 1 an
        YearToDate,     // Depuis le début de l'année
        Inception       // Depuis la création
    }

    /// <summary>
    /// Performance summary data transfer object
    /// Objet de transfert de données résumé de performance
    /// </summary>
    public class PerformanceSummary
    {
        public DateTime Date { get; set; }
        public decimal? Return1Y { get; set; }
        public decimal? Return3Y { get; set; }
        public decimal? ReturnInception { get; set; }
        public decimal? ActiveReturn1Y { get; set; }
        public decimal? ActiveReturnInception { get; set; }
        public bool? OutperformingBenchmark1Y { get; set; }
        public bool? OutperformingBenchmarkInception { get; set; }
    }

    /// <summary>
    /// Asset allocation entity for tracking portfolio composition
    /// Entité allocation d'actifs pour le suivi de la composition du portefeuille
    /// </summary>
    public class AssetAllocation : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        [Required]
        public Guid AssetClassId { get; private set; }

        [Required]
        public decimal MarketValue { get; private set; }

        [Required]
        public decimal Percentage { get; private set; }

        public decimal? TargetPercentage { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;
        public virtual AssetClass AssetClass { get; private set; } = null!;

        protected AssetAllocation() { } // For EF Core

        public AssetAllocation(Guid portfolioId, DateTime date, Guid assetClassId, 
                              decimal marketValue, decimal percentage, decimal? targetPercentage = null)
        {
            PortfolioId = portfolioId;
            Date = date.Date;
            AssetClassId = assetClassId;
            MarketValue = marketValue;
            Percentage = percentage;
            TargetPercentage = targetPercentage;

            ValidateAllocation();
        }

        /// <summary>
        /// Updates allocation values
        /// Met à jour les valeurs d'allocation
        /// </summary>
        public void UpdateAllocation(decimal marketValue, decimal percentage, decimal? targetPercentage = null)
        {
            MarketValue = marketValue;
            Percentage = percentage;
            TargetPercentage = targetPercentage;
            
            ValidateAllocation();
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates deviation from target allocation
        /// Calcule l'écart par rapport à l'allocation cible
        /// </summary>
        public decimal? GetAllocationDeviation()
        {
            if (!TargetPercentage.HasValue)
                return null;

            return Percentage - TargetPercentage.Value;
        }

        /// <summary>
        /// Checks if allocation is within tolerance of target
        /// Vérifie si l'allocation est dans la tolérance de la cible
        /// </summary>
        public bool IsWithinTolerance(decimal tolerancePercentage = 2.0m)
        {
            var deviation = GetAllocationDeviation();
            if (!deviation.HasValue)
                return true; // No target set, assume OK

            return Math.Abs(deviation.Value) <= tolerancePercentage;
        }

        /// <summary>
        /// Gets rebalancing recommendation
        /// Obtient une recommandation de rééquilibrage
        /// </summary>
        public RebalancingAction GetRebalancingRecommendation(decimal tolerancePercentage = 2.0m)
        {
            var deviation = GetAllocationDeviation();
            if (!deviation.HasValue)
                return RebalancingAction.NoAction;

            if (Math.Abs(deviation.Value) <= tolerancePercentage)
                return RebalancingAction.NoAction;

            return deviation.Value > 0 ? RebalancingAction.Reduce : RebalancingAction.Increase;
        }

        /// <summary>
        /// Validates allocation data
        /// Valide les données d'allocation
        /// </summary>
        private void ValidateAllocation()
        {
            if (MarketValue < 0)
                throw new ArgumentException("Market value cannot be negative / La valeur de marché ne peut pas être négative");

            if (Percentage < 0 || Percentage > 100)
                throw new ArgumentException("Percentage must be between 0 and 100 / Le pourcentage doit être entre 0 et 100");

            if (TargetPercentage.HasValue && (TargetPercentage.Value < 0 || TargetPercentage.Value > 100))
                throw new ArgumentException("Target percentage must be between 0 and 100 / Le pourcentage cible doit être entre 0 et 100");
        }
    }

    /// <summary>
    /// Rebalancing action enumeration
    /// Énumération des actions de rééquilibrage
    /// </summary>
    public enum RebalancingAction
    {
        NoAction,   // Aucune action
        Increase,   // Augmenter
        Reduce      // Réduire
    }
}