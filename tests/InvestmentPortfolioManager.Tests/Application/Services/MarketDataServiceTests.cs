using InvestmentPortfolioManager.Application.Services;
using InvestmentPortfolioManager.Application.Services.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InvestmentPortfolioManager.Tests.Application.Services;

/// <summary>
/// Tests unitaires pour le service de données de marché / Unit tests for market data service
/// </summary>
public class MarketDataServiceTests
{
    private readonly MarketDataService _service;
    private readonly Mock<ILogger<MarketDataService>> _mockLogger;

    public MarketDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<MarketDataService>>();
        _service = new MarketDataService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_ValidSymbol_ShouldReturnPrice()
    {
        // Arrange
        var symbol = "RY.TO"; // Royal Bank of Canada

        // Act
        var result = await _service.GetCurrentPriceAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.True(result > 0);
        Assert.True(result < 1000m); // Prix raisonnable / Reasonable price
    }

    [Fact]
    public async Task GetCurrentPriceAsync_InvalidSymbol_ShouldReturnNull()
    {
        // Arrange
        var symbol = "INVALID.SYMBOL";

        // Act
        var result = await _service.GetCurrentPriceAsync(symbol);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRealTimeDataAsync_ValidSymbol_ShouldReturnCompleteData()
    {
        // Arrange
        var symbol = "TD.TO"; // Toronto-Dominion Bank

        // Act
        var result = await _service.GetRealTimeDataAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.True(result.Price > 0);
        Assert.True(result.Volume > 0);
        Assert.True(result.Bid > 0);
        Assert.True(result.Ask > 0);
        Assert.True(result.Ask >= result.Bid); // Ask >= Bid
        Assert.Equal("CAD", result.Currency);
        Assert.True(result.LastUpdated <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetHistoricalDataAsync_ValidDateRange_ShouldReturnOrderedData()
    {
        // Arrange
        var symbol = "SHOP.TO";
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today.AddDays(-1);

        // Act
        var result = await _service.GetHistoricalDataAsync(symbol, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        var dataList = result.ToList();
        Assert.NotEmpty(dataList);
        
        // Vérifier l'ordre chronologique / Verify chronological order
        for (int i = 1; i < dataList.Count; i++)
        {
            Assert.True(dataList[i].Date >= dataList[i - 1].Date);
        }
        
        // Vérifier que toutes les données sont dans la plage / Verify all data is within range
        Assert.All(dataList, data => 
        {
            Assert.True(data.Date >= startDate);
            Assert.True(data.Date <= endDate);
            Assert.True(data.Price > 0);
            Assert.True(data.Volume > 0);
        });
    }

    [Fact]
    public async Task GetOhlcvDataAsync_ValidParameters_ShouldReturnOhlcvData()
    {
        // Arrange
        var symbol = "CNR.TO";
        var startDate = DateTime.Today.AddDays(-10);
        var endDate = DateTime.Today.AddDays(-1);
        var interval = TimeInterval.Daily;

        // Act
        var result = await _service.GetOhlcvDataAsync(symbol, startDate, endDate, interval);

        // Assert
        Assert.NotNull(result);
        var ohlcvList = result.ToList();
        Assert.NotEmpty(ohlcvList);
        
        Assert.All(ohlcvList, ohlcv =>
        {
            Assert.True(ohlcv.High >= ohlcv.Low);
            Assert.True(ohlcv.High >= ohlcv.Open);
            Assert.True(ohlcv.High >= ohlcv.Close);
            Assert.True(ohlcv.Low <= ohlcv.Open);
            Assert.True(ohlcv.Low <= ohlcv.Close);
            Assert.True(ohlcv.Volume > 0);
        });
    }

    [Fact]
    public async Task GetTechnicalIndicatorsAsync_ValidSymbol_ShouldReturnIndicators()
    {
        // Arrange
        var symbol = "ENB.TO";
        var period = 20;

        // Act
        var result = await _service.GetTechnicalIndicatorsAsync(symbol, period);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.True(result.SimpleMovingAverage > 0);
        Assert.True(result.ExponentialMovingAverage > 0);
        Assert.True(result.RelativeStrengthIndex >= 0 && result.RelativeStrengthIndex <= 100);
        Assert.True(result.BollingerBandUpper >= result.BollingerBandMiddle);
        Assert.True(result.BollingerBandMiddle >= result.BollingerBandLower);
        Assert.True(result.Volatility >= 0);
        Assert.True(result.CalculatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task StartRealTimeFeedAsync_MultipleSymbols_ShouldInitializeSuccessfully()
    {
        // Arrange
        var symbols = new[] { "RY.TO", "TD.TO", "SHOP.TO" };

        // Act
        await _service.StartRealTimeFeedAsync(symbols);

        // Assert - Vérifier que les prix sont disponibles / Verify prices are available
        foreach (var symbol in symbols)
        {
            var price = await _service.GetCurrentPriceAsync(symbol);
            Assert.NotNull(price);
            Assert.True(price > 0);
        }
    }

    [Fact]
    public async Task StopRealTimeFeedAsync_ShouldStopSuccessfully()
    {
        // Arrange
        var symbols = new[] { "RY.TO" };
        await _service.StartRealTimeFeedAsync(symbols);

        // Act
        await _service.StopRealTimeFeedAsync();

        // Assert - Le test passe si aucune exception n'est levée / Test passes if no exception is thrown
        Assert.True(true);
    }

    [Fact]
    public async Task ValidateDataQualityAsync_ValidSymbol_ShouldReturnQualityReport()
    {
        // Arrange
        var symbol = "BAM.A.TO";
        var date = DateTime.Today.AddDays(-1);

        // Act
        var result = await _service.ValidateDataQualityAsync(symbol, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.Equal(date, result.Date);
        Assert.True(result.CompletionRate >= 0 && result.CompletionRate <= 1);
        Assert.NotNull(result.Issues);
        Assert.NotNull(result.Warnings);
    }

    [Fact]
    public async Task SynchronizeMarketDataAsync_ShouldCompleteSuccessfully()
    {
        // Arrange & Act
        await _service.SynchronizeMarketDataAsync();

        // Assert - Le test passe si aucune exception n'est levée / Test passes if no exception is thrown
        Assert.True(true);
    }

    [Fact]
    public async Task GetRealTimeDataAsync_MultipleCallsSameSymbol_ShouldShowPriceVariation()
    {
        // Arrange
        var symbol = "SU.TO";

        // Act - Obtenir les données plusieurs fois / Get data multiple times
        var data1 = await _service.GetRealTimeDataAsync(symbol);
        await Task.Delay(10); // Petite pause / Small delay
        var data2 = await _service.GetRealTimeDataAsync(symbol);

        // Assert - Les prix peuvent varier légèrement / Prices may vary slightly
        Assert.NotNull(data1);
        Assert.NotNull(data2);
        Assert.Equal(symbol, data1.Symbol);
        Assert.Equal(symbol, data2.Symbol);
        
        // Les prix sont dans une plage raisonnable / Prices are within reasonable range
        Assert.True(Math.Abs(data1.Price - data2.Price) < data1.Price * 0.1m); // Variation < 10%
    }

    [Fact]
    public async Task GetHistoricalDataAsync_WeekendDates_ShouldExcludeWeekends()
    {
        // Arrange
        var symbol = "XIU.TO";
        var startDate = new DateTime(2024, 1, 1); // Lundi / Monday
        var endDate = new DateTime(2024, 1, 7);   // Dimanche / Sunday

        // Act
        var result = await _service.GetHistoricalDataAsync(symbol, startDate, endDate);

        // Assert
        var dataList = result.ToList();
        Assert.NotEmpty(dataList);
        
        // Aucune donnée pour samedi ou dimanche / No data for Saturday or Sunday
        Assert.All(dataList, data =>
        {
            Assert.True(data.Date.DayOfWeek != DayOfWeek.Saturday);
            Assert.True(data.Date.DayOfWeek != DayOfWeek.Sunday);
        });
    }

    [Theory]
    [InlineData("RY.TO")]
    [InlineData("TD.TO")]
    [InlineData("SHOP.TO")]
    [InlineData("CNR.TO")]
    public async Task GetCurrentPriceAsync_KnownCanadianStocks_ShouldReturnReasonablePrices(string symbol)
    {
        // Act
        var price = await _service.GetCurrentPriceAsync(symbol);

        // Assert
        Assert.NotNull(price);
        Assert.True(price > 1m);     // Au moins 1$ / At least $1
        Assert.True(price < 1000m);  // Moins de 1000$ / Less than $1000
    }

    [Fact]
    public async Task GetTechnicalIndicatorsAsync_BollingerBands_ShouldBeLogicallyOrdered()
    {
        // Arrange
        var symbol = "AC.TO";

        // Act
        var indicators = await _service.GetTechnicalIndicatorsAsync(symbol);

        // Assert
        Assert.True(indicators.BollingerBandUpper >= indicators.BollingerBandMiddle);
        Assert.True(indicators.BollingerBandMiddle >= indicators.BollingerBandLower);
        Assert.True(indicators.BollingerBandMiddle == indicators.SimpleMovingAverage);
    }
}