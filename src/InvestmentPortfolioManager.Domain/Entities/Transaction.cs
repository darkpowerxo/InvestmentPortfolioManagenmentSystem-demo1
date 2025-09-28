using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Transaction financière / Financial transaction
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    
    [Required]
    public int PortfolioId { get; set; }
    
    [Required]
    public int SecurityId { get; set; }
    
    [Required]
    public TransactionType TransactionType { get; set; }
    
    // Alias for TransactionType to match DTO expectations - backed by TransactionType
    public TransactionType Type
    {
        get => TransactionType;
        set => TransactionType = value;
    }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; } // Price per unit
    
    public decimal TotalAmount { get; set; } // Quantity * Price
    public decimal Commission { get; set; }
    public decimal OtherFees { get; set; }
    
    // Alias for OtherFees to match DTO expectations - backed by OtherFees
    public decimal Tax
    {
        get => OtherFees;
        set => OtherFees = value;
    }
    public decimal NetAmount { get; set; } // TotalAmount +/- Commission + OtherFees
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "CAD";
    
    public decimal? ExchangeRate { get; set; } // To base currency if different
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    public DateTime SettlementDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? OrderId { get; set; } // Reference to original order
    
    // Navigation properties
    public virtual Portfolio Portfolio { get; set; } = null!;
    public virtual Security Security { get; set; } = null!;
}