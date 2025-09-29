using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Position dans un portefeuille / Position in a portfolio
/// </summary>
public class Position
{
    public int Id { get; set; }
    
    [Required]
    public int PortfolioId { get; set; }
    
    [Required]
    public int SecurityId { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal AverageCost { get; set; } // Cost basis per unit
    
    public decimal CurrentPrice { get; set; }
    public decimal MarketValue { get; set; } // Quantity * CurrentPrice
    public decimal UnrealizedGainLoss { get; set; } // MarketValue - (Quantity * AverageCost)
    public decimal UnrealizedGainLossPercent { get; set; }
    
    public DateTime FirstPurchaseDate { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Allocation percentage within the portfolio
    public decimal AllocationPercentage { get; set; }
    
    // Navigation properties
    public virtual Portfolio Portfolio { get; set; } = null!;
    public virtual Security Security { get; set; } = null!;
}