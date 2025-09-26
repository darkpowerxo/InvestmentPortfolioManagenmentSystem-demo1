// Risk Alert Service - Simplified Working Version
// Service d'alerte de risque - Version de travail simplifiée
// Real-time portfolio risk monitoring and alerting for institutional portfolio management
// Surveillance en temps réel des risques de portefeuille et alertes pour la gestion de portefeuille institutionnel

using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.ValueObjects;
using EntityRiskAlertType = InvestmentPortfolioManager.Domain.Entities.RiskAlertType;
using EntityRiskSeverity = InvestmentPortfolioManager.Domain.Entities.RiskSeverity;
using EntityRiskAlertPriority = InvestmentPortfolioManager.Domain.Entities.RiskAlertPriority;
using ValueObjectRiskAlertType = InvestmentPortfolioManager.Domain.ValueObjects.RiskAlertType;
using ValueObjectRiskAlertSeverity = InvestmentPortfolioManager.Domain.ValueObjects.RiskAlertSeverity;

namespace InvestmentPortfolioManager.Domain.Services
{
    /// <summary>
    /// Service for generating and managing risk alerts based on portfolio analysis
    /// Service pour générer et gérer les alertes de risque basées sur l'analyse de portefeuille
    /// </summary>
    public class RiskAlertService
    {
        private readonly RiskManagementService _riskManagementService;
        private readonly PortfolioAnalyticsService _portfolioAnalyticsService;

        public RiskAlertService(RiskManagementService riskManagementService, 
                               PortfolioAnalyticsService portfolioAnalyticsService)
        {
            _riskManagementService = riskManagementService ?? throw new ArgumentNullException(nameof(riskManagementService));
            _portfolioAnalyticsService = portfolioAnalyticsService ?? throw new ArgumentNullException(nameof(portfolioAnalyticsService));
        }

        /// <summary>
        /// Monitors portfolio for risk threshold breaches and generates alerts
        /// Surveille le portefeuille pour les dépassements de seuils de risque et génère des alertes
        /// </summary>
        public async Task<List<RiskAlert>> MonitorPortfolioRiskAsync(Portfolio portfolio, IEnumerable<MarketData> marketData, RiskThresholds thresholds)
        {
            var alerts = new List<RiskAlert>();

            try
            {
                // Perform risk analysis
                var riskAnalysis = await _riskManagementService.AnalyzePortfolioRiskAsync(portfolio, marketData, new RiskAnalysisParameters());

                // Check VaR thresholds
                alerts.AddRange(CheckVaRThresholds(portfolio, riskAnalysis.ValueAtRisk, thresholds));

                // Check concentration risk
                alerts.AddRange(CheckConcentrationRisk(portfolio, riskAnalysis.ConcentrationRisk, thresholds));

                // Check liquidity risk  
                alerts.AddRange(CheckLiquidityRisk(portfolio, riskAnalysis.LiquidityRisk, thresholds));

                return alerts;
            }
            catch (Exception ex)
            {
                // Log error and return empty list rather than throwing
                Console.WriteLine($"Error monitoring portfolio risk: {ex.Message}");
                return alerts;
            }
        }

        /// <summary>
        /// Generates alerts for individual security risks
        /// Génère des alertes pour les risques de titres individuels
        /// </summary>
        public async Task<List<RiskAlert>> MonitorIndividualSecurityRiskAsync(Portfolio portfolio, 
                                                                              Security security, 
                                                                              RiskThresholds thresholds)
        {
            var alerts = new List<RiskAlert>();

            try
            {
                var holding = portfolio.Holdings.FirstOrDefault(h => h.SecurityId == security.Id);
                if (holding == null) return alerts;

                var portfolioValue = portfolio.Holdings.Sum(h => h.MarketValue ?? 0);
                if (portfolioValue == 0) return alerts;

                var positionWeight = (holding.MarketValue ?? 0) / portfolioValue * 100;

                // Check position size
                if (positionWeight > thresholds.MaxSinglePositionPercentage)
                {
                    alerts.Add(CreatePositionSizeAlert(portfolio.Id, security, positionWeight, thresholds.MaxSinglePositionPercentage));
                }

                return alerts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring individual security risk: {ex.Message}");
                return alerts;
            }
        }

        /// <summary>
        /// Checks VaR threshold breaches
        /// Vérifie les dépassements de seuils VaR
        /// </summary>
        private List<RiskAlert> CheckVaRThresholds(Portfolio portfolio, VaRResult? varResult, RiskThresholds thresholds)
        {
            var alerts = new List<RiskAlert>();

            if (varResult == null) return alerts;

            var portfolioValue = portfolio.Holdings.Sum(h => h.MarketValue ?? 0);
            if (portfolioValue == 0) return alerts;

            var varPercentage = (varResult.RecommendedVaR / portfolioValue) * 100;

            if (varPercentage > thresholds.MaxVaRPercentage)
            {
                alerts.Add(CreateVaRAlert(portfolio.Id, varPercentage, thresholds.MaxVaRPercentage, varResult.RecommendedVaR));
            }

            return alerts;
        }

        /// <summary>
        /// Checks concentration risk thresholds
        /// Vérifie les seuils de risque de concentration
        /// </summary>
        private List<RiskAlert> CheckConcentrationRisk(Portfolio portfolio, ConcentrationRiskAnalysis? concentration, RiskThresholds thresholds)
        {
            var alerts = new List<RiskAlert>();

            if (concentration == null) return alerts;

            // Check Herfindahl Index
            if ((decimal)concentration.HerfindahlIndex > thresholds.MaxHerfindahlIndex)
            {
                alerts.Add(CreateConcentrationAlert(portfolio.Id, (decimal)concentration.HerfindahlIndex, thresholds.MaxHerfindahlIndex));
            }

            return alerts;
        }

        /// <summary>
        /// Checks liquidity risk thresholds
        /// Vérifie les seuils de risque de liquidité
        /// </summary>
        private List<RiskAlert> CheckLiquidityRisk(Portfolio portfolio, LiquidityRiskAnalysis? liquidity, RiskThresholds thresholds)
        {
            var alerts = new List<RiskAlert>();

            if (liquidity == null) return alerts;

            var illiquidPercentage = liquidity.IlliquidHoldingsPercentage;

            if (illiquidPercentage > thresholds.MaxIlliquidPercentage)
            {
                alerts.Add(CreateLiquidityAlert(portfolio.Id, illiquidPercentage, thresholds.MaxIlliquidPercentage));
            }

            return alerts;
        }

        #region Alert Creation Methods

        private RiskAlert CreateVaRAlert(Guid portfolioId, decimal varPercentage, decimal threshold, decimal recommendedVaR)
        {
            return new RiskAlert
            {
                PortfolioId = portfolioId,
                AlertType = EntityRiskAlertType.VaR_Breach,
                Severity = DetermineSeverity(varPercentage),
                Priority = DeterminePriority(varPercentage),
                TitleEN = "Value at Risk Threshold Exceeded",
                TitleFR = "Seuil de valeur à risque dépassé",
                MessageEN = $"Portfolio VaR of {varPercentage:F2}% exceeds threshold of {threshold:F2}%. Recommended VaR: {recommendedVaR:C}",
                MessageFR = $"La VaR du portefeuille de {varPercentage:F2}% dépasse le seuil de {threshold:F2}%. VaR recommandée: {recommendedVaR:C}",
                CurrentValue = varPercentage,
                ThresholdValue = threshold,
                RecommendedActions = "Consider reducing portfolio risk through diversification or position sizing adjustments."
            };
        }

        private RiskAlert CreateConcentrationAlert(Guid portfolioId, decimal herfindahlIndex, decimal threshold)
        {
            return new RiskAlert
            {
                PortfolioId = portfolioId,
                AlertType = EntityRiskAlertType.Concentration,
                Severity = EntityRiskSeverity.Medium,
                Priority = EntityRiskAlertPriority.Medium,
                TitleEN = "Portfolio Concentration Risk",
                TitleFR = "Risque de concentration du portefeuille",
                MessageEN = $"Herfindahl-Hirschman Index of {herfindahlIndex:F3} exceeds threshold of {threshold:F3}",
                MessageFR = $"L'indice Herfindahl-Hirschman de {herfindahlIndex:F3} dépasse le seuil de {threshold:F3}",
                CurrentValue = herfindahlIndex,
                ThresholdValue = threshold,
                RecommendedActions = "Improve portfolio diversification by reducing large position concentrations."
            };
        }

        private RiskAlert CreateLiquidityAlert(Guid portfolioId, decimal illiquidPercentage, decimal threshold)
        {
            return new RiskAlert
            {
                PortfolioId = portfolioId,
                AlertType = EntityRiskAlertType.VaR_Breach, // Using existing enum value
                Severity = EntityRiskSeverity.Medium,
                Priority = EntityRiskAlertPriority.Medium,
                TitleEN = "Liquidity Risk Warning",
                TitleFR = "Avertissement de risque de liquidité",
                MessageEN = $"Illiquid holdings represent {illiquidPercentage:F1}% of portfolio, exceeding {threshold:F1}% threshold",
                MessageFR = $"Les positions illiquides représentent {illiquidPercentage:F1}% du portefeuille, dépassant le seuil de {threshold:F1}%",
                CurrentValue = illiquidPercentage,
                ThresholdValue = threshold,
                RecommendedActions = "Reduce exposure to illiquid securities or maintain adequate cash reserves."
            };
        }

        private RiskAlert CreatePositionSizeAlert(Guid portfolioId, Security security, decimal positionWeight, decimal threshold)
        {
            return new RiskAlert
            {
                PortfolioId = portfolioId,
                AlertType = EntityRiskAlertType.Concentration,
                Severity = DetermineSeverity(positionWeight),
                Priority = DeterminePriority(positionWeight),
                TitleEN = $"Position Size Alert - {security.Symbol}",
                TitleFR = $"Alerte de taille de position - {security.Symbol}",
                MessageEN = $"Position in {security.NameEN} represents {positionWeight:F1}% of portfolio, exceeding {threshold:F1}% threshold",
                MessageFR = $"La position dans {security.NameFR} représente {positionWeight:F1}% du portefeuille, dépassant le seuil de {threshold:F1}%",
                CurrentValue = positionWeight,
                ThresholdValue = threshold,
                RecommendedActions = $"Consider reducing position size in {security.Symbol} to manage concentration risk."
            };
        }

        #endregion

        #region Helper Methods

        private EntityRiskSeverity DetermineSeverity(decimal riskValue)
        {
            return riskValue switch
            {
                >= 50m => EntityRiskSeverity.Critical,
                >= 25m => EntityRiskSeverity.High,
                >= 10m => EntityRiskSeverity.Medium,
                _ => EntityRiskSeverity.Low
            };
        }

        private EntityRiskAlertPriority DeterminePriority(decimal riskValue)
        {
            return riskValue switch
            {
                >= 50m => EntityRiskAlertPriority.Critical,
                >= 25m => EntityRiskAlertPriority.High,
                >= 10m => EntityRiskAlertPriority.Medium,
                _ => EntityRiskAlertPriority.Low
            };
        }

        #endregion
    }
}