// Risk Management Value Objects and Result Classes
// Objets de valeur et classes de résultats pour la gestion des risques
// Comprehensive risk analysis data structures for institutional portfolio management
// Structures de données d'analyse des risques complètes pour la gestion de portefeuille institutionnel

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.ValueObjects
{
    /// <summary>
    /// Parameters for risk analysis configuration
    /// Paramètres pour la configuration de l'analyse des risques
    /// </summary>
    public class RiskAnalysisParameters
    {
        [Range(0.01, 0.999)]
        public decimal ConfidenceLevel { get; set; } = 0.95m;

        [Range(1, 365)]
        public int TimeHorizonDays { get; set; } = 10;

        public List<StressScenario> StressScenarios { get; set; } = new();
    }

    /// <summary>
    /// Comprehensive portfolio risk analysis results
    /// Résultats d'analyse complète des risques de portefeuille
    /// </summary>
    public class PortfolioRiskAnalysis
    {
        public Guid PortfolioId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public int TimeHorizon { get; set; }

        public VaRResult ValueAtRisk { get; set; } = new();
        public decimal ExpectedShortfall { get; set; }
        public ConcentrationRiskAnalysis ConcentrationRisk { get; set; } = new();
        public CorrelationMatrix CorrelationMatrix { get; set; } = new();
        public LiquidityRiskAnalysis LiquidityRisk { get; set; } = new();
        public decimal MaxDrawdown { get; set; }
        public List<StressTestAnalysisResult> StressTestResults { get; set; } = new();
    }

    /// <summary>
    /// Value at Risk calculation results using multiple methodologies
    /// Résultats de calcul de la valeur à risque utilisant plusieurs méthodologies
    /// </summary>
    public class VaRResult
    {
        public decimal PortfolioValue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public int TimeHorizon { get; set; }
        
        public decimal HistoricalVaR { get; set; }
        public decimal ParametricVaR { get; set; }
        public decimal MonteCarloVaR { get; set; }
        public decimal RecommendedVaR { get; set; }

        public string GetLocalizedSummary(string languageCode = "EN")
        {
            var template = languageCode.ToUpper() == "FR" 
                ? "VaR à {0}% de confiance sur {1} jours: {2:C} (Recommandé)"
                : "VaR at {0}% confidence over {1} days: {2:C} (Recommended)";
            
            return string.Format(template, ConfidenceLevel * 100, TimeHorizon, RecommendedVaR);
        }
    }

    /// <summary>
    /// Stress testing scenario definition
    /// Définition de scénario de test de résistance
    /// </summary>
    public class StressScenario
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, decimal> StressFactors { get; set; } = new();

        public StressScenario() { }

        public StressScenario(string name, string description, Dictionary<string, decimal> stressFactors)
        {
            Name = name;
            Description = description;
            StressFactors = stressFactors;
        }
    }

    /// <summary>
    /// Results of stress test execution
    /// Résultats d'exécution de test de résistance
    /// </summary>
    public class StressTestAnalysisResult
    {
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioDescription { get; set; } = string.Empty;
        public decimal InitialPortfolioValue { get; set; }
        public decimal StressedPortfolioValue { get; set; }
        public decimal AbsoluteImpact { get; set; }
        public decimal PercentageImpact { get; set; }
        public StressSeverity Severity { get; set; }
        public DateTime ExecutionDate { get; set; }

        public string GetLocalizedSummary(string languageCode = "EN")
        {
            var template = languageCode.ToUpper() == "FR"
                ? "{0}: Impact de {1:P2} ({2:C})"
                : "{0}: Impact of {1:P2} ({2:C})";
            
            return string.Format(template, ScenarioName, PercentageImpact / 100, AbsoluteImpact);
        }
    }

    /// <summary>
    /// Portfolio concentration risk analysis
    /// Analyse du risque de concentration du portefeuille
    /// </summary>
    public class ConcentrationRiskAnalysis
    {
        public decimal HerfindahlIndex { get; set; }
        public int NumberOfHoldings { get; set; }
        public decimal Top10HoldingsWeight { get; set; }
        
        public List<ConcentrationMetric> AssetClassConcentration { get; set; } = new();
        public List<ConcentrationMetric> SectorConcentration { get; set; } = new();
        public List<ConcentrationMetric> CountryConcentration { get; set; } = new();
        
        public ConcentrationRiskLevel ConcentrationRiskLevel { get; set; }

        public string GetLocalizedAssessment(string languageCode = "EN")
        {
            var level = ConcentrationRiskLevel.ToString();
            var template = languageCode.ToUpper() == "FR"
                ? "Niveau de concentration: {0}, Top 10: {1:P1}, HHI: {2:F3}"
                : "Concentration Level: {0}, Top 10: {1:P1}, HHI: {2:F3}";
            
            return string.Format(template, level, Top10HoldingsWeight, HerfindahlIndex);
        }
    }

    /// <summary>
    /// Concentration metric for specific category
    /// Métrique de concentration pour une catégorie spécifique
    /// </summary>
    public class ConcentrationMetric
    {
        public string Category { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Correlation matrix for portfolio securities
    /// Matrice de corrélation pour les titres du portefeuille
    /// </summary>
    public class CorrelationMatrix
    {
        public List<InvestmentPortfolioManager.Domain.Entities.Security> Securities { get; set; } = new();
        public Dictionary<(Guid, Guid), decimal> Correlations { get; set; } = new();
        public decimal AverageCorrelation { get; set; }
        public decimal DiversificationRatio { get; set; }

        public decimal GetCorrelation(Guid security1Id, Guid security2Id)
        {
            return Correlations.GetValueOrDefault((security1Id, security2Id), 0);
        }

        public string GetDiversificationAssessment(string languageCode = "EN")
        {
            var assessment = DiversificationRatio switch
            {
                > 2.0m => languageCode.ToUpper() == "FR" ? "Excellente" : "Excellent",
                > 1.5m => languageCode.ToUpper() == "FR" ? "Bonne" : "Good", 
                > 1.2m => languageCode.ToUpper() == "FR" ? "Correcte" : "Fair",
                _ => languageCode.ToUpper() == "FR" ? "Faible" : "Poor"
            };

            var template = languageCode.ToUpper() == "FR"
                ? "Diversification: {0} (Ratio: {1:F2})"
                : "Diversification: {0} (Ratio: {1:F2})";
            
            return string.Format(template, assessment, DiversificationRatio);
        }
    }

    /// <summary>
    /// Liquidity risk analysis for portfolio
    /// Analyse du risque de liquidité pour le portefeuille
    /// </summary>
    public class LiquidityRiskAnalysis
    {
        public decimal TotalPortfolioValue { get; set; }
        public decimal WeightedAverageLiquidationTime { get; set; }
        public decimal IlliquidHoldingsPercentage { get; set; }
        public List<HoldingLiquidityMetric> HoldingLiquidityMetrics { get; set; } = new();
        public LiquidityRiskLevel OverallLiquidityRating { get; set; }

        public string GetLocalizedSummary(string languageCode = "EN")
        {
            var template = languageCode.ToUpper() == "FR"
                ? "Liquidité: {0}, Positions illiquides: {1:P1}, Temps moyen: {2:F1} jours"
                : "Liquidity: {0}, Illiquid positions: {1:P1}, Average time: {2:F1} days";
            
            return string.Format(template, OverallLiquidityRating.ToString(), 
                               IlliquidHoldingsPercentage / 100, WeightedAverageLiquidationTime);
        }
    }

    /// <summary>
    /// Liquidity metrics for individual holding
    /// Métriques de liquidité pour une position individuelle
    /// </summary>
    public class HoldingLiquidityMetric
    {
        public Guid SecurityId { get; set; }
        public string SecurityName { get; set; } = string.Empty;
        public decimal PositionValue { get; set; }
        public decimal AverageVolume { get; set; }
        public decimal EstimatedLiquidationDays { get; set; }
        public LiquidityRating LiquidityRating { get; set; }

        public string GetLocalizedRating(string languageCode = "EN")
        {
            var rating = LiquidityRating switch
            {
                LiquidityRating.Excellent => languageCode.ToUpper() == "FR" ? "Excellente" : "Excellent",
                LiquidityRating.Good => languageCode.ToUpper() == "FR" ? "Bonne" : "Good",
                LiquidityRating.Fair => languageCode.ToUpper() == "FR" ? "Correcte" : "Fair",
                LiquidityRating.Poor => languageCode.ToUpper() == "FR" ? "Faible" : "Poor",
                LiquidityRating.Illiquid => languageCode.ToUpper() == "FR" ? "Illiquide" : "Illiquid",
                _ => "Unknown"
            };

            return $"{SecurityName}: {rating} ({EstimatedLiquidationDays:F1} {(languageCode.ToUpper() == "FR" ? "jours" : "days")})";
        }
    }

    #region Enumerations / Énumérations

    /// <summary>
    /// Stress test severity levels
    /// Niveaux de gravité des tests de résistance
    /// </summary>
    public enum StressSeverity
    {
        Low,        // Faible
        Medium,     // Moyen
        High,       // Élevé
        Extreme     // Extrême
    }

    /// <summary>
    /// Concentration risk levels
    /// Niveaux de risque de concentration
    /// </summary>
    public enum ConcentrationRiskLevel
    {
        Low,        // Faible
        Medium,     // Moyen
        High        // Élevé
    }

    /// <summary>
    /// Individual security liquidity ratings
    /// Évaluations de liquidité des titres individuels
    /// </summary>
    public enum LiquidityRating
    {
        Excellent,  // Excellente
        Good,       // Bonne
        Fair,       // Correcte
        Poor,       // Faible
        Illiquid    // Illiquide
    }

    /// <summary>
    /// Overall portfolio liquidity risk levels
    /// Niveaux de risque de liquidité globaux du portefeuille
    /// </summary>
    public enum LiquidityRiskLevel
    {
        Low,        // Faible
        Medium,     // Moyen
        High        // Élevé
    }

    #endregion
}