using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Transaction management API controller
    /// Contrôleur API de gestion des transactions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ILogger<TransactionsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get transactions for a portfolio
        /// Obtenir les transactions d'un portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="startDate">Filter start date / Date de début du filtre</param>
        /// <param name="endDate">Filter end date / Date de fin du filtre</param>
        /// <param name="transactionType">Filter by transaction type / Filtrer par type de transaction</param>
        /// <param name="pageNumber">Page number (default 1) / Numéro de page (défaut 1)</param>
        /// <param name="pageSize">Page size (default 50) / Taille de page (défaut 50)</param>
        /// <returns>Paginated transaction list / Liste paginée des transactions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<TransactionSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] Guid? portfolioId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] TransactionType? transactionType = null,
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Getting transactions for portfolio {PortfolioId}", portfolioId);

                var start = startDate ?? DateTime.Today.AddMonths(-3);
                var end = endDate ?? DateTime.Today.AddDays(1);

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date / La date de début doit être antérieure à la date de fin");
                }

                // Generate mock transactions
                var allTransactions = CreateMockTransactions(portfolioId)
                    .Where(t => t.TransactionDate >= start && t.TransactionDate <= end)
                    .Where(t => transactionType == null || t.TransactionType == transactionType)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                var totalCount = allTransactions.Count;
                var transactions = allTransactions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TransactionSummary
                    {
                        Id = t.Id,
                        PortfolioId = t.PortfolioId,
                        SecurityId = t.SecurityId,
                        SecuritySymbol = GetSecuritySymbol(t.SecurityId),
                        TransactionType = t.TransactionType,
                        Quantity = t.Quantity,
                        Price = t.Price,
                        TotalAmount = t.TotalAmount,
                        TransactionDate = t.TransactionDate,
                        SettlementDate = t.SettlementDate,
                        Currency = "CAD"
                    })
                    .ToList();

                var result = new PaginatedResult<TransactionSummary>
                {
                    Items = transactions,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions");
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get detailed transaction information
        /// Obtenir les informations détaillées d'une transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID / ID de la transaction</param>
        /// <returns>Detailed transaction information / Informations détaillées de la transaction</returns>
        [HttpGet("{transactionId}")]
        [ProducesResponseType(typeof(TransactionDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionDetail([FromRoute] Guid transactionId)
        {
            try
            {
                _logger.LogInformation("Getting transaction detail for {TransactionId}", transactionId);

                var transaction = CreateMockTransactions()
                    .FirstOrDefault(t => t.Id == transactionId);

                if (transaction == null)
                {
                    return NotFound($"Transaction not found / Transaction non trouvée: {transactionId}");
                }

                var detail = new TransactionDetail
                {
                    Id = transaction.Id,
                    PortfolioId = transaction.PortfolioId,
                    SecurityId = transaction.SecurityId,
                    SecuritySymbol = GetSecuritySymbol(transaction.SecurityId),
                    SecurityName = GetSecurityName(transaction.SecurityId),
                    TransactionType = transaction.TransactionType,
                    Quantity = transaction.Quantity,
                    Price = transaction.Price,
                    TotalAmount = transaction.TotalAmount,
                    Commission = transaction.Commission,
                    OtherFees = transaction.OtherFees,
                    TransactionDate = transaction.TransactionDate,
                    SettlementDate = transaction.SettlementDate,
                    Currency = "CAD",
                    Notes = transaction.Notes
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction detail for {TransactionId}", transactionId);
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Create a new transaction
        /// Créer une nouvelle transaction
        /// </summary>
        /// <param name="request">Transaction creation request / Demande de création de transaction</param>
        /// <returns>Created transaction / Transaction créée</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionDetail), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new transaction for portfolio {PortfolioId}", request.PortfolioId);

                // Validate business rules
                if (request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be positive / La quantité doit être positive");
                }

                if (request.Price <= 0)
                {
                    return BadRequest("Price must be positive / Le prix doit être positif");
                }

                if (request.TransactionDate > DateTime.Today)
                {
                    return BadRequest("Transaction date cannot be in the future / La date de transaction ne peut pas être dans le futur");
                }

                // Create mock transaction (in production, this would save to database)
                var transactionId = Guid.NewGuid();
                var newTransaction = new TransactionDetail
                {
                    Id = transactionId,
                    PortfolioId = request.PortfolioId,
                    SecurityId = request.SecurityId,
                    SecuritySymbol = GetSecuritySymbol(request.SecurityId),
                    SecurityName = GetSecurityName(request.SecurityId),
                    TransactionType = request.TransactionType,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    TotalAmount = request.Quantity * request.Price,
                    Commission = request.Commission ?? 0,
                    OtherFees = request.OtherFees ?? 0,
                    TransactionDate = request.TransactionDate,
                    SettlementDate = CalculateSettlementDate(request.TransactionDate),
                    Currency = "CAD",
                    Notes = request.Notes
                };

                return CreatedAtAction(
                    nameof(GetTransactionDetail),
                    new { transactionId = transactionId },
                    newTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        /// <summary>
        /// Get transaction statistics for a portfolio
        /// Obtenir les statistiques de transaction pour un portefeuille
        /// </summary>
        /// <param name="portfolioId">Portfolio ID / ID du portefeuille</param>
        /// <param name="startDate">Statistics start date / Date de début des statistiques</param>
        /// <param name="endDate">Statistics end date / Date de fin des statistiques</param>
        /// <returns>Transaction statistics / Statistiques des transactions</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(TransactionStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionStatistics(
            [FromQuery] Guid? portfolioId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-12);
                var end = endDate ?? DateTime.Today;

                if (start >= end)
                {
                    return BadRequest("Start date must be before end date / La date de début doit être antérieure à la date de fin");
                }

                _logger.LogInformation("Getting transaction statistics for portfolio {PortfolioId}", portfolioId);

                var transactions = CreateMockTransactions(portfolioId)
                    .Where(t => t.TransactionDate >= start && t.TransactionDate <= end)
                    .ToList();

                var statistics = new TransactionStatistics
                {
                    PortfolioId = portfolioId,
                    StartDate = start,
                    EndDate = end,
                    TotalTransactions = transactions.Count,
                    TotalBuyTransactions = transactions.Count(t => t.TransactionType == TransactionType.Buy),
                    TotalSellTransactions = transactions.Count(t => t.TransactionType == TransactionType.Sell),
                    TotalDividendTransactions = transactions.Count(t => t.TransactionType == TransactionType.Dividend),
                    TotalBuyAmount = transactions.Where(t => t.TransactionType == TransactionType.Buy).Sum(t => t.TotalAmount),
                    TotalSellAmount = transactions.Where(t => t.TransactionType == TransactionType.Sell).Sum(t => t.TotalAmount),
                    TotalCommissions = transactions.Sum(t => t.Commission),
                    TotalFees = transactions.Sum(t => t.OtherFees),
                    Currency = "CAD"
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction statistics");
                return StatusCode(500, "Internal server error / Erreur interne du serveur");
            }
        }

        #region Mock Data Helpers

        private List<Transaction> CreateMockTransactions(Guid? portfolioId = null)
        {
            var transactions = new List<Transaction>();
            var random = new Random(42); // Fixed seed for consistent results

            var mockPortfolioId = portfolioId ?? Guid.NewGuid();
            var securityIds = new[]
            {
                Guid.NewGuid(), // SHOP.TO
                Guid.NewGuid(), // RY.TO
                Guid.NewGuid(), // CNR.TO
                Guid.NewGuid(), // GOOGL
                Guid.NewGuid()  // MSFT
            };

            // Generate mock transactions over the last 6 months
            for (int i = 0; i < 50; i++)
            {
                var transactionDate = DateTime.Today.AddDays(-random.Next(0, 180));
                var securityId = securityIds[random.Next(securityIds.Length)];
                var transactionType = (TransactionType)random.Next(0, 4);
                var quantity = random.Next(10, 1000);
                var price = (decimal)(random.NextDouble() * 500 + 10);

                var transaction = new Transaction(
                    mockPortfolioId,
                    securityId,
                    transactionType,
                    quantity,
                    price,
                    transactionDate
                )
                {
                    Id = Guid.NewGuid(),
                    Commission = (decimal)(random.NextDouble() * 50),
                    OtherFees = (decimal)(random.NextDouble() * 10),
                    Notes = i % 5 == 0 ? "Sample transaction note / Note de transaction exemple" : null
                };

                transactions.Add(transaction);
            }

            return transactions;
        }

        private string GetSecuritySymbol(Guid securityId)
        {
            // Mock symbol mapping - in production, this would come from database
            var symbols = new[] { "SHOP.TO", "RY.TO", "CNR.TO", "GOOGL", "MSFT" };
            var hash = Math.Abs(securityId.GetHashCode());
            return symbols[hash % symbols.Length];
        }

        private string GetSecurityName(Guid securityId)
        {
            var symbol = GetSecuritySymbol(securityId);
            return symbol switch
            {
                "SHOP.TO" => "Shopify Inc",
                "RY.TO" => "Royal Bank of Canada",
                "CNR.TO" => "Canadian National Railway",
                "GOOGL" => "Alphabet Inc",
                "MSFT" => "Microsoft Corporation",
                _ => "Unknown Security"
            };
        }

        private DateTime CalculateSettlementDate(DateTime transactionDate)
        {
            // T+2 settlement for most securities
            var settlementDate = transactionDate.AddDays(2);
            
            // Skip weekends
            while (settlementDate.DayOfWeek == DayOfWeek.Saturday || settlementDate.DayOfWeek == DayOfWeek.Sunday)
            {
                settlementDate = settlementDate.AddDays(1);
            }
            
            return settlementDate;
        }

        #endregion
    }

    #region DTOs

    public class TransactionSummary
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public Guid SecurityId { get; set; }
        public string SecuritySymbol { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class TransactionDetail : TransactionSummary
    {
        public string SecurityName { get; set; } = string.Empty;
        public decimal Commission { get; set; }
        public decimal OtherFees { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateTransactionRequest
    {
        [Required]
        public Guid PortfolioId { get; set; }

        [Required]
        public Guid SecurityId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Commission { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OtherFees { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class TransactionStatistics
    {
        public Guid? PortfolioId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalBuyTransactions { get; set; }
        public int TotalSellTransactions { get; set; }
        public int TotalDividendTransactions { get; set; }
        public decimal TotalBuyAmount { get; set; }
        public decimal TotalSellAmount { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TotalFees { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    #endregion
}