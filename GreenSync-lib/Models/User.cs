using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents a user in the GreenSync system extending ASP.NET Core Identity
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [StringLength(100)]
    [DisplayName("First Name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [StringLength(100)]
    [DisplayName("Last Name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name computed property
    /// </summary>
    [NotMapped]
    [DisplayName("Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// User's address
    /// </summary>
    [StringLength(500)]
    [DisplayName("Address")]
    public string? Address { get; set; }

    /// <summary>
    /// User's default latitude for location-based services
    /// </summary>
    [DisplayName("Latitude")]
    public double? DefaultLatitude { get; set; }

    /// <summary>
    /// User's default longitude for location-based services
    /// </summary>
    [DisplayName("Longitude")]
    public double? DefaultLongitude { get; set; }

    /// <summary>
    /// User account type for business classification (not for authorization - use Identity Roles for that)
    /// </summary>
    [Required]
    [DisplayName("Account Type")]
    public AccountType AccountType { get; set; } = AccountType.Resident;

    /// <summary>
    /// Whether the user is active
    /// </summary>
    [DisplayName("Is Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    [Required]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    [DisplayName("Updated At")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    [DisplayName("Deleted At")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// User's waste reports
    /// </summary>
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    /// <summary>
    /// User's eco-credit account
    /// </summary>
    public virtual EcoCredit? EcoCredit { get; set; }

    /// <summary>
    /// User's eco-credit transactions
    /// </summary>
    public virtual ICollection<EcoCreditTransaction> EcoCreditTransactions { get; set; } = new List<EcoCreditTransaction>();

    /// <summary>
    /// Routes assigned to this user (for drivers)
    /// </summary>
    public virtual ICollection<Route> AssignedRoutes { get; set; } = new List<Route>();

    /// <summary>
    /// Fleet vehicles assigned to this user (for drivers)
    /// </summary>
    public virtual ICollection<FleetVehicle> AssignedVehicles { get; set; } = new List<FleetVehicle>();
}

/// <summary>
/// Account type for business classification (separate from Identity roles)
/// - Resident: Individual household users
/// - Commercial: Business/organization users  
/// - Municipal: Government/municipal organization users
/// 
/// NOTE: For authorization, use ASP.NET Core Identity Roles:
/// - "User": Standard users (residents and commercial)
/// - "Driver": Fleet drivers
/// - "Administrator": System administrators
/// - "Supervisor": Supervisory staff
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Individual residential users
    /// </summary>
    Resident,
    
    /// <summary>
    /// Commercial/business users
    /// </summary>
    Commercial,
    
    /// <summary>
    /// Municipal/government organization users
    /// </summary>
    Municipal
}
