using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents an optimized collection route for waste pickup
/// </summary>
[Table("Routes")]
public class Route
{
    /// <summary>
    /// Primary key - Route ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Descriptive name for the route
    /// </summary>
    [Required]
    [StringLength(200)]
    [DisplayName("Route Name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    [Required]
    [DisplayName("Duration (Minutes)")]
    public int Duration { get; set; }
    
    /// <summary>
    /// Fuel efficiency improvement percentage
    /// </summary>
    [Required]
    [DisplayName("Fuel Savings (%)")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal FuelSavingsMetric { get; set; }
    
    /// <summary>
    /// Total route distance in kilometers
    /// </summary>
    [DisplayName("Total Distance (km)")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalDistance { get; set; }
    
    /// <summary>
    /// Estimated fuel cost for this route
    /// </summary>
    [DisplayName("Estimated Fuel Cost")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal EstimatedFuelCost { get; set; }
    
    /// <summary>
    /// Number of pickup stops on this route
    /// </summary>
    [DisplayName("Number of Stops")]
    public int NumberOfStops { get; set; }
    
    /// <summary>
    /// ID of the assigned vehicle
    /// </summary>
    [DisplayName("Assigned Vehicle ID")]
    public Guid? AssignedVehicleId { get; set; }
    
    /// <summary>
    /// ID of the assigned driver
    /// </summary>
    [DisplayName("Driver ID")]
    public Guid? DriverId { get; set; }
    
    /// <summary>
    /// Current status of the route
    /// </summary>
    [Required]
    [DisplayName("Route Status")]
    public RouteStatus Status { get; set; } = RouteStatus.Planned;
    
    /// <summary>
    /// Route creation timestamp
    /// </summary>
    [Required]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Route start timestamp
    /// </summary>
    [DisplayName("Started At")]
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Route completion timestamp
    /// </summary>
    [DisplayName("Completed At")]
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Algorithm used for route optimization
    /// </summary>
    [StringLength(100)]
    [DisplayName("Optimization Algorithm")]
    public string OptimizationAlgorithm { get; set; } = "Distance-based";
    
    /// <summary>
    /// Overall efficiency score (0-100)
    /// </summary>
    [DisplayName("Efficiency Score")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal EfficiencyScore { get; set; }
    
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
    /// Assigned vehicle
    /// </summary>
    [ForeignKey(nameof(AssignedVehicleId))]
    public virtual FleetVehicle? AssignedVehicle { get; set; }
    
    /// <summary>
    /// Assigned driver
    /// </summary>
    [ForeignKey(nameof(DriverId))]
    public virtual ApplicationUser? Driver { get; set; }
    
    /// <summary>
    /// Reports assigned to this route
    /// </summary>
    public virtual ICollection<Report> AssignedReports { get; set; } = new List<Report>();
    
    /// <summary>
    /// Route waypoints in optimized order
    /// </summary>
    public virtual ICollection<RouteWaypoint> Waypoints { get; set; } = new List<RouteWaypoint>();
}

/// <summary>
/// Represents a waypoint (stop) in an optimized route
/// </summary>
[Table("RouteWaypoints")]
public class RouteWaypoint
{
    /// <summary>
    /// Primary key - Waypoint ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Associated route ID
    /// </summary>
    [Required]
    [DisplayName("Route ID")]
    public Guid RouteId { get; set; }
    
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
    /// Address or location description
    /// </summary>
    [StringLength(500)]
    [DisplayName("Address")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Order in the route sequence (1-based)
    /// </summary>
    [Required]
    [DisplayName("Stop Order")]
    public int StopOrder { get; set; }
    
    /// <summary>
    /// Estimated arrival time (minutes from route start)
    /// </summary>
    [DisplayName("Estimated Arrival (Minutes)")]
    public int? EstimatedArrivalMinutes { get; set; }
    
    /// <summary>
    /// Associated report ID (if this stop is for a specific report)
    /// </summary>
    [DisplayName("Report ID")]
    public Guid? ReportId { get; set; }
    
    /// <summary>
    /// Actual arrival timestamp
    /// </summary>
    [DisplayName("Actual Arrival")]
    public DateTime? ActualArrival { get; set; }
    
    /// <summary>
    /// Whether this waypoint has been completed
    /// </summary>
    [DisplayName("Is Completed")]
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// Completion timestamp
    /// </summary>
    [DisplayName("Completed At")]
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Associated route
    /// </summary>
    [ForeignKey(nameof(RouteId))]
    public virtual Route Route { get; set; } = null!;
    
    /// <summary>
    /// Associated report (if applicable)
    /// </summary>
    [ForeignKey(nameof(ReportId))]
    public virtual Report? Report { get; set; }
}

public enum RouteStatus
{
    Planned,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}
