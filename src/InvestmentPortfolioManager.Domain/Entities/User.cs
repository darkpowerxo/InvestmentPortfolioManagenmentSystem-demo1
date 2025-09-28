using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Domain.Entities;

/// <summary>
/// Utilisateur du système / System user
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; } = UserRole.Viewer;
    
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en"; // en, fr
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Portfolio> ManagedPortfolios { get; set; } = new List<Portfolio>();
    public virtual ICollection<TradeOrder> CreatedOrders { get; set; } = new List<TradeOrder>();
}