using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.Application.Services.Contracts;

/// <summary>
/// Service pour le traitement des transactions / Service for transaction processing
/// </summary>
public interface ITransactionProcessingService
{
    /// <summary>
    /// Créer un ordre de transaction / Create a trade order
    /// </summary>
    Task<TradeOrderResultDto> CreateTradeOrderAsync(CreateTradeOrderDto request);
    
    /// <summary>
    /// Valider un ordre avant exécution / Validate order before execution
    /// </summary>
    Task<OrderValidationResultDto> ValidateOrderAsync(CreateTradeOrderDto request);
    
    /// <summary>
    /// Exécuter un ordre de transaction / Execute trade order
    /// </summary>
    Task<TransactionResultDto> ExecuteTradeOrderAsync(Guid orderId);
    
    /// <summary>
    /// Annuler un ordre en attente / Cancel pending order
    /// </summary>
    Task<bool> CancelOrderAsync(Guid orderId);
    
    /// <summary>
    /// Calculer les coûts de transaction / Calculate transaction costs
    /// </summary>
    Task<TransactionCostDto> CalculateTransactionCostAsync(string symbol, decimal quantity, decimal price, TransactionType transactionType);
    
    /// <summary>
    /// Obtenir les ordres en attente pour un portefeuille / Get pending orders for portfolio
    /// </summary>
    Task<IEnumerable<TradeOrderDto>> GetPendingOrdersAsync(Guid portfolioId);
    
    /// <summary>
    /// Obtenir l'historique des transactions / Get transaction history
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid portfolioId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Effectuer le règlement des transactions / Process transaction settlement
    /// </summary>
    Task<SettlementResultDto> ProcessSettlementAsync(Guid transactionId);
    
    /// <summary>
    /// Vérifier la conformité réglementaire / Check regulatory compliance
    /// </summary>
    Task<ComplianceCheckResultDto> CheckComplianceAsync(CreateTradeOrderDto request);
    
    /// <summary>
    /// Calculer l'impact sur le portefeuille / Calculate portfolio impact
    /// </summary>
    Task<PortfolioImpactDto> CalculatePortfolioImpactAsync(Guid portfolioId, CreateTradeOrderDto request);
}

/// <summary>
/// DTO pour créer un ordre de transaction / DTO for creating trade order
/// </summary>
public class CreateTradeOrderDto
{
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public OrderType OrderType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Instructions { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour le résultat de création d'ordre / DTO for trade order creation result
/// </summary>
public class TradeOrderResultDto
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal? EstimatedCost { get; set; }
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
}

/// <summary>
/// DTO pour le résultat de validation d'ordre / DTO for order validation result
/// </summary>
public class OrderValidationResultDto
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
    public decimal RequiredCash { get; set; }
    public decimal AvailableCash { get; set; }
    public bool HasSufficientFunds { get; set; }
    public ComplianceCheckResultDto? ComplianceResult { get; set; }
}

/// <summary>
/// DTO pour le résultat d'exécution de transaction / DTO for transaction execution result
/// </summary>
public class TransactionResultDto
{
    public Guid TransactionId { get; set; }
    public Guid OrderId { get; set; }
    public decimal ExecutedQuantity { get; set; }
    public decimal ExecutedPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string ExecutionVenue { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
}

/// <summary>
/// DTO pour les coûts de transaction / DTO for transaction costs
/// </summary>
public class TransactionCostDto
{
    public decimal Commission { get; set; }
    public decimal MarketImpact { get; set; }
    public decimal SpreadCost { get; set; }
    public decimal TaxesAndFees { get; set; }
    public decimal TotalCost { get; set; }
    public decimal CostBasisPoints { get; set; }
    public string Currency { get; set; } = "CAD";
}

/// <summary>
/// DTO pour un ordre de transaction / DTO for trade order
/// </summary>
public class TradeOrderDto
{
    public Guid OrderId { get; set; }
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public OrderType OrderType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Instructions { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour une transaction / DTO for transaction
/// </summary>
public class TransactionDto
{
    public Guid TransactionId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid? OrderId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Commission { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime SettlementDate { get; set; }
    public TransactionStatus Status { get; set; }
    public string Currency { get; set; } = "CAD";
}

/// <summary>
/// DTO pour le résultat de règlement / DTO for settlement result
/// </summary>
public class SettlementResultDto
{
    public Guid TransactionId { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string SettlementVenue { get; set; } = string.Empty;
    public IEnumerable<string> Issues { get; set; } = new List<string>();
    public decimal CashMovement { get; set; }
    public decimal SecurityMovement { get; set; }
}

/// <summary>
/// DTO pour le résultat de vérification de conformité / DTO for compliance check result
/// </summary>
public class ComplianceCheckResultDto
{
    public bool IsCompliant { get; set; }
    public IEnumerable<string> Violations { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
    public decimal MaxPositionSize { get; set; }
    public decimal CurrentExposure { get; set; }
    public decimal ProposedExposure { get; set; }
    public bool RequiresApproval { get; set; }
    public string ComplianceOfficer { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour l'impact sur le portefeuille / DTO for portfolio impact
/// </summary>
public class PortfolioImpactDto
{
    public Guid PortfolioId { get; set; }
    public decimal CashImpact { get; set; }
    public decimal NewCashBalance { get; set; }
    public Dictionary<string, decimal> SectorExposureChanges { get; set; } = new();
    public Dictionary<string, decimal> RegionExposureChanges { get; set; } = new();
    public decimal RiskImpact { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal TrackingErrorChange { get; set; }
    public IEnumerable<string> RiskWarnings { get; set; } = new List<string>();
}

/// <summary>
/// Statut de transaction / Transaction status
/// </summary>
public enum TransactionStatus
{
    Pending,
    Settled,
    Failed,
    Cancelled
}