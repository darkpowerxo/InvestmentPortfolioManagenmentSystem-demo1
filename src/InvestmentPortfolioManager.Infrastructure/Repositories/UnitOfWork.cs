using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using InvestmentPortfolioManager.Infrastructure.Data;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;

namespace InvestmentPortfolioManager.Infrastructure.Repositories;

/// <summary>
/// Implémentation du pattern Unit of Work / Unit of Work pattern implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PortfolioDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private IUserRepository? _users;
    private IPortfolioRepository? _portfolios;
    private ISecurityRepository? _securities;
    private IPositionRepository? _positions;
    private ITransactionRepository? _transactions;
    private IMarketDataRepository? _marketData;
    private IPerformanceMetricRepository? _performanceMetrics;
    private ITradeOrderRepository? _tradeOrders;

    public UnitOfWork(PortfolioDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IPortfolioRepository Portfolios => _portfolios ??= new PortfolioRepository(_context);
    public ISecurityRepository Securities => _securities ??= new SecurityRepository(_context);
    public IPositionRepository Positions => _positions ??= new PositionRepository(_context);
    public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);
    public IMarketDataRepository MarketData => _marketData ??= new MarketDataRepository(_context);
    public IPerformanceMetricRepository PerformanceMetrics => _performanceMetrics ??= new PerformanceMetricRepository(_context);
    public ITradeOrderRepository TradeOrders => _tradeOrders ??= new TradeOrderRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            throw new InvalidOperationException("A concurrency conflict occurred while saving changes.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle database update errors
            throw new InvalidOperationException("An error occurred while updating the database.", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}