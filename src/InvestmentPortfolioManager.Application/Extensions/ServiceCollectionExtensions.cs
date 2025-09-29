using InvestmentPortfolioManager.Application.Services;
using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Application.Services.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace InvestmentPortfolioManager.Application.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services / Extensions for service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre tous les services de l'application / Registers all application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services principaux / Core services
        services.AddScoped<IFinancialCalculationsService, FinancialCalculationsService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<ITransactionProcessingService, TransactionProcessingService>();
        
        // Authentication services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Reporting services
        services.AddScoped<IPdfReportService, PdfReportService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Business service interfaces (to be implemented)
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPerformanceMetricsService, PerformanceMetricsService>();

        return services;
    }
}