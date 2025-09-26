// Risk Alert Value Objects and Configuration Classes
// Objets de valeur et classes de configuration pour les alertes de risque
// Risk monitoring and alerting configuration for institutional portfolio management
// Configuration de surveillance et d'alerte des risques pour la gestion de portefeuille institutionnel

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.ValueObjects
{
    /// <summary>
    /// Configuration class for risk monitoring thresholds
    /// Classe de configuration pour les seuils de surveillance des risques
    /// </summary>
    public class RiskThresholds
    {
        // VaR Thresholds / Seuils VaR
        [Range(0.1, 50.0)]
        public decimal MaxVaRPercentage { get; set; } = 5.0m; // 5% of portfolio value

        // Concentration Risk Thresholds / Seuils de risque de concentration
        [Range(0.01, 1.0)]
        public decimal MaxHerfindahlIndex { get; set; } = 0.25m;

        [Range(1.0, 100.0)]
        public decimal MaxTop10Percentage { get; set; } = 70.0m; // 70% max for top 10 holdings

        [Range(1.0, 50.0)]
        public decimal MaxSectorPercentage { get; set; } = 25.0m; // 25% max per sector

        [Range(1.0, 50.0)]
        public decimal MaxSinglePositionPercentage { get; set; } = 10.0m; // 10% max per position

        // Liquidity Risk Thresholds / Seuils de risque de liquidité
        [Range(0.1, 50.0)]
        public decimal MaxIlliquidPercentage { get; set; } = 15.0m; // 15% max illiquid holdings

        [Range(1, 365)]
        public decimal MaxLiquidationDays { get; set; } = 30m; // 30 days max average liquidation time

        // Drawdown Thresholds / Seuils de perte maximale
        [Range(1.0, 50.0)]
        public decimal MaxDrawdownPercentage { get; set; } = 20.0m; // 20% max drawdown

        // Correlation Thresholds / Seuils de corrélation
        [Range(0.1, 1.0)]
        public decimal MaxAverageCorrelation { get; set; } = 0.7m; // 70% max average correlation

        [Range(1.0, 10.0)]
        public decimal MinDiversificationRatio { get; set; } = 1.5m; // Minimum diversification ratio

        // Stress Test Thresholds / Seuils de test de résistance
        [Range(1.0, 100.0)]
        public decimal MaxStressTestImpact { get; set; } = 25.0m; // 25% max impact in stress scenarios

        // Volatility Thresholds / Seuils de volatilité
        [Range(0.1, 20.0)]
        public decimal MaxSecurityVolatility { get; set; } = 5.0m; // 5% max daily volatility per security

        // Allocation Drift Thresholds / Seuils de dérive d'allocation
        [Range(0.1, 20.0)]
        public decimal MaxAllocationDrift { get; set; } = 5.0m; // 5% max drift from target allocation

        /// <summary>
        /// Creates conservative risk thresholds suitable for institutional investors
        /// Crée des seuils de risque conservateurs adaptés aux investisseurs institutionnels
        /// </summary>
        public static RiskThresholds CreateConservativeThresholds()
        {
            return new RiskThresholds
            {
                MaxVaRPercentage = 3.0m,
                MaxHerfindahlIndex = 0.15m,
                MaxTop10Percentage = 50.0m,
                MaxSectorPercentage = 15.0m,
                MaxSinglePositionPercentage = 5.0m,
                MaxIlliquidPercentage = 10.0m,
                MaxLiquidationDays = 20m,
                MaxDrawdownPercentage = 15.0m,
                MaxAverageCorrelation = 0.6m,
                MinDiversificationRatio = 2.0m,
                MaxStressTestImpact = 20.0m,
                MaxSecurityVolatility = 3.0m,
                MaxAllocationDrift = 3.0m
            };
        }

        /// <summary>
        /// Creates moderate risk thresholds for balanced portfolios
        /// Crée des seuils de risque modérés pour les portefeuilles équilibrés
        /// </summary>
        public static RiskThresholds CreateModerateThresholds()
        {
            return new RiskThresholds(); // Uses default values
        }

        /// <summary>
        /// Creates aggressive risk thresholds for growth-oriented portfolios
        /// Crée des seuils de risque agressifs pour les portefeuilles orientés croissance
        /// </summary>
        public static RiskThresholds CreateAggressiveThresholds()
        {
            return new RiskThresholds
            {
                MaxVaRPercentage = 8.0m,
                MaxHerfindahlIndex = 0.35m,
                MaxTop10Percentage = 85.0m,
                MaxSectorPercentage = 35.0m,
                MaxSinglePositionPercentage = 15.0m,
                MaxIlliquidPercentage = 25.0m,
                MaxLiquidationDays = 45m,
                MaxDrawdownPercentage = 30.0m,
                MaxAverageCorrelation = 0.8m,
                MinDiversificationRatio = 1.2m,
                MaxStressTestImpact = 35.0m,
                MaxSecurityVolatility = 8.0m,
                MaxAllocationDrift = 8.0m
            };
        }

        /// <summary>
        /// Validates that all thresholds are within acceptable ranges
        /// Valide que tous les seuils sont dans des plages acceptables
        /// </summary>
        public bool ValidateThresholds(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (MaxVaRPercentage <= 0 || MaxVaRPercentage > 50)
                validationErrors.Add("VaR percentage must be between 0.1% and 50%");

            if (MaxHerfindahlIndex <= 0 || MaxHerfindahlIndex > 1)
                validationErrors.Add("Herfindahl Index must be between 0.01 and 1.0");

            if (MaxTop10Percentage <= 0 || MaxTop10Percentage > 100)
                validationErrors.Add("Top 10 percentage must be between 1% and 100%");

            if (MaxSectorPercentage <= 0 || MaxSectorPercentage > 50)
                validationErrors.Add("Sector percentage must be between 1% and 50%");

            if (MaxSinglePositionPercentage <= 0 || MaxSinglePositionPercentage > 50)
                validationErrors.Add("Single position percentage must be between 1% and 50%");

            if (MaxIlliquidPercentage <= 0 || MaxIlliquidPercentage > 50)
                validationErrors.Add("Illiquid percentage must be between 0.1% and 50%");

            if (MaxLiquidationDays <= 0 || MaxLiquidationDays > 365)
                validationErrors.Add("Liquidation days must be between 1 and 365");

            if (MaxDrawdownPercentage <= 0 || MaxDrawdownPercentage > 50)
                validationErrors.Add("Drawdown percentage must be between 1% and 50%");

            if (MaxAverageCorrelation <= 0 || MaxAverageCorrelation > 1)
                validationErrors.Add("Average correlation must be between 0.1 and 1.0");

            if (MinDiversificationRatio <= 0 || MinDiversificationRatio > 10)
                validationErrors.Add("Diversification ratio must be between 1.0 and 10.0");

            if (MaxStressTestImpact <= 0 || MaxStressTestImpact > 100)
                validationErrors.Add("Stress test impact must be between 1% and 100%");

            if (MaxSecurityVolatility <= 0 || MaxSecurityVolatility > 20)
                validationErrors.Add("Security volatility must be between 0.1% and 20%");

            if (MaxAllocationDrift <= 0 || MaxAllocationDrift > 20)
                validationErrors.Add("Allocation drift must be between 0.1% and 20%");

            return !validationErrors.Any();
        }

        /// <summary>
        /// Gets localized threshold descriptions
        /// Obtient les descriptions localisées des seuils
        /// </summary>
        public Dictionary<string, string> GetLocalizedDescriptions(string languageCode = "EN")
        {
            var descriptions = new Dictionary<string, string>();

            if (languageCode.ToUpper() == "FR")
            {
                descriptions.Add("MaxVaRPercentage", $"VaR maximale: {MaxVaRPercentage:F1}% de la valeur du portefeuille");
                descriptions.Add("MaxHerfindahlIndex", $"Indice Herfindahl maximum: {MaxHerfindahlIndex:F3}");
                descriptions.Add("MaxTop10Percentage", $"Concentration Top 10 maximale: {MaxTop10Percentage:F1}%");
                descriptions.Add("MaxSectorPercentage", $"Concentration sectorielle maximale: {MaxSectorPercentage:F1}%");
                descriptions.Add("MaxSinglePositionPercentage", $"Position individuelle maximale: {MaxSinglePositionPercentage:F1}%");
                descriptions.Add("MaxIlliquidPercentage", $"Positions illiquides maximales: {MaxIlliquidPercentage:F1}%");
                descriptions.Add("MaxLiquidationDays", $"Temps de liquidation maximum: {MaxLiquidationDays:F0} jours");
                descriptions.Add("MaxDrawdownPercentage", $"Perte maximale: {MaxDrawdownPercentage:F1}%");
                descriptions.Add("MaxAverageCorrelation", $"Corrélation moyenne maximale: {MaxAverageCorrelation:F2}");
                descriptions.Add("MinDiversificationRatio", $"Ratio de diversification minimum: {MinDiversificationRatio:F1}");
                descriptions.Add("MaxStressTestImpact", $"Impact de test de résistance maximum: {MaxStressTestImpact:F1}%");
                descriptions.Add("MaxSecurityVolatility", $"Volatilité de titre maximale: {MaxSecurityVolatility:F1}%");
                descriptions.Add("MaxAllocationDrift", $"Dérive d'allocation maximale: {MaxAllocationDrift:F1}%");
            }
            else
            {
                descriptions.Add("MaxVaRPercentage", $"Maximum VaR: {MaxVaRPercentage:F1}% of portfolio value");
                descriptions.Add("MaxHerfindahlIndex", $"Maximum Herfindahl Index: {MaxHerfindahlIndex:F3}");
                descriptions.Add("MaxTop10Percentage", $"Maximum Top 10 concentration: {MaxTop10Percentage:F1}%");
                descriptions.Add("MaxSectorPercentage", $"Maximum sector concentration: {MaxSectorPercentage:F1}%");
                descriptions.Add("MaxSinglePositionPercentage", $"Maximum single position: {MaxSinglePositionPercentage:F1}%");
                descriptions.Add("MaxIlliquidPercentage", $"Maximum illiquid holdings: {MaxIlliquidPercentage:F1}%");
                descriptions.Add("MaxLiquidationDays", $"Maximum liquidation time: {MaxLiquidationDays:F0} days");
                descriptions.Add("MaxDrawdownPercentage", $"Maximum drawdown: {MaxDrawdownPercentage:F1}%");
                descriptions.Add("MaxAverageCorrelation", $"Maximum average correlation: {MaxAverageCorrelation:F2}");
                descriptions.Add("MinDiversificationRatio", $"Minimum diversification ratio: {MinDiversificationRatio:F1}");
                descriptions.Add("MaxStressTestImpact", $"Maximum stress test impact: {MaxStressTestImpact:F1}%");
                descriptions.Add("MaxSecurityVolatility", $"Maximum security volatility: {MaxSecurityVolatility:F1}%");
                descriptions.Add("MaxAllocationDrift", $"Maximum allocation drift: {MaxAllocationDrift:F1}%");
            }

            return descriptions;
        }
    }

    #region Risk Alert Enumerations / Énumérations d'alerte de risque

    /// <summary>
    /// Types of risk alerts that can be generated
    /// Types d'alertes de risque qui peuvent être générées
    /// </summary>
    public enum RiskAlertType
    {
        VaRBreach,              // Dépassement VaR
        ConcentrationRisk,      // Risque de concentration
        SectorConcentration,    // Concentration sectorielle
        LiquidityRisk,          // Risque de liquidité
        DrawdownRisk,           // Risque de perte maximale
        CorrelationRisk,        // Risque de corrélation
        DiversificationRisk,    // Risque de diversification
        StressTestFailure,      // Échec du test de résistance
        PositionSizeRisk,       // Risque de taille de position
        VolatilityRisk,         // Risque de volatilité
        AllocationDrift,        // Dérive d'allocation
        CurrencyRisk,           // Risque de change
        CreditRisk,             // Risque de crédit
        MarketRisk,             // Risque de marché
        OperationalRisk         // Risque opérationnel
    }

    /// <summary>
    /// Severity levels for risk alerts
    /// Niveaux de gravité pour les alertes de risque
    /// </summary>
    public enum RiskAlertSeverity
    {
        Low,        // Faible - monitoring required
        Medium,     // Moyen - attention needed
        High,       // Élevé - action required
        Critical    // Critique - immediate action required
    }

    /// <summary>
    /// Priority levels for risk alert processing
    /// Niveaux de priorité pour le traitement des alertes de risque
    /// </summary>
    public enum RiskAlertPriority
    {
        Low,        // Faible - review during next business day
        Medium,     // Moyen - review within business hours
        High,       // Élevé - review within 2 hours
        Urgent      // Urgent - immediate review required
    }

    /// <summary>
    /// Status of risk alert processing
    /// État du traitement des alertes de risque
    /// </summary>
    public enum RiskAlertStatus
    {
        Open,           // Ouverte - newly generated
        Acknowledged,   // Reconnue - reviewed by risk manager
        InProgress,     // En cours - action being taken
        Resolved,       // Résolue - issue addressed
        Dismissed,      // Rejetée - determined not actionable
        Escalated       // Escaladée - elevated to senior management
    }

    #endregion

    /// <summary>
    /// Extended RiskAlert entity with additional properties for comprehensive risk monitoring
    /// Entité RiskAlert étendue avec des propriétés supplémentaires pour une surveillance complète des risques
    /// </summary>
    public class RiskAlertExtended
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PortfolioId { get; set; }
        public RiskAlertType AlertType { get; set; }
        public RiskAlertSeverity Severity { get; set; }
        public RiskAlertPriority Priority { get; set; }
        public RiskAlertStatus Status { get; set; } = RiskAlertStatus.Open;

        // Localized titles and messages
        public string TitleEN { get; set; } = string.Empty;
        public string TitleFR { get; set; } = string.Empty;
        public string MessageEN { get; set; } = string.Empty;
        public string MessageFR { get; set; } = string.Empty;

        // Risk metrics
        public decimal CurrentValue { get; set; }
        public decimal ThresholdValue { get; set; }
        public string? Unit { get; set; }

        // Recommendations and actions
        public List<string> RecommendedActions { get; set; } = new();
        public string? ActionsTaken { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AssignedTo { get; set; }
        public string? Comments { get; set; }

        // Risk context
        public Dictionary<string, object> RiskContext { get; set; } = new();

        /// <summary>
        /// Gets localized title based on language preference
        /// Obtient le titre localisé selon la préférence linguistique
        /// </summary>
        public string GetLocalizedTitle(string languageCode = "EN")
        {
            return languageCode.ToUpper() switch
            {
                "FR" => !string.IsNullOrEmpty(TitleFR) ? TitleFR : TitleEN,
                _ => !string.IsNullOrEmpty(TitleEN) ? TitleEN : TitleFR
            };
        }

        /// <summary>
        /// Gets localized message based on language preference
        /// Obtient le message localisé selon la préférence linguistique
        /// </summary>
        public string GetLocalizedMessage(string languageCode = "EN")
        {
            return languageCode.ToUpper() switch
            {
                "FR" => !string.IsNullOrEmpty(MessageFR) ? MessageFR : MessageEN,
                _ => !string.IsNullOrEmpty(MessageEN) ? MessageEN : MessageFR
            };
        }

        /// <summary>
        /// Gets formatted risk metric display
        /// Obtient l'affichage formaté de la métrique de risque
        /// </summary>
        public string GetFormattedRiskMetric(string languageCode = "EN")
        {
            var unit = Unit ?? "";
            var template = languageCode.ToUpper() == "FR"
                ? "Actuel: {0}{2}, Seuil: {1}{2}"
                : "Current: {0}{2}, Threshold: {1}{2}";

            return string.Format(template, CurrentValue.ToString("F2"), ThresholdValue.ToString("F2"), unit);
        }

        /// <summary>
        /// Calculates age of the alert in hours
        /// Calcule l'âge de l'alerte en heures
        /// </summary>
        public double GetAgeInHours()
        {
            return (DateTime.UtcNow - CreatedAt).TotalHours;
        }

        /// <summary>
        /// Determines if alert requires immediate attention based on age and priority
        /// Détermine si l'alerte nécessite une attention immédiate selon l'âge et la priorité
        /// </summary>
        public bool RequiresImmediateAttention()
        {
            var ageInHours = GetAgeInHours();
            
            return Priority switch
            {
                RiskAlertPriority.Urgent => Status == RiskAlertStatus.Open,
                RiskAlertPriority.High => Status == RiskAlertStatus.Open && ageInHours > 2,
                RiskAlertPriority.Medium => Status == RiskAlertStatus.Open && ageInHours > 8,
                RiskAlertPriority.Low => Status == RiskAlertStatus.Open && ageInHours > 24,
                _ => false
            };
        }

        /// <summary>
        /// Updates alert status and tracks timestamps
        /// Met à jour le statut de l'alerte et suit les horodatages
        /// </summary>
        public void UpdateStatus(RiskAlertStatus newStatus, string? assignedTo = null, string? comments = null)
        {
            var previousStatus = Status;
            Status = newStatus;

            if (!string.IsNullOrEmpty(assignedTo))
                AssignedTo = assignedTo;

            if (!string.IsNullOrEmpty(comments))
                Comments = string.IsNullOrEmpty(Comments) ? comments : $"{Comments}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {comments}";

            // Update timestamps based on status change
            switch (newStatus)
            {
                case RiskAlertStatus.Acknowledged when previousStatus == RiskAlertStatus.Open:
                    AcknowledgedAt = DateTime.UtcNow;
                    break;
                case RiskAlertStatus.Resolved:
                case RiskAlertStatus.Dismissed:
                    ResolvedAt = DateTime.UtcNow;
                    break;
            }
        }

        /// <summary>
        /// Adds context information to the risk alert
        /// Ajoute des informations de contexte à l'alerte de risque
        /// </summary>
        public void AddRiskContext(string key, object value)
        {
            RiskContext[key] = value;
        }

        /// <summary>
        /// Gets summary for dashboard display
        /// Obtient un résumé pour l'affichage du tableau de bord
        /// </summary>
        public string GetDashboardSummary(string languageCode = "EN")
        {
            var title = GetLocalizedTitle(languageCode);
            var age = GetAgeInHours();
            var ageText = languageCode.ToUpper() == "FR" 
                ? age < 1 ? "< 1h" : $"{age:F0}h"
                : age < 1 ? "< 1h" : $"{age:F0}h";

            return $"{title} ({Severity}) - {ageText}";
        }
    }
}