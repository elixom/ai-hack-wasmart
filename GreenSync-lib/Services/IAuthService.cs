using System.Security.Claims;

namespace GreenSync.Lib.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<IEnumerable<User>> GetAllUsersAsync();
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
    public List<Claim> Claims { get; set; } = new();
}

public class User
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public enum UserRole
{
    User,
    Admin
}
