using InvestmentPortfolioManager.Application.Services.Contracts;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Application.Services
{
    public class PositionService : IPositionService
    {
        private readonly PortfolioDbContext _context;

        public PositionService(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<Position> CreateAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position?> GetByIdAsync(int id)
        {
            return await _context.Positions
                .Include(p => p.Security)
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            return await _context.Positions
                .Include(p => p.Security)
                .Include(p => p.Portfolio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Position>> GetPortfolioPositionsAsync(int portfolioId)
        {
            return await _context.Positions
                .Include(p => p.Security)
                .Where(p => p.PortfolioId == portfolioId && p.Quantity > 0)
                .ToListAsync();
        }

        public async Task<Position?> GetByPortfolioAndSecurityAsync(int portfolioId, int securityId)
        {
            return await _context.Positions
                .Include(p => p.Security)
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId && p.SecurityId == securityId);
        }

        public async Task<Position> UpdateAsync(Position position)
        {
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
                return false;

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculatePositionValueAsync(int positionId)
        {
            var position = await GetByIdAsync(positionId);
            return position?.MarketValue ?? 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Positions.AnyAsync(p => p.Id == id);
        }

        public async Task UpdateMarketValuesAsync(Dictionary<int, decimal> securityPrices)
        {
            var positions = await _context.Positions
                .Include(p => p.Security)
                .Where(p => p.Quantity > 0)
                .ToListAsync();

            foreach (var position in positions)
            {
                if (securityPrices.TryGetValue(position.SecurityId, out var newPrice))
                {
                    position.CurrentPrice = newPrice;
                    position.MarketValue = position.Quantity * newPrice;
                    var costBasis = position.Quantity * position.AverageCost;
                    position.UnrealizedGainLoss = position.MarketValue - costBasis;
                    position.UnrealizedGainLossPercent = costBasis != 0 
                        ? (position.UnrealizedGainLoss / costBasis) * 100 
                        : 0;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalMarketValueAsync(int portfolioId)
        {
            var positions = await GetPortfolioPositionsAsync(portfolioId);
            return positions.Sum(p => p.MarketValue);
        }
    }
}