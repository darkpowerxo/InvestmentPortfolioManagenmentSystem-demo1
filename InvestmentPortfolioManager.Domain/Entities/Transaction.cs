// Transaction entity for trade execution and portfolio activity tracking
// Entité transaction pour l'exécution des transactions et le suivi d'activité du portefeuille
// Comprehensive transaction model supporting various transaction types
// Modèle complet de transaction supportant divers types de transactions

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Transaction entity representing all portfolio transactions
    /// Entité transaction représentant toutes les transactions du portefeuille
    /// </summary>
    public class Transaction : BaseEntity
    {
        [Required]
        public Guid PortfolioId { get; private set; }

        [Required]
        public Guid SecurityId { get; private set; }

        [Required]
        public TransactionType TransactionType { get; private set; }

        [Required]
        public decimal Quantity { get; private set; }

        [Required]
        public decimal Price { get; private set; }

        public decimal Commission { get; private set; } = 0m;
        public decimal Fees { get; private set; } = 0m;

        [Required]
        public DateTime TradeDate { get; private set; }

        [Required]
        public DateTime SettlementDate { get; private set; }

        public Guid? ExecutedBy { get; private set; }

        [MaxLength(1000)]
        public string Notes { get; private set; } = string.Empty;

        [Required]
        public TransactionStatus Status { get; private set; } = TransactionStatus.Executed;

        // Calculated properties / Propriétés calculées
        public decimal GrossAmount => Math.Abs(Quantity) * Price;

        public decimal NetAmount
        {
            get
            {
                return TransactionType switch
                {
                    TransactionType.Buy => -1 * (GrossAmount + Commission + Fees),
                    TransactionType.Sell => GrossAmount - Commission - Fees,
                    TransactionType.Dividend or TransactionType.Interest => Quantity * Price,
                    TransactionType.Split => 0, // No cash impact / Aucun impact monétaire
                    TransactionType.Merger => Quantity * Price - Fees,
                    _ => 0
                };
            }
        }

        // Navigation properties / Propriétés de navigation
        public virtual Portfolio Portfolio { get; private set; } = null!;
        public virtual Security Security { get; private set; } = null!;
        public virtual User? ExecutedByUser { get; private set; }

        protected Transaction() { } // For EF Core

        public Transaction(Guid portfolioId, Guid securityId, TransactionType transactionType,
                          decimal quantity, decimal price, DateTime tradeDate, DateTime settlementDate,
                          decimal commission = 0m, decimal fees = 0m, Guid? executedBy = null, string notes = "")
        {
            PortfolioId = portfolioId;
            SecurityId = securityId;
            TransactionType = transactionType;
            Quantity = quantity;
            Price = price;
            TradeDate = tradeDate.Date;
            SettlementDate = settlementDate.Date;
            Commission = commission;
            Fees = fees;
            ExecutedBy = executedBy;
            Notes = notes;
            Status = TransactionStatus.Executed;

            ValidateTransaction();
        }

        /// <summary>
        /// Updates transaction status
        /// Met à jour le statut de la transaction
        /// </summary>
        public void UpdateStatus(TransactionStatus newStatus)
        {
            Status = newStatus;
            UpdateTimestamp();
        }

        /// <summary>
        /// Adds or updates notes
        /// Ajoute ou met à jour les notes
        /// </summary>
        public void UpdateNotes(string notes)
        {
            Notes = notes ?? string.Empty;
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates total transaction cost including fees
        /// Calcule le coût total de la transaction incluant les frais
        /// </summary>
        public decimal CalculateTotalCost()
        {
            return GrossAmount + Commission + Fees;
        }

        /// <summary>
        /// Calculates effective price per unit including all costs
        /// Calcule le prix effectif par unité incluant tous les coûts
        /// </summary>
        public decimal CalculateEffectivePrice()
        {
            if (Math.Abs(Quantity) == 0) return Price;

            return TransactionType switch
            {
                TransactionType.Buy => (GrossAmount + Commission + Fees) / Math.Abs(Quantity),
                TransactionType.Sell => (GrossAmount - Commission - Fees) / Math.Abs(Quantity),
                _ => Price
            };
        }

        /// <summary>
        /// Checks if transaction has settled
        /// Vérifie si la transaction a été réglée
        /// </summary>
        public bool HasSettled(DateTime? asOfDate = null)
        {
            var checkDate = asOfDate ?? DateTime.Today;
            return SettlementDate <= checkDate && Status == TransactionStatus.Executed;
        }

        /// <summary>
        /// Gets days until settlement
        /// Obtient les jours jusqu'au règlement
        /// </summary>
        public int DaysUntilSettlement(DateTime? fromDate = null)
        {
            var referenceDate = fromDate ?? DateTime.Today;
            return (SettlementDate - referenceDate).Days;
        }

        /// <summary>
        /// Checks if transaction is a cash flow transaction
        /// Vérifie si la transaction est une transaction de flux de trésorerie
        /// </summary>
        public bool IsCashFlowTransaction()
        {
            return TransactionType == TransactionType.Dividend || 
                   TransactionType == TransactionType.Interest;
        }

        /// <summary>
        /// Checks if transaction affects security position
        /// Vérifie si la transaction affecte la position de titre
        /// </summary>
        public bool AffectsPosition()
        {
            return TransactionType == TransactionType.Buy || 
                   TransactionType == TransactionType.Sell ||
                   TransactionType == TransactionType.Split;
        }

        /// <summary>
        /// Gets transaction impact on cash (positive = cash in, negative = cash out)
        /// Obtient l'impact de la transaction sur les liquidités (positif = entrée, négatif = sortie)
        /// </summary>
        public decimal GetCashImpact()
        {
            return NetAmount;
        }

        /// <summary>
        /// Gets transaction impact on position quantity
        /// Obtient l'impact de la transaction sur la quantité de position
        /// </summary>
        public decimal GetPositionImpact()
        {
            return TransactionType switch
            {
                TransactionType.Buy => Math.Abs(Quantity),
                TransactionType.Sell => -Math.Abs(Quantity),
                TransactionType.Split => Quantity, // Quantity should reflect the split ratio
                _ => 0
            };
        }

        /// <summary>
        /// Creates a reversal transaction (for corrections)
        /// Crée une transaction d'annulation (pour les corrections)
        /// </summary>
        public Transaction CreateReversal(Guid executedBy, string reason)
        {
            var reversalType = TransactionType switch
            {
                TransactionType.Buy => TransactionType.Sell,
                TransactionType.Sell => TransactionType.Buy,
                _ => throw new InvalidOperationException($"Cannot reverse transaction of type {TransactionType}")
            };

            return new Transaction(
                PortfolioId, SecurityId, reversalType, Math.Abs(Quantity), Price,
                DateTime.Today, DateTime.Today.AddDays(2), // T+2 settlement
                Commission, Fees, executedBy, $"Reversal of transaction {Id}: {reason}"
            );
        }

        /// <summary>
        /// Validates transaction data
        /// Valide les données de transaction
        /// </summary>
        private void ValidateTransaction()
        {
            if (Price < 0)
                throw new ArgumentException("Price cannot be negative / Le prix ne peut pas être négatif");

            if (Commission < 0)
                throw new ArgumentException("Commission cannot be negative / La commission ne peut pas être négative");

            if (Fees < 0)
                throw new ArgumentException("Fees cannot be negative / Les frais ne peuvent pas être négatifs");

            if (SettlementDate < TradeDate)
                throw new ArgumentException("Settlement date cannot be before trade date / La date de règlement ne peut pas être antérieure à la date de transaction");

            // Validate quantity based on transaction type
            // Valider la quantité selon le type de transaction
            switch (TransactionType)
            {
                case TransactionType.Buy:
                case TransactionType.Sell:
                    if (Quantity == 0)
                        throw new ArgumentException("Quantity cannot be zero for buy/sell transactions / La quantité ne peut pas être zéro pour les transactions d'achat/vente");
                    break;

                case TransactionType.Dividend:
                case TransactionType.Interest:
                    // These can have any quantity (including zero for special distributions)
                    // Ceux-ci peuvent avoir n'importe quelle quantité (y compris zéro pour les distributions spéciales)
                    break;

                case TransactionType.Split:
                    if (Quantity <= 0)
                        throw new ArgumentException("Split ratio must be positive / Le ratio de fractionnement doit être positif");
                    break;
            }
        }
    }

    /// <summary>
    /// Transaction types supported
    /// Types de transactions supportés
    /// </summary>
    public enum TransactionType
    {
        Buy,        // Achat
        Sell,       // Vente
        Dividend,   // Dividende
        Interest,   // Intérêt
        Split,      // Fractionnement d'actions
        Merger      // Fusion
    }

    /// <summary>
    /// Transaction status enumeration
    /// Énumération du statut de transaction
    /// </summary>
    public enum TransactionStatus
    {
        Pending,    // En attente
        Executed,   // Exécuté
        Cancelled,  // Annulé
        Failed      // Échoué
    }
}