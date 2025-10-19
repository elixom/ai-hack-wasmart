using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents a waste report submitted by users
/// </summary>
[Table("Reports")]
public class Report
{
    /// <summary>
    /// Primary key - Report ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Location description or address
    /// </summary>
    [Required]
    [StringLength(500)]
    [DisplayName("Location")]
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// GPS latitude coordinate
    /// </summary>
    [Required]
    [DisplayName("Latitude")]
    public double Latitude { get; set; }
    
    /// <summary>
    /// GPS longitude coordinate
    /// </summary>
    [Required]
    [DisplayName("Longitude")]
    public double Longitude { get; set; }
    
    /// <summary>
    /// Current status of the report
    /// </summary>
    [Required]
    [DisplayName("Status")]
    public ReportStatus Status { get; set; } = ReportStatus.Reported;
    
    /// <summary>
    /// Detailed description of the waste issue
    /// </summary>
    [StringLength(1000)]
    [DisplayName("Description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// URL to uploaded image of the waste
    /// </summary>
    [StringLength(500)]
    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// When the report was submitted
    /// </summary>
    [Required]
    [DisplayName("Reported At")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// ID of the user who submitted the report
    /// </summary>
    [Required]
    [DisplayName("User ID")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// When the waste was collected
    /// </summary>
    [DisplayName("Collected At")]
    public DateTime? CollectedAt { get; set; }
    
    /// <summary>
    /// ID of the assigned fleet vehicle
    /// </summary>
    [DisplayName("Assigned Vehicle ID")]
    public Guid? AssignedVehicleId { get; set; }
    
    /// <summary>
    /// ID of the assigned route
    /// </summary>
    [DisplayName("Assigned Route ID")]
    public Guid? AssignedRouteId { get; set; }
    
    /// <summary>
    /// Priority level of the report
    /// </summary>
    [Required]
    [DisplayName("Priority")]
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    
    /// <summary>
    /// Estimated volume of waste in cubic meters
    /// </summary>
    [DisplayName("Estimated Volume (m³)")]
    [Column(TypeName = "decimal(6,2)")]
    public decimal EstimatedVolume { get; set; }
    
    /// <summary>
    /// Type of waste reported
    /// </summary>
    [Required]
    [DisplayName("Waste Type")]
    public WasteType WasteType { get; set; } = WasteType.General;
    
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
    
    /// <summary>
    /// Eco-credits earned from this report
    /// </summary>
    [DisplayName("Credits Earned")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal CreditsEarned { get; set; }

    // Navigation properties
    /// <summary>
    /// User who submitted the report
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// Assigned fleet vehicle
    /// </summary>
    [ForeignKey(nameof(AssignedVehicleId))]
    public virtual FleetVehicle? AssignedVehicle { get; set; }
    
    /// <summary>
    /// Assigned route
    /// </summary>
    [ForeignKey(nameof(AssignedRouteId))]
    public virtual Route? AssignedRoute { get; set; }
}

public enum ReportStatus
{
    Reported,
    Assigned,
    InProgress,
    Collected,
    Cancelled
}

public enum ReportPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum WasteType
{
    General,
    Recyclable,
    Organic,
    Hazardous,
    Electronic,
    Bulky
}
