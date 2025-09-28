using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Application.DTOs;

namespace InvestmentPortfolioManager.API.Extensions
{
    public static class TransactionMappingExtensions
    {
        // Transaction mappings
        public static TransactionDto ToDto(this Transaction transaction)
        {
            return new TransactionDto(
                Id: transaction.Id,
                PortfolioId: transaction.PortfolioId,
                PortfolioName: transaction.Portfolio?.Name ?? "",
                SecurityId: transaction.SecurityId,
                SecuritySymbol: transaction.Security?.Symbol ?? "",
                SecurityName: transaction.Security?.Name ?? "",
                Type: transaction.Type,
                Quantity: transaction.Quantity,
                Price: transaction.Price,
                Commission: transaction.Commission,
                Tax: transaction.Tax,
                NetAmount: transaction.NetAmount,
                TransactionDate: transaction.TransactionDate,
                Notes: transaction.Notes,
                CreatedAt: transaction.CreatedAt
            );
        }

        public static TransactionSummaryDto ToSummaryDto(this Transaction transaction)
        {
            return new TransactionSummaryDto(
                Id: transaction.Id,
                SecuritySymbol: transaction.Security?.Symbol ?? "",
                Type: transaction.Type,
                Quantity: transaction.Quantity,
                Price: transaction.Price,
                NetAmount: transaction.NetAmount,
                TransactionDate: transaction.TransactionDate
            );
        }

        public static Transaction ToEntity(this CreateTransactionDto dto)
        {
            var netAmount = dto.Type switch
            {
                Domain.Enums.TransactionType.Buy => -(dto.Quantity * dto.Price + dto.Commission + dto.Tax),
                Domain.Enums.TransactionType.Sell => dto.Quantity * dto.Price - dto.Commission - dto.Tax,
                Domain.Enums.TransactionType.Dividend => dto.Quantity * dto.Price - dto.Tax,
                _ => dto.Quantity * dto.Price
            };

            return new Transaction
            {
                PortfolioId = dto.PortfolioId,
                SecurityId = dto.SecurityId,
                Type = dto.Type,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Commission = dto.Commission,
                Tax = dto.Tax,
                NetAmount = netAmount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes
            };
        }

        public static void UpdateFromDto(this Transaction transaction, UpdateTransactionDto dto)
        {
            if (dto.Type is not null) transaction.Type = dto.Type.Value;
            if (dto.Quantity is not null) transaction.Quantity = dto.Quantity.Value;
            if (dto.Price is not null) transaction.Price = dto.Price.Value;
            if (dto.Commission is not null) transaction.Commission = dto.Commission.Value;
            if (dto.Tax is not null) transaction.Tax = dto.Tax.Value;
            if (dto.TransactionDate is not null) transaction.TransactionDate = dto.TransactionDate.Value;
            if (dto.Notes is not null) transaction.Notes = dto.Notes;

            // Recalculate net amount
            transaction.NetAmount = transaction.Type switch
            {
                Domain.Enums.TransactionType.Buy => -(transaction.Quantity * transaction.Price + transaction.Commission + transaction.Tax),
                Domain.Enums.TransactionType.Sell => transaction.Quantity * transaction.Price - transaction.Commission - transaction.Tax,
                Domain.Enums.TransactionType.Dividend => transaction.Quantity * transaction.Price - transaction.Tax,
                _ => transaction.Quantity * transaction.Price
            };
        }

        // Market Data mappings
        public static MarketDataDto ToDto(this MarketData marketData)
        {
            return new MarketDataDto(
                Id: marketData.Id,
                SecurityId: marketData.SecurityId,
                SecuritySymbol: marketData.Security?.Symbol ?? "",
                Open: marketData.Open,
                High: marketData.High,
                Low: marketData.Low,
                Close: marketData.Close,
                Volume: marketData.Volume,
                AdjustedClose: marketData.AdjustedClose,
                Date: marketData.Date,
                CreatedAt: marketData.CreatedAt
            );
        }

        public static MarketDataSummaryDto ToSummaryDto(this MarketData marketData, MarketData? previousData = null)
        {
            decimal dayChange = 0;
            decimal dayChangePercent = 0;

            if (previousData != null)
            {
                dayChange = marketData.Close - previousData.Close;
                dayChangePercent = previousData.Close != 0 ? (dayChange / previousData.Close) * 100 : 0;
            }

            return new MarketDataSummaryDto(
                Id: marketData.Id,
                SecuritySymbol: marketData.Security?.Symbol ?? "",
                Close: marketData.Close,
                DayChange: dayChange,
                DayChangePercent: dayChangePercent,
                Volume: marketData.Volume,
                Date: marketData.Date
            );
        }

        public static MarketData ToEntity(this CreateMarketDataDto dto)
        {
            return new MarketData
            {
                SecurityId = dto.SecurityId,
                Open = dto.Open,
                High = dto.High,
                Low = dto.Low,
                Close = dto.Close,
                Volume = dto.Volume,
                AdjustedClose = dto.AdjustedClose,
                Date = dto.Date
            };
        }

        public static void UpdateFromDto(this MarketData marketData, UpdateMarketDataDto dto)
        {
            if (dto.Open is not null) marketData.Open = dto.Open.Value;
            if (dto.High is not null) marketData.High = dto.High.Value;
            if (dto.Low is not null) marketData.Low = dto.Low.Value;
            if (dto.Close is not null) marketData.Close = dto.Close.Value;
            if (dto.Volume is not null) marketData.Volume = dto.Volume.Value;
            if (dto.AdjustedClose is not null) marketData.AdjustedClose = dto.AdjustedClose.Value;
        }

        // Performance Metric mappings
        public static PerformanceMetricDto ToDto(this PerformanceMetric metric)
        {
            return new PerformanceMetricDto(
                Id: metric.Id,
                PortfolioId: metric.PortfolioId,
                PortfolioName: metric.Portfolio?.Name ?? "",
                CalculationDate: metric.CalculationDate,
                MonthlyReturn: metric.MonthlyReturn,
                YtdReturn: metric.YtdReturn,
                TotalReturn: metric.TotalReturn,
                SharpeRatio: metric.SharpeRatio,
                Volatility: metric.Volatility,
                MaxDrawdown: metric.MaxDrawdown,
                Alpha: metric.Alpha,
                Beta: metric.Beta,
                CreatedAt: metric.CreatedAt
            );
        }

        public static PerformanceMetricSummaryDto ToSummaryDto(this PerformanceMetric metric)
        {
            return new PerformanceMetricSummaryDto(
                Id: metric.Id,
                CalculationDate: metric.CalculationDate,
                MonthlyReturn: metric.MonthlyReturn,
                YtdReturn: metric.YtdReturn,
                SharpeRatio: metric.SharpeRatio,
                Volatility: metric.Volatility
            );
        }

        public static PerformanceMetric ToEntity(this CreatePerformanceMetricDto dto)
        {
            return new PerformanceMetric
            {
                PortfolioId = dto.PortfolioId,
                CalculationDate = dto.CalculationDate,
                MonthlyReturn = dto.MonthlyReturn,
                YtdReturn = dto.YtdReturn,
                TotalReturn = dto.TotalReturn,
                SharpeRatio = dto.SharpeRatio,
                Volatility = dto.Volatility,
                MaxDrawdown = dto.MaxDrawdown,
                Alpha = dto.Alpha,
                Beta = dto.Beta
            };
        }

        public static void UpdateFromDto(this PerformanceMetric metric, UpdatePerformanceMetricDto dto)
        {
            if (dto.MonthlyReturn is not null) metric.MonthlyReturn = dto.MonthlyReturn.Value;
            if (dto.YtdReturn is not null) metric.YtdReturn = dto.YtdReturn.Value;
            if (dto.TotalReturn is not null) metric.TotalReturn = dto.TotalReturn.Value;
            if (dto.SharpeRatio is not null) metric.SharpeRatio = dto.SharpeRatio.Value;
            if (dto.Volatility is not null) metric.Volatility = dto.Volatility.Value;
            if (dto.MaxDrawdown is not null) metric.MaxDrawdown = dto.MaxDrawdown.Value;
            if (dto.Alpha is not null) metric.Alpha = dto.Alpha.Value;
            if (dto.Beta is not null) metric.Beta = dto.Beta.Value;
        }

        // Trade Order mappings
        public static TradeOrderDto ToDto(this TradeOrder order)
        {
            return new TradeOrderDto(
                Id: order.Id,
                PortfolioId: order.PortfolioId,
                PortfolioName: order.Portfolio?.Name ?? "",
                SecurityId: order.SecurityId,
                SecuritySymbol: order.Security?.Symbol ?? "",
                Type: order.Type,
                Side: order.Side,
                Quantity: order.Quantity,
                LimitPrice: order.LimitPrice,
                StopPrice: order.StopPrice,
                Status: order.Status,
                QuantityFilled: order.QuantityFilled,
                AverageExecutionPrice: order.AverageExecutionPrice,
                OrderDate: order.OrderDate,
                ExpirationDate: order.ExpirationDate,
                FilledDate: order.FilledDate,
                Notes: order.Notes,
                CreatedAt: order.CreatedAt,
                UpdatedAt: order.UpdatedAt
            );
        }

        public static TradeOrderSummaryDto ToSummaryDto(this TradeOrder order)
        {
            return new TradeOrderSummaryDto(
                Id: order.Id,
                SecuritySymbol: order.Security?.Symbol ?? "",
                Type: order.Type,
                Side: order.Side,
                Quantity: order.Quantity,
                QuantityFilled: order.QuantityFilled,
                Status: order.Status,
                OrderDate: order.OrderDate
            );
        }

        public static TradeOrder ToEntity(this CreateTradeOrderDto dto)
        {
            return new TradeOrder
            {
                PortfolioId = dto.PortfolioId,
                SecurityId = dto.SecurityId,
                Type = dto.Type,
                Side = dto.Side,
                Quantity = dto.Quantity,
                LimitPrice = dto.LimitPrice,
                StopPrice = dto.StopPrice,
                Status = Domain.Enums.OrderStatus.Pending,
                QuantityFilled = 0,
                OrderDate = DateTime.UtcNow,
                ExpirationDate = dto.ExpirationDate,
                Notes = dto.Notes
            };
        }

        public static void UpdateFromDto(this TradeOrder order, UpdateTradeOrderDto dto)
        {
            if (dto.Type is not null) order.Type = dto.Type.Value;
            if (dto.Quantity is not null) order.Quantity = dto.Quantity.Value;
            if (dto.LimitPrice is not null) order.LimitPrice = dto.LimitPrice;
            if (dto.StopPrice is not null) order.StopPrice = dto.StopPrice;
            if (dto.Status is not null) order.Status = dto.Status.Value;
            if (dto.QuantityFilled is not null) order.QuantityFilled = dto.QuantityFilled.Value;
            if (dto.AverageExecutionPrice is not null) order.AverageExecutionPrice = dto.AverageExecutionPrice.Value;
            if (dto.ExpirationDate is not null) order.ExpirationDate = dto.ExpirationDate;
            if (dto.FilledDate is not null) order.FilledDate = dto.FilledDate;
            if (dto.Notes is not null) order.Notes = dto.Notes;
        }
    }
}