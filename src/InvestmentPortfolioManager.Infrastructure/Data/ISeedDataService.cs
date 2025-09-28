namespace InvestmentPortfolioManager.Infrastructure.Data
{
    public interface ISeedDataService
    {
        Task SeedDataAsync();
        Task<bool> IsDatabaseSeededAsync();
    }
}