// Market data entity for storing price and volume information
// Entité données de marché pour stocker les informations de prix et de volume
// Comprehensive market data model for financial securities
// Modèle complet de données de marché pour les titres financiers

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Market data entity storing historical and real-time price information
    /// Entité données de marché stockant les informations de prix historiques et en temps réel
    /// </summary>
    public class MarketData : BaseEntity
    {
        [Required]
        public Guid SecurityId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        public decimal? OpenPrice { get; private set; }
        public decimal? HighPrice { get; private set; }
        public decimal? LowPrice { get; private set; }

        [Required]
        public decimal ClosePrice { get; private set; }

        public long? Volume { get; private set; }
        public decimal? AdjustedClose { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Security Security { get; private set; } = null!;

        protected MarketData() { } // For EF Core

        public MarketData(Guid securityId, DateTime date, decimal closePrice, 
                         decimal? openPrice = null, decimal? highPrice = null, 
                         decimal? lowPrice = null, long? volume = null, decimal? adjustedClose = null)
        {
            SecurityId = securityId;
            Date = date.Date; // Ensure only date part
            ClosePrice = closePrice;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
            AdjustedClose = adjustedClose ?? closePrice;

            ValidatePrices();
        }

        /// <summary>
        /// Updates market data prices
        /// Met à jour les prix des données de marché
        /// </summary>
        public void UpdatePrices(decimal closePrice, decimal? openPrice = null, 
                                decimal? highPrice = null, decimal? lowPrice = null, 
                                long? volume = null, decimal? adjustedClose = null)
        {
            ClosePrice = closePrice;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
            AdjustedClose = adjustedClose ?? closePrice;
            
            ValidatePrices();
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates daily return compared to previous close
        /// Calcule le rendement quotidien par rapport à la clôture précédente
        /// </summary>
        public decimal? CalculateDailyReturn(decimal? previousClose)
        {
            if (!previousClose.HasValue || previousClose.Value <= 0)
                return null;

            return (ClosePrice - previousClose.Value) / previousClose.Value;
        }

        /// <summary>
        /// Calculates intraday volatility (high-low range)
        /// Calcule la volatilité intrajournalière (fourchette haut-bas)
        /// </summary>
        public decimal? CalculateIntradayVolatility()
        {
            if (!HighPrice.HasValue || !LowPrice.HasValue || ClosePrice <= 0)
                return null;

            return (HighPrice.Value - LowPrice.Value) / ClosePrice;
        }

        /// <summary>
        /// Gets the typical price (average of high, low, and close)
        /// Obtient le prix typique (moyenne du haut, bas et clôture)
        /// </summary>
        public decimal? GetTypicalPrice()
        {
            if (!HighPrice.HasValue || !LowPrice.HasValue)
                return ClosePrice;

            return (HighPrice.Value + LowPrice.Value + ClosePrice) / 3m;
        }

        /// <summary>
        /// Validates that prices are logical
        /// Valide que les prix sont logiques
        /// </summary>
        private void ValidatePrices()
        {
            if (ClosePrice <= 0)
                throw new ArgumentException("Close price must be positive / Le prix de clôture doit être positif");

            if (OpenPrice.HasValue && OpenPrice.Value <= 0)
                throw new ArgumentException("Open price must be positive / Le prix d'ouverture doit être positif");

            if (HighPrice.HasValue && LowPrice.HasValue && HighPrice.Value < LowPrice.Value)
                throw new ArgumentException("High price cannot be less than low price / Le prix haut ne peut pas être inférieur au prix bas");

            if (HighPrice.HasValue && ClosePrice > HighPrice.Value)
                throw new ArgumentException("Close price cannot exceed high price / Le prix de clôture ne peut pas dépasser le prix haut");

            if (LowPrice.HasValue && ClosePrice < LowPrice.Value)
                throw new ArgumentException("Close price cannot be less than low price / Le prix de clôture ne peut pas être inférieur au prix bas");

            if (Volume.HasValue && Volume.Value < 0)
                throw new ArgumentException("Volume cannot be negative / Le volume ne peut pas être négatif");
        }

        /// <summary>
        /// Checks if this is a valid trading day data
        /// Vérifie si c'est des données d'un jour de négociation valide
        /// </summary>
        public bool IsValidTradingData()
        {
            // Basic validation for complete OHLCV data
            // Validation de base pour des données OHLCV complètes
            return OpenPrice.HasValue && HighPrice.HasValue && LowPrice.HasValue && 
                   Volume.HasValue && Volume.Value > 0;
        }

        /// <summary>
        /// Creates a copy of this market data for a different date
        /// Crée une copie de ces données de marché pour une date différente
        /// </summary>
        public MarketData CopyForDate(DateTime newDate)
        {
            return new MarketData(SecurityId, newDate, ClosePrice, OpenPrice, HighPrice, LowPrice, Volume, AdjustedClose);
        }
    }

    /// <summary>
    /// Exchange rate entity for currency conversion
    /// Entité taux de change pour la conversion de devises
    /// </summary>
    public class ExchangeRate : BaseEntity
    {
        [Required]
        public Guid FromCurrencyId { get; private set; }

        [Required]
        public Guid ToCurrencyId { get; private set; }

        [Required]
        public DateTime Date { get; private set; }

        [Required]
        public decimal Rate { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual Currency FromCurrency { get; private set; } = null!;
        public virtual Currency ToCurrency { get; private set; } = null!;

        protected ExchangeRate() { } // For EF Core

        public ExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, DateTime date, decimal rate)
        {
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
            Date = date.Date;
            Rate = rate;

            if (rate <= 0)
                throw new ArgumentException("Exchange rate must be positive / Le taux de change doit être positif");

            if (fromCurrencyId == toCurrencyId)
                throw new ArgumentException("From and To currencies cannot be the same / Les devises de source et de destination ne peuvent pas être identiques");
        }

        /// <summary>
        /// Updates the exchange rate
        /// Met à jour le taux de change
        /// </summary>
        public void UpdateRate(decimal newRate)
        {
            if (newRate <= 0)
                throw new ArgumentException("Exchange rate must be positive / Le taux de change doit être positif");

            Rate = newRate;
            UpdateTimestamp();
        }

        /// <summary>
        /// Converts an amount using this exchange rate
        /// Convertit un montant en utilisant ce taux de change
        /// </summary>
        public decimal ConvertAmount(decimal amount)
        {
            return amount * Rate;
        }

        /// <summary>
        /// Gets the inverse exchange rate
        /// Obtient le taux de change inverse
        /// </summary>
        public decimal GetInverseRate()
        {
            return 1m / Rate;
        }

        /// <summary>
        /// Checks if the exchange rate is current (within last trading day)
        /// Vérifie si le taux de change est actuel (dans le dernier jour de négociation)
        /// </summary>
        public bool IsCurrent(DateTime? referenceDate = null)
        {
            var reference = referenceDate?.Date ?? DateTime.Today;
            var daysDifference = (reference - Date).TotalDays;
            
            // Consider current if within 3 days (accounting for weekends)
            // Considérer comme actuel si dans les 3 jours (en tenant compte des week-ends)
            return daysDifference <= 3;
        }
    }
}