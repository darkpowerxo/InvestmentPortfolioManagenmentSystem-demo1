using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InvestmentPortfolioManager.Application.DTOs;
using InvestmentPortfolioManager.API.Extensions;
using InvestmentPortfolioManager.Domain.Enums;
using System.Linq;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Controller for managing portfolio transactions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Transactions")]
    public class TransactionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all transactions with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<SearchResultDto<TransactionDto>>>> GetTransactions(
            [FromQuery] int? portfolioId = null,
            [FromQuery] int? securityId = null,
            [FromQuery] string? securitySymbol = null,
            [FromQuery] TransactionType? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? sortBy = "transactionDate",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
                List<InvestmentPortfolioManager.Domain.Entities.Transaction> transactions = allTransactions.ToList();

                // Apply filtering
                if (portfolioId.HasValue)
                {
                    transactions = transactions.Where(t => t.PortfolioId == portfolioId.Value).ToList();
                }

                if (securityId.HasValue)
                {
                    transactions = transactions.Where(t => t.SecurityId == securityId.Value).ToList();
                }

                if (!string.IsNullOrWhiteSpace(securitySymbol))
                {
                    transactions = transactions.Where(t => t.Security?.Symbol.Equals(securitySymbol, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                if (type.HasValue)
                {
                    transactions = transactions.Where(t => t.Type == type.Value).ToList();
                }

                if (startDate.HasValue)
                {
                    transactions = transactions.Where(t => t.TransactionDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    transactions = transactions.Where(t => t.TransactionDate <= endDate.Value).ToList();
                }

                if (minAmount.HasValue)
                {
                    transactions = transactions.Where(t => Math.Abs(t.NetAmount) >= minAmount.Value).ToList();
                }

                if (maxAmount.HasValue)
                {
                    transactions = transactions.Where(t => Math.Abs(t.NetAmount) <= maxAmount.Value).ToList();
                }

                // Apply sorting
                transactions = sortBy?.ToLower() switch
                {
                    "transactiondate" => sortDescending ? transactions.OrderByDescending(t => t.TransactionDate).ToList() : transactions.OrderBy(t => t.TransactionDate).ToList(),
                    "symbol" => sortDescending ? transactions.OrderByDescending(t => t.Security?.Symbol).ToList() : transactions.OrderBy(t => t.Security?.Symbol).ToList(),
                    "type" => sortDescending ? transactions.OrderByDescending(t => t.Type).ToList() : transactions.OrderBy(t => t.Type).ToList(),
                    "quantity" => sortDescending ? transactions.OrderByDescending(t => t.Quantity).ToList() : transactions.OrderBy(t => t.Quantity).ToList(),
                    "price" => sortDescending ? transactions.OrderByDescending(t => t.Price).ToList() : transactions.OrderBy(t => t.Price).ToList(),
                    "netamount" => sortDescending ? transactions.OrderByDescending(t => t.NetAmount).ToList() : transactions.OrderBy(t => t.NetAmount).ToList(),
                    "commission" => sortDescending ? transactions.OrderByDescending(t => t.Commission).ToList() : transactions.OrderBy(t => t.Commission).ToList(),
                    "createdat" => sortDescending ? transactions.OrderByDescending(t => t.CreatedAt).ToList() : transactions.OrderBy(t => t.CreatedAt).ToList(),
                    _ => transactions.OrderByDescending(t => t.TransactionDate).ToList()
                };

                // Apply pagination
                var totalCount = transactions.Count;
                var pagedTransactions = transactions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var transactionDtos = pagedTransactions.Select(t => t.ToDto()).ToList();

                var searchResult = new SearchResultDto<TransactionDto>(
                    Items: transactionDtos,
                    TotalCount: totalCount,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    HasNextPage: pageNumber * pageSize < totalCount,
                    HasPreviousPage: pageNumber > 1
                );

                return SearchSuccess(searchResult);
            }
            catch (Exception ex)
            {
                return Error<SearchResultDto<TransactionDto>>($"Failed to retrieve transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a specific transaction by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<TransactionDto>>> GetTransaction(int id)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound<TransactionDto>($"Transaction with ID {id} not found");
                }

                return Success(transaction.ToDto());
            }
            catch (Exception ex)
            {
                return Error<TransactionDto>($"Failed to retrieve transaction: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get transactions for a specific portfolio
        /// </summary>
        [HttpGet("by-portfolio/{portfolioId:int}")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetTransactionsByPortfolio(int portfolioId)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsByPortfolioIdAsync(portfolioId);
                var transactionDtos = transactions.Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get transactions for a specific security
        /// </summary>
        [HttpGet("by-security/{securityId:int}")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetTransactionsBySecurity(int securityId)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsBySecurityIdAsync(securityId);
                var transactionDtos = transactions.Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get transactions by type
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetTransactionsByType(TransactionType type)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsByTypeAsync(type);
                var transactionDtos = transactions.Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get transactions within a date range
        /// </summary>
        [HttpGet("by-date-range")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsByDateRangeAsync(startDate, endDate);
                var transactionDtos = transactions.Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<TransactionDto>>> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<TransactionDto>(validationErrors);
            }

            try
            {
                // Verify portfolio exists
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(createTransactionDto.PortfolioId);
                if (portfolio == null)
                {
                    return Error<TransactionDto>("Portfolio not found", statusCode: 404);
                }

                // Verify security exists
                var security = await _unitOfWork.Securities.GetByIdAsync(createTransactionDto.SecurityId);
                if (security == null)
                {
                    return Error<TransactionDto>("Security not found", statusCode: 404);
                }

                var transaction = createTransactionDto.ToEntity();
                await _unitOfWork.Transactions.AddAsync(transaction);

                // Update or create position based on transaction
                await UpdatePositionFromTransaction(transaction);

                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var createdTransaction = await _unitOfWork.Transactions.GetByIdAsync(transaction.Id);
                return Success(createdTransaction!.ToDto(), "Transaction created successfully");
            }
            catch (Exception ex)
            {
                return Error<TransactionDto>($"Failed to create transaction: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Update an existing transaction
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<TransactionDto>>> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateTransactionDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<TransactionDto>(validationErrors);
            }

            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound<TransactionDto>($"Transaction with ID {id} not found");
                }

                transaction.UpdateFromDto(updateTransactionDto);
                await _unitOfWork.Transactions.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var updatedTransaction = await _unitOfWork.Transactions.GetByIdAsync(id);
                return Success(updatedTransaction!.ToDto(), "Transaction updated successfully");
            }
            catch (Exception ex)
            {
                return Error<TransactionDto>($"Failed to update transaction: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a transaction
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteTransaction(int id)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound<object>($"Transaction with ID {id} not found");
                }

                await _unitOfWork.Transactions.DeleteAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                return Success<object>(new { }, "Transaction deleted successfully");
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to delete transaction: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get large transactions above a threshold
        /// </summary>
        [HttpGet("large")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetLargeTransactions([FromQuery] decimal threshold = 100000)
        {
            try
            {
                var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
                var largeTransactions = allTransactions.Where(t => Math.Abs(t.Quantity * t.Price) >= threshold);
                var transactionDtos = largeTransactions.Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve large transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get recent transactions within specified days
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionDto>>>> GetRecentTransactions([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                var endDate = DateTime.UtcNow;
                
                var recentTransactions = await _unitOfWork.Transactions.GetTransactionsByDateRangeAsync(startDate, endDate);
                var transactionDtos = recentTransactions.OrderByDescending(t => t.TransactionDate).Select(t => t.ToDto()).ToList();

                return Success(transactionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<TransactionDto>>($"Failed to retrieve recent transactions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get transaction summary for a date range
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetTransactionSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? portfolioId = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var transactions = await _unitOfWork.Transactions.GetTransactionsByDateRangeAsync(start, end);

                if (portfolioId.HasValue)
                {
                    transactions = transactions.Where(t => t.PortfolioId == portfolioId.Value).ToList();
                }

                var transactionsList = transactions.ToList();
                var summary = new
                {
                    TotalTransactions = transactionsList.Count,
                    TotalVolume = transactionsList.Sum(t => Math.Abs(t.NetAmount)),
                    TotalCommissions = transactionsList.Sum(t => t.Commission),
                    TotalTaxes = transactionsList.Sum(t => t.Tax),
                    ByType = transactionsList.GroupBy(t => t.Type)
                        .Select(g => new
                        {
                            Type = g.Key.ToString(),
                            Count = g.Count(),
                            Volume = g.Sum(t => Math.Abs(t.NetAmount))
                        }).ToList(),
                    Period = new
                    {
                        StartDate = start,
                        EndDate = end
                    }
                };

                return Success<object>(summary);
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to generate transaction summary: {ex.Message}", statusCode: 500);
            }
        }

        private async Task UpdatePositionFromTransaction(Domain.Entities.Transaction transaction)
        {
            // Get existing position for this portfolio-security combination
            var positions = await _unitOfWork.Positions.GetPositionsByPortfolioIdAsync(transaction.PortfolioId);
            var existingPosition = positions.FirstOrDefault(p => p.SecurityId == transaction.SecurityId);

            if (existingPosition == null && (transaction.Type == TransactionType.Buy || transaction.Type == TransactionType.StockSplit))
            {
                // Create new position for buy transactions
                var newPosition = new Domain.Entities.Position
                {
                    PortfolioId = transaction.PortfolioId,
                    SecurityId = transaction.SecurityId,
                    Quantity = transaction.Quantity,
                    AveragePrice = transaction.Price,
                    FirstPurchaseDate = transaction.TransactionDate,
                    LastTransactionDate = transaction.TransactionDate
                };

                await _unitOfWork.Positions.AddAsync(newPosition);
            }
            else if (existingPosition != null)
            {
                // Update existing position
                switch (transaction.Type)
                {
                    case TransactionType.Buy:
                        var newTotalCost = (existingPosition.Quantity * existingPosition.AveragePrice) + (transaction.Quantity * transaction.Price);
                        var newTotalQuantity = existingPosition.Quantity + transaction.Quantity;
                        existingPosition.AveragePrice = newTotalQuantity > 0 ? newTotalCost / newTotalQuantity : 0;
                        existingPosition.Quantity = newTotalQuantity;
                        break;

                    case TransactionType.Sell:
                        existingPosition.Quantity -= transaction.Quantity;
                        if (existingPosition.Quantity <= 0)
                        {
                            await _unitOfWork.Positions.DeleteAsync(existingPosition);
                            return;
                        }
                        break;

                    case TransactionType.StockSplit:
                        existingPosition.Quantity += transaction.Quantity;
                        // Average price adjusts automatically due to split
                        break;
                }

                existingPosition.LastTransactionDate = transaction.TransactionDate;
                await _unitOfWork.Positions.UpdateAsync(existingPosition);
            }
        }
    }
}