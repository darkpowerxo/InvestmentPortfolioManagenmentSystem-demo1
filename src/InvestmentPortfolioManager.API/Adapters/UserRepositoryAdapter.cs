using InvestmentPortfolioManager.Domain.Entities;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Application.Contracts;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InfrastructureContracts = InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;

namespace InvestmentPortfolioManager.API.Adapters;

/// <summary>
/// Adapter that bridges between Infrastructure IUserRepository and Application IUserRepository
/// </summary>
public class UserRepositoryAdapter : InvestmentPortfolioManager.Application.Contracts.IUserRepository
{
    private readonly InfrastructureContracts.IUserRepository _userRepository;

    public UserRepositoryAdapter(InfrastructureContracts.IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _userRepository.GetUsersByRoleAsync(role);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetActiveUsersAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _userRepository.AddAsync(user);
    }

    public async Task<User> UpdateAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _userRepository.DeleteAsync(user);
        }
    }
}