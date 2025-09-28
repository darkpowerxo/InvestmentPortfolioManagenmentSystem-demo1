using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Portefeuille d'investissement / Investment portfolio
/// </summary>
public class Portfolio
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public PortfolioType PortfolioType { get; set; }
    
    // Alias for PortfolioType to match DTO expectations - backed by PortfolioType
    public PortfolioType Type
    {
        get => PortfolioType;
        set => PortfolioType = value;
    }
    
    [Required]
    public RiskLevel RiskLevel { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string BaseCurrency { get; set; } = "CAD";
    
    [Required]
    public int ManagerId { get; set; } // Foreign key to User
    
    public decimal InitialValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal CashBalance { get; set; } = 0;
    
    public DateTime InceptionDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Target allocations (percentages)
    public decimal TargetEquityAllocation { get; set; } = 60;
    public decimal TargetBondAllocation { get; set; } = 30;
    public decimal TargetAlternativeAllocation { get; set; } = 10;
    public decimal TargetCashAllocation { get; set; } = 0;
    
    // Navigation properties
    public virtual User Manager { get; set; } = null!;
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();
    public virtual ICollection<TradeOrder> TradeOrders { get; set; } = new List<TradeOrder>();
}