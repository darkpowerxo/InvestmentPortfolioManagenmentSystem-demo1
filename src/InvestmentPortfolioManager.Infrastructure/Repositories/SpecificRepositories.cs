using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Infrastructure.Data;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;

namespace InvestmentPortfolioManager.Infrastructure.Repositories;

/// <summary>
/// Dépôt pour les utilisateurs / Repository for users
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet.Where(u => u.IsActive).ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les portefeuilles / Repository for portfolios
/// </summary>
public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByManagerIdAsync(int managerId)
    {
        return await _dbSet
            .Include(p => p.Manager)
            .Where(p => p.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByTypeAsync(Domain.Enums.PortfolioType portfolioType)
    {
        return await _dbSet
            .Where(p => p.PortfolioType == portfolioType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Portfolio>> GetActivePortfoliosAsync()
    {
        return await _dbSet
            .Include(p => p.Manager)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Portfolio?> GetPortfolioWithPositionsAsync(int portfolioId)
    {
        return await _dbSet
            .Include(p => p.Positions)
            .ThenInclude(pos => pos.Security)
            .FirstOrDefaultAsync(p => p.Id == portfolioId);
    }

    public async Task<Portfolio?> GetPortfolioWithTransactionsAsync(int portfolioId)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .ThenInclude(t => t.Security)
            .FirstOrDefaultAsync(p => p.Id == portfolioId);
    }

    public async Task<decimal> GetTotalAssetsUnderManagementAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .SumAsync(p => p.CurrentValue);
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByRiskLevelAsync(Domain.Enums.RiskLevel riskLevel)
    {
        return await _dbSet
            .Where(p => p.RiskLevel == riskLevel && p.IsActive)
            .ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les titres / Repository for securities
/// </summary>
public class SecurityRepository : Repository<Security>, ISecurityRepository
{
    public SecurityRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<Security?> GetBySymbolAsync(string symbol)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Symbol == symbol);
    }

    public async Task<IEnumerable<Security>> GetSecuritiesByTypeAsync(Domain.Enums.SecurityType securityType)
    {
        return await _dbSet
            .Where(s => s.SecurityType == securityType && s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> GetSecuritiesBySectorAsync(string sector)
    {
        return await _dbSet
            .Where(s => s.Sector == sector && s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> GetSecuritiesByExchangeAsync(string exchange)
    {
        return await _dbSet
            .Where(s => s.Exchange == exchange && s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> GetActiveSecuritiesAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Symbol)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> SearchSecuritiesAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(s => s.IsActive && 
                   (s.Symbol.ToLower().Contains(lowerSearchTerm) || 
                    s.Name.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(s => s.Symbol)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> GetSecuritiesByCountryAsync(string country)
    {
        return await _dbSet
            .Where(s => s.Country == country && s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Security>> GetSecuritiesByCurrencyAsync(string currency)
    {
        return await _dbSet
            .Where(s => s.Currency == currency && s.IsActive)
            .ToListAsync();
    }
}

/// <summary>
/// Dépôt pour les positions / Repository for positions
/// </summary>
public class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(PortfolioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Position>> GetPositionsByPortfolioIdAsync(int portfolioId)
    {
        return await _dbSet
            .Include(p => p.Security)
            .Where(p => p.PortfolioId == portfolioId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Position>> GetPositionsBySecurityIdAsync(int securityId)
    {
        return await _dbSet
            .Include(p => p.Portfolio)
            .Where(p => p.SecurityId == securityId)
            .ToListAsync();
    }

    public async Task<Position?> GetPositionAsync(int portfolioId, int securityId)
    {
        return await _dbSet
            .Include(p => p.Security)
            .Include(p => p.Portfolio)
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId && p.SecurityId == securityId);
    }

    public async Task<IEnumerable<Position>> GetActivePositionsAsync()
    {
        return await _dbSet
            .Include(p => p.Security)
            .Include(p => p.Portfolio)
            .Where(p => p.Quantity > 0)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPositionValueAsync(int portfolioId)
    {
        return await _dbSet
            .Where(p => p.PortfolioId == portfolioId)
            .SumAsync(p => p.Quantity * p.CurrentPrice);
    }

    public async Task<IEnumerable<Position>> GetPositionsAboveThresholdAsync(decimal minValue)
    {
        return await _dbSet
            .Include(p => p.Security)
            .Include(p => p.Portfolio)
            .Where(p => p.Quantity * p.CurrentPrice >= minValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<Position>> GetConcentratedPositionsAsync(int portfolioId, decimal concentrationThreshold)
    {
        var totalValue = await GetTotalPositionValueAsync(portfolioId);
        if (totalValue == 0) return Enumerable.Empty<Position>();

        var threshold = totalValue * concentrationThreshold;
        return await _dbSet
            .Include(p => p.Security)
            .Where(p => p.PortfolioId == portfolioId && p.Quantity * p.CurrentPrice >= threshold)
            .ToListAsync();
    }
}