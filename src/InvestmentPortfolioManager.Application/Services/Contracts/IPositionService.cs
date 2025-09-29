using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Contracts
{
    public interface IPositionService
    {
        Task<Position?> GetByIdAsync(int id);
        Task<IEnumerable<Position>> GetPortfolioPositionsAsync(int portfolioId);
        Task<Position> CreateAsync(Position position);
        Task<Position> UpdateAsync(Position position);
        Task<bool> DeleteAsync(int id);
        Task<decimal> GetTotalMarketValueAsync(int portfolioId);
    }
}