using InvestmentPortfolioManager.Domain.Entities;

namespace InvestmentPortfolioManager.Application.Services.Contracts
{
    public interface ITransactionService
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetTransactionsByPortfolioAsync(int portfolioId, DateTime startDate, DateTime endDate);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int portfolioId, int count = 20);
    }
}