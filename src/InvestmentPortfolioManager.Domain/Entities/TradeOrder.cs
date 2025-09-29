using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Ordre de transaction / Trade order
/// </summary>
public class TradeOrder
{
    public int Id { get; set; }
    
    [Required]
    public int PortfolioId { get; set; }
    
    [Required]
    public int SecurityId { get; set; }
    
    [Required]
    public TransactionType OrderSide { get; set; } // Buy or Sell
    
    [Required]
    public OrderType OrderType { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public decimal QuantityFilled { get; set; } = 0;
    public decimal AverageExecutionPrice { get; set; } = 0;
    
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExecutionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    [Required]
    public int CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Portfolio Portfolio { get; set; } = null!;
    public virtual Security Security { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
}