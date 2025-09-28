using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InvestmentPortfolioManager.Application.DTOs;
using InvestmentPortfolioManager.API.Extensions;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Controller for managing portfolio positions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Positions")]
    public class PositionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public PositionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all positions with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<SearchResultDto<PositionDto>>>> GetPositions(
            [FromQuery] int? portfolioId = null,
            [FromQuery] int? securityId = null,
            [FromQuery] string? securitySymbol = null,
            [FromQuery] decimal? minValue = null,
            [FromQuery] decimal? maxValue = null,
            [FromQuery] decimal? minWeight = null,
            [FromQuery] decimal? maxWeight = null,
            [FromQuery] string? sortBy = "marketValue",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var positions = await _unitOfWork.Positions.GetAllWithDetailsAsync();

                // Apply filtering
                if (portfolioId.HasValue)
                {
                    positions = positions.Where(p => p.PortfolioId == portfolioId.Value).ToList();
                }

                if (securityId.HasValue)
                {
                    positions = positions.Where(p => p.SecurityId == securityId.Value).ToList();
                }

                if (!string.IsNullOrWhiteSpace(securitySymbol))
                {
                    positions = positions.Where(p => p.Security?.Symbol.Equals(securitySymbol, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                // Calculate market values for filtering
                var positionsWithValues = positions.Select(p => new
                {
                    Position = p,
                    MarketValue = p.Quantity * (p.Security?.CurrentPrice ?? 0),
                    PortfolioTotalValue = p.Portfolio?.Positions?.Sum(pos => pos.Quantity * (pos.Security?.CurrentPrice ?? 0)) ?? 0
                }).ToList();

                // Calculate weights and apply value filters
                positionsWithValues = positionsWithValues.Select(p => new
                {
                    p.Position,
                    p.MarketValue,
                    Weight = p.PortfolioTotalValue > 0 ? (p.MarketValue / p.PortfolioTotalValue) * 100 : 0
                }).ToList();

                if (minValue.HasValue)
                {
                    positionsWithValues = positionsWithValues.Where(p => p.MarketValue >= minValue.Value).ToList();
                }

                if (maxValue.HasValue)
                {
                    positionsWithValues = positionsWithValues.Where(p => p.MarketValue <= maxValue.Value).ToList();
                }

                if (minWeight.HasValue)
                {
                    positionsWithValues = positionsWithValues.Where(p => p.Weight >= minWeight.Value).ToList();
                }

                if (maxWeight.HasValue)
                {
                    positionsWithValues = positionsWithValues.Where(p => p.Weight <= maxWeight.Value).ToList();
                }

                // Apply sorting
                positionsWithValues = sortBy?.ToLower() switch
                {
                    "symbol" => sortDescending ? positionsWithValues.OrderByDescending(p => p.Position.Security?.Symbol).ToList() : positionsWithValues.OrderBy(p => p.Position.Security?.Symbol).ToList(),
                    "quantity" => sortDescending ? positionsWithValues.OrderByDescending(p => p.Position.Quantity).ToList() : positionsWithValues.OrderBy(p => p.Position.Quantity).ToList(),
                    "averageprice" => sortDescending ? positionsWithValues.OrderByDescending(p => p.Position.AveragePrice).ToList() : positionsWithValues.OrderBy(p => p.Position.AveragePrice).ToList(),
                    "marketvalue" => sortDescending ? positionsWithValues.OrderByDescending(p => p.MarketValue).ToList() : positionsWithValues.OrderBy(p => p.MarketValue).ToList(),
                    "weight" => sortDescending ? positionsWithValues.OrderByDescending(p => p.Weight).ToList() : positionsWithValues.OrderBy(p => p.Weight).ToList(),
                    "unrealizedgain" => sortDescending ? positionsWithValues.OrderByDescending(p => p.MarketValue - (p.Position.Quantity * p.Position.AveragePrice)).ToList() : positionsWithValues.OrderBy(p => p.MarketValue - (p.Position.Quantity * p.Position.AveragePrice)).ToList(),
                    "lasttransaction" => sortDescending ? positionsWithValues.OrderByDescending(p => p.Position.LastTransactionDate).ToList() : positionsWithValues.OrderBy(p => p.Position.LastTransactionDate).ToList(),
                    _ => positionsWithValues.OrderByDescending(p => p.MarketValue).ToList()
                };

                // Apply pagination
                var totalCount = positionsWithValues.Count;
                var pagedPositions = positionsWithValues.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var positionDtos = pagedPositions.Select(p =>
                {
                    var dto = p.Position.ToDto();
                    return dto with { Weight = p.Weight }; // Update weight with calculated value
                }).ToList();

                var searchResult = new SearchResultDto<PositionDto>(
                    Items: positionDtos,
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
                return Error<SearchResultDto<PositionDto>>($"Failed to retrieve positions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a specific position by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<PositionDto>>> GetPosition(int id)
        {
            try
            {
                var position = await _unitOfWork.Positions.GetByIdWithDetailsAsync(id);
                if (position == null)
                {
                    return NotFound<PositionDto>($"Position with ID {id} not found");
                }

                return Success(position.ToDto());
            }
            catch (Exception ex)
            {
                return Error<PositionDto>($"Failed to retrieve position: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get positions for a specific portfolio
        /// </summary>
        [HttpGet("by-portfolio/{portfolioId:int}")]
        public async Task<ActionResult<ApiResponseDto<List<PositionDto>>>> GetPositionsByPortfolio(int portfolioId)
        {
            try
            {
                var positions = await _unitOfWork.Positions.GetByPortfolioIdAsync(portfolioId);
                
                // Calculate portfolio total for weights
                var portfolioTotal = positions.Sum(p => p.Quantity * (p.Security?.CurrentPrice ?? 0));

                var positionDtos = positions.Select(p =>
                {
                    var dto = p.ToDto();
                    var weight = portfolioTotal > 0 ? ((p.Quantity * (p.Security?.CurrentPrice ?? 0)) / portfolioTotal) * 100 : 0;
                    return dto with { Weight = weight };
                }).ToList();

                return Success(positionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PositionDto>>($"Failed to retrieve positions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get positions for a specific security
        /// </summary>
        [HttpGet("by-security/{securityId:int}")]
        public async Task<ActionResult<ApiResponseDto<List<PositionDto>>>> GetPositionsBySecurity(int securityId)
        {
            try
            {
                var positions = await _unitOfWork.Positions.GetBySecurityIdAsync(securityId);
                var positionDtos = positions.Select(p => p.ToDto()).ToList();

                return Success(positionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PositionDto>>($"Failed to retrieve positions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get concentrated positions above a weight threshold
        /// </summary>
        [HttpGet("concentrated")]
        public async Task<ActionResult<ApiResponseDto<List<PositionDto>>>> GetConcentratedPositions([FromQuery] decimal weightThreshold = 5.0m)
        {
            try
            {
                // Get all positions and filter for large ones based on quantity value
                var allPositions = await _unitOfWork.Positions.GetAllAsync();
                var concentratedPositions = allPositions.Where(p => 
                    p.Quantity * p.AverageCost >= weightThreshold * 10000).ToList(); // Simplified logic
                var positionDtos = concentratedPositions.Select(p => p.ToDto()).ToList();

                return Success(positionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PositionDto>>($"Failed to retrieve concentrated positions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Create a new position
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<PositionDto>>> CreatePosition([FromBody] CreatePositionDto createPositionDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<PositionDto>(validationErrors);
            }

            try
            {
                // Verify portfolio exists
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(createPositionDto.PortfolioId);
                if (portfolio == null)
                {
                    return Error<PositionDto>("Portfolio not found", statusCode: 404);
                }

                // Verify security exists
                var security = await _unitOfWork.Securities.GetByIdAsync(createPositionDto.SecurityId);
                if (security == null)
                {
                    return Error<PositionDto>("Security not found", statusCode: 404);
                }

                // Check if position already exists for this portfolio-security combination
                var existingPositions = await _unitOfWork.Positions.GetByPortfolioIdAsync(createPositionDto.PortfolioId);
                if (existingPositions.Any(p => p.SecurityId == createPositionDto.SecurityId))
                {
                    return Error<PositionDto>("A position already exists for this security in this portfolio", statusCode: 409);
                }

                var position = createPositionDto.ToEntity();
                await _unitOfWork.Positions.AddAsync(position);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var createdPosition = await _unitOfWork.Positions.GetByIdWithDetailsAsync(position.Id);
                return Success(createdPosition!.ToDto(), "Position created successfully");
            }
            catch (Exception ex)
            {
                return Error<PositionDto>($"Failed to create position: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Update an existing position
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<PositionDto>>> UpdatePosition(int id, [FromBody] UpdatePositionDto updatePositionDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<PositionDto>(validationErrors);
            }

            try
            {
                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null)
                {
                    return NotFound<PositionDto>($"Position with ID {id} not found");
                }

                position.UpdateFromDto(updatePositionDto);
                await _unitOfWork.Positions.UpdateAsync(position);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                var updatedPosition = await _unitOfWork.Positions.GetByIdWithDetailsAsync(id);
                return Success(updatedPosition!.ToDto(), "Position updated successfully");
            }
            catch (Exception ex)
            {
                return Error<PositionDto>($"Failed to update position: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a position
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeletePosition(int id)
        {
            try
            {
                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null)
                {
                    return NotFound<object>($"Position with ID {id} not found");
                }

                await _unitOfWork.Positions.DeleteAsync(position);
                await _unitOfWork.SaveChangesAsync();

                return Success<object>(null, "Position deleted successfully");
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to delete position: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get top positions by market value
        /// </summary>
        [HttpGet("top")]
        public async Task<ActionResult<ApiResponseDto<List<PositionDto>>>> GetTopPositions([FromQuery] int count = 10)
        {
            try
            {
                var topPositions = await _unitOfWork.Positions.GetTopPositionsByValueAsync(count);
                var positionDtos = topPositions.Select(p => p.ToDto()).ToList();

                return Success(positionDtos);
            }
            catch (Exception ex)
            {
                return Error<List<PositionDto>>($"Failed to retrieve top positions: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get positions with unrealized gains/losses above threshold
        /// </summary>
        [HttpGet("gainers-losers")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetGainersAndLosers([FromQuery] decimal threshold = 10000)
        {
            try
            {
                var allPositions = await _unitOfWork.Positions.GetAllWithDetailsAsync();
                
                var positionsWithGains = allPositions.Select(p => new
                {
                    Position = p,
                    MarketValue = p.Quantity * (p.Security?.CurrentPrice ?? 0),
                    CostBasis = p.Quantity * p.AveragePrice,
                    UnrealizedGain = (p.Quantity * (p.Security?.CurrentPrice ?? 0)) - (p.Quantity * p.AveragePrice)
                }).ToList();

                var gainers = positionsWithGains
                    .Where(p => p.UnrealizedGain >= threshold)
                    .OrderByDescending(p => p.UnrealizedGain)
                    .Select(p => p.Position.ToDto())
                    .ToList();

                var losers = positionsWithGains
                    .Where(p => p.UnrealizedGain <= -threshold)
                    .OrderBy(p => p.UnrealizedGain)
                    .Select(p => p.Position.ToDto())
                    .ToList();

                var result = new
                {
                    Gainers = gainers,
                    Losers = losers,
                    Threshold = threshold
                };

                return Success<object>(result);
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to retrieve gainers and losers: {ex.Message}", statusCode: 500);
            }
        }
    }
}