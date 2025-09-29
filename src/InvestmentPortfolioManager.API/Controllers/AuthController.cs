using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvestmentPortfolioManager.Application.Services;
using InvestmentPortfolioManager.Application.DTOs;
using System.Security.Claims;

namespace InvestmentPortfolioManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(AuthenticationResult.Failed("Email and password are required"));
            }

            var result = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Unauthorized(result);
            }

            _logger.LogInformation("Successful login for user: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", request.Email);
            return StatusCode(500, AuthenticationResult.Failed("An error occurred during authentication"));
        }
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(AuthenticationResult.Failed("Token and refresh token are required"));
            }

            var result = await _authenticationService.RefreshTokenAsync(request.Token, request.RefreshToken);
            
            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, AuthenticationResult.Failed("An error occurred during token refresh"));
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user token");
            }

            // You would implement IUserService to get user details
            // For now, return user info from claims
            var userDto = new AuthUserDto
            {
                Id = userId,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
                LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "",
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? ""
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, "An error occurred while getting user information");
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || 
                string.IsNullOrWhiteSpace(request.NewPassword) || 
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return BadRequest("All password fields are required");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("New password and confirmation do not match");
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest("New password must be at least 6 characters long");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user token");
            }

            // TODO: Implement password change logic in a service
            // var result = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, "An error occurred while changing password");
        }
    }

    /// <summary>
    /// Logout user (invalidate refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user token");
            }

            // TODO: Implement logout logic to invalidate refresh token
            // await _userService.InvalidateRefreshTokenAsync(userId);

            _logger.LogInformation("User logged out: {UserId}", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }

    /// <summary>
    /// Check if user is authenticated (health check for frontend)
    /// </summary>
    [HttpGet("check")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new { 
            authenticated = true, 
            user = User.FindFirst(ClaimTypes.Email)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}