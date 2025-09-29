using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly PortfolioDbContext _context;

        public PortfolioService(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<Portfolio?> GetByIdAsync(int id)
        {
            return await _context.Portfolios
                .Include(p => p.Positions)
                .ThenInclude(p => p.Security)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Portfolio>> GetAllAsync()
        {
            return await _context.Portfolios
                .Include(p => p.Positions)
                .ThenInclude(p => p.Security)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Portfolio>> GetByUserIdAsync(int userId)
        {
            return await _context.Portfolios
                .Include(p => p.Positions)
                .ThenInclude(p => p.Security)
                .Where(p => p.ManagerId == userId && p.IsActive)
                .ToListAsync();
        }

        public async Task<Portfolio> UpdateAsync(Portfolio portfolio)
        {
            _context.Portfolios.Update(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null)
                return false;

            portfolio.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateTotalValueAsync(int portfolioId)
        {
            var portfolio = await GetByIdAsync(portfolioId);
            if (portfolio == null)
                return 0;

            var positionsValue = portfolio.Positions.Sum(p => p.MarketValue);
            return positionsValue + portfolio.CashBalance;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Portfolios.AnyAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<IEnumerable<Portfolio>> GetByManagerIdAsync(int managerId)
        {
            return await _context.Portfolios
                .Include(p => p.Positions)
                .ThenInclude(p => p.Security)
                .Where(p => p.ManagerId == managerId && p.IsActive)
                .ToListAsync();
        }
    }
}