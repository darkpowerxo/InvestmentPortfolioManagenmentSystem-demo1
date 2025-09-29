using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using InvestmentPortfolioManager.API.Services;

namespace InvestmentPortfolioManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;

        public LanguageController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        [HttpGet("current")]
        public IActionResult GetCurrentLanguage()
        {
            var currentCulture = _localizationService.GetCurrentCulture();
            return Ok(new { Language = currentCulture });
        }

        [HttpPost("set")]
        public IActionResult SetLanguage([FromBody] SetLanguageRequest request)
        {
            if (string.IsNullOrEmpty(request.Language) || 
                (request.Language != "en" && request.Language != "fr"))
            {
                return BadRequest(_localizationService.GetLocalizedString("InvalidLanguage"));
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(request.Language)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                }
            );

            return Ok(new { 
                Message = _localizationService.GetLocalizedString("LanguageChanged"),
                Language = request.Language 
            });
        }

        [HttpGet("resources")]
        public IActionResult GetResources()
        {
            var resources = new Dictionary<string, string>
            {
                // Common Terms
                { "Portfolio", _localizationService.GetLocalizedString("Portfolio") },
                { "Portfolios", _localizationService.GetLocalizedString("Portfolios") },
                { "Security", _localizationService.GetLocalizedString("Security") },
                { "Securities", _localizationService.GetLocalizedString("Securities") },
                { "Position", _localizationService.GetLocalizedString("Position") },
                { "Positions", _localizationService.GetLocalizedString("Positions") },
                { "Transaction", _localizationService.GetLocalizedString("Transaction") },
                { "Transactions", _localizationService.GetLocalizedString("Transactions") },
                { "User", _localizationService.GetLocalizedString("User") },
                { "Users", _localizationService.GetLocalizedString("Users") },
                
                // Dashboard Terms
                { "Dashboard", _localizationService.GetLocalizedString("Dashboard") },
                { "TotalPortfolioValue", _localizationService.GetLocalizedString("TotalPortfolioValue") },
                { "TodaysChange", _localizationService.GetLocalizedString("TodaysChange") },
                { "TotalPositions", _localizationService.GetLocalizedString("TotalPositions") },
                { "CashBalance", _localizationService.GetLocalizedString("CashBalance") },
                { "PortfolioAllocation", _localizationService.GetLocalizedString("PortfolioAllocation") },
                { "TopPositions", _localizationService.GetLocalizedString("TopPositions") },
                { "RecentTransactions", _localizationService.GetLocalizedString("RecentTransactions") },
                { "PortfolioPerformance", _localizationService.GetLocalizedString("PortfolioPerformance") },
                
                // Action Terms
                { "Create", _localizationService.GetLocalizedString("Create") },
                { "Edit", _localizationService.GetLocalizedString("Edit") },
                { "Delete", _localizationService.GetLocalizedString("Delete") },
                { "Save", _localizationService.GetLocalizedString("Save") },
                { "Cancel", _localizationService.GetLocalizedString("Cancel") },
                { "Search", _localizationService.GetLocalizedString("Search") },
                { "Filter", _localizationService.GetLocalizedString("Filter") },
                { "Export", _localizationService.GetLocalizedString("Export") },
                
                // Financial Terms
                { "MarketValue", _localizationService.GetLocalizedString("MarketValue") },
                { "BookValue", _localizationService.GetLocalizedString("BookValue") },
                { "UnrealizedGainLoss", _localizationService.GetLocalizedString("UnrealizedGainLoss") },
                { "RealizedGainLoss", _localizationService.GetLocalizedString("RealizedGainLoss") },
                { "Quantity", _localizationService.GetLocalizedString("Quantity") },
                { "Price", _localizationService.GetLocalizedString("Price") },
                { "Amount", _localizationService.GetLocalizedString("Amount") },
                { "Currency", _localizationService.GetLocalizedString("Currency") },
                
                // Status Terms
                { "Active", _localizationService.GetLocalizedString("Active") },
                { "Inactive", _localizationService.GetLocalizedString("Inactive") },
                { "Pending", _localizationService.GetLocalizedString("Pending") },
                { "Completed", _localizationService.GetLocalizedString("Completed") },
                { "Failed", _localizationService.GetLocalizedString("Failed") },
                
                // Validation Messages
                { "RequiredField", _localizationService.GetLocalizedString("RequiredField") },
                { "InvalidEmail", _localizationService.GetLocalizedString("InvalidEmail") },
                { "InvalidAmount", _localizationService.GetLocalizedString("InvalidAmount") },
                { "InvalidDate", _localizationService.GetLocalizedString("InvalidDate") },
                
                // Error Messages
                { "ErrorOccurred", _localizationService.GetLocalizedString("ErrorOccurred") },
                { "NotFound", _localizationService.GetLocalizedString("NotFound") },
                { "Unauthorized", _localizationService.GetLocalizedString("Unauthorized") },
                { "LoginRequired", _localizationService.GetLocalizedString("LoginRequired") }
            };

            return Ok(resources);
        }
    }

    public class SetLanguageRequest
    {
        public string Language { get; set; } = string.Empty;
    }
}