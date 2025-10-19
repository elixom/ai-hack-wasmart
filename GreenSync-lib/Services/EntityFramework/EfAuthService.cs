using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GreenSync.Lib.Data;
using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services.EntityFramework;

/// <summary>
/// Entity Framework implementation of the Auth service using ASP.NET Core Identity
/// </summary>
public class EfAuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GreenSyncDbContext _context;
    private readonly ILogger<EfAuthService> _logger;

    public EfAuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IHttpContextAccessor httpContextAccessor,
        GreenSyncDbContext context,
        ILogger<EfAuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            // Find user by username or email
            var applicationUser = await _userManager.FindByNameAsync(username) 
                                ?? await _userManager.FindByEmailAsync(username);

            if (applicationUser == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", username);
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Check if user is active
            if (!applicationUser.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {UserId}", applicationUser.Id);
                return new AuthResult
                {
                    Success = false,
                    Message = "Account is not active"
                };
            }

            if (string.IsNullOrEmpty(applicationUser.PasswordHash))
            {
                await _userManager.RemovePasswordAsync(applicationUser);
                await _userManager.AddPasswordAsync(applicationUser, "password123");
                applicationUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(applicationUser);
            }

            // Attempt sign-in
            var result = await _signInManager.PasswordSignInAsync(
                applicationUser, password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Update last login timestamp
                applicationUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(applicationUser);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(applicationUser);
                var claims = await _userManager.GetClaimsAsync(applicationUser);

                // Create claims list
                var allClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new(ClaimTypes.Name, applicationUser.UserName ?? applicationUser.Email),
                    new(ClaimTypes.Email, applicationUser.Email ?? string.Empty),
                    new("FullName", $"{applicationUser.FirstName} {applicationUser.LastName}"),
                    new("AccountType", applicationUser.AccountType.ToString())
                };

                // Add role claims
                allClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                
                // Add additional claims
                allClaims.AddRange(claims);

                _logger.LogInformation("User {UserId} logged in successfully", applicationUser.Id);

                return new AuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    User = MapToLegacyUser(applicationUser, roles),
                    Claims = allClaims
                };
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {UserId} account is locked out", applicationUser.Id);
                return new AuthResult
                {
                    Success = false,
                    Message = "Account is locked due to too many failed login attempts"
                };
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("User {UserId} login not allowed", applicationUser.Id);
                return new AuthResult
                {
                    Success = false,
                    Message = "Login not allowed. Please confirm your email address"
                };
            }

            _logger.LogWarning("Failed login attempt for user: {Username}", username);
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return new AuthResult
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var currentUser = await GetCurrentApplicationUserAsync();
            if (currentUser != null)
            {
                _logger.LogInformation("User {UserId} logging out", currentUser.Id);
            }

            await _signInManager.SignOutAsync();

            // Clear session if available
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                _httpContextAccessor.HttpContext.Session.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var applicationUser = await GetCurrentApplicationUserAsync();
            if (applicationUser == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(applicationUser);
            return MapToLegacyUser(applicationUser, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return null;
        }
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role {Role} for user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            var applicationUsers = await _context.Users
                .Where(u => u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var users = new List<User>();
            
            foreach (var appUser in applicationUsers)
            {
                var roles = await _userManager.GetRolesAsync(appUser);
                users.Add(MapToLegacyUser(appUser, roles));
            }

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return Enumerable.Empty<User>();
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Get the current ApplicationUser from the Identity context
    /// </summary>
    private async Task<ApplicationUser?> GetCurrentApplicationUserAsync()
    {
        if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }

    /// <summary>
    /// Map ApplicationUser to legacy User class for backward compatibility
    /// </summary>
    private static User MapToLegacyUser(ApplicationUser applicationUser, IList<string> roles)
    {
        // Determine the primary role for the legacy UserRole enum
        var userRole = UserRole.User; // Default
        
        if (roles.Contains("Administrator"))
        {
            userRole = UserRole.Admin;
        }

        return new User
        {
            Id = applicationUser.Id,
            Username = applicationUser.UserName ?? applicationUser.Email ?? string.Empty,
            Email = applicationUser.Email ?? string.Empty,
            FullName = $"{applicationUser.FirstName} {applicationUser.LastName}".Trim(),
            Role = userRole,
            CreatedAt = applicationUser.CreatedAt,
            LastLoginAt = applicationUser.UpdatedAt // Using UpdatedAt as approximate last login
        };
    }

    #endregion
}
