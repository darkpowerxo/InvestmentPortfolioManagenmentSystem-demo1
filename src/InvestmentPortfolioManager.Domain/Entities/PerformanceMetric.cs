using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Métriques de performance du portefeuille / Portfolio performance metrics
/// </summary>
public class PerformanceMetric
{
    public int Id { get; set; }
    
    [Required]
    public int PortfolioId { get; set; }
    
    [Required]
    public DateTime CalculationDate { get; set; }
    
    // Return metrics
    public decimal DailyReturn { get; set; }
    public decimal WeeklyReturn { get; set; }
    public decimal MonthlyReturn { get; set; }
    public decimal YearToDateReturn { get; set; }
    public decimal OneYearReturn { get; set; }
    public decimal ThreeYearReturn { get; set; }
    public decimal FiveYearReturn { get; set; }
    public decimal SinceInceptionReturn { get; set; }
    
    // Risk metrics
    public decimal Volatility { get; set; } // Annualized standard deviation
    public decimal SharpeRatio { get; set; }
    public decimal SortinoRatio { get; set; }
    public decimal Beta { get; set; } // Against benchmark
    public decimal Alpha { get; set; } // Against benchmark
    public decimal MaxDrawdown { get; set; }
    public decimal VaR95 { get; set; } // Value at Risk 95%
    public decimal VaR99 { get; set; } // Value at Risk 99%
    
    // Benchmark comparison
    [MaxLength(20)]
    public string BenchmarkSymbol { get; set; } = string.Empty;
    public decimal BenchmarkReturn { get; set; }
    public decimal TrackingError { get; set; }
    public decimal InformationRatio { get; set; }
    
    // Portfolio composition
    public decimal EquityAllocation { get; set; }
    public decimal BondAllocation { get; set; }
    public decimal AlternativeAllocation { get; set; }
    public decimal CashAllocation { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Portfolio Portfolio { get; set; } = null!;
}