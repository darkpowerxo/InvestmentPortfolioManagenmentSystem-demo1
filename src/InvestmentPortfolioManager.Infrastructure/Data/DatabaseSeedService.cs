using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Infrastructure.Data
{
    /// <summary>
    /// Service pour générer des données de départ réalistes / Service for generating realistic seed data
    /// </summary>
    public class DatabaseSeedService : ISeedDataService
    {
        private readonly PortfolioDbContext _context;

        public DatabaseSeedService(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsDatabaseSeededAsync()
        {
            return await _context.Users.AnyAsync() || 
                   await _context.Securities.AnyAsync() || 
                   await _context.Portfolios.AnyAsync();
        }

        public async Task SeedDataAsync()
        {
            // Check if database is already seeded
            if (await IsDatabaseSeededAsync())
            {
                return;
            }

            await SeedUsersAsync();
            await SeedSecuritiesAsync();
            await SeedPortfoliosAsync();
            await SeedPositionsAsync();
            await SeedTransactionsAsync();
            await SeedMarketDataAsync();
            await SeedPerformanceMetricsAsync();
            await SeedTradeOrdersAsync();
        }

        private async Task SeedUsersAsync()
        {
            var users = new[]
            {
                new User
                {
                    Email = "marie.tremblay@cdpq.com",
                    FirstName = "Marie",
                    LastName = "Tremblay",
                    Role = UserRole.PortfolioManager,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddMonths(-1)
                },
                new User
                {
                    Email = "john.smith@cdpq.com",
                    FirstName = "John",
                    LastName = "Smith",
                    Role = UserRole.Analyst,
                    CreatedAt = DateTime.UtcNow.AddMonths(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new User
                {
                    Email = "pierre.lavoie@cdpq.com",
                    FirstName = "Pierre",
                    LastName = "Lavoie",
                    Role = UserRole.PortfolioManager,
                    CreatedAt = DateTime.UtcNow.AddMonths(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new User
                {
                    Email = "sarah.johnson@cdpq.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Role = UserRole.RiskManager,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new User
                {
                    Email = "admin@cdpq.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.Administrator,
                    CreatedAt = DateTime.UtcNow.AddYears(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSecuritiesAsync()
        {
            var securities = new[]
            {
                // Canadian Banks
                new Security
                {
                    Symbol = "RY.TO",
                    Name = "Royal Bank of Canada",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Financial Services",
                    Industry = "Banks - Diversified",
                    CurrentPrice = 142.50m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "TD.TO",
                    Name = "Toronto-Dominion Bank",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Financial Services",
                    Industry = "Banks - Diversified",
                    CurrentPrice = 87.45m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "BNS.TO",
                    Name = "Bank of Nova Scotia",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Financial Services",
                    Industry = "Banks - Diversified",
                    CurrentPrice = 68.32m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "BMO.TO",
                    Name = "Bank of Montreal",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Financial Services",
                    Industry = "Banks - Diversified",
                    CurrentPrice = 129.85m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // Canadian Tech Stocks
                new Security
                {
                    Symbol = "SHOP.TO",
                    Name = "Shopify Inc.",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Technology",
                    Industry = "Software - Application",
                    CurrentPrice = 95.75m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "BB.TO",
                    Name = "BlackBerry Limited",
                    Type = SecurityType.Stock,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Technology",
                    Industry = "Software - Infrastructure",
                    CurrentPrice = 5.42m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // US Tech Giants
                new Security
                {
                    Symbol = "AAPL",
                    Name = "Apple Inc.",
                    Type = SecurityType.Stock,
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Sector = "Technology",
                    Industry = "Consumer Electronics",
                    CurrentPrice = 195.89m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "MSFT",
                    Name = "Microsoft Corporation",
                    Type = SecurityType.Stock,
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Sector = "Technology",
                    Industry = "Software - Infrastructure",
                    CurrentPrice = 418.72m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "GOOGL",
                    Name = "Alphabet Inc. Class A",
                    Type = SecurityType.Stock,
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Sector = "Technology",
                    Industry = "Internet Content & Information",
                    CurrentPrice = 171.23m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // ETFs
                new Security
                {
                    Symbol = "VTI",
                    Name = "Vanguard Total Stock Market ETF",
                    Type = SecurityType.ETF,
                    Exchange = "NYSE",
                    Currency = "USD",
                    Sector = "Diversified",
                    Industry = "Exchange Traded Fund",
                    CurrentPrice = 267.45m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Security
                {
                    Symbol = "VFV.TO",
                    Name = "Vanguard S&P 500 Index ETF",
                    Type = SecurityType.ETF,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Diversified",
                    Industry = "Exchange Traded Fund",
                    CurrentPrice = 108.32m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // Bonds
                new Security
                {
                    Symbol = "GOC.10Y",
                    Name = "Government of Canada 10 Year Bond",
                    Type = SecurityType.Bond,
                    Exchange = "TSX",
                    Currency = "CAD",
                    Sector = "Government",
                    Industry = "Government Bond",
                    CurrentPrice = 98.75m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.Securities.AddRangeAsync(securities);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPortfoliosAsync()
        {
            var portfolios = new[]
            {
                new Portfolio
                {
                    Name = "Canadian Equity Growth Fund",
                    Description = "Focused on Canadian growth stocks with emphasis on technology and financial services",
                    Type = PortfolioType.Growth,
                    ManagerId = 1, // Marie Tremblay
                    CashBalance = 2500000.00m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Portfolio
                {
                    Name = "Balanced Income Portfolio",
                    Description = "Diversified portfolio balancing growth and income generation",
                    Type = PortfolioType.Balanced,
                    ManagerId = 3, // Pierre Lavoie
                    CashBalance = 1850000.00m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-24),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Portfolio
                {
                    Name = "Fixed Income Conservative Fund",
                    Description = "Conservative portfolio focused on government and corporate bonds",
                    Type = PortfolioType.Conservative,
                    ManagerId = 1, // Marie Tremblay
                    CashBalance = 3200000.00m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Portfolio
                {
                    Name = "US Technology Innovation Fund",
                    Description = "High-growth technology portfolio targeting US market leaders",
                    Type = PortfolioType.Growth,
                    ManagerId = 3, // Pierre Lavoie
                    CashBalance = 4750000.00m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.Portfolios.AddRangeAsync(portfolios);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPositionsAsync()
        {
            var positions = new[]
            {
                // Canadian Equity Growth Fund positions
                new Position
                {
                    PortfolioId = 1,
                    SecurityId = 1, // RY.TO
                    Quantity = 125000,
                    AveragePrice = 138.45m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-15),
                    LastTransactionDate = DateTime.UtcNow.AddMonths(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 1,
                    SecurityId = 2, // TD.TO
                    Quantity = 95000,
                    AveragePrice = 85.20m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-12),
                    LastTransactionDate = DateTime.UtcNow.AddMonths(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 1,
                    SecurityId = 5, // SHOP.TO
                    Quantity = 75000,
                    AveragePrice = 89.32m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-8),
                    LastTransactionDate = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // Balanced Income Portfolio positions
                new Position
                {
                    PortfolioId = 2,
                    SecurityId = 3, // BNS.TO
                    Quantity = 180000,
                    AveragePrice = 66.75m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-18),
                    LastTransactionDate = DateTime.UtcNow.AddMonths(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 2,
                    SecurityId = 11, // VFV.TO
                    Quantity = 85000,
                    AveragePrice = 105.20m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-10),
                    LastTransactionDate = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 2,
                    SecurityId = 12, // GOC.10Y
                    Quantity = 250000,
                    AveragePrice = 99.85m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-24),
                    LastTransactionDate = DateTime.UtcNow.AddMonths(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // US Technology Innovation Fund positions
                new Position
                {
                    PortfolioId = 4,
                    SecurityId = 7, // AAPL
                    Quantity = 65000,
                    AveragePrice = 182.45m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-10),
                    LastTransactionDate = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 4,
                    SecurityId = 8, // MSFT
                    Quantity = 45000,
                    AveragePrice = 385.20m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-12),
                    LastTransactionDate = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Position
                {
                    PortfolioId = 4,
                    SecurityId = 9, // GOOGL
                    Quantity = 35000,
                    AveragePrice = 158.75m,
                    FirstPurchaseDate = DateTime.UtcNow.AddMonths(-6),
                    LastTransactionDate = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.Positions.AddRangeAsync(positions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTransactionsAsync()
        {
            var transactions = new List<Transaction>();
            var random = new Random(42); // Fixed seed for reproducible data

            // Generate 6 months of transaction history
            var startDate = DateTime.UtcNow.AddMonths(-6);
            var endDate = DateTime.UtcNow;

            // Sample transactions for different portfolios
            var sampleTransactions = new[]
            {
                new { PortfolioId = 1, SecurityId = 1, Type = TransactionType.Buy, Quantity = 50000m, Price = 140.25m },
                new { PortfolioId = 1, SecurityId = 1, Type = TransactionType.Buy, Quantity = 75000m, Price = 136.85m },
                new { PortfolioId = 1, SecurityId = 2, Type = TransactionType.Buy, Quantity = 45000m, Price = 83.50m },
                new { PortfolioId = 1, SecurityId = 2, Type = TransactionType.Buy, Quantity = 50000m, Price = 86.75m },
                new { PortfolioId = 1, SecurityId = 5, Type = TransactionType.Buy, Quantity = 75000m, Price = 89.32m },
                new { PortfolioId = 2, SecurityId = 3, Type = TransactionType.Buy, Quantity = 100000m, Price = 65.20m },
                new { PortfolioId = 2, SecurityId = 3, Type = TransactionType.Buy, Quantity = 80000m, Price = 68.45m },
                new { PortfolioId = 2, SecurityId = 11, Type = TransactionType.Buy, Quantity = 85000m, Price = 105.20m },
                new { PortfolioId = 4, SecurityId = 7, Type = TransactionType.Buy, Quantity = 35000m, Price = 175.20m },
                new { PortfolioId = 4, SecurityId = 7, Type = TransactionType.Buy, Quantity = 30000m, Price = 189.85m },
                new { PortfolioId = 4, SecurityId = 8, Type = TransactionType.Buy, Quantity = 25000m, Price = 365.40m },
                new { PortfolioId = 4, SecurityId = 8, Type = TransactionType.Buy, Quantity = 20000m, Price = 405.75m }
            };

            for (int i = 0; i < sampleTransactions.Length; i++)
            {
                var sample = sampleTransactions[i];
                var transactionDate = startDate.AddDays(random.Next(0, (endDate - startDate).Days));
                var commission = sample.Quantity * sample.Price * 0.0015m; // 15 basis points
                var tax = 0m; // No tax on purchases

                var netAmount = sample.Type == TransactionType.Buy 
                    ? -(sample.Quantity * sample.Price + commission)
                    : sample.Quantity * sample.Price - commission;

                transactions.Add(new Transaction
                {
                    PortfolioId = sample.PortfolioId,
                    SecurityId = sample.SecurityId,
                    Type = sample.Type,
                    Quantity = sample.Quantity,
                    Price = sample.Price,
                    Commission = commission,
                    Tax = tax,
                    NetAmount = netAmount,
                    TransactionDate = transactionDate,
                    Notes = $"Institutional {sample.Type.ToString().ToLower()} order - {sample.SecurityId}",
                    CreatedAt = transactionDate.AddMinutes(random.Next(1, 30))
                });
            }

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedMarketDataAsync()
        {
            var marketDataList = new List<MarketData>();
            var random = new Random(42);

            // Generate 30 days of market data for each security
            var securities = await _context.Securities.ToListAsync();
            var startDate = DateTime.UtcNow.AddDays(-30);

            foreach (var security in securities)
            {
                var basePrice = security.CurrentPrice;
                var currentPrice = basePrice * 0.95m; // Start 5% below current price

                for (int day = 0; day < 30; day++)
                {
                    var date = startDate.AddDays(day);
                    
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Generate realistic OHLCV data
                    var dailyVolatility = 0.02m; // 2% daily volatility
                    var priceChange = (decimal)(random.NextDouble() - 0.5) * dailyVolatility * currentPrice;
                    
                    var open = currentPrice;
                    var high = open + Math.Abs(priceChange) * (decimal)random.NextDouble();
                    var low = open - Math.Abs(priceChange) * (decimal)random.NextDouble();
                    var close = open + priceChange;
                    
                    // Ensure high >= max(open, close) and low <= min(open, close)
                    high = Math.Max(high, Math.Max(open, close));
                    low = Math.Min(low, Math.Min(open, close));

                    var volume = (long)(random.Next(100000, 2000000));

                    marketDataList.Add(new MarketData
                    {
                        SecurityId = security.Id,
                        Date = date.Date,
                        Open = Math.Round(open, 2),
                        High = Math.Round(high, 2),
                        Low = Math.Round(low, 2),
                        Close = Math.Round(close, 2),
                        Volume = volume,
                        AdjustedClose = Math.Round(close, 2), // Same as close for simplicity
                        CreatedAt = date.AddHours(16) // Market close time
                    });

                    currentPrice = close;
                }

                // Update security's current price to the last close price
                security.CurrentPrice = currentPrice;
                security.UpdatedAt = DateTime.UtcNow;
            }

            await _context.MarketData.AddRangeAsync(marketDataList);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPerformanceMetricsAsync()
        {
            var performanceMetrics = new List<PerformanceMetric>();
            var random = new Random(42);

            var portfolios = await _context.Portfolios.ToListAsync();

            foreach (var portfolio in portfolios)
            {
                // Generate monthly performance metrics for the last 12 months
                for (int month = 0; month < 12; month++)
                {
                    var calculationDate = DateTime.UtcNow.AddMonths(-month).Date;
                    calculationDate = new DateTime(calculationDate.Year, calculationDate.Month, DateTime.DaysInMonth(calculationDate.Year, calculationDate.Month));

                    // Generate realistic performance metrics based on portfolio type
                    var baseReturn = portfolio.Type switch
                    {
                        PortfolioType.Growth => 0.08m, // 8% annual return
                        PortfolioType.Balanced => 0.06m, // 6% annual return
                        PortfolioType.Conservative => 0.04m, // 4% annual return
                        _ => 0.05m
                    };

                    var monthlyReturn = (baseReturn / 12) + ((decimal)(random.NextDouble() - 0.5) * 0.04m);
                    var ytdReturn = monthlyReturn * (12 - month);
                    var totalReturn = baseReturn + ((decimal)(random.NextDouble() - 0.5) * 0.15m);

                    var volatility = portfolio.Type switch
                    {
                        PortfolioType.Growth => 0.15m + ((decimal)random.NextDouble() * 0.05m),
                        PortfolioType.Balanced => 0.10m + ((decimal)random.NextDouble() * 0.03m),
                        PortfolioType.Conservative => 0.06m + ((decimal)random.NextDouble() * 0.02m),
                        _ => 0.10m
                    };

                    var sharpeRatio = volatility > 0 ? (totalReturn - 0.02m) / volatility : 0; // Risk-free rate = 2%
                    var maxDrawdown = -Math.Abs((decimal)random.NextDouble() * 0.08m);
                    var beta = 0.8m + ((decimal)random.NextDouble() * 0.4m); // Beta between 0.8 and 1.2
                    var alpha = totalReturn - (0.05m * beta); // Market return = 5%

                    performanceMetrics.Add(new PerformanceMetric
                    {
                        PortfolioId = portfolio.Id,
                        CalculationDate = calculationDate,
                        MonthlyReturn = Math.Round(monthlyReturn, 4),
                        YtdReturn = Math.Round(ytdReturn, 4),
                        TotalReturn = Math.Round(totalReturn, 4),
                        SharpeRatio = Math.Round(sharpeRatio, 4),
                        Volatility = Math.Round(volatility, 4),
                        MaxDrawdown = Math.Round(maxDrawdown, 4),
                        Alpha = Math.Round(alpha, 4),
                        Beta = Math.Round(beta, 4),
                        CreatedAt = calculationDate.AddDays(1)
                    });
                }
            }

            await _context.PerformanceMetrics.AddRangeAsync(performanceMetrics);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTradeOrdersAsync()
        {
            var tradeOrders = new List<TradeOrder>();
            var random = new Random(42);

            var portfolios = await _context.Portfolios.ToListAsync();
            var securities = await _context.Securities.ToListAsync();

            // Generate mix of pending, filled, and cancelled orders
            var orderStatuses = new[] { OrderStatus.Pending, OrderStatus.Filled, OrderStatus.Cancelled, OrderStatus.PartiallyFilled };
            var orderTypes = new[] { OrderType.Market, OrderType.Limit, OrderType.Stop, OrderType.StopLimit };
            var orderSides = new[] { OrderSide.Buy, OrderSide.Sell };

            for (int i = 0; i < 50; i++)
            {
                var portfolio = portfolios[random.Next(portfolios.Count)];
                var security = securities[random.Next(securities.Count)];
                var orderType = orderTypes[random.Next(orderTypes.Length)];
                var orderSide = orderSides[random.Next(orderSides.Length)];
                var status = orderStatuses[random.Next(orderStatuses.Length)];

                var quantity = random.Next(1000, 50000);
                var limitPrice = orderType == OrderType.Limit || orderType == OrderType.StopLimit 
                    ? security.CurrentPrice * (decimal)(0.95 + random.NextDouble() * 0.1) 
                    : (decimal?)null;
                var stopPrice = orderType == OrderType.Stop || orderType == OrderType.StopLimit 
                    ? security.CurrentPrice * (decimal)(0.90 + random.NextDouble() * 0.2) 
                    : (decimal?)null;

                var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));
                var expirationDate = orderDate.AddDays(random.Next(1, 30));

                decimal quantityFilled = 0;
                decimal? averageExecutionPrice = null;
                DateTime? filledDate = null;

                if (status == OrderStatus.Filled)
                {
                    quantityFilled = quantity;
                    averageExecutionPrice = security.CurrentPrice * (decimal)(0.98 + random.NextDouble() * 0.04);
                    filledDate = orderDate.AddHours(random.Next(1, 8));
                }
                else if (status == OrderStatus.PartiallyFilled)
                {
                    quantityFilled = quantity * (decimal)(0.3 + random.NextDouble() * 0.4);
                    averageExecutionPrice = security.CurrentPrice * (decimal)(0.98 + random.NextDouble() * 0.04);
                    filledDate = orderDate.AddHours(random.Next(1, 8));
                }

                tradeOrders.Add(new TradeOrder
                {
                    PortfolioId = portfolio.Id,
                    SecurityId = security.Id,
                    Type = orderType,
                    Side = orderSide,
                    Quantity = quantity,
                    LimitPrice = limitPrice,
                    StopPrice = stopPrice,
                    Status = status,
                    QuantityFilled = quantityFilled,
                    AverageExecutionPrice = averageExecutionPrice ?? 0,
                    OrderDate = orderDate,
                    ExpirationDate = expirationDate,
                    FilledDate = filledDate,
                    Notes = $"{orderType} {orderSide} order for {security.Symbol}",
                    CreatedAt = orderDate,
                    UpdatedAt = filledDate ?? orderDate
                });
            }

            await _context.TradeOrders.AddRangeAsync(tradeOrders);
            await _context.SaveChangesAsync();
        }
    }
}