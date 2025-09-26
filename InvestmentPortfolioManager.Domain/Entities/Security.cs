// Security entity representing financial instruments
// Entité titre représentant les instruments financiers
// Comprehensive security model supporting stocks, bonds, ETFs, alternatives, etc.
// Modèle de titre complet supportant actions, obligations, FNB, alternatifs, etc.

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Security entity representing financial instruments (stocks, bonds, ETFs, etc.)
    /// Entité titre représentant les instruments financiers (actions, obligations, FNB, etc.)
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
        public int? PaymentsPerYear { get; private set; }

        // Equity-specific properties / Propriétés spécifiques aux actions
        public decimal? DividendYield { get; private set; }
        public decimal? Beta { get; private set; }
        public decimal? HistoricalVolatility { get; private set; }

        // Localization support / Support de localisation
        public string GetLocalizedName(string languageCode = "EN")
        {
            return languageCode.ToUpper() switch
            {
                "FR" => !string.IsNullOrEmpty(NameFR) ? NameFR : NameEN,
                _ => !string.IsNullOrEmpty(NameEN) ? NameEN : NameFR
            };
        }

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
                       Guid assetClassId, Guid currencyId, string exchange = "")
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol cannot be empty / Le symbole ne peut pas être vide");
            if (string.IsNullOrWhiteSpace(nameEN))
                throw new ArgumentException("English name cannot be empty / Le nom anglais ne peut pas être vide");
            if (string.IsNullOrWhiteSpace(nameFR))
                throw new ArgumentException("French name cannot be empty / Le nom français ne peut pas être vide");

            Symbol = symbol.ToUpper();
            NameEN = nameEN;
            NameFR = nameFR;
            SecurityType = securityType;
            AssetClassId = assetClassId;
            CurrencyId = currencyId;
            Exchange = exchange;
        }

        /// <summary>
        /// Updates basic security information
        /// Met à jour les informations de base du titre
        /// </summary>
        public void UpdateBasicInfo(string? nameEN = null, string? nameFR = null, string? exchange = null)
        {
            if (!string.IsNullOrWhiteSpace(nameEN))
                NameEN = nameEN;
            if (!string.IsNullOrWhiteSpace(nameFR))
                NameFR = nameFR;
            if (exchange != null)
                Exchange = exchange;

            UpdateTimestamp();
        }

        /// <summary>
        /// Updates bond-specific information
        /// Met à jour les informations spécifiques aux obligations
        /// </summary>
        public void UpdateBondInfo(decimal? faceValue, DateTime? maturityDate, decimal? couponRate, int? paymentsPerYear)
        {
            if (SecurityType != SecurityType.Bond)
                throw new InvalidOperationException("Bond info can only be updated for bonds / Les informations obligataires ne peuvent être mises à jour que pour les obligations");

            FaceValue = faceValue;
            MaturityDate = maturityDate;
            CouponRate = couponRate;
            PaymentsPerYear = paymentsPerYear;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates equity-specific information
        /// Met à jour les informations spécifiques aux actions
        /// </summary>
        public void UpdateEquityInfo(decimal? dividendYield, decimal? beta)
        {
            if (SecurityType != SecurityType.Stock && SecurityType != SecurityType.ETF)
                throw new InvalidOperationException("Equity info can only be updated for stocks and ETFs / Les informations sur les actions ne peuvent être mises à jour que pour les actions et les ETF");

            DividendYield = dividendYield;
            Beta = beta;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates historical volatility
        /// Met à jour la volatilité historique
        /// </summary>
        public void UpdateHistoricalVolatility(decimal volatility)
        {
            if (volatility < 0)
                throw new ArgumentException("Volatility cannot be negative / La volatilité ne peut pas être négative");

            HistoricalVolatility = volatility;
            UpdateTimestamp();
        }

        /// <summary>
        /// Updates identifiers (ISIN, CUSIP)
        /// Met à jour les identifiants (ISIN, CUSIP)
        /// </summary>
        public void UpdateIdentifiers(string? isin = null, string? cusip = null)
        {
            if (!string.IsNullOrWhiteSpace(isin))
            {
                if (isin.Length != 12)
                    throw new ArgumentException("ISIN must be 12 characters / L'ISIN doit faire 12 caractères");
                ISIN = isin.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(cusip))
            {
                if (cusip.Length != 9)
                    throw new ArgumentException("CUSIP must be 9 characters / Le CUSIP doit faire 9 caractères");
                CUSIP = cusip.ToUpper();
            }

            UpdateTimestamp();
        }

        /// <summary>
        /// Calculates bond duration using Macaulay formula
        /// Calcule la duration de l'obligation avec la formule de Macaulay
        /// </summary>
        public decimal? CalculateDuration(decimal? currentYield = null)
        {
            if (SecurityType != SecurityType.Bond || !MaturityDate.HasValue || !CouponRate.HasValue || !FaceValue.HasValue)
                return null;

            var yield = currentYield ?? CouponRate.Value;
            var yearsToMaturity = (MaturityDate.Value - DateTime.Today).TotalDays / 365.25;
            var paymentsPerYear = PaymentsPerYear ?? 2;

            if (yearsToMaturity <= 0)
                return 0;

            // Simplified duration calculation for demonstration
            // Production code would use more sophisticated calculation
            var periodicCoupon = FaceValue.Value * CouponRate.Value / paymentsPerYear;
            var periodicYield = yield / paymentsPerYear;
            var periods = (int)(yearsToMaturity * paymentsPerYear);

            if (periodicYield == 0)
                return (decimal)yearsToMaturity;

            decimal presentValueSum = 0;
            decimal weightedCashFlowSum = 0;

            for (int t = 1; t <= periods; t++)
            {
                var cashFlow = periodicCoupon;
                if (t == periods)
                    cashFlow += FaceValue.Value;

                var presentValue = cashFlow / (decimal)Math.Pow((double)(1 + periodicYield), t);
                presentValueSum += presentValue;
                weightedCashFlowSum += presentValue * t / paymentsPerYear;
            }

            return presentValueSum > 0 ? weightedCashFlowSum / presentValueSum : null;
        }

        /// <summary>
        /// Calculates bond convexity
        /// Calcule la convexité de l'obligation
        /// </summary>
        public decimal? CalculateConvexity(decimal? currentYield = null)
        {
            if (SecurityType != SecurityType.Bond || !MaturityDate.HasValue || !CouponRate.HasValue || !FaceValue.HasValue)
                return null;

            var yield = currentYield ?? CouponRate.Value;
            var yearsToMaturity = (MaturityDate.Value - DateTime.Today).TotalDays / 365.25;
            var paymentsPerYear = PaymentsPerYear ?? 2;

            if (yearsToMaturity <= 0)
                return 0;

            var periodicCoupon = FaceValue.Value * CouponRate.Value / paymentsPerYear;
            var periodicYield = yield / paymentsPerYear;
            var periods = (int)(yearsToMaturity * paymentsPerYear);

            if (periodicYield == 0)
                return 0;

            decimal presentValueSum = 0;
            decimal convexitySum = 0;

            for (int t = 1; t <= periods; t++)
            {
                var cashFlow = periodicCoupon;
                if (t == periods)
                    cashFlow += FaceValue.Value;

                var presentValue = cashFlow / (decimal)Math.Pow((double)(1 + periodicYield), t);
                presentValueSum += presentValue;
                convexitySum += presentValue * t * (t + 1);
            }

            var bondPrice = presentValueSum;
            return bondPrice > 0 ? convexitySum / (bondPrice * (decimal)Math.Pow((double)(1 + periodicYield), 2) * paymentsPerYear * paymentsPerYear) : null;
        }

        /// <summary>
        /// Calculates price volatility based on recent price movements
        /// Calcule la volatilité des prix basée sur les mouvements de prix récents
        /// </summary>
        public decimal? CalculatePriceVolatility(int days = 30)
        {
            var recentData = MarketData
                .Where(md => md.Date >= DateTime.Today.AddDays(-days))
                .OrderBy(md => md.Date)
                .ToList();

            if (recentData.Count < 2)
                return null;

            var returns = new List<decimal>();
            for (int i = 1; i < recentData.Count; i++)
            {
                var dailyReturn = (decimal)Math.Log((double)(recentData[i].ClosePrice / recentData[i - 1].ClosePrice));
                returns.Add(dailyReturn);
            }

            if (!returns.Any())
                return null;

            var mean = returns.Average();
            var variance = returns.Sum(r => (r - mean) * (r - mean)) / (returns.Count - 1);
            var volatility = (decimal)Math.Sqrt((double)variance);

            // Annualize the volatility
            return volatility * (decimal)Math.Sqrt(252); // 252 trading days per year
        }
    }

    /// <summary>
    /// Security type enumeration
    /// Énumération des types de titres
    /// </summary>
    public enum SecurityType
    {
        Stock,          // Action
        Bond,           // Obligation
        ETF,            // Fonds négocié en bourse
        Index,          // Indice
        MutualFund,     // Fonds commun de placement
        Alternative,    // Alternatif (REITs, commodities, etc.)
        Cash,           // Liquidités
        Derivative      // Dérivé
    }
}