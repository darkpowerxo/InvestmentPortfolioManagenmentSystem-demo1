// Risk alert and stress testing entities for comprehensive risk management
// Entités d'alerte de risque et de test de stress pour la gestion complète des risques
// Advanced risk monitoring and scenario analysis for institutional portfolios
// Surveillance avancée des risques et analyse de scénarios pour portefeuilles institutionnels

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Risk alert entity for monitoring portfolio risk thresholds
    /// Entité alerte de risque pour surveiller les seuils de risque du portefeuille
    /// </summary>
    public class RiskAlert : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; set; }

        [Required]
        public RiskAlertType AlertType { get; set; }

        [Required]
        public RiskSeverity Severity { get; set; }

        [Required]
        public RiskAlertPriority Priority { get; set; } = RiskAlertPriority.Medium;

        [Required, MaxLength(1000)]
        public string Message { get; private set; } = string.Empty;

        // Bilingual titles
        [MaxLength(500)]
        public string TitleEN { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string TitleFR { get; set; } = string.Empty;

        // Bilingual messages
        [MaxLength(2000)]
        public string MessageEN { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string MessageFR { get; set; } = string.Empty;

        public decimal? Threshold { get; private set; }
        public decimal? ActualValue { get; private set; }

        // Additional properties for risk service compatibility
        public decimal? CurrentValue { get; set; }
        public decimal? ThresholdValue { get; set; }
        
        [MaxLength(2000)]
        public string RecommendedActions { get; set; } = string.Empty;

        [Required]
        public RiskAlertStatus Status { get; private set; } = RiskAlertStatus.Active;

        public DateTime? AcknowledgedAt { get; private set; }
        public Guid? AcknowledgedBy { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public Guid? ResolvedBy { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;
        public virtual User? AcknowledgedByUser { get; private set; }
        public virtual User? ResolvedByUser { get; private set; }

        public RiskAlert() { } // For EF Core - made public

        public RiskAlert(Guid portfolioId, RiskAlertType alertType, RiskSeverity severity, 
                        string message, decimal? threshold = null, decimal? actualValue = null)
        {
            PortfolioId = portfolioId;
            AlertType = alertType;
            Severity = severity;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Threshold = threshold;
            ActualValue = actualValue;
            Status = RiskAlertStatus.Active;
        }

        /// <summary>
        /// Acknowledges the risk alert
        /// Accuse réception de l'alerte de risque
        /// </summary>
        public void Acknowledge(Guid userId)
        {
            if (Status != RiskAlertStatus.Active)
                throw new InvalidOperationException("Only active alerts can be acknowledged / Seules les alertes actives peuvent être accusées de réception");

            Status = RiskAlertStatus.Acknowledged;
            AcknowledgedAt = DateTime.UtcNow;
            AcknowledgedBy = userId;
            UpdateTimestamp();
        }

        /// <summary>
        /// Resolves the risk alert
        /// Résout l'alerte de risque
        /// </summary>
        public void Resolve(Guid userId)
        {
            Status = RiskAlertStatus.Resolved;
            ResolvedAt = DateTime.UtcNow;
            ResolvedBy = userId;
            UpdateTimestamp();
        }

        /// <summary>
        /// Reactivates a resolved alert (if conditions persist)
        /// Réactive une alerte résolue (si les conditions persistent)
        /// </summary>
        public void Reactivate()
        {
            Status = RiskAlertStatus.Active;
            AcknowledgedAt = null;
            AcknowledgedBy = null;
            ResolvedAt = null;
            ResolvedBy = null;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates alert values and severity
        /// Met à jour les valeurs et la gravité de l'alerte
        /// </summary>
        public void UpdateAlert(decimal? actualValue, RiskSeverity? newSeverity = null, string? newMessage = null)
        {
            ActualValue = actualValue;
            
            if (newSeverity.HasValue)
                Severity = newSeverity.Value;
            
            if (!string.IsNullOrWhiteSpace(newMessage))
                Message = newMessage;

            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates how long the alert has been active
        /// Calcule depuis combien de temps l'alerte est active
        /// </summary>
        public TimeSpan GetActiveTime()
        {
            var endTime = Status switch
            {
                RiskAlertStatus.Active => DateTime.UtcNow,
                RiskAlertStatus.Acknowledged => AcknowledgedAt ?? DateTime.UtcNow,
                RiskAlertStatus.Resolved => ResolvedAt ?? DateTime.UtcNow,
                _ => DateTime.UtcNow
            };

            return endTime - CreatedAt;
        }

        /// <summary>
        /// Checks if alert requires immediate attention
        /// Vérifie si l'alerte nécessite une attention immédiate
        /// </summary>
        public bool RequiresImmediateAttention()
        {
            return Status == RiskAlertStatus.Active && 
                   (Severity == RiskSeverity.High || Severity == RiskSeverity.Critical);
        }

        /// <summary>
        /// Gets localized alert type description
        /// Obtient la description localisée du type d'alerte
        /// </summary>
        public string GetLocalizedAlertType(string language = "EN")
        {
            return language.ToUpper() == "FR" ? GetFrenchAlertType() : GetEnglishAlertType();
        }

        private string GetEnglishAlertType()
        {
            return AlertType switch
            {
                RiskAlertType.VaR_Breach => "VaR Threshold Breach",
                RiskAlertType.Concentration => "Concentration Risk",
                RiskAlertType.Volatility => "High Volatility",
                RiskAlertType.Drawdown => "Maximum Drawdown",
                RiskAlertType.Compliance => "Compliance Violation",
                _ => AlertType.ToString()
            };
        }

        private string GetFrenchAlertType()
        {
            return AlertType switch
            {
                RiskAlertType.VaR_Breach => "Dépassement du seuil VaR",
                RiskAlertType.Concentration => "Risque de concentration",
                RiskAlertType.Volatility => "Volatilité élevée",
                RiskAlertType.Drawdown => "Drawdown maximum",
                RiskAlertType.Compliance => "Violation de conformité",
                _ => AlertType.ToString()
            };
        }
    }

    /// <summary>
    /// Risk alert types
    /// Types d'alertes de risque
    /// </summary>
    public enum RiskAlertType
    {
        VaR_Breach,     // Dépassement VaR
        Concentration,  // Concentration
        Volatility,     // Volatilité
        Drawdown,       // Drawdown
        Compliance      // Conformité
    }

    /// <summary>
    /// Risk severity levels
    /// Niveaux de gravité des risques
    /// </summary>
    public enum RiskSeverity
    {
        Low,        // Faible
        Medium,     // Moyenne
        High,       // Élevée
        Critical    // Critique
    }

    /// <summary>
    /// Risk alert status
    /// Statut d'alerte de risque
    /// </summary>
    public enum RiskAlertStatus
    {
        Active,         // Actif
        Acknowledged,   // Accusé réception
        Resolved        // Résolu
    }

    /// <summary>
    /// Risk alert priority levels
    /// Niveaux de priorité des alertes de risque
    /// </summary>
    public enum RiskAlertPriority
    {
        Low,        // Faible
        Medium,     // Moyenne
        High,       // Élevée
        Critical    // Critique
    }

    /// <summary>
    /// Stress test scenario entity for defining risk scenarios
    /// Entité scénario de test de stress pour définir les scénarios de risque
    /// </summary>
    public class StressTestScenario : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Name { get; private set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; private set; } = string.Empty;

        [Required]
        public StressTestType ScenarioType { get; private set; }

        [MaxLength(4000)] // JSON parameters
        public string Parameters { get; private set; } = string.Empty;

        [Required]
        public Guid CreatedBy { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual User Creator { get; private set; } = null!;
        public virtual ICollection<StressTestResult> Results { get; private set; } = new List<StressTestResult>();

        protected StressTestScenario() { } // For EF Core

        public StressTestScenario(string name, StressTestType scenarioType, string parameters, 
                                 Guid createdBy, string description = "")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ScenarioType = scenarioType;
            Parameters = parameters ?? string.Empty;
            CreatedBy = createdBy;
            Description = description;
        }

        /// <summary>
        /// Updates scenario information
        /// Met à jour les informations du scénario
        /// </summary>
        public void UpdateScenario(string name, string description, string parameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Parameters = parameters ?? string.Empty;
            UpdateTimestamp();
        }

        /// <summary>
        /// Gets localized scenario type description
        /// Obtient la description localisée du type de scénario
        /// </summary>
        public string GetLocalizedScenarioType(string language = "EN")
        {
            return language.ToUpper() == "FR" ? GetFrenchScenarioType() : GetEnglishScenarioType();
        }

        private string GetEnglishScenarioType()
        {
            return ScenarioType switch
            {
                StressTestType.Market_Crash => "Market Crash",
                StressTestType.Interest_Rate => "Interest Rate Shock",
                StressTestType.Currency => "Currency Crisis",
                StressTestType.Credit => "Credit Event",
                StressTestType.Liquidity => "Liquidity Crisis",
                StressTestType.Custom => "Custom Scenario",
                _ => ScenarioType.ToString()
            };
        }

        private string GetFrenchScenarioType()
        {
            return ScenarioType switch
            {
                StressTestType.Market_Crash => "Krach boursier",
                StressTestType.Interest_Rate => "Choc des taux d'intérêt",
                StressTestType.Currency => "Crise monétaire",
                StressTestType.Credit => "Événement de crédit",
                StressTestType.Liquidity => "Crise de liquidité",
                StressTestType.Custom => "Scénario personnalisé",
                _ => ScenarioType.ToString()
            };
        }

        /// <summary>
        /// Checks if scenario has been used recently
        /// Vérifie si le scénario a été utilisé récemment
        /// </summary>
        public bool HasRecentResults(int daysBack = 30)
        {
            var cutoffDate = DateTime.Today.AddDays(-daysBack);
            return Results.Any(r => r.Date >= cutoffDate);
        }

        /// <summary>
        /// Gets the most recent result for a portfolio
        /// Obtient le résultat le plus récent pour un portefeuille
        /// </summary>
        public StressTestResult? GetLatestResult(Guid portfolioId)
        {
            return Results.Where(r => r.PortfolioId == portfolioId)
                         .OrderByDescending(r => r.Date)
                         .FirstOrDefault();
        }
    }

    /// <summary>
    /// Stress test types
    /// Types de tests de stress
    /// </summary>
    public enum StressTestType
    {
        Market_Crash,   // Krach boursier
        Interest_Rate,  // Taux d'intérêt
        Currency,       // Change
        Credit,         // Crédit
        Liquidity,      // Liquidité
        Custom          // Personnalisé
    }

    /// <summary>
    /// Stress test result entity for storing scenario outcomes
    /// Entité résultat de test de stress pour stocker les résultats de scénarios
    /// </summary>
    public class StressTestResult : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public Guid ScenarioId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        [Required]
        public decimal CurrentValue { get; private set; }

        [Required]
        public decimal StressedValue { get; private set; }

        // Calculated properties / Propriétés calculées
        public decimal PnL => StressedValue - CurrentValue;
        public decimal PnLPercentage => CurrentValue != 0 ? (PnL / CurrentValue) * 100 : 0;

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;
        public virtual StressTestScenario Scenario { get; private set; } = null!;

        protected StressTestResult() { } // For EF Core

        public StressTestResult(Guid portfolioId, Guid scenarioId, DateTime date, 
                               decimal currentValue, decimal stressedValue)
        {
            PortfolioId = portfolioId;
            ScenarioId = scenarioId;
            Date = date.Date;
            CurrentValue = currentValue;
            StressedValue = stressedValue;

            if (currentValue < 0)
                throw new ArgumentException("Current value cannot be negative / La valeur actuelle ne peut pas être négative");
        }

        /// <summary>
        /// Updates stress test results
        /// Met à jour les résultats du test de stress
        /// </summary>
        public void UpdateResults(decimal stressedValue)
        {
            StressedValue = stressedValue;
            UpdateTimestamp();
        }

        /// <summary>
        /// Gets the impact severity based on loss percentage
        /// Obtient la gravité de l'impact basée sur le pourcentage de perte
        /// </summary>
        public StressTestImpact GetImpactSeverity()
        {
            var lossPercentage = Math.Abs(PnLPercentage);

            return lossPercentage switch
            {
                < 5m => StressTestImpact.Low,       // < 5% loss
                < 15m => StressTestImpact.Moderate, // 5-15% loss
                < 30m => StressTestImpact.High,     // 15-30% loss
                _ => StressTestImpact.Severe        // > 30% loss
            };
        }

        /// <summary>
        /// Checks if result indicates acceptable stress tolerance
        /// Vérifie si le résultat indique une tolérance au stress acceptable
        /// </summary>
        public bool IsWithinStressTolerance(decimal maxLossPercentage = 20m)
        {
            return Math.Abs(PnLPercentage) <= maxLossPercentage;
        }

        /// <summary>
        /// Gets formatted result summary
        /// Obtient un résumé formaté du résultat
        /// </summary>
        public StressTestSummary GetSummary(string language = "EN")
        {
            return new StressTestSummary
            {
                Date = Date,
                ScenarioName = Scenario?.Name ?? "Unknown",
                CurrentValue = CurrentValue,
                StressedValue = StressedValue,
                PnL = PnL,
                PnLPercentage = PnLPercentage,
                Impact = GetImpactSeverity(),
                IsAcceptable = IsWithinStressTolerance(),
                Language = language
            };
        }
    }

    /// <summary>
    /// Stress test impact levels
    /// Niveaux d'impact des tests de stress
    /// </summary>
    public enum StressTestImpact
    {
        Low,        // Faible
        Moderate,   // Modéré
        High,       // Élevé
        Severe      // Sévère
    }

    /// <summary>
    /// Stress test summary data transfer object
    /// Objet de transfert de données résumé de test de stress
    /// </summary>
    public class StressTestSummary
    {
        public DateTime Date { get; set; }
        public string ScenarioName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal StressedValue { get; set; }
        public decimal PnL { get; set; }
        public decimal PnLPercentage { get; set; }
        public StressTestImpact Impact { get; set; }
        public bool IsAcceptable { get; set; }
        public string Language { get; set; } = "EN";

        /// <summary>
        /// Gets localized impact description
        /// Obtient la description localisée de l'impact
        /// </summary>
        public string GetLocalizedImpact()
        {
            if (Language.ToUpper() == "FR")
            {
                return Impact switch
                {
                    StressTestImpact.Low => "Faible",
                    StressTestImpact.Moderate => "Modéré",
                    StressTestImpact.High => "Élevé",
                    StressTestImpact.Severe => "Sévère",
                    _ => Impact.ToString()
                };
            }

            return Impact.ToString();
        }
    }
}