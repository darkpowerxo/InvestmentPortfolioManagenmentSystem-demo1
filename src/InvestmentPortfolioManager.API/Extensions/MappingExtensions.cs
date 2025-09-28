using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Application.DTOs;

namespace InvestmentPortfolioManager.API.Extensions
{
    public static class MappingExtensions
    {
        // User mappings
        public static UserDto ToDto(this User user)
        {
            return new UserDto(
                Id: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Role: user.Role,
                CreatedAt: user.CreatedAt,
                UpdatedAt: user.UpdatedAt
            );
        }

        public static User ToEntity(this CreateUserDto dto)
        {
            return new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role
            };
        }

        public static void UpdateFromDto(this User user, UpdateUserDto dto)
        {
            if (dto.FirstName is not null) user.FirstName = dto.FirstName;
            if (dto.LastName is not null) user.LastName = dto.LastName;
            if (dto.Role is not null) user.Role = dto.Role.Value;
        }

        // Portfolio mappings
        public static PortfolioDto ToDto(this Portfolio portfolio)
        {
            return new PortfolioDto(
                Id: portfolio.Id,
                Name: portfolio.Name,
                Description: portfolio.Description,
                Type: portfolio.Type,
                ManagerId: portfolio.ManagerId,
                ManagerName: $"{portfolio.Manager?.FirstName} {portfolio.Manager?.LastName}".Trim(),
                TotalValue: portfolio.Positions?.Sum(p => p.MarketValue) ?? 0,
                CashBalance: portfolio.CashBalance,
                CreatedAt: portfolio.CreatedAt,
                UpdatedAt: portfolio.UpdatedAt,
                PositionCount: portfolio.Positions?.Count ?? 0,
                YtdReturn: 0, // Would be calculated from performance metrics
                TotalReturn: 0 // Would be calculated from performance metrics
            );
        }

        public static PortfolioSummaryDto ToSummaryDto(this Portfolio portfolio)
        {
            return new PortfolioSummaryDto(
                Id: portfolio.Id,
                Name: portfolio.Name,
                Type: portfolio.Type,
                TotalValue: portfolio.Positions?.Sum(p => p.MarketValue) ?? 0,
                YtdReturn: 0, // Would be calculated from performance metrics
                CashBalance: portfolio.CashBalance,
                PositionCount: portfolio.Positions?.Count ?? 0
            );
        }

        public static Portfolio ToEntity(this CreatePortfolioDto dto)
        {
            return new Portfolio
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                ManagerId = dto.ManagerId,
                CashBalance = dto.CashBalance
            };
        }

        public static void UpdateFromDto(this Portfolio portfolio, UpdatePortfolioDto dto)
        {
            if (dto.Name is not null) portfolio.Name = dto.Name;
            if (dto.Description is not null) portfolio.Description = dto.Description;
            if (dto.Type is not null) portfolio.Type = dto.Type.Value;
            if (dto.ManagerId is not null) portfolio.ManagerId = dto.ManagerId.Value;
            if (dto.CashBalance is not null) portfolio.CashBalance = dto.CashBalance.Value;
        }

        // Security mappings
        public static SecurityDto ToDto(this Security security)
        {
            var latestData = security.MarketDataHistory?.OrderByDescending(md => md.Date).FirstOrDefault();
            var previousData = security.MarketDataHistory?.OrderByDescending(md => md.Date).Skip(1).FirstOrDefault();
            
            decimal? dayChange = null;
            decimal? dayChangePercent = null;
            
            if (latestData != null && previousData != null)
            {
                dayChange = latestData.Close - previousData.Close;
                dayChangePercent = (dayChange / previousData.Close) * 100;
            }

            return new SecurityDto(
                Id: security.Id,
                Symbol: security.Symbol,
                Name: security.Name,
                Type: security.Type,
                Exchange: security.Exchange,
                Currency: security.Currency,
                Sector: security.Sector,
                Industry: security.Industry,
                CurrentPrice: latestData?.Close ?? security.CurrentPrice,
                DayChange: dayChange,
                DayChangePercent: dayChangePercent,
                LastUpdated: latestData?.Date ?? security.UpdatedAt
            );
        }

        public static SecuritySummaryDto ToSummaryDto(this Security security)
        {
            var latestData = security.MarketDataHistory?.OrderByDescending(md => md.Date).FirstOrDefault();
            var previousData = security.MarketDataHistory?.OrderByDescending(md => md.Date).Skip(1).FirstOrDefault();
            
            decimal? dayChangePercent = null;
            if (latestData != null && previousData != null)
            {
                var dayChange = latestData.Close - previousData.Close;
                dayChangePercent = (dayChange / previousData.Close) * 100;
            }

            return new SecuritySummaryDto(
                Id: security.Id,
                Symbol: security.Symbol,
                Name: security.Name,
                Type: security.Type,
                CurrentPrice: latestData?.Close ?? security.CurrentPrice,
                DayChangePercent: dayChangePercent
            );
        }

        public static Security ToEntity(this CreateSecurityDto dto)
        {
            return new Security
            {
                Symbol = dto.Symbol,
                Name = dto.Name,
                Type = dto.Type,
                Exchange = dto.Exchange,
                Currency = dto.Currency,
                Sector = dto.Sector,
                Industry = dto.Industry,
                CurrentPrice = dto.CurrentPrice
            };
        }

        public static void UpdateFromDto(this Security security, UpdateSecurityDto dto)
        {
            if (dto.Name is not null) security.Name = dto.Name;
            if (dto.Type is not null) security.Type = dto.Type.Value;
            if (dto.Exchange is not null) security.Exchange = dto.Exchange;
            if (dto.Currency is not null) security.Currency = dto.Currency;
            if (dto.Sector is not null) security.Sector = dto.Sector;
            if (dto.Industry is not null) security.Industry = dto.Industry;
            if (dto.CurrentPrice is not null) security.CurrentPrice = dto.CurrentPrice.Value;
        }

        // Position mappings
        public static PositionDto ToDto(this Position position)
        {
            var currentPrice = position.Security?.CurrentPrice ?? 0;
            var marketValue = position.Quantity * currentPrice;
            var costBasis = position.Quantity * position.AveragePrice;
            var unrealizedGainLoss = marketValue - costBasis;
            var unrealizedGainLossPercent = costBasis != 0 ? (unrealizedGainLoss / costBasis) * 100 : 0;

            return new PositionDto(
                Id: position.Id,
                PortfolioId: position.PortfolioId,
                PortfolioName: position.Portfolio?.Name ?? "",
                SecurityId: position.SecurityId,
                SecuritySymbol: position.Security?.Symbol ?? "",
                SecurityName: position.Security?.Name ?? "",
                Quantity: position.Quantity,
                AveragePrice: position.AveragePrice,
                CurrentPrice: currentPrice,
                MarketValue: marketValue,
                UnrealizedGainLoss: unrealizedGainLoss,
                UnrealizedGainLossPercent: unrealizedGainLossPercent,
                Weight: 0, // Would be calculated against portfolio total
                FirstPurchaseDate: position.FirstPurchaseDate,
                LastTransactionDate: position.LastTransactionDate,
                UpdatedAt: position.UpdatedAt
            );
        }

        public static PositionSummaryDto ToSummaryDto(this Position position)
        {
            var currentPrice = position.Security?.CurrentPrice ?? 0;
            var marketValue = position.Quantity * currentPrice;
            var costBasis = position.Quantity * position.AveragePrice;
            var unrealizedGainLoss = marketValue - costBasis;

            return new PositionSummaryDto(
                Id: position.Id,
                SecuritySymbol: position.Security?.Symbol ?? "",
                Quantity: position.Quantity,
                MarketValue: marketValue,
                UnrealizedGainLoss: unrealizedGainLoss,
                Weight: 0 // Would be calculated against portfolio total
            );
        }

        public static Position ToEntity(this CreatePositionDto dto)
        {
            return new Position
            {
                PortfolioId = dto.PortfolioId,
                SecurityId = dto.SecurityId,
                Quantity = dto.Quantity,
                AveragePrice = dto.AveragePrice,
                FirstPurchaseDate = DateTime.UtcNow,
                LastTransactionDate = DateTime.UtcNow
            };
        }

        public static void UpdateFromDto(this Position position, UpdatePositionDto dto)
        {
            if (dto.Quantity is not null) position.Quantity = dto.Quantity.Value;
            if (dto.AveragePrice is not null) position.AveragePrice = dto.AveragePrice.Value;
        }
    }
}