using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InvestmentPortfolioManager.Infrastructure.Data;
using InvestmentPortfolioManager.Infrastructure.Repositories;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
// using InvestmentPortfolioManager.Infrastructure.Adapters;
// using ApplicationContracts = InvestmentPortfolioManager.Application.Contracts;

namespace InvestmentPortfolioManager.Infrastructure.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services d'infrastructure / Extensions for infrastructure service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services d'infrastructure / Add infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<PortfolioDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, b => 
            {
                b.MigrationsAssembly("InvestmentPortfolioManager.Infrastructure");
                b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
            
            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Repository Registration
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<ApplicationContracts.IUserRepository, UserRepositoryAdapter>(); // Temporarily disabled
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ISecurityRepository, SecurityRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<IPerformanceMetricRepository, PerformanceMetricRepository>();
        services.AddScoped<ITradeOrderRepository, TradeOrderRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seed Data Service
        services.AddScoped<ISeedDataService, DatabaseSeedService>();

        // TODO: Add health checks package if needed
        // services.AddHealthChecks().AddDbContextCheck<PortfolioDbContext>("database");

        return services;
    }

    /// <summary>
    /// Ajoute la configuration de base pour l'infrastructure / Add base infrastructure configuration
    /// </summary>
    public static IServiceCollection AddInfrastructureConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Connection String Validation
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
        }

        // Database Connection Pooling
        services.AddDbContextPool<PortfolioDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });
        }, poolSize: 128);

        return services;
    }

    /// <summary>
    /// Configuration pour les migrations automatiques / Configuration for automatic migrations
    /// </summary>
    public static async Task<IServiceProvider> EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        
        try
        {
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log the exception (you would inject ILogger here in a real scenario)
            throw new InvalidOperationException($"Failed to migrate database: {ex.Message}", ex);
        }

        return serviceProvider;
    }

    /// <summary>
    /// Validation de la configuration de base de données / Database configuration validation
    /// </summary>
    public static void ValidateDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Database connection string is required", nameof(configuration));
        }

        // Additional validation for connection string format
        if (!connectionString.Contains("Server=") && !connectionString.Contains("Data Source="))
        {
            throw new ArgumentException("Invalid connection string format", nameof(configuration));
        }
    }
}