using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Infrastructure.Repositories.Contracts;
using InvestmentPortfolioManager.Application.DTOs;
using InvestmentPortfolioManager.API.Extensions;
using InvestmentPortfolioManager.Domain.Enums;
using InvestmentPortfolioManager.Domain.Entities;
using System.Linq;

namespace InvestmentPortfolioManager.API.Controllers
{
    /// <summary>
    /// Controller for managing users and user-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Users")]
    public class UsersController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all users with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<SearchResultDto<UserDto>>>> GetUsers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] string? sortBy = "lastName",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                List<User> users = allUsers.ToList();

                // Apply filtering
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    users = users.Where(u => 
                        u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (role.HasValue)
                {
                    users = users.Where(u => u.Role == role.Value).ToList();
                }

                // Apply sorting
                List<User> sortedUsers;
                if (sortBy?.ToLower() == "firstname")
                {
                    sortedUsers = sortDescending ? users.OrderByDescending(u => u.FirstName).ToList() : users.OrderBy(u => u.FirstName).ToList();
                }
                else if (sortBy?.ToLower() == "email")
                {
                    sortedUsers = sortDescending ? users.OrderByDescending(u => u.Email).ToList() : users.OrderBy(u => u.Email).ToList();
                }
                else if (sortBy?.ToLower() == "role")
                {
                    sortedUsers = sortDescending ? users.OrderByDescending(u => u.Role).ToList() : users.OrderBy(u => u.Role).ToList();
                }
                else if (sortBy?.ToLower() == "createdat")
                {
                    sortedUsers = sortDescending ? users.OrderByDescending(u => u.CreatedAt).ToList() : users.OrderBy(u => u.CreatedAt).ToList();
                }
                else
                {
                    sortedUsers = sortDescending ? users.OrderByDescending(u => u.LastName).ToList() : users.OrderBy(u => u.LastName).ToList();
                }
                
                users = sortedUsers;

                // Apply pagination
                var totalCount = users.Count;
                var pagedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var userDtos = pagedUsers.Select<User, UserDto>(user => user.ToDto()).ToList();

                var searchResult = new SearchResultDto<UserDto>(
                    Items: userDtos,
                    TotalCount: totalCount,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    HasNextPage: pageNumber * pageSize < totalCount,
                    HasPreviousPage: pageNumber > 1
                );

                return SearchSuccess(searchResult);
            }
            catch (Exception ex)
            {
                return Error<SearchResultDto<UserDto>>($"Failed to retrieve users: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUser(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound<UserDto>($"User with ID {id} not found");
                }

                return Success(user.ToDto());
            }
            catch (Exception ex)
            {
                return Error<UserDto>($"Failed to retrieve user: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get a user by email address
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                {
                    return NotFound<UserDto>($"User with email {email} not found");
                }

                return Success(user.ToDto());
            }
            catch (Exception ex)
            {
                return Error<UserDto>($"Failed to retrieve user: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("by-role/{role}")]
        public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetUsersByRole(UserRole role)
        {
            try
            {
                var users = await _unitOfWork.Users.GetUsersByRoleAsync(role);
                var userDtos = users.Select(u => u.ToDto()).ToList();

                return Success(userDtos);
            }
            catch (Exception ex)
            {
                return Error<List<UserDto>>($"Failed to retrieve users: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<UserDto>(validationErrors);
            }

            try
            {
                // Check if user with email already exists
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return Error<UserDto>("A user with this email already exists", statusCode: 409);
                }

                var user = createUserDto.ToEntity();
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Success(user.ToDto(), "User created successfully");
            }
            catch (Exception ex)
            {
                return Error<UserDto>($"Failed to create user: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var validationErrors = ValidateModel();
            if (validationErrors.Any())
            {
                return ValidationError<UserDto>(validationErrors);
            }

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound<UserDto>($"User with ID {id} not found");
                }

                user.UpdateFromDto(updateUserDto);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Success(user.ToDto(), "User updated successfully");
            }
            catch (Exception ex)
            {
                return Error<UserDto>($"Failed to update user: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteUser(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound<object>($"User with ID {id} not found");
                }

                // Check if user has associated portfolios
                var allPortfolios = await _unitOfWork.Portfolios.GetAllAsync();
                var portfolios = allPortfolios.Where(p => p.ManagerId == id);
                if (portfolios.Any())
                {
                    return Error<object>("Cannot delete user with associated portfolios", statusCode: 409);
                }

                await _unitOfWork.Users.DeleteAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Success((object)new { }, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return Error<object>($"Failed to delete user: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Get portfolio managers only
        /// </summary>
        [HttpGet("portfolio-managers")]
        public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetPortfolioManagers()
        {
            try
            {
                var managers = await _unitOfWork.Users.GetUsersByRoleAsync(UserRole.PortfolioManager);
                var managerDtos = managers.Select(u => u.ToDto()).ToList();

                return Success(managerDtos);
            }
            catch (Exception ex)
            {
                return Error<List<UserDto>>($"Failed to retrieve portfolio managers: {ex.Message}", statusCode: 500);
            }
        }
    }
}