using InvestmentPortfolioManager.Application.Services;
using InvestmentPortfolioManager.Application.Services.Contracts;
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

        return services;
    }
}