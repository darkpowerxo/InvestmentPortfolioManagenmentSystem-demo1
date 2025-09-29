using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InvestmentPortfolioManager.Application.DTOs;
using InvestmentPortfolioManager.API.Extensions;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Controller for managing investment portfolios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Portfolios")]
    public class PortfoliosController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public PortfoliosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all portfolios with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<SearchResultDto<PortfolioDto>>>> GetPortfolios(
            [FromQuery] string? searchTerm = null,
            [FromQuery] PortfolioType? type = null,
            [FromQuery] int? managerId = null,
            [FromQuery] decimal? minValue = null,
            [FromQuery] decimal? maxValue = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var portfolios = await _unitOfWork.Portfolios.GetAllAsync();

                // Apply filtering
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    portfolios = portfolios.Where(p => 
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
                }

                if (type.HasValue)
                {
                    portfolios = portfolios.Where(p => p.Type == type.Value).ToList();
                }

                if (managerId.HasValue)
                {
                    portfolios = portfolios.Where(p => p.ManagerId == managerId.Value).ToList();
                }

                // Calculate total values for filtering
                var portfoliosWithValues = portfolios.Select(p => new
                {
                    Portfolio = p,
                    TotalValue = p.Positions?.Sum(pos => pos.Quantity * (pos.Security?.CurrentPrice ?? 0)) ?? 0
                }).ToList();

                if (minValue.HasValue)
                {
                    portfoliosWithValues = portfoliosWithValues.Where(p => p.TotalValue >= minValue.Value).ToList();
                }

                if (maxValue.HasValue)
                {
                    portfoliosWithValues = portfoliosWithValues.Where(p => p.TotalValue <= maxValue.Value).ToList();
                }

                // Apply sorting
                portfoliosWithValues = sortBy?.ToLower() switch
                {
                    "name" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.Portfolio.Name).ToList() : portfoliosWithValues.OrderBy(p => p.Portfolio.Name).ToList(),
                    "type" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.Portfolio.Type).ToList() : portfoliosWithValues.OrderBy(p => p.Portfolio.Type).ToList(),
                    "manager" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.Portfolio.Manager?.LastName).ToList() : portfoliosWithValues.OrderBy(p => p.Portfolio.Manager?.LastName).ToList(),
                    "value" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.TotalValue).ToList() : portfoliosWithValues.OrderBy(p => p.TotalValue).ToList(),
                    "cashbalance" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.Portfolio.CashBalance).ToList() : portfoliosWithValues.OrderBy(p => p.Portfolio.CashBalance).ToList(),
                    "createdat" => sortDescending ? portfoliosWithValues.OrderByDescending(p => p.Portfolio.CreatedAt).ToList() : portfoliosWithValues.OrderBy(p => p.Portfolio.CreatedAt).ToList(),
                    _ => portfoliosWithValues.OrderBy(p => p.Portfolio.Name).ToList()
                };

                // Apply pagination
                var totalCount = portfoliosWithValues.Count;
                var pagedPortfolios = portfoliosWithValues.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var portfolioDtos = pagedPortfolios.Select(p => p.Portfolio.ToDto()).ToList();

                var searchResult = new SearchResultDto<PortfolioDto>(
                    Items: portfolioDtos,
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
                return Error<SearchResultDto<PortfolioDto>>($"Failed to retrieve portfolios: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a specific portfolio by ID with full details
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<PortfolioDto>>> GetPortfolio(int id)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(id);
                if (portfolio == null)
                {
                    return NotFound<PortfolioDto>($"Portfolio with ID {id} not found");
                }

                return Success(portfolio.ToDto());
            }
            catch (Exception ex)
            {
                return Error<PortfolioDto>($"Failed to retrieve portfolio: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get portfolios managed by a specific user
        /// </summary>
        [HttpGet("by-manager/{managerId:int}")]
        public async Task<ActionResult<ApiResponseDto<List<PortfolioDto>>>> GetPortfoliosByManager(int managerId)
        {
            try
            {
                var portfolios = await _unitOfWork.Portfolios.GetPortfoliosByManagerIdAsync(managerId);
                var portfolioDtos = portfolios.Select(p => p.ToDto()).ToList();

                return Success(portfolioDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PortfolioDto>>($"Failed to retrieve portfolios: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get portfolios by type
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<ActionResult<ApiResponseDto<List<PortfolioDto>>>> GetPortfoliosByType(PortfolioType type)
        {
            try
            {
                var portfolios = await _unitOfWork.Portfolios.GetPortfoliosByTypeAsync(type);
                var portfolioDtos = portfolios.Select(p => p.ToDto()).ToList();

                return Success(portfolioDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PortfolioDto>>($"Failed to retrieve portfolios: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get portfolio summary (lightweight version)
        /// </summary>
        [HttpGet("{id:int}/summary")]
        public async Task<ActionResult<ApiResponseDto<PortfolioSummaryDto>>> GetPortfolioSummary(int id)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(id);
                if (portfolio == null)
                {
                    return NotFound<PortfolioSummaryDto>($"Portfolio with ID {id} not found");
                }

                return Success(portfolio.ToSummaryDto());
            }
            catch (Exception ex)
            {
                return Error<PortfolioSummaryDto>($"Failed to retrieve portfolio summary: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Create a new portfolio
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<PortfolioDto>>> CreatePortfolio([FromBody] CreatePortfolioDto createPortfolioDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<PortfolioDto>(validationErrors);
            }

            try
            {
                // Verify manager exists
                var manager = await _unitOfWork.Users.GetByIdAsync(createPortfolioDto.ManagerId);
                if (manager == null)
                {
                    return Error<PortfolioDto>("Manager not found", statusCode: 404);
                }

                // Check if portfolio name already exists for this manager
                var existingPortfolios = await _unitOfWork.Portfolios.GetPortfoliosByManagerIdAsync(createPortfolioDto.ManagerId);
                if (existingPortfolios.Any(p => p.Name.Equals(createPortfolioDto.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Error<PortfolioDto>("A portfolio with this name already exists for this manager", statusCode: 409);
                }

                var portfolio = createPortfolioDto.ToEntity();
                await _unitOfWork.Portfolios.AddAsync(portfolio);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var createdPortfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolio.Id);
                return Success(createdPortfolio!.ToDto(), "Portfolio created successfully");
            }
            catch (Exception ex)
            {
                return Error<PortfolioDto>($"Failed to create portfolio: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Update an existing portfolio
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<PortfolioDto>>> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<PortfolioDto>(validationErrors);
            }

            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(id);
                if (portfolio == null)
                {
                    return NotFound<PortfolioDto>($"Portfolio with ID {id} not found");
                }

                // Verify manager exists if changing
                if (updatePortfolioDto.ManagerId.HasValue)
                {
                    var manager = await _unitOfWork.Users.GetByIdAsync(updatePortfolioDto.ManagerId.Value);
                    if (manager == null)
                    {
                        return Error<PortfolioDto>("Manager not found", statusCode: 404);
                    }
                }

                portfolio.UpdateFromDto(updatePortfolioDto);
                await _unitOfWork.Portfolios.UpdateAsync(portfolio);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var updatedPortfolio = await _unitOfWork.Portfolios.GetByIdAsync(id);
                return Success(updatedPortfolio!.ToDto(), "Portfolio updated successfully");
            }
            catch (Exception ex)
            {
                return Error<PortfolioDto>($"Failed to update portfolio: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a portfolio
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeletePortfolio(int id)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(id);
                if (portfolio == null)
                {
                    return NotFound<object>($"Portfolio with ID {id} not found");
                }

                // Check if portfolio has positions - get positions separately
                var positions = await _unitOfWork.Positions.GetPositionsByPortfolioIdAsync(id);
                if (positions.Any())
                {
                    return Error<object>("Cannot delete portfolio with existing positions", statusCode: 409);
                }

                await _unitOfWork.Portfolios.DeleteAsync(portfolio);
                await _unitOfWork.SaveChangesAsync();

                return Success<object>(new { }, "Portfolio deleted successfully");
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to delete portfolio: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get large portfolios (above specified threshold)
        /// </summary>
        [HttpGet("large")]
        public async Task<ActionResult<ApiResponseDto<List<PortfolioDto>>>> GetLargePortfolios([FromQuery] decimal threshold = 1000000)
        {
            try
            {
                var allPortfolios = await _unitOfWork.Portfolios.GetAllAsync();
                var largePortfolios = allPortfolios.Where(p => p.CurrentValue >= threshold);
                var portfolioDtos = largePortfolios.Select(p => p.ToDto()).ToList();

                return Success(portfolioDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PortfolioDto>>($"Failed to retrieve large portfolios: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get portfolios with low cash balances
        /// </summary>
        [HttpGet("low-cash")]
        public async Task<ActionResult<ApiResponseDto<List<PortfolioDto>>>> GetPortfoliosWithLowCash([FromQuery] decimal threshold = 10000)
        {
            try
            {
                var allPortfolios = await _unitOfWork.Portfolios.GetAllAsync();
                var lowCashPortfolios = allPortfolios.Where(p => p.CashBalance <= threshold);
                var portfolioDtos = lowCashPortfolios.Select(p => p.ToDto()).ToList();

                return Success(portfolioDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PortfolioDto>>($"Failed to retrieve portfolios with low cash: {ex.Message}", statusCode: 500);
            }
        }
    }
}