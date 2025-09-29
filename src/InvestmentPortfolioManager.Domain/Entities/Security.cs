using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Sécurité financière / Financial security
/// </summary>
public class Security
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty; // e.g., AAPL, MSFT
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public SecurityType SecurityType { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "CAD";
    
    [MaxLength(100)]
    public string Exchange { get; set; } = string.Empty; // TSX, NYSE, NASDAQ
    
    [MaxLength(100)]
    public string Sector { get; set; } = string.Empty; // Technology, Healthcare, etc.
    
    [MaxLength(100)]
    public string Industry { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Country { get; set; } = "Canada";
    
    [MaxLength(50)]
    public string Region { get; set; } = string.Empty; // North America, Europe, etc.
    
    // For bonds
    public double? CouponRate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public string? CreditRating { get; set; }
    
    // For options
    public double? StrikePrice { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? OptionType { get; set; } // Call, Put
    public int? UnderlyingSecurityId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Security? UnderlyingSecurity { get; set; }
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<MarketData> MarketDataHistory { get; set; } = new List<MarketData>();
    public virtual ICollection<TradeOrder> TradeOrders { get; set; } = new List<TradeOrder>();
}