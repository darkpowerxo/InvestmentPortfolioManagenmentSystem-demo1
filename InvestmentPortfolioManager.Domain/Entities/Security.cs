// Security entity representing financial instruments
// Entité titre représentant les instruments financiers
// Comprehensive security model supporting stocks, bonds, ETFs, alternatives, etc.
// Modèle de titre complet supportant actions, obligations, FNB, alternatifs, etc.

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Security entity representing tradable financial instruments
    /// Entité titre représentant les instruments financiers négociables
    /// </summary>
    public class Security : BaseEntity
    {
        [Required, MaxLength(20)]
        public string Symbol { get; private set; } = string.Empty;

        [MaxLength(12)] // ISIN format
        public string? ISIN { get; private set; }

        [MaxLength(9)] // CUSIP format
        public string? CUSIP { get; private set; }

        [Required, MaxLength(255)]
        public string NameEN { get; private set; } = string.Empty;

        [Required, MaxLength(255)]
        public string NameFR { get; private set; } = string.Empty;

        [Required]
        public SecurityType SecurityType { get; private set; }

        [Required]
        public Guid AssetClassId { get; private set; }

        public Guid? SectorId { get; private set; }

        public Guid? CountryId { get; private set; }

        [Required]
        public Guid CurrencyId { get; private set; }

        [MaxLength(50)]
        public string Exchange { get; private set; } = string.Empty;

        // Bond-specific properties / Propriétés spécifiques aux obligations
        public decimal? FaceValue { get; private set; }
        public DateTime? MaturityDate { get; private set; }
        public decimal? CouponRate { get; private set; }

        // Equity-specific properties / Propriétés spécifiques aux actions
        public decimal? DividendYield { get; private set; }

        // Risk metrics / Métriques de risque
        public decimal? Beta { get; private set; }

        // Navigation properties / Propriétés de navigation
        public virtual AssetClass AssetClass { get; private set; } = null!;
        public virtual Sector? Sector { get; private set; }
        public virtual Country? Country { get; private set; }
        public virtual Currency Currency { get; private set; } = null!;
        public virtual ICollection<MarketData> MarketData { get; private set; } = new List<MarketData>();
        public virtual ICollection<Holding> Holdings { get; private set; } = new List<Holding>();
        public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

        protected Security() { } // For EF Core

        public Security(string symbol, string nameEN, string nameFR, SecurityType securityType,
                       Guid assetClassId, Guid currencyId, string exchange = "",
                       string? isin = null, string? cusip = null)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            NameEN = nameEN ?? throw new ArgumentNullException(nameof(nameEN));
            NameFR = nameFR ?? throw new ArgumentNullException(nameFR);
            SecurityType = securityType;
            AssetClassId = assetClassId;
            CurrencyId = currencyId;
            Exchange = exchange;
            ISIN = isin;
            CUSIP = cusip;
        }

        /// <summary>
        /// Gets localized name based on language
        /// Obtient le nom localisé selon la langue
        /// </summary>
        public string GetLocalizedName(string language) => language?.ToUpper() == "FR" ? NameFR : NameEN;

        /// <summary>
        /// Updates bond-specific information
        /// Met à jour les informations spécifiques aux obligations
        /// </summary>
        public void UpdateBondInfo(decimal faceValue, DateTime maturityDate, decimal couponRate)
        {
            if (SecurityType != SecurityType.Bond)
                throw new InvalidOperationException("Bond information can only be set for bond securities / Les informations d'obligation ne peuvent être définies que pour les titres obligataires");

            FaceValue = faceValue;
            MaturityDate = maturityDate;
            CouponRate = couponRate;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates equity-specific information
        /// Met à jour les informations spécifiques aux actions
        /// </summary>
        public void UpdateEquityInfo(decimal? dividendYield, decimal? beta)
        {
            if (SecurityType != SecurityType.Stock && SecurityType != SecurityType.ETF)
                throw new InvalidOperationException("Equity information can only be set for stock or ETF securities / Les informations d'actions ne peuvent être définies que pour les actions ou FNB");

            DividendYield = dividendYield;
            Beta = beta;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates sector and country classification
        /// Met à jour la classification par secteur et pays
        /// </summary>
        public void UpdateClassification(Guid? sectorId, Guid? countryId)
        {
            SectorId = sectorId;
            CountryId = countryId;
            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates duration for bond securities
        /// Calcule la duration pour les titres obligataires
        /// </summary>
        public decimal? CalculateDuration(decimal currentYield)
        {
            if (SecurityType != SecurityType.Bond || !MaturityDate.HasValue || !CouponRate.HasValue || !FaceValue.HasValue)
                return null;

            // Simplified Macaulay duration calculation
            // Calcul simplifié de la duration de Macaulay
            var yearsToMaturity = (MaturityDate.Value - DateTime.Today).TotalDays / 365.25;
            if (yearsToMaturity <= 0) return 0;

            // For simplicity, assume annual coupon payments
            // Pour simplifier, supposons des paiements de coupon annuels
            var annualCoupon = FaceValue.Value * CouponRate.Value;
            var presentValue = 0m;
            var weightedCashFlows = 0m;

            for (int t = 1; t <= (int)yearsToMaturity; t++)
            {
                var cashFlow = t == (int)yearsToMaturity ? annualCoupon + FaceValue.Value : annualCoupon;
                var pv = cashFlow / (decimal)Math.Pow((double)(1 + currentYield), t);
                presentValue += pv;
                weightedCashFlows += pv * t;
            }

            return presentValue > 0 ? weightedCashFlows / presentValue : null;
        }

        /// <summary>
        /// Calculates convexity for bond securities
        /// Calcule la convexité pour les titres obligataires
        /// </summary>
        public decimal? CalculateConvexity(decimal currentYield)
        {
            if (SecurityType != SecurityType.Bond || !MaturityDate.HasValue || !CouponRate.HasValue || !FaceValue.HasValue)
                return null;

            var yearsToMaturity = (MaturityDate.Value - DateTime.Today).TotalDays / 365.25;
            if (yearsToMaturity <= 0) return 0;

            var annualCoupon = FaceValue.Value * CouponRate.Value;
            var presentValue = 0m;
            var convexitySum = 0m;

            for (int t = 1; t <= (int)yearsToMaturity; t++)
            {
                var cashFlow = t == (int)yearsToMaturity ? annualCoupon + FaceValue.Value : annualCoupon;
                var pv = cashFlow / (decimal)Math.Pow((double)(1 + currentYield), t);
                presentValue += pv;
                convexitySum += pv * t * (t + 1);
            }

            return presentValue > 0 ? convexitySum / (presentValue * (decimal)Math.Pow((double)(1 + currentYield), 2)) : null;
        }

        /// <summary>
        /// Gets the latest market price
        /// Obtient le dernier prix de marché
        /// </summary>
        public decimal? GetLatestPrice()
        {
            return MarketData?.OrderByDescending(md => md.Date)?.FirstOrDefault()?.ClosePrice;
        }

        /// <summary>
        /// Gets market data for a specific date range
        /// Obtient les données de marché pour une plage de dates spécifique
        /// </summary>
        public IEnumerable<MarketData> GetMarketDataRange(DateTime startDate, DateTime endDate)
        {
            return MarketData?.Where(md => md.Date >= startDate && md.Date <= endDate)?.OrderBy(md => md.Date) ?? Enumerable.Empty<MarketData>();
        }

        /// <summary>
        /// Calculates price volatility over a period
        /// Calcule la volatilité des prix sur une période
        /// </summary>
        public decimal? CalculateVolatility(int days = 30)
        {
            var data = MarketData?.OrderByDescending(md => md.Date)?.Take(days)?.Select(md => md.ClosePrice)?.ToList();
            
            if (data == null || data.Count < 2)
                return null;

            // Calculate daily returns / Calculer les rendements quotidiens
            var returns = new List<decimal>();
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i] > 0)
                    returns.Add((data[i-1] - data[i]) / data[i]);
            }

            if (returns.Count == 0)
                return null;

            // Calculate standard deviation / Calculer l'écart-type
            var mean = returns.Average();
            var sumSquaredDeviations = returns.Sum(r => (r - mean) * (r - mean));
            var variance = sumSquaredDeviations / returns.Count;
            
            return (decimal)Math.Sqrt((double)variance) * (decimal)Math.Sqrt(252); // Annualized / Annualisé
        }
    }

    /// <summary>
    /// Types of securities supported
    /// Types de titres supportés
    /// </summary>
    public enum SecurityType
    {
        Stock,          // Action
        Bond,           // Obligation
        ETF,            // FNB (Fonds négocié en bourse)
        MutualFund,     // Fonds commun de placement
        Option,         // Option
        Future,         // Contrat à terme
        Alternative     // Investissement alternatif
    }
}