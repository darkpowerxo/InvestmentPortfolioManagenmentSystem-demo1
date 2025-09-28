using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Infrastructure.Data;

/// <summary>
/// Contexte de base de données principal / Main database context
/// </summary>
public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    // DbSets pour toutes les entités / DbSets for all entities
    public DbSet<User> Users { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Security> Securities { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<MarketData> MarketData { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
    public DbSet<TradeOrder> TradeOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration des entités / Entity configurations
        ConfigureUserEntity(modelBuilder);
        ConfigurePortfolioEntity(modelBuilder);
        ConfigureSecurityEntity(modelBuilder);
        ConfigurePositionEntity(modelBuilder);
        ConfigureTransactionEntity(modelBuilder);
        ConfigureMarketDataEntity(modelBuilder);
        ConfigurePerformanceMetricEntity(modelBuilder);
        ConfigureTradeOrderEntity(modelBuilder);

        // Données de départ / Seed data
        SeedDataService.SeedData(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigurePortfolioEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BaseCurrency).IsRequired().HasMaxLength(10).HasDefaultValue("CAD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Précision pour les montants décimaux / Precision for decimal amounts
            entity.Property(e => e.InitialValue).HasPrecision(18, 4);
            entity.Property(e => e.CurrentValue).HasPrecision(18, 4);
            entity.Property(e => e.TargetEquityAllocation).HasPrecision(5, 2);
            entity.Property(e => e.TargetBondAllocation).HasPrecision(5, 2);
            entity.Property(e => e.TargetAlternativeAllocation).HasPrecision(5, 2);
            entity.Property(e => e.TargetCashAllocation).HasPrecision(5, 2);

            // Relations / Relationships
            entity.HasOne(e => e.Manager)
                  .WithMany(u => u.ManagedPortfolios)
                  .HasForeignKey(e => e.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSecurityEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Security>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("CAD");
            entity.Property(e => e.Exchange).HasMaxLength(100);
            entity.Property(e => e.Sector).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Canada");
            entity.Property(e => e.Region).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index unique sur Symbol et Exchange / Unique index on Symbol and Exchange
            entity.HasIndex(e => new { e.Symbol, e.Exchange }).IsUnique();

            // Relation auto-référentielle pour les options / Self-referencing relationship for options
            entity.HasOne(e => e.UnderlyingSecurity)
                  .WithMany()
                  .HasForeignKey(e => e.UnderlyingSecurityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePositionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.AverageCost).HasPrecision(18, 4);
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 4);
            entity.Property(e => e.MarketValue).HasPrecision(18, 4);
            entity.Property(e => e.UnrealizedGainLoss).HasPrecision(18, 4);
            entity.Property(e => e.UnrealizedGainLossPercent).HasPrecision(8, 4);
            entity.Property(e => e.AllocationPercentage).HasPrecision(5, 2);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index unique sur Portfolio et Security / Unique index on Portfolio and Security
            entity.HasIndex(e => new { e.PortfolioId, e.SecurityId }).IsUnique();

            // Relations / Relationships
            entity.HasOne(e => e.Portfolio)
                  .WithMany(p => p.Positions)
                  .HasForeignKey(e => e.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Security)
                  .WithMany(s => s.Positions)
                  .HasForeignKey(e => e.SecurityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTransactionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.Commission).HasPrecision(18, 4);
            entity.Property(e => e.OtherFees).HasPrecision(18, 4);
            entity.Property(e => e.NetAmount).HasPrecision(18, 4);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("CAD");
            entity.Property(e => e.ExchangeRate).HasPrecision(10, 6);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index sur les dates pour les performances / Index on dates for performance
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.SettlementDate);

            // Relations / Relationships
            entity.HasOne(e => e.Portfolio)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(e => e.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Security)
                  .WithMany(s => s.Transactions)
                  .HasForeignKey(e => e.SecurityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMarketDataEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MarketData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OpenPrice).HasPrecision(18, 4);
            entity.Property(e => e.HighPrice).HasPrecision(18, 4);
            entity.Property(e => e.LowPrice).HasPrecision(18, 4);
            entity.Property(e => e.ClosePrice).HasPrecision(18, 4);
            entity.Property(e => e.AdjustedClosePrice).HasPrecision(18, 4);
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("CAD");
            entity.Property(e => e.SMA20).HasPrecision(18, 4);
            entity.Property(e => e.SMA50).HasPrecision(18, 4);
            entity.Property(e => e.SMA200).HasPrecision(18, 4);
            entity.Property(e => e.EMA12).HasPrecision(18, 4);
            entity.Property(e => e.EMA26).HasPrecision(18, 4);
            entity.Property(e => e.RSI).HasPrecision(5, 2);
            entity.Property(e => e.Volatility).HasPrecision(8, 4);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index unique sur Security et Date / Unique index on Security and Date
            entity.HasIndex(e => new { e.SecurityId, e.Date }).IsUnique();
            
            // Index sur Date pour les requêtes de plage / Index on Date for range queries
            entity.HasIndex(e => e.Date);

            // Relations / Relationships
            entity.HasOne(e => e.Security)
                  .WithMany(s => s.MarketDataHistory)
                  .HasForeignKey(e => e.SecurityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigurePerformanceMetricEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Toutes les métriques avec précision appropriée / All metrics with appropriate precision
            var returnProperties = new[]
            {
                nameof(PerformanceMetric.DailyReturn), nameof(PerformanceMetric.WeeklyReturn),
                nameof(PerformanceMetric.MonthlyReturn), nameof(PerformanceMetric.YearToDateReturn),
                nameof(PerformanceMetric.OneYearReturn), nameof(PerformanceMetric.ThreeYearReturn),
                nameof(PerformanceMetric.FiveYearReturn), nameof(PerformanceMetric.SinceInceptionReturn),
                nameof(PerformanceMetric.BenchmarkReturn)
            };

            foreach (var property in returnProperties)
            {
                entity.Property(property).HasPrecision(8, 4);
            }

            var riskProperties = new[]
            {
                nameof(PerformanceMetric.Volatility), nameof(PerformanceMetric.SharpeRatio),
                nameof(PerformanceMetric.SortinoRatio), nameof(PerformanceMetric.Beta),
                nameof(PerformanceMetric.Alpha), nameof(PerformanceMetric.MaxDrawdown),
                nameof(PerformanceMetric.VaR95), nameof(PerformanceMetric.VaR99),
                nameof(PerformanceMetric.TrackingError), nameof(PerformanceMetric.InformationRatio)
            };

            foreach (var property in riskProperties)
            {
                entity.Property(property).HasPrecision(8, 4);
            }

            var allocationProperties = new[]
            {
                nameof(PerformanceMetric.EquityAllocation), nameof(PerformanceMetric.BondAllocation),
                nameof(PerformanceMetric.AlternativeAllocation), nameof(PerformanceMetric.CashAllocation)
            };

            foreach (var property in allocationProperties)
            {
                entity.Property(property).HasPrecision(5, 2);
            }

            entity.Property(e => e.BenchmarkSymbol).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index unique sur Portfolio et Date / Unique index on Portfolio and Date
            entity.HasIndex(e => new { e.PortfolioId, e.CalculationDate }).IsUnique();

            // Relations / Relationships
            entity.HasOne(e => e.Portfolio)
                  .WithMany(p => p.PerformanceMetrics)
                  .HasForeignKey(e => e.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureTradeOrderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TradeOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.LimitPrice).HasPrecision(18, 4);
            entity.Property(e => e.StopPrice).HasPrecision(18, 4);
            entity.Property(e => e.QuantityFilled).HasPrecision(18, 8);
            entity.Property(e => e.AverageExecutionPrice).HasPrecision(18, 4);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index sur les dates et statut / Index on dates and status
            entity.HasIndex(e => e.OrderDate);
            entity.HasIndex(e => e.Status);

            // Relations / Relationships
            entity.HasOne(e => e.Portfolio)
                  .WithMany(p => p.TradeOrders)
                  .HasForeignKey(e => e.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Security)
                  .WithMany(s => s.TradeOrders)
                  .HasForeignKey(e => e.SecurityId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany(u => u.CreatedOrders)
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Données de départ des utilisateurs / Seed user data
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "Jean",
                LastName = "Tremblay",
                Email = "jean.tremblay@cdpq.com",
                PasswordHash = "$2a$11$dummy.hash.for.demo.purposes.only", // In real app, use proper hashing
                Role = UserRole.PortfolioManager,
                PreferredLanguage = "fr",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Id = 2,
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@cdpq.com",
                PasswordHash = "$2a$11$dummy.hash.for.demo.purposes.only",
                Role = UserRole.Analyst,
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );

        // Données de départ des titres / Seed security data
        modelBuilder.Entity<Security>().HasData(
            // Actions canadiennes / Canadian stocks
            new Security
            {
                Id = 1,
                Symbol = "SHOP.TO",
                Name = "Shopify Inc.",
                SecurityType = Domain.Enums.SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Technology",
                Industry = "Software",
                Country = "Canada",
                Region = "North America",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Security
            {
                Id = 2,
                Symbol = "RY.TO",
                Name = "Royal Bank of Canada",
                SecurityType = Domain.Enums.SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Financial Services",
                Industry = "Banking",
                Country = "Canada",
                Region = "North America",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            // Obligations / Bonds
            new Security
            {
                Id = 3,
                Symbol = "CAN.GOVT.10Y",
                Name = "Government of Canada 10-Year Bond",
                SecurityType = Domain.Enums.SecurityType.Bond,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Government",
                Country = "Canada",
                Region = "North America",
                CouponRate = 3.25,
                MaturityDate = DateTime.UtcNow.AddYears(10),
                CreditRating = "AAA",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            // ETF
            new Security
            {
                Id = 4,
                Symbol = "VTI",
                Name = "Vanguard Total Stock Market ETF",
                SecurityType = Domain.Enums.SecurityType.ETF,
                Currency = "USD",
                Exchange = "NYSE",
                Sector = "Diversified",
                Industry = "Investment Fund",
                Country = "United States",
                Region = "North America",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }
}