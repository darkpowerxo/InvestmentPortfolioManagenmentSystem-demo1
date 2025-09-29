using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Infrastructure.Data;

namespace InvestmentPortfolioManager.Infrastructure.Data;

/// <summary>
/// Service pour générer des données de départ réalistes / Service for generating realistic seed data
/// </summary>
public static class SeedDataService
{
    /// <summary>
    /// Configure les données de départ / Configure seed data
    /// </summary>
    public static void SeedData(ModelBuilder modelBuilder)
    {
        SeedUsers(modelBuilder);
        SeedSecurities(modelBuilder);
        SeedPortfolios(modelBuilder);
        SeedPositions(modelBuilder);
        SeedTransactions(modelBuilder);
        SeedMarketData(modelBuilder);
        SeedPerformanceMetrics(modelBuilder);
        SeedTradeOrders(modelBuilder);
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var users = new[]
        {
            new User
            {
                Id = 1,
                Email = "marie.tremblay@cdpq.com",
                FirstName = "Marie",
                LastName = "Tremblay",
                Role = "Portfolio Manager",
                PreferredLanguage = "fr",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-24)
            },
            new User
            {
                Id = 2,
                Email = "john.smith@cdpq.com",
                FirstName = "John",
                LastName = "Smith",
                Role = "Senior Analyst",
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-18)
            },
            new User
            {
                Id = 3,
                Email = "sophie.dubois@cdpq.com",
                FirstName = "Sophie",
                LastName = "Dubois",
                Role = "Risk Manager",
                PreferredLanguage = "fr",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            },
            new User
            {
                Id = 4,
                Email = "david.wang@cdpq.com",
                FirstName = "David",
                LastName = "Wang",
                Role = "Quantitative Analyst",
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            }
        };

        modelBuilder.Entity<User>().HasData(users);
    }

    private static void SeedSecurities(ModelBuilder modelBuilder)
    {
        var securities = new[]
        {
            // Canadian Equities
            new Security
            {
                Id = 1,
                Symbol = "RY.TO",
                Name = "Royal Bank of Canada",
                SecurityType = SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Financials",
                Industry = "Banks",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 2,
                Symbol = "SHOP.TO",
                Name = "Shopify Inc",
                SecurityType = SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Technology",
                Industry = "Software",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 3,
                Symbol = "CNR.TO",
                Name = "Canadian National Railway Company",
                SecurityType = SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Industrials",
                Industry = "Transportation",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 4,
                Symbol = "ENB.TO",
                Name = "Enbridge Inc",
                SecurityType = SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Energy",
                Industry = "Pipeline",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 5,
                Symbol = "TD.TO",
                Name = "The Toronto-Dominion Bank",
                SecurityType = SecurityType.Stock,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Financials",
                Industry = "Banks",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            // US Equities
            new Security
            {
                Id = 6,
                Symbol = "AAPL",
                Name = "Apple Inc",
                SecurityType = SecurityType.Stock,
                Currency = "USD",
                Exchange = "NASDAQ",
                Sector = "Technology",
                Industry = "Consumer Electronics",
                Country = "United States",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 7,
                Symbol = "MSFT",
                Name = "Microsoft Corporation",
                SecurityType = SecurityType.Stock,
                Currency = "USD",
                Exchange = "NASDAQ",
                Sector = "Technology",
                Industry = "Software",
                Country = "United States",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 8,
                Symbol = "GOOGL",
                Name = "Alphabet Inc",
                SecurityType = SecurityType.Stock,
                Currency = "USD",
                Exchange = "NASDAQ",
                Sector = "Technology",
                Industry = "Internet Services",
                Country = "United States",
                Region = "North America",
                IsActive = true
            },
            // ETFs
            new Security
            {
                Id = 9,
                Symbol = "VTI",
                Name = "Vanguard Total Stock Market ETF",
                SecurityType = SecurityType.ETF,
                Currency = "USD",
                Exchange = "NYSE",
                Sector = "Diversified",
                Industry = "ETF",
                Country = "United States",
                Region = "North America",
                IsActive = true
            },
            new Security
            {
                Id = 10,
                Symbol = "TDB902",
                Name = "TD Canadian Bond Index Fund - I",
                SecurityType = SecurityType.ETF,
                Currency = "CAD",
                Exchange = "TSX",
                Sector = "Fixed Income",
                Industry = "ETF",
                Country = "Canada",
                Region = "North America",
                IsActive = true
            },
            // Bonds
            new Security
            {
                Id = 11,
                Symbol = "GC001",
                Name = "Government of Canada Bond 2.75% Dec 1, 2028",
                SecurityType = SecurityType.Bond,
                Currency = "CAD",
                Exchange = "OTC",
                Sector = "Government",
                Industry = "Government Bond",
                Country = "Canada",
                Region = "North America",
                CouponRate = 2.75,
                MaturityDate = new DateTime(2028, 12, 1),
                CreditRating = "AAA",
                IsActive = true
            },
            new Security
            {
                Id = 12,
                Symbol = "CORP001",
                Name = "RBC 3.25% Corporate Bond Mar 15, 2027",
                SecurityType = SecurityType.Bond,
                Currency = "CAD",
                Exchange = "OTC",
                Sector = "Financials",
                Industry = "Corporate Bond",
                Country = "Canada",
                Region = "North America",
                CouponRate = 3.25,
                MaturityDate = new DateTime(2027, 3, 15),
                CreditRating = "AA-",
                IsActive = true
            }
        };

        modelBuilder.Entity<Security>().HasData(securities);
    }

    private static void SeedPortfolios(ModelBuilder modelBuilder)
    {
        var portfolios = new[]
        {
            new Portfolio
            {
                Id = 1,
                Name = "CDPQ Equity Growth Fund",
                Description = "Growth-oriented equity portfolio focused on Canadian and US large-cap stocks",
                PortfolioType = PortfolioType.Equity,
                RiskLevel = RiskLevel.ModerateAggressive,
                BaseCurrency = "CAD",
                ManagerId = 1,
                InitialValue = 50000000m,
                CurrentValue = 52500000m,
                InceptionDate = DateTime.UtcNow.AddMonths(-18),
                TargetEquityAllocation = 85m,
                TargetBondAllocation = 10m,
                TargetAlternativeAllocation = 5m,
                TargetCashAllocation = 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-18)
            },
            new Portfolio
            {
                Id = 2,
                Name = "CDPQ Balanced Fund",
                Description = "Balanced portfolio with diversified mix of equities and fixed income",
                PortfolioType = PortfolioType.Balanced,
                RiskLevel = RiskLevel.Moderate,
                BaseCurrency = "CAD",
                ManagerId = 2,
                InitialValue = 100000000m,
                CurrentValue = 103200000m,
                InceptionDate = DateTime.UtcNow.AddMonths(-24),
                TargetEquityAllocation = 60m,
                TargetBondAllocation = 35m,
                TargetAlternativeAllocation = 5m,
                TargetCashAllocation = 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-24)
            },
            new Portfolio
            {
                Id = 3,
                Name = "CDPQ Fixed Income Fund",
                Description = "Conservative fixed income portfolio with government and high-grade corporate bonds",
                PortfolioType = PortfolioType.FixedIncome,
                RiskLevel = RiskLevel.Conservative,
                BaseCurrency = "CAD",
                ManagerId = 3,
                InitialValue = 75000000m,
                CurrentValue = 76800000m,
                InceptionDate = DateTime.UtcNow.AddMonths(-12),
                TargetEquityAllocation = 5m,
                TargetBondAllocation = 90m,
                TargetAlternativeAllocation = 0m,
                TargetCashAllocation = 5m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            }
        };

        modelBuilder.Entity<Portfolio>().HasData(portfolios);
    }

    private static void SeedPositions(ModelBuilder modelBuilder)
    {
        var positions = new[]
        {
            // CDPQ Equity Growth Fund positions
            new Position
            {
                Id = 1,
                PortfolioId = 1,
                SecurityId = 1, // RY.TO
                Quantity = 50000,
                AverageCost = 145.50m,
                CurrentPrice = 152.75m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-12),
                LastTransactionDate = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 2,
                PortfolioId = 1,
                SecurityId = 2, // SHOP.TO
                Quantity = 25000,
                AverageCost = 185.25m,
                CurrentPrice = 198.50m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-8),
                LastTransactionDate = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 3,
                PortfolioId = 1,
                SecurityId = 6, // AAPL
                Quantity = 75000,
                AverageCost = 175.80m,
                CurrentPrice = 182.30m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-15),
                LastTransactionDate = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 4,
                PortfolioId = 1,
                SecurityId = 7, // MSFT
                Quantity = 40000,
                AverageCost = 340.25m,
                CurrentPrice = 355.75m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-10),
                LastTransactionDate = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            // CDPQ Balanced Fund positions
            new Position
            {
                Id = 5,
                PortfolioId = 2,
                SecurityId = 1, // RY.TO
                Quantity = 80000,
                AverageCost = 142.30m,
                CurrentPrice = 152.75m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-20),
                LastTransactionDate = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 6,
                PortfolioId = 2,
                SecurityId = 9, // VTI
                Quantity = 100000,
                AverageCost = 220.15m,
                CurrentPrice = 228.90m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-18),
                LastTransactionDate = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 7,
                PortfolioId = 2,
                SecurityId = 10, // TDB902
                Quantity = 200000,
                AverageCost = 25.80m,
                CurrentPrice = 25.95m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-22),
                LastTransactionDate = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 8,
                PortfolioId = 2,
                SecurityId = 11, // GC001
                Quantity = 500,
                AverageCost = 1025.50m,
                CurrentPrice = 1018.75m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-14),
                LastTransactionDate = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            // CDPQ Fixed Income Fund positions
            new Position
            {
                Id = 9,
                PortfolioId = 3,
                SecurityId = 11, // GC001
                Quantity = 1000,
                AverageCost = 1020.00m,
                CurrentPrice = 1018.75m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-11),
                LastTransactionDate = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Position
            {
                Id = 10,
                PortfolioId = 3,
                SecurityId = 12, // CORP001
                Quantity = 750,
                AverageCost = 1035.25m,
                CurrentPrice = 1042.10m,
                FirstPurchaseDate = DateTime.UtcNow.AddMonths(-9),
                LastTransactionDate = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        modelBuilder.Entity<Position>().HasData(positions);
    }

    private static void SeedTransactions(ModelBuilder modelBuilder)
    {
        var random = new Random(42); // Seed for reproducibility
        var transactions = new List<Transaction>();
        var transactionId = 1;

        // Generate transactions for the past 6 months
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var portfolioIds = new[] { 1, 2, 3 };
        var securityIds = new[] { 1, 2, 6, 7, 9, 10, 11, 12 };

        for (int i = 0; i < 50; i++)
        {
            var portfolioId = portfolioIds[random.Next(portfolioIds.Length)];
            var securityId = securityIds[random.Next(securityIds.Length)];
            var transactionDate = startDate.AddDays(random.Next(180));
            var transactionType = random.NextDouble() > 0.5 ? TransactionType.Buy : TransactionType.Sell;
            var quantity = random.Next(100, 5000);
            var price = 50m + (decimal)(random.NextDouble() * 300);

            transactions.Add(new Transaction
            {
                Id = transactionId++,
                PortfolioId = portfolioId,
                SecurityId = securityId,
                TransactionType = transactionType,
                Quantity = quantity,
                Price = price,
                Commission = price * quantity * 0.001m, // 0.1% commission
                TransactionDate = transactionDate,
                SettlementDate = transactionDate.AddDays(2),
                CreatedAt = transactionDate
            });
        }

        // Add some dividend transactions
        for (int i = 0; i < 10; i++)
        {
            transactions.Add(new Transaction
            {
                Id = transactionId++,
                PortfolioId = random.Next(1, 4),
                SecurityId = 1, // RY.TO pays dividends
                TransactionType = TransactionType.Dividend,
                Quantity = 0,
                Price = 1.35m, // Dividend per share
                Commission = 0,
                TransactionDate = DateTime.UtcNow.AddDays(-random.Next(90)),
                SettlementDate = DateTime.UtcNow.AddDays(-random.Next(90)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(90))
            });
        }

        modelBuilder.Entity<Transaction>().HasData(transactions);
    }

    private static void SeedMarketData(ModelBuilder modelBuilder)
    {
        var marketData = new List<MarketData>();
        var marketDataId = 1;
        var random = new Random(42);

        // Generate market data for the past 30 days for key securities
        var securityIds = new[] { 1, 2, 6, 7, 9, 10, 11, 12 };
        var basePrices = new Dictionary<int, decimal>
        {
            { 1, 152.75m }, // RY.TO
            { 2, 198.50m }, // SHOP.TO
            { 6, 182.30m }, // AAPL
            { 7, 355.75m }, // MSFT
            { 9, 228.90m }, // VTI
            { 10, 25.95m }, // TDB902
            { 11, 1018.75m }, // GC001
            { 12, 1042.10m }  // CORP001
        };

        foreach (var securityId in securityIds)
        {
            var basePrice = basePrices[securityId];
            var currentPrice = basePrice;

            for (int day = 30; day >= 0; day--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-day);
                
                // Simulate daily price movement (-2% to +2%)
                var dailyChange = (decimal)((random.NextDouble() - 0.5) * 0.04);
                currentPrice *= (1 + dailyChange);

                var open = currentPrice * (1 + (decimal)((random.NextDouble() - 0.5) * 0.01));
                var high = Math.Max(open, currentPrice) * (1 + (decimal)(random.NextDouble() * 0.02));
                var low = Math.Min(open, currentPrice) * (1 - (decimal)(random.NextDouble() * 0.02));
                var volume = random.Next(10000, 1000000);

                marketData.Add(new MarketData
                {
                    Id = marketDataId++,
                    SecurityId = securityId,
                    Date = date,
                    OpenPrice = Math.Round(open, 2),
                    HighPrice = Math.Round(high, 2),
                    LowPrice = Math.Round(low, 2),
                    ClosePrice = Math.Round(currentPrice, 2),
                    Volume = volume,
                    CreatedAt = date.AddHours(16) // End of trading day
                });
            }
        }

        modelBuilder.Entity<MarketData>().HasData(marketData);
    }

    private static void SeedPerformanceMetrics(ModelBuilder modelBuilder)
    {
        var metrics = new List<PerformanceMetric>();
        var metricId = 1;

        // Generate monthly performance metrics for the past 6 months
        for (int month = 6; month >= 1; month--)
        {
            var date = DateTime.UtcNow.AddMonths(-month).Date;
            
            foreach (int portfolioId in new[] { 1, 2, 3 })
            {
                // Monthly return
                metrics.Add(new PerformanceMetric
                {
                    Id = metricId++,
                    PortfolioId = portfolioId,
                    MetricType = "Monthly Return",
                    Value = (decimal)((new Random(42).NextDouble() - 0.5) * 0.08), // -4% to +4%
                    Date = date,
                    CreatedAt = date.AddDays(1)
                });

                // Sharpe Ratio
                metrics.Add(new PerformanceMetric
                {
                    Id = metricId++,
                    PortfolioId = portfolioId,
                    MetricType = "Sharpe Ratio",
                    Value = 0.8m + (decimal)(new Random(42 + portfolioId).NextDouble() * 0.8), // 0.8 to 1.6
                    Date = date,
                    CreatedAt = date.AddDays(1)
                });

                // Volatility
                metrics.Add(new PerformanceMetric
                {
                    Id = metricId++,
                    PortfolioId = portfolioId,
                    MetricType = "Volatility",
                    Value = 0.10m + (decimal)(new Random(42 + portfolioId * 2).NextDouble() * 0.10), // 10% to 20%
                    Date = date,
                    CreatedAt = date.AddDays(1)
                });
            }
        }

        modelBuilder.Entity<PerformanceMetric>().HasData(metrics);
    }

    private static void SeedTradeOrders(ModelBuilder modelBuilder)
    {
        var orders = new[]
        {
            new TradeOrder
            {
                Id = 1,
                PortfolioId = 1,
                SecurityId = 2, // SHOP.TO
                OrderType = OrderType.Limit,
                Quantity = 1000,
                LimitPrice = 195.00m,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ValidUntil = DateTime.UtcNow.AddDays(1)
            },
            new TradeOrder
            {
                Id = 2,
                PortfolioId = 2,
                SecurityId = 6, // AAPL
                OrderType = OrderType.Market,
                Quantity = 2500,
                Status = OrderStatus.Filled,
                ExecutedPrice = 182.30m,
                ExecutedQuantity = 2500,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExecutedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(5)
            },
            new TradeOrder
            {
                Id = 3,
                PortfolioId = 3,
                SecurityId = 11, // GC001
                OrderType = OrderType.Limit,
                Quantity = 100,
                LimitPrice = 1015.00m,
                Status = OrderStatus.PartiallyFilled,
                ExecutedPrice = 1018.75m,
                ExecutedQuantity = 50,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                ExecutedAt = DateTime.UtcNow.AddHours(-4),
                ValidUntil = DateTime.UtcNow.AddDays(7)
            }
        };

        modelBuilder.Entity<TradeOrder>().HasData(orders);
    }
}