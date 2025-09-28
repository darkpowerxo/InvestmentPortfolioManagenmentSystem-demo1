using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Données de marché historiques / Historical market data
/// </summary>
public class MarketData
{
    public int Id { get; set; }
    
    [Required]
    public int SecurityId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public decimal OpenPrice { get; set; }
    
    [Required]
    public decimal HighPrice { get; set; }
    
    [Required]
    public decimal LowPrice { get; set; }
    
    [Required]
    public decimal ClosePrice { get; set; }
    
    [Required]
    public decimal AdjustedClosePrice { get; set; } // Adjusted for splits and dividends
    
    public long Volume { get; set; }
    
    // Aliases for DTO compatibility - backed by original properties
    public decimal Open
    {
        get => OpenPrice;
        set => OpenPrice = value;
    }
    
    public decimal High
    {
        get => HighPrice;
        set => HighPrice = value;
    }
    
    public decimal Low
    {
        get => LowPrice;
        set => LowPrice = value;
    }
    
    public decimal Close
    {
        get => ClosePrice;
        set => ClosePrice = value;
    }
    
    public decimal AdjustedClose
    {
        get => AdjustedClosePrice;
        set => AdjustedClosePrice = value;
    }
    
    [MaxLength(10)]
    public string Currency { get; set; } = "CAD";
    
    // Technical indicators
    public decimal? SMA20 { get; set; } // 20-day Simple Moving Average
    public decimal? SMA50 { get; set; } // 50-day Simple Moving Average
    public decimal? SMA200 { get; set; } // 200-day Simple Moving Average
    public decimal? EMA12 { get; set; } // 12-day Exponential Moving Average
    public decimal? EMA26 { get; set; } // 26-day Exponential Moving Average
    public decimal? RSI { get; set; } // Relative Strength Index
    public decimal? Volatility { get; set; } // Historical volatility (annualized)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Security Security { get; set; } = null!;
}