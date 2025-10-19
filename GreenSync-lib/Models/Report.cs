using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GreenSync.Lib.Models;

public class Report
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [DisplayName("Location")]
    public string Location { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Latitude")]
    public double Latitude { get; set; }
    
    [Required]
    [DisplayName("Longitude")]
    public double Longitude { get; set; }
    
    [Required]
    [DisplayName("Status")]
    public ReportStatus Status { get; set; } = ReportStatus.Reported;
    
    [DisplayName("Description")]
    public string Description { get; set; } = string.Empty;
    
    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; }
    
    [Required]
    [DisplayName("Reported At")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    [DisplayName("User ID")]
    public string UserId { get; set; } = string.Empty;
    
    [DisplayName("Collected At")]
    public DateTime? CollectedAt { get; set; }
    
    [DisplayName("Assigned Truck ID")]
    public string? AssignedTruckId { get; set; }
    
    [DisplayName("Priority")]
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    
    [DisplayName("Estimated Volume")]
    public double EstimatedVolume { get; set; }
    
    [DisplayName("Waste Type")]
    public WasteType WasteType { get; set; } = WasteType.General;
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
