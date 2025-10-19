using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GreenSync.Lib.Services;

public class InMemoryAuthService : IAuthService
{
    private readonly List<User> _users = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User? _currentUser;

    public InMemoryAuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        SeedSampleUsers();
    }

    public Task<AuthResult> LoginAsync(string username, string password)
    {
        // Simple mock authentication - in real app, hash passwords properly
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        if (user == null || !ValidatePassword(password, user))
        {
            return Task.FromResult(new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        user.LastLoginAt = DateTime.UtcNow;
        _currentUser = user;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FullName", user.FullName)
        };

        // Store user info in session
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _httpContextAccessor.HttpContext.Session.SetString("UserId", user.Id);
            _httpContextAccessor.HttpContext.Session.SetString("Username", user.Username);
            _httpContextAccessor.HttpContext.Session.SetString("Role", user.Role.ToString());
        }

        return Task.FromResult(new AuthResult
        {
            Success = true,
            Message = "Login successful",
            User = user,
            Claims = claims
        });
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }

        return Task.CompletedTask;
    }

    public Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return Task.FromResult<User?>(_currentUser);

        // Try to get from session
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                _currentUser = _users.FirstOrDefault(u => u.Id == userId);
            }
        }

        return Task.FromResult(_currentUser);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return false;

        return user.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.AsEnumerable());
    }

    private void SeedSampleUsers()
    {
        _users.AddRange(new[]
        {
            new User
            {
                Id = "user1",
                Username = "john.doe",
                Email = "john.doe@example.com",
                FullName = "John Doe",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = "user2",
                Username = "jane.smith",
                Email = "jane.smith@example.com",
                FullName = "Jane Smith",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Id = "user3",
                Username = "mike.wilson",
                Email = "mike.wilson@example.com",
                FullName = "Mike Wilson",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Id = "admin1",
                Username = "admin",
                Email = "admin@greensync.com",
                FullName = "System Administrator",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            }
        });
    }

    private static bool ValidatePassword(string password, User user)
    {
        // Mock password validation - in real app, use proper hashing
        // For demo: user passwords are "password123", admin password is "admin123"
        return user.Role == UserRole.Admin 
            ? password == "admin123" 
            : password == "password123";
    }
}
