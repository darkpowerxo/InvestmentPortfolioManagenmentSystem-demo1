using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Contracts
{
    public interface IPortfolioService
    {
        Task<Portfolio?> GetByIdAsync(int id);
        Task<IEnumerable<Portfolio>> GetAllAsync();
        Task<Portfolio> CreateAsync(Portfolio portfolio);
        Task<Portfolio> UpdateAsync(Portfolio portfolio);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Portfolio>> GetByManagerIdAsync(int managerId);
    }
}