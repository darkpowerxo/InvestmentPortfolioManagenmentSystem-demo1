using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Application.DTOs
{
    // User DTOs
    public record UserDto(
        int Id,
        string Email,
        string FirstName,
        string LastName,
        UserRole Role,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateUserDto(
        string Email,
        string FirstName,
        string LastName,
        UserRole Role
    );

    public record UpdateUserDto(
        string? FirstName,
        string? LastName,
        UserRole? Role
    );

    // Portfolio DTOs
    public record PortfolioDto(
        int Id,
        string Name,
        string? Description,
        PortfolioType Type,
        int ManagerId,
        string ManagerName,
        decimal TotalValue,
        decimal CashBalance,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int PositionCount,
        decimal YtdReturn,
        decimal TotalReturn
    );

    public record CreatePortfolioDto(
        string Name,
        string? Description,
        PortfolioType Type,
        int ManagerId,
        decimal CashBalance
    );

    public record UpdatePortfolioDto(
        string? Name,
        string? Description,
        PortfolioType? Type,
        int? ManagerId,
        decimal? CashBalance
    );

    public record PortfolioSummaryDto(
        int Id,
        string Name,
        PortfolioType Type,
        decimal TotalValue,
        decimal YtdReturn,
        decimal CashBalance,
        int PositionCount
    );

    // Security DTOs
    public record SecurityDto(
        int Id,
        string Symbol,
        string Name,
        SecurityType Type,
        string Exchange,
        string Currency,
        string? Sector,
        string? Industry,
        decimal CurrentPrice,
        decimal? DayChange,
        decimal? DayChangePercent,
        DateTime LastUpdated
    );

    public record CreateSecurityDto(
        string Symbol,
        string Name,
        SecurityType Type,
        string Exchange,
        string Currency,
        string? Sector,
        string? Industry,
        decimal CurrentPrice
    );

    public record UpdateSecurityDto(
        string? Name,
        SecurityType? Type,
        string? Exchange,
        string? Currency,
        string? Sector,
        string? Industry,
        decimal? CurrentPrice
    );

    public record SecuritySummaryDto(
        int Id,
        string Symbol,
        string Name,
        SecurityType Type,
        decimal CurrentPrice,
        decimal? DayChangePercent
    );

    // Position DTOs
    public record PositionDto(
        int Id,
        int PortfolioId,
        string PortfolioName,
        int SecurityId,
        string SecuritySymbol,
        string SecurityName,
        decimal Quantity,
        decimal AveragePrice,
        decimal CurrentPrice,
        decimal MarketValue,
        decimal UnrealizedGainLoss,
        decimal UnrealizedGainLossPercent,
        decimal Weight,
        DateTime FirstPurchaseDate,
        DateTime LastTransactionDate,
        DateTime UpdatedAt
    );

    public record CreatePositionDto(
        int PortfolioId,
        int SecurityId,
        decimal Quantity,
        decimal AveragePrice
    );

    public record UpdatePositionDto(
        decimal? Quantity,
        decimal? AveragePrice
    );

    public record PositionSummaryDto(
        int Id,
        string SecuritySymbol,
        decimal Quantity,
        decimal MarketValue,
        decimal UnrealizedGainLoss,
        decimal Weight
    );
}