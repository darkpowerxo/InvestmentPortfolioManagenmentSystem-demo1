using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InvestmentPortfolioManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Securities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SecurityType = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "CAD"),
                    Exchange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Canada"),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CouponRate = table.Column<double>(type: "float", nullable: true),
                    MaturityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreditRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrikePrice = table.Column<double>(type: "float", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OptionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnderlyingSecurityId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Securities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Securities_Securities_UnderlyingSecurityId",
                        column: x => x.UnderlyingSecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HighPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LowPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AdjustedClosePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "CAD"),
                    SMA20 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SMA50 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SMA200 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    EMA12 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    EMA26 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RSI = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Volatility = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketData_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PortfolioType = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    BaseCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "CAD"),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    InitialValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InceptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TargetEquityAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetBondAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetAlternativeAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetCashAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    WeeklyReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    MonthlyReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    YearToDateReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    OneYearReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    ThreeYearReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    FiveYearReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    SinceInceptionReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    Volatility = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    SortinoRatio = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    Beta = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    Alpha = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    MaxDrawdown = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    VaR95 = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    VaR99 = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    BenchmarkSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BenchmarkReturn = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    TrackingError = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    InformationRatio = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    EquityAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BondAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AlternativeAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CashAllocation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MarketValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnrealizedGainLoss = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnrealizedGainLossPercent = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    FirstPurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AllocationPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    OrderSide = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LimitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    StopPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuantityFilled = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    AverageExecutionPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeOrders_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeOrders_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOrders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherFees = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "CAD"),
                    ExchangeRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Securities",
                columns: new[] { "Id", "Country", "CouponRate", "CreatedAt", "CreditRating", "Currency", "Exchange", "ExpirationDate", "Industry", "IsActive", "MaturityDate", "Name", "OptionType", "Region", "Sector", "SecurityType", "StrikePrice", "Symbol", "UnderlyingSecurityId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Canada", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4017), null, "CAD", "TSX", null, "Software", true, null, "Shopify Inc.", null, "North America", "Technology", 1, null, "SHOP.TO", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(2737) },
                    { 2, "Canada", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4314), null, "CAD", "TSX", null, "Banking", true, null, "Royal Bank of Canada", null, "North America", "Financial Services", 1, null, "RY.TO", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4241) },
                    { 3, "Canada", 3.25, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4888), "AAA", "CAD", "TSX", null, "", true, new DateTime(2035, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4558), "Government of Canada 10-Year Bond", null, "North America", "Government", 2, null, "CAN.GOVT.10Y", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4316) },
                    { 4, "United States", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4893), null, "USD", "NYSE", null, "Investment Fund", true, null, "Vanguard Total Stock Market ETF", null, "North America", "Diversified", 3, null, "VTI", null, new DateTime(2025, 9, 26, 16, 43, 52, 792, DateTimeKind.Utc).AddTicks(4890) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "PreferredLanguage", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 26, 16, 43, 52, 791, DateTimeKind.Utc).AddTicks(6808), "jean.tremblay@cdpq.com", "Jean", true, null, "Tremblay", "$2a$11$dummy.hash.for.demo.purposes.only", "fr", "PortfolioManager" },
                    { 2, new DateTime(2025, 9, 26, 16, 43, 52, 791, DateTimeKind.Utc).AddTicks(7054), "sarah.johnson@cdpq.com", "Sarah", true, null, "Johnson", "$2a$11$dummy.hash.for.demo.purposes.only", "en", "Analyst" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketData_Date",
                table: "MarketData",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_MarketData_SecurityId_Date",
                table: "MarketData",
                columns: new[] { "SecurityId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_PortfolioId_CalculationDate",
                table: "PerformanceMetrics",
                columns: new[] { "PortfolioId", "CalculationDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_ManagerId",
                table: "Portfolios",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PortfolioId_SecurityId",
                table: "Positions",
                columns: new[] { "PortfolioId", "SecurityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_SecurityId",
                table: "Positions",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_Securities_Symbol_Exchange",
                table: "Securities",
                columns: new[] { "Symbol", "Exchange" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Securities_UnderlyingSecurityId",
                table: "Securities",
                column: "UnderlyingSecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrders_CreatedByUserId",
                table: "TradeOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrders_OrderDate",
                table: "TradeOrders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrders_PortfolioId",
                table: "TradeOrders",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrders_SecurityId",
                table: "TradeOrders",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrders_Status",
                table: "TradeOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PortfolioId",
                table: "Transactions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SecurityId",
                table: "Transactions",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SettlementDate",
                table: "Transactions",
                column: "SettlementDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketData");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "TradeOrders");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "Securities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
