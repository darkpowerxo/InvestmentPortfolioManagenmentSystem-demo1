using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InvestmentPortfolioManager.Application.DTOs;
using InvestmentPortfolioManager.API.Extensions;
using InvestmentPortfolioManager.Domain.Enums;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Controller for managing securities (stocks, bonds, ETFs, etc.)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Securities")]
    public class SecuritiesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public SecuritiesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all securities with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<SearchResultDto<SecurityDto>>>> GetSecurities(
            [FromQuery] string? searchTerm = null,
            [FromQuery] SecurityType? type = null,
            [FromQuery] string? exchange = null,
            [FromQuery] string? sector = null,
            [FromQuery] string? currency = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = "symbol",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetAllWithMarketDataAsync();

                // Apply filtering
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    securities = securities.Where(s => 
                        s.Symbol.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (s.Sector != null && s.Sector.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (s.Industry != null && s.Industry.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
                }

                if (type.HasValue)
                {
                    securities = securities.Where(s => s.Type == type.Value).ToList();
                }

                if (!string.IsNullOrWhiteSpace(exchange))
                {
                    securities = securities.Where(s => s.Exchange.Equals(exchange, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(sector))
                {
                    securities = securities.Where(s => s.Sector != null && s.Sector.Equals(sector, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(currency))
                {
                    securities = securities.Where(s => s.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (minPrice.HasValue)
                {
                    securities = securities.Where(s => s.CurrentPrice >= minPrice.Value).ToList();
                }

                if (maxPrice.HasValue)
                {
                    securities = securities.Where(s => s.CurrentPrice <= maxPrice.Value).ToList();
                }

                // Apply sorting
                securities = sortBy?.ToLower() switch
                {
                    "symbol" => sortDescending ? securities.OrderByDescending(s => s.Symbol).ToList() : securities.OrderBy(s => s.Symbol).ToList(),
                    "name" => sortDescending ? securities.OrderByDescending(s => s.Name).ToList() : securities.OrderBy(s => s.Name).ToList(),
                    "type" => sortDescending ? securities.OrderByDescending(s => s.Type).ToList() : securities.OrderBy(s => s.Type).ToList(),
                    "exchange" => sortDescending ? securities.OrderByDescending(s => s.Exchange).ToList() : securities.OrderBy(s => s.Exchange).ToList(),
                    "sector" => sortDescending ? securities.OrderByDescending(s => s.Sector).ToList() : securities.OrderBy(s => s.Sector).ToList(),
                    "price" => sortDescending ? securities.OrderByDescending(s => s.CurrentPrice).ToList() : securities.OrderBy(s => s.CurrentPrice).ToList(),
                    "currency" => sortDescending ? securities.OrderByDescending(s => s.Currency).ToList() : securities.OrderBy(s => s.Currency).ToList(),
                    _ => securities.OrderBy(s => s.Symbol).ToList()
                };

                // Apply pagination
                var totalCount = securities.Count;
                var pagedSecurities = securities.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var securityDtos = pagedSecurities.Select(s => s.ToDto()).ToList();

                var searchResult = new SearchResultDto<SecurityDto>(
                    Items: securityDtos,
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
                return Error<SearchResultDto<SecurityDto>>($"Failed to retrieve securities: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a specific security by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<SecurityDto>>> GetSecurity(int id)
        {
            try
            {
                var security = await _unitOfWork.Securities.GetByIdWithMarketDataAsync(id);
                if (security == null)
                {
                    return NotFound<SecurityDto>($"Security with ID {id} not found");
                }

                return Success(security.ToDto());
            }
            catch (Exception ex)
            {
                return Error<SecurityDto>($"Failed to retrieve security: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a security by symbol
        /// </summary>
        [HttpGet("by-symbol/{symbol}")]
        public async Task<ActionResult<ApiResponseDto<SecurityDto>>> GetSecurityBySymbol(string symbol)
        {
            try
            {
                var security = await _unitOfWork.Securities.GetBySymbolAsync(symbol);
                if (security == null)
                {
                    return NotFound<SecurityDto>($"Security with symbol {symbol} not found");
                }

                return Success(security.ToDto());
            }
            catch (Exception ex)
            {
                return Error<SecurityDto>($"Failed to retrieve security: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get securities by exchange
        /// </summary>
        [HttpGet("by-exchange/{exchange}")]
        public async Task<ActionResult<ApiResponseDto<List<SecurityDto>>>> GetSecuritiesByExchange(string exchange)
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetByExchangeAsync(exchange);
                var securityDtos = securities.Select(s => s.ToDto()).ToList();

                return Success(securityDtos);
            }
            catch (Exception ex)
            {
                return Error<List<SecurityDto>>($"Failed to retrieve securities: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get securities by sector
        /// </summary>
        [HttpGet("by-sector/{sector}")]
        public async Task<ActionResult<ApiResponseDto<List<SecurityDto>>>> GetSecuritiesBySector(string sector)
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetBySectorAsync(sector);
                var securityDtos = securities.Select(s => s.ToDto()).ToList();

                return Success(securityDtos);
            }
            catch (Exception ex)
            {
                return Error<List<SecurityDto>>($"Failed to retrieve securities: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get securities by type
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<ActionResult<ApiResponseDto<List<SecurityDto>>>> GetSecuritiesByType(SecurityType type)
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetByTypeAsync(type);
                var securityDtos = securities.Select(s => s.ToDto()).ToList();

                return Success(securityDtos);
            }
            catch (Exception ex)
            {
                return Error<List<SecurityDto>>($"Failed to retrieve securities: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Create a new security
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<SecurityDto>>> CreateSecurity([FromBody] CreateSecurityDto createSecurityDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<SecurityDto>(validationErrors);
            }

            try
            {
                // Check if security with symbol already exists
                var existingSecurity = await _unitOfWork.Securities.GetBySymbolAsync(createSecurityDto.Symbol);
                if (existingSecurity != null)
                {
                    return Error<SecurityDto>("A security with this symbol already exists", statusCode: 409);
                }

                var security = createSecurityDto.ToEntity();
                await _unitOfWork.Securities.AddAsync(security);
                await _unitOfWork.SaveChangesAsync();

                return Success(security.ToDto(), "Security created successfully");
            }
            catch (Exception ex)
            {
                return Error<SecurityDto>($"Failed to create security: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Update an existing security
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<SecurityDto>>> UpdateSecurity(int id, [FromBody] UpdateSecurityDto updateSecurityDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<SecurityDto>(validationErrors);
            }

            try
            {
                var security = await _unitOfWork.Securities.GetByIdAsync(id);
                if (security == null)
                {
                    return NotFound<SecurityDto>($"Security with ID {id} not found");
                }

                security.UpdateFromDto(updateSecurityDto);
                await _unitOfWork.Securities.UpdateAsync(security);
                await _unitOfWork.SaveChangesAsync();

                return Success(security.ToDto(), "Security updated successfully");
            }
            catch (Exception ex)
            {
                return Error<SecurityDto>($"Failed to update security: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a security
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteSecurity(int id)
        {
            try
            {
                var security = await _unitOfWork.Securities.GetByIdAsync(id);
                if (security == null)
                {
                    return NotFound<object>($"Security with ID {id} not found");
                }

                // Check if security has associated positions
                var positions = await _unitOfWork.Positions.GetBySecurityIdAsync(id);
                if (positions.Any())
                {
                    return Error<object>("Cannot delete security with existing positions", statusCode: 409);
                }

                await _unitOfWork.Securities.DeleteAsync(security);
                await _unitOfWork.SaveChangesAsync();

                return Success<object>(null, "Security deleted successfully");
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to delete security: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get unique sectors
        /// </summary>
        [HttpGet("sectors")]
        public async Task<ActionResult<ApiResponseDto<List<string>>>> GetSectors()
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetAllAsync();
                var sectors = securities
                    .Where(s => !string.IsNullOrWhiteSpace(s.Sector))
                    .Select(s => s.Sector!)
                    .Distinct()
                    .OrderBy(sector => sector)
                    .ToList();

                return Success(sectors);
            }
            catch (Exception ex)
            {
                return Error<List<string>>($"Failed to retrieve sectors: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get unique exchanges
        /// </summary>
        [HttpGet("exchanges")]
        public async Task<ActionResult<ApiResponseDto<List<string>>>> GetExchanges()
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetAllAsync();
                var exchanges = securities
                    .Select(s => s.Exchange)
                    .Distinct()
                    .OrderBy(exchange => exchange)
                    .ToList();

                return Success(exchanges);
            }
            catch (Exception ex)
            {
                return Error<List<string>>($"Failed to retrieve exchanges: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get high-priced securities above threshold
        /// </summary>
        [HttpGet("high-priced")]
        public async Task<ActionResult<ApiResponseDto<List<SecurityDto>>>> GetHighPricedSecurities([FromQuery] decimal threshold = 100)
        {
            try
            {
                var securities = await _unitOfWork.Securities.GetHighPricedSecuritiesAsync(threshold);
                var securityDtos = securities.Select(s => s.ToDto()).ToList();

                return Success(securityDtos);
            }
            catch (Exception ex)
            {
                return Error<List<SecurityDto>>($"Failed to retrieve high-priced securities: {ex.Message}", statusCode: 500);
            }
        }
    }
}