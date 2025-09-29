using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly PortfolioDbContext _context;

        public TransactionService(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Include(t => t.Portfolio)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Include(t => t.Portfolio)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByPortfolioIdAsync(int portfolioId)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetBySecurityIdAsync(int securityId)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Include(t => t.Portfolio)
                .Where(t => t.SecurityId == securityId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Include(t => t.Portfolio)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByTransactionTypeAsync(TransactionType transactionType)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Include(t => t.Portfolio)
                .Where(t => t.TransactionType == transactionType)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Transactions.AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByPortfolioAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Where(t => t.PortfolioId == portfolioId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int portfolioId, int count)
        {
            return await _context.Transactions
                .Include(t => t.Security)
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalTransactionValueAsync(int portfolioId, DateTime? fromDate = null)
        {
            var query = _context.Transactions
                .Where(t => t.PortfolioId == portfolioId);

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= fromDate.Value);
            }

            return await query.SumAsync(t => Math.Abs(t.NetAmount));
        }

        public async Task<Dictionary<TransactionType, decimal>> GetTransactionSummaryByTypeAsync(int portfolioId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.PortfolioId == portfolioId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate <= endDate)
                .GroupBy(t => t.TransactionType)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => Math.Abs(t.NetAmount)) })
                .ToListAsync();

            return transactions.ToDictionary(t => t.Type, t => t.Total);
        }
    }
}