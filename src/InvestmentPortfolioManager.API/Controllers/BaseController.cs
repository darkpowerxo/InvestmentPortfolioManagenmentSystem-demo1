using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Application.DTOs;

namespace InvestmentPortfolioManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected ActionResult<ApiResponseDto<T>> Success<T>(T data, string? message = null)
        {
            return Ok(new ApiResponseDto<T>(
                Data: data,
                Success: true,
                Message: message,
                Errors: null,
                Timestamp: DateTime.UtcNow
            ));
        }

        protected ActionResult<ApiResponseDto<T>> Error<T>(string message, List<string>? errors = null, int statusCode = 400)
        {
            var response = new ApiResponseDto<T>(
                Data: default(T)!,
                Success: false,
                Message: message,
                Errors: errors,
                Timestamp: DateTime.UtcNow
            );

            return statusCode switch
            {
                400 => BadRequest(response),
                401 => Unauthorized(response),
                403 => StatusCode(403, response),
                404 => NotFound(response),
                409 => Conflict(response),
                422 => UnprocessableEntity(response),
                500 => StatusCode(500, response),
                _ => StatusCode(statusCode, response)
            };
        }

        protected ActionResult<ApiResponseDto<T>> NotFound<T>(string message = "Resource not found")
        {
            return Error<T>(message, statusCode: 404);
        }

        protected ActionResult<ApiResponseDto<T>> ValidationError<T>(List<ValidationErrorDto> validationErrors)
        {
            var errors = validationErrors.Select(v => $"{v.Field}: {v.Message}").ToList();
            return Error<T>("Validation failed", errors, 422);
        }

        protected ActionResult<ApiResponseDto<SearchResultDto<T>>> SearchSuccess<T>(SearchResultDto<T> searchResult, string? message = null)
        {
            return Success(searchResult, message);
        }

        protected FilterCriteriaDto GetFilterCriteria(
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sectors = null,
            string? assetTypes = null,
            decimal? minValue = null,
            decimal? maxValue = null,
            string? sortBy = null,
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 20)
        {
            // Limit page size to prevent abuse
            if (pageSize > 100) pageSize = 100;
            if (pageSize < 1) pageSize = 20;
            if (pageNumber < 1) pageNumber = 1;

            return new FilterCriteriaDto(
                SearchTerm: searchTerm,
                StartDate: startDate,
                EndDate: endDate,
                Sectors: sectors?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                AssetTypes: assetTypes?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                MinValue: minValue,
                MaxValue: maxValue,
                SortBy: sortBy,
                SortDescending: sortDescending,
                PageNumber: pageNumber,
                PageSize: pageSize
            );
        }

        protected List<ValidationErrorDto> ValidateModel()
        {
            var errors = new List<ValidationErrorDto>();

            if (!ModelState.IsValid)
            {
                foreach (var (key, value) in ModelState)
                {
                    if (value.Errors.Count > 0)
                    {
                        errors.Add(new ValidationErrorDto(
                            Field: key,
                            Message: value.Errors.First().ErrorMessage,
                            AttemptedValue: value.AttemptedValue
                        ));
                    }
                }
            }

            return errors;
        }
    }
}