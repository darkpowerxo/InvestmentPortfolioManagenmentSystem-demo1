using InvestmentPortfolioManager.Application.Services;
using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InvestmentPortfolioManager.Tests.Application.Services;

/// <summary>
/// Tests unitaires pour le service de traitement des transactions
/// Unit tests for transaction processing service
/// </summary>
public class TransactionProcessingServiceTests
{
    private readonly Mock<ILogger<TransactionProcessingService>> _mockLogger;
    private readonly Mock<IMarketDataService> _mockMarketDataService;
    private readonly TransactionProcessingService _service;

    public TransactionProcessingServiceTests()
    {
        _mockLogger = new Mock<ILogger<TransactionProcessingService>>();
        _mockMarketDataService = new Mock<IMarketDataService>();
        _service = new TransactionProcessingService(_mockLogger.Object, _mockMarketDataService.Object);
    }

    #region Tests de création d'ordres / Order Creation Tests

    [Fact]
    public async Task CreateTradeOrderAsync_ValidBuyOrder_ShouldCreateSuccessfully()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var request = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100,
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("RY.TO"))
            .ReturnsAsync(150.50m);

        // Act
        var result = await _service.CreateTradeOrderAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal("Order created successfully", result.StatusMessage);
        Assert.True(result.EstimatedCost > 0);
    }

    [Fact]
    public async Task CreateTradeOrderAsync_ValidSellOrder_ShouldCreateSuccessfully()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var request = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "TD.TO",
            TransactionType = TransactionType.Sell,
            OrderType = OrderType.Limit,
            Quantity = 50,
            LimitPrice = 95.00m
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("TD.TO"))
            .ReturnsAsync(94.50m);

        // Act
        var result = await _service.CreateTradeOrderAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal("Order created successfully", result.StatusMessage);
    }

    [Fact]
    public async Task CreateTradeOrderAsync_InvalidSymbol_ShouldRejectOrder()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "INVALID.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("INVALID.TO"))
            .ReturnsAsync((decimal?)null);

        // Act
        var result = await _service.CreateTradeOrderAsync(request);

        // Assert
        Assert.Equal(Guid.Empty, result.OrderId);
        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public async Task CreateTradeOrderAsync_ZeroQuantity_ShouldRejectOrder()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 0
        };

        // Act
        var result = await _service.CreateTradeOrderAsync(request);

        // Assert
        Assert.Equal(Guid.Empty, result.OrderId);
        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains("positive", result.StatusMessage);
    }

    #endregion

    #region Tests de validation d'ordres / Order Validation Tests

    [Fact]
    public async Task ValidateOrderAsync_ValidOrder_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "SHOP.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 25
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("SHOP.TO"))
            .ReturnsAsync(1200.00m);

        // Act
        var result = await _service.ValidateOrderAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.True(result.RequiredCash > 0);
    }

    [Fact]
    public async Task ValidateOrderAsync_LimitOrderWithoutPrice_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Limit,
            Quantity = 100
            // LimitPrice not set
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("RY.TO"))
            .ReturnsAsync(150.50m);

        // Act
        var result = await _service.ValidateOrderAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Limit price"));
    }

    [Fact]
    public async Task ValidateOrderAsync_EmptyPortfolioId_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.Empty,
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100
        };

        // Act
        var result = await _service.ValidateOrderAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Portfolio ID"));
    }

    #endregion

    #region Tests d'exécution d'ordres / Order Execution Tests

    [Fact]
    public async Task ExecuteTradeOrderAsync_PendingOrder_ShouldExecuteSuccessfully()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var createRequest = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "CNR.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 50
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("CNR.TO"))
            .ReturnsAsync(175.25m);

        // Créer l'ordre d'abord / Create order first
        var orderResult = await _service.CreateTradeOrderAsync(createRequest);
        Assert.Equal(OrderStatus.Pending, orderResult.Status);

        // Act - Exécuter l'ordre / Execute order
        var executionResult = await _service.ExecuteTradeOrderAsync(orderResult.OrderId);

        // Assert
        Assert.NotEqual(Guid.Empty, executionResult.TransactionId);
        Assert.Equal(orderResult.OrderId, executionResult.OrderId);
        Assert.Equal(50, executionResult.ExecutedQuantity);
        Assert.True(executionResult.ExecutedPrice > 0);
        Assert.True(executionResult.TotalValue > 0);
        Assert.Equal(TransactionStatus.Pending, executionResult.Status);
    }

    [Fact]
    public async Task ExecuteTradeOrderAsync_NonExistentOrder_ShouldThrowException()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ExecuteTradeOrderAsync(nonExistentOrderId));
        
        Assert.Contains("not found", exception.Message);
    }

    #endregion

    #region Tests d'annulation d'ordres / Order Cancellation Tests

    [Fact]
    public async Task CancelOrderAsync_PendingOrder_ShouldCancelSuccessfully()
    {
        // Arrange
        var createRequest = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "ENB.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 75
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("ENB.TO"))
            .ReturnsAsync(55.50m);

        var orderResult = await _service.CreateTradeOrderAsync(createRequest);

        // Act
        var cancelResult = await _service.CancelOrderAsync(orderResult.OrderId);

        // Assert
        Assert.True(cancelResult);
    }

    [Fact]
    public async Task CancelOrderAsync_NonExistentOrder_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var result = await _service.CancelOrderAsync(nonExistentOrderId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Tests de calcul des coûts / Cost Calculation Tests

    [Fact]
    public async Task CalculateTransactionCostAsync_EquityTrade_ShouldCalculateCorrectly()
    {
        // Arrange
        var symbol = "RY.TO";
        var quantity = 100m;
        var price = 150.00m;
        var transactionType = TransactionType.Buy;

        // Act
        var result = await _service.CalculateTransactionCostAsync(symbol, quantity, price, transactionType);

        // Assert
        Assert.True(result.Commission >= 9.95m); // Minimum commission
        Assert.True(result.TotalCost > result.Commission);
        Assert.True(result.CostBasisPoints > 0);
        Assert.Equal("CAD", result.Currency);
    }

    [Fact]
    public async Task CalculateTransactionCostAsync_LargeTrade_ShouldHaveHigherCosts()
    {
        // Arrange
        var symbol = "TD.TO";
        var smallQuantity = 10m;
        var largeQuantity = 1000m;
        var price = 95.00m;
        var transactionType = TransactionType.Buy;

        // Act
        var smallTradeCost = await _service.CalculateTransactionCostAsync(symbol, smallQuantity, price, transactionType);
        var largeTradeCost = await _service.CalculateTransactionCostAsync(symbol, largeQuantity, price, transactionType);

        // Assert
        Assert.True(largeTradeCost.TotalCost > smallTradeCost.TotalCost);
        Assert.True(largeTradeCost.MarketImpact > smallTradeCost.MarketImpact);
    }

    #endregion

    #region Tests de vérification de conformité / Compliance Check Tests

    [Fact]
    public async Task CheckComplianceAsync_NormalTrade_ShouldPassCompliance()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "SU.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("SU.TO"))
            .ReturnsAsync(45.00m);

        // Act
        var result = await _service.CheckComplianceAsync(request);

        // Assert
        Assert.True(result.IsCompliant);
        Assert.Empty(result.Violations);
        Assert.True(result.MaxPositionSize > 0);
        Assert.True(result.CurrentExposure >= 0);
        Assert.NotNull(result.ComplianceOfficer);
    }

    [Fact]
    public async Task CheckComplianceAsync_LargeTrade_ShouldRequireApproval()
    {
        // Arrange
        var request = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "SHOP.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100 // At $1200 per share = $120,000
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("SHOP.TO"))
            .ReturnsAsync(1200.00m);

        // Act
        var result = await _service.CheckComplianceAsync(request);

        // Assert
        Assert.True(result.RequiresApproval);
        Assert.Contains(result.Warnings, w => w.Contains("approval"));
    }

    #endregion

    #region Tests de règlement / Settlement Tests

    [Fact]
    public async Task ProcessSettlementAsync_ValidTransaction_ShouldProcessSettlement()
    {
        // Arrange - Créer et exécuter un ordre d'abord / Create and execute order first
        var createRequest = new CreateTradeOrderDto
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 50
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("RY.TO"))
            .ReturnsAsync(150.00m);

        var orderResult = await _service.CreateTradeOrderAsync(createRequest);
        var executionResult = await _service.ExecuteTradeOrderAsync(orderResult.OrderId);

        // Act
        var settlementResult = await _service.ProcessSettlementAsync(executionResult.TransactionId);

        // Assert
        Assert.Equal(executionResult.TransactionId, settlementResult.TransactionId);
        Assert.NotNull(settlementResult.SettlementVenue);
        Assert.NotEqual(0, settlementResult.CashMovement);
        Assert.NotEqual(0, settlementResult.SecurityMovement);
    }

    [Fact]
    public async Task ProcessSettlementAsync_NonExistentTransaction_ShouldThrowException()
    {
        // Arrange
        var nonExistentTransactionId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ProcessSettlementAsync(nonExistentTransactionId));
        
        Assert.Contains("not found", exception.Message);
    }

    #endregion

    #region Tests d'impact sur le portefeuille / Portfolio Impact Tests

    [Fact]
    public async Task CalculatePortfolioImpactAsync_BuyOrder_ShouldShowNegativeCashImpact()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var request = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "TD.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("TD.TO"))
            .ReturnsAsync(95.00m);

        // Act
        var result = await _service.CalculatePortfolioImpactAsync(portfolioId, request);

        // Assert
        Assert.Equal(portfolioId, result.PortfolioId);
        Assert.True(result.CashImpact < 0); // Negative for buy orders
        Assert.NotNull(result.SectorExposureChanges);
        Assert.NotNull(result.RegionExposureChanges);
        Assert.True(result.RiskImpact >= 0);
    }

    [Fact]
    public async Task CalculatePortfolioImpactAsync_SellOrder_ShouldShowPositiveCashImpact()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var request = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "CNR.TO",
            TransactionType = TransactionType.Sell,
            OrderType = OrderType.Market,
            Quantity = 50
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("CNR.TO"))
            .ReturnsAsync(175.00m);

        // Act
        var result = await _service.CalculatePortfolioImpactAsync(portfolioId, request);

        // Assert
        Assert.Equal(portfolioId, result.PortfolioId);
        Assert.True(result.CashImpact > 0); // Positive for sell orders
        Assert.NotEmpty(result.SectorExposureChanges);
        Assert.NotEmpty(result.RegionExposureChanges);
    }

    #endregion

    #region Tests de récupération des données / Data Retrieval Tests

    [Fact]
    public async Task GetPendingOrdersAsync_WithPendingOrders_ShouldReturnOrders()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var createRequest = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "ENB.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 25
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("ENB.TO"))
            .ReturnsAsync(55.00m);

        await _service.CreateTradeOrderAsync(createRequest);

        // Act
        var pendingOrders = await _service.GetPendingOrdersAsync(portfolioId);

        // Assert
        Assert.NotEmpty(pendingOrders);
        Assert.All(pendingOrders, order => Assert.Equal(OrderStatus.Pending, order.Status));
        Assert.All(pendingOrders, order => Assert.Equal(portfolioId, order.PortfolioId));
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_WithDateRange_ShouldFilterCorrectly()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var createRequest = new CreateTradeOrderDto
        {
            PortfolioId = portfolioId,
            Symbol = "RY.TO",
            TransactionType = TransactionType.Buy,
            OrderType = OrderType.Market,
            Quantity = 100
        };

        _mockMarketDataService.Setup(x => x.GetCurrentPriceAsync("RY.TO"))
            .ReturnsAsync(150.00m);

        var orderResult = await _service.CreateTradeOrderAsync(createRequest);
        await _service.ExecuteTradeOrderAsync(orderResult.OrderId);

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var transactions = await _service.GetTransactionHistoryAsync(portfolioId, startDate, endDate);

        // Assert
        Assert.NotEmpty(transactions);
        Assert.All(transactions, t => Assert.Equal(portfolioId, t.PortfolioId));
        Assert.All(transactions, t => Assert.True(t.TransactionDate >= startDate && t.TransactionDate <= endDate));
    }

    #endregion
}