using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Application.DTOs
{
    // Transaction DTOs
    public record TransactionDto(
        int Id,
        int PortfolioId,
        string PortfolioName,
        int SecurityId,
        string SecuritySymbol,
        string SecurityName,
        TransactionType Type,
        decimal Quantity,
        decimal Price,
        decimal Commission,
        decimal Tax,
        decimal NetAmount,
        DateTime TransactionDate,
        string? Notes,
        DateTime CreatedAt
    );

    public record CreateTransactionDto(
        int PortfolioId,
        int SecurityId,
        TransactionType Type,
        decimal Quantity,
        decimal Price,
        decimal Commission,
        decimal Tax,
        DateTime TransactionDate,
        string? Notes
    );

    public record UpdateTransactionDto(
        TransactionType? Type,
        decimal? Quantity,
        decimal? Price,
        decimal? Commission,
        decimal? Tax,
        DateTime? TransactionDate,
        string? Notes
    );

    public record TransactionSummaryDto(
        int Id,
        string SecuritySymbol,
        TransactionType Type,
        decimal Quantity,
        decimal Price,
        decimal NetAmount,
        DateTime TransactionDate
    );

    // Market Data DTOs
    public record MarketDataDto(
        int Id,
        int SecurityId,
        string SecuritySymbol,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        long Volume,
        decimal AdjustedClose,
        DateTime Date,
        DateTime CreatedAt
    );

    public record CreateMarketDataDto(
        int SecurityId,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        long Volume,
        decimal AdjustedClose,
        DateTime Date
    );

    public record UpdateMarketDataDto(
        decimal? Open,
        decimal? High,
        decimal? Low,
        decimal? Close,
        long? Volume,
        decimal? AdjustedClose
    );

    public record MarketDataSummaryDto(
        int Id,
        string SecuritySymbol,
        decimal Close,
        decimal DayChange,
        decimal DayChangePercent,
        long Volume,
        DateTime Date
    );

    // Performance Metric DTOs
    public record PerformanceMetricDto(
        int Id,
        int PortfolioId,
        string PortfolioName,
        DateTime CalculationDate,
        decimal MonthlyReturn,
        decimal YtdReturn,
        decimal TotalReturn,
        decimal SharpeRatio,
        decimal Volatility,
        decimal MaxDrawdown,
        decimal Alpha,
        decimal Beta,
        DateTime CreatedAt
    );

    public record CreatePerformanceMetricDto(
        int PortfolioId,
        DateTime CalculationDate,
        decimal MonthlyReturn,
        decimal YtdReturn,
        decimal TotalReturn,
        decimal SharpeRatio,
        decimal Volatility,
        decimal MaxDrawdown,
        decimal Alpha,
        decimal Beta
    );

    public record UpdatePerformanceMetricDto(
        decimal? MonthlyReturn,
        decimal? YtdReturn,
        decimal? TotalReturn,
        decimal? SharpeRatio,
        decimal? Volatility,
        decimal? MaxDrawdown,
        decimal? Alpha,
        decimal? Beta
    );

    public record PerformanceMetricSummaryDto(
        int Id,
        DateTime CalculationDate,
        decimal MonthlyReturn,
        decimal YtdReturn,
        decimal SharpeRatio,
        decimal Volatility
    );

    // Trade Order DTOs
    public record TradeOrderDto(
        int Id,
        int PortfolioId,
        string PortfolioName,
        int SecurityId,
        string SecuritySymbol,
        OrderType Type,
        OrderSide Side,
        decimal Quantity,
        decimal? LimitPrice,
        decimal? StopPrice,
        OrderStatus Status,
        decimal QuantityFilled,
        decimal? AverageExecutionPrice,
        DateTime OrderDate,
        DateTime? ExpirationDate,
        DateTime? FilledDate,
        string? Notes,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateTradeOrderDto(
        int PortfolioId,
        int SecurityId,
        OrderType Type,
        OrderSide Side,
        decimal Quantity,
        decimal? LimitPrice,
        decimal? StopPrice,
        DateTime? ExpirationDate,
        string? Notes
    );

    public record UpdateTradeOrderDto(
        OrderType? Type,
        decimal? Quantity,
        decimal? LimitPrice,
        decimal? StopPrice,
        OrderStatus? Status,
        decimal? QuantityFilled,
        decimal? AverageExecutionPrice,
        DateTime? ExpirationDate,
        DateTime? FilledDate,
        string? Notes
    );

    public record TradeOrderSummaryDto(
        int Id,
        string SecuritySymbol,
        OrderType Type,
        OrderSide Side,
        decimal Quantity,
        decimal QuantityFilled,
        OrderStatus Status,
        DateTime OrderDate
    );
}