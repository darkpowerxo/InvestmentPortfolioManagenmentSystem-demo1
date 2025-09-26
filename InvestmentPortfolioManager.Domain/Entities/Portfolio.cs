// Portfolio and holding entities for investment management
// Entités portefeuille et position pour la gestion d'investissements
// Comprehensive portfolio model with holdings, transactions, and performance tracking
// Modèle complet de portefeuille avec positions, transactions et suivi de performance

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Portfolio entity representing a collection of investments
    /// Entité portefeuille représentant une collection d'investissements
    /// </summary>
    public class Portfolio : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Name { get; private set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; private set; } = string.Empty;

        [Required]
        public Guid PortfolioManagerId { get; private set; }

        [Required]
        public Guid BaseCurrencyId { get; private set; }

        [Required]
        public DateTime InceptionDate { get; private set; }

        public Guid? BenchmarkSecurityId { get; private set; }

        [Required]
        public PortfolioStatus Status { get; private set; } = PortfolioStatus.Active;

        // Navigation properties / Propriétés de navigation
        public virtual User PortfolioManager { get; private set; } = null!;
        public virtual Currency BaseCurrency { get; private set; } = null!;
        public virtual Security? BenchmarkSecurity { get; private set; }
        public virtual ICollection<Holding> Holdings { get; private set; } = new List<Holding>();
        public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public virtual ICollection<RiskMetric> RiskMetrics { get; private set; } = new List<RiskMetric>();
        public virtual ICollection<PortfolioPerformance> PerformanceHistory { get; private set; } = new List<PortfolioPerformance>();
        public virtual ICollection<AssetAllocation> AssetAllocations { get; private set; } = new List<AssetAllocation>();

        protected Portfolio() { } // For EF Core

        public Portfolio(string name, Guid portfolioManagerId, Guid baseCurrencyId, 
                        DateTime inceptionDate, string description = "", Guid? benchmarkSecurityId = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PortfolioManagerId = portfolioManagerId;
            BaseCurrencyId = baseCurrencyId;
            InceptionDate = inceptionDate.Date;
            Description = description;
            BenchmarkSecurityId = benchmarkSecurityId;
            Status = PortfolioStatus.Active;
        }

        /// <summary>
        /// Updates portfolio basic information
        /// Met à jour les informations de base du portefeuille
        /// </summary>
        public void UpdateInfo(string name, string description, Guid? benchmarkSecurityId = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            BenchmarkSecurityId = benchmarkSecurityId;
            UpdateTimestamp();
        }

        /// <summary>
        /// Changes portfolio status
        /// Modifie le statut du portefeuille
        /// </summary>
        public void ChangeStatus(PortfolioStatus newStatus)
        {
            Status = newStatus;
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates total portfolio value in base currency
        /// Calcule la valeur totale du portefeuille en devise de base
        /// </summary>
        public decimal CalculateTotalValue()
        {
            return Holdings.Where(h => h.CurrentPrice.HasValue)
                          .Sum(h => h.MarketValue ?? 0);
        }

        /// <summary>
        /// Gets current asset allocation percentages
        /// Obtient les pourcentages d'allocation d'actifs actuels
        /// </summary>
        public Dictionary<string, decimal> GetCurrentAssetAllocation()
        {
            var totalValue = CalculateTotalValue();
            if (totalValue == 0) return new Dictionary<string, decimal>();

            return Holdings.Where(h => h.CurrentPrice.HasValue && h.Security?.AssetClass != null)
                          .GroupBy(h => h.Security!.AssetClass.Code)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Sum(h => h.MarketValue ?? 0) / totalValue * 100
                          );
        }

        /// <summary>
        /// Gets portfolio performance since inception
        /// Obtient la performance du portefeuille depuis sa création
        /// </summary>
        public decimal? GetInceptionToDateReturn()
        {
            var latestPerformance = PerformanceHistory.OrderByDescending(p => p.Date).FirstOrDefault();
            return latestPerformance?.TotalReturnInception;
        }

        /// <summary>
        /// Gets number of holdings in the portfolio
        /// Obtient le nombre de positions dans le portefeuille
        /// </summary>
        public int GetHoldingsCount()
        {
            return Holdings.Count(h => h.Quantity != 0);
        }

        /// <summary>
        /// Gets largest holding by market value
        /// Obtient la plus grande position par valeur marchande
        /// </summary>
        public Holding? GetLargestHolding()
        {
            return Holdings.Where(h => h.MarketValue.HasValue)
                          .OrderByDescending(h => h.MarketValue!.Value)
                          .FirstOrDefault();
        }

        /// <summary>
        /// Checks if portfolio is diversified (no single holding > threshold%)
        /// Vérifie si le portefeuille est diversifié (aucune position > seuil%)
        /// </summary>
        public bool IsDiversified(decimal maxConcentrationThreshold = 0.10m) // 10% default
        {
            var totalValue = CalculateTotalValue();
            if (totalValue == 0) return true;

            var maxHoldingPercentage = Holdings.Where(h => h.MarketValue.HasValue)
                                              .Max(h => (h.MarketValue!.Value / totalValue));

            return maxHoldingPercentage <= maxConcentrationThreshold;
        }

        /// <summary>
        /// Gets age of portfolio in years
        /// Obtient l'âge du portefeuille en années
        /// </summary>
        public double GetAgeInYears()
        {
            return (DateTime.Today - InceptionDate).TotalDays / 365.25;
        }
    }

    /// <summary>
    /// Portfolio status enumeration
    /// Énumération du statut du portefeuille
    /// </summary>
    public enum PortfolioStatus
    {
        Active,     // Actif
        Closed,     // Fermé
        Suspended   // Suspendu
    }

    /// <summary>
    /// Holding entity representing a position in a security
    /// Entité position représentant une position dans un titre
    /// </summary>
    public class Holding : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public Guid SecurityId { get; private set; }

        [Required]
        public decimal Quantity { get; private set; }

        [Required]
        public decimal AverageCost { get; private set; }

        public decimal? CurrentPrice { get; private set; }

        public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;

        // Calculated properties / Propriétés calculées
        public decimal? MarketValue => Quantity != 0 && CurrentPrice.HasValue ? 
                                      Math.Abs(Quantity) * CurrentPrice.Value : null;

        public decimal? UnrealizedGainLoss => Quantity != 0 && CurrentPrice.HasValue ? 
                                             Quantity * (CurrentPrice.Value - AverageCost) : null;

        public decimal? UnrealizedGainLossPercentage => AverageCost != 0 && CurrentPrice.HasValue ? 
                                                       (CurrentPrice.Value - AverageCost) / AverageCost : null;

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;
        public virtual Security Security { get; private set; } = null!;

        protected Holding() { } // For EF Core

        public Holding(Guid portfolioId, Guid securityId, decimal quantity, decimal averageCost, decimal? currentPrice = null)
        {
            PortfolioId = portfolioId;
            SecurityId = securityId;
            Quantity = quantity;
            AverageCost = averageCost;
            CurrentPrice = currentPrice;

            if (averageCost < 0)
                throw new ArgumentException("Average cost cannot be negative / Le coût moyen ne peut pas être négatif");
        }

        /// <summary>
        /// Updates the current market price
        /// Met à jour le prix de marché actuel
        /// </summary>
        public void UpdateCurrentPrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative / Le prix ne peut pas être négatif");

            CurrentPrice = newPrice;
            LastUpdated = DateTime.UtcNow;
            UpdateTimestamp();
        }

        /// <summary>
        /// Adds to the position (buy more shares)
        /// Ajoute à la position (acheter plus d'actions)
        /// </summary>
        public void AddToPosition(decimal additionalQuantity, decimal purchasePrice)
        {
            if (additionalQuantity <= 0)
                throw new ArgumentException("Additional quantity must be positive / La quantité supplémentaire doit être positive");

            if (purchasePrice < 0)
                throw new ArgumentException("Purchase price cannot be negative / Le prix d'achat ne peut pas être négatif");

            // Calculate new average cost / Calculer le nouveau coût moyen
            var totalValue = (Math.Abs(Quantity) * AverageCost) + (additionalQuantity * purchasePrice);
            var newQuantity = Quantity + additionalQuantity;
            
            AverageCost = Math.Abs(newQuantity) > 0 ? totalValue / Math.Abs(newQuantity) : 0;
            Quantity = newQuantity;
            UpdateTimestamp();
        }

        /// <summary>
        /// Reduces the position (sell shares)
        /// Réduit la position (vendre des actions)
        /// </summary>
        public void ReducePosition(decimal quantityToSell)
        {
            if (quantityToSell <= 0)
                throw new ArgumentException("Quantity to sell must be positive / La quantité à vendre doit être positive");

            if (quantityToSell > Math.Abs(Quantity))
                throw new ArgumentException("Cannot sell more than current position / Impossible de vendre plus que la position actuelle");

            var remainingQuantity = Quantity - quantityToSell;
            Quantity = remainingQuantity;
            
            // Average cost remains the same / Le coût moyen reste le même
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates weight in portfolio
        /// Calcule le poids dans le portefeuille
        /// </summary>
        public decimal? CalculateWeight(decimal totalPortfolioValue)
        {
            if (totalPortfolioValue <= 0 || !MarketValue.HasValue)
                return null;

            return MarketValue.Value / totalPortfolioValue;
        }

        /// <summary>
        /// Checks if position is profitable
        /// Vérifie si la position est rentable
        /// </summary>
        public bool? IsProfitable()
        {
            if (!CurrentPrice.HasValue)
                return null;

            return CurrentPrice.Value > AverageCost;
        }

        /// <summary>
        /// Gets days since last update
        /// Obtient les jours depuis la dernière mise à jour
        /// </summary>
        public int DaysSinceLastUpdate()
        {
            return (DateTime.UtcNow - LastUpdated).Days;
        }

        /// <summary>
        /// Checks if price data is stale
        /// Vérifie si les données de prix sont obsolètes
        /// </summary>
        public bool IsPriceStale(int staleDays = 1)
        {
            return DaysSinceLastUpdate() > staleDays;
        }
    }
}