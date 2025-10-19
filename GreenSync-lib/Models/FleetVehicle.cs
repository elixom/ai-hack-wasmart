using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents a fleet vehicle used for waste collection
/// </summary>
[Table("FleetVehicles")]
public class FleetVehicle
{
    /// <summary>
    /// Primary key - Vehicle ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Vehicle license plate number
    /// </summary>
    [Required]
    [StringLength(20)]
    [DisplayName("License Plate")]
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle make (e.g., Ford, Chevrolet)
    /// </summary>
    [Required]
    [StringLength(100)]
    [DisplayName("Make")]
    public string Make { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle model
    /// </summary>
    [Required]
    [StringLength(100)]
    [DisplayName("Model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturing year
    /// </summary>
    [Required]
    [Range(1900, 2100)]
    [DisplayName("Year")]
    public int Year { get; set; }

    /// <summary>
    /// Vehicle identification number
    /// </summary>
    [StringLength(17)]
    [DisplayName("VIN")]
    public string? VIN { get; set; }

    /// <summary>
    /// Current vehicle status
    /// </summary>
    [Required]
    [DisplayName("Status")]
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    /// <summary>
    /// Maximum capacity in cubic meters
    /// </summary>
    [Required]
    [Range(0.1, 100.0)]
    [DisplayName("Capacity (m³)")]
    [Column(TypeName = "decimal(6,2)")]
    public decimal Capacity { get; set; }

    /// <summary>
    /// Current fuel level percentage
    /// </summary>
    [Range(0, 100)]
    [DisplayName("Fuel Level (%)")]
    public int FuelLevel { get; set; } = 100;

    /// <summary>
    /// Current mileage/odometer reading
    /// </summary>
    [DisplayName("Mileage")]
    public int Mileage { get; set; }

    /// <summary>
    /// Last maintenance date
    /// </summary>
    [DisplayName("Last Maintenance")]
    [DataType(DataType.Date)]
    public DateTime? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Next scheduled maintenance date
    /// </summary>
    [DisplayName("Next Maintenance")]
    [DataType(DataType.Date)]
    public DateTime? NextMaintenanceDate { get; set; }

    /// <summary>
    /// Current GPS latitude
    /// </summary>
    [DisplayName("Current Latitude")]
    public double? CurrentLatitude { get; set; }

    /// <summary>
    /// Current GPS longitude
    /// </summary>
    [DisplayName("Current Longitude")]
    public double? CurrentLongitude { get; set; }

    /// <summary>
    /// Last GPS update timestamp
    /// </summary>
    [DisplayName("Last GPS Update")]
    public DateTime? LastGPSUpdate { get; set; }

    /// <summary>
    /// Currently assigned driver ID
    /// </summary>
    [DisplayName("Assigned Driver ID")]
    public Guid? AssignedDriverId { get; set; }

    /// <summary>
    /// Vehicle creation timestamp
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
    /// Currently assigned driver
    /// </summary>
    [ForeignKey(nameof(AssignedDriverId))]
    public virtual ApplicationUser? AssignedDriver { get; set; }

    /// <summary>
    /// Routes assigned to this vehicle
    /// </summary>
    public virtual ICollection<Route> AssignedRoutes { get; set; } = new List<Route>();

    /// <summary>
    /// Maintenance records for this vehicle
    /// </summary>
    public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}

/// <summary>
/// Vehicle status enumeration
/// </summary>
public enum VehicleStatus
{
    Available,
    InUse,
    Maintenance,
    OutOfService,
    Refueling
}

/// <summary>
/// Vehicle maintenance record
/// </summary>
[Table("MaintenanceRecords")]
public class MaintenanceRecord
{
    /// <summary>
    /// Primary key - Maintenance record ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Associated vehicle ID
    /// </summary>
    [Required]
    [DisplayName("Vehicle ID")]
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Maintenance type
    /// </summary>
    [Required]
    [DisplayName("Maintenance Type")]
    public MaintenanceType Type { get; set; }

    /// <summary>
    /// Maintenance description
    /// </summary>
    [Required]
    [StringLength(1000)]
    [DisplayName("Description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Maintenance cost
    /// </summary>
    [DisplayName("Cost")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    /// <summary>
    /// Maintenance date
    /// </summary>
    [Required]
    [DisplayName("Maintenance Date")]
    [DataType(DataType.Date)]
    public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Maintenance performed by
    /// </summary>
    [StringLength(200)]
    [DisplayName("Performed By")]
    public string? PerformedBy { get; set; }

    /// <summary>
    /// Mileage at time of maintenance
    /// </summary>
    [DisplayName("Mileage")]
    public int? MileageAtMaintenance { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    [Required]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Associated vehicle
    /// </summary>
    [ForeignKey(nameof(VehicleId))]
    public virtual FleetVehicle Vehicle { get; set; } = null!;
}

/// <summary>
/// Maintenance type enumeration
/// </summary>
public enum MaintenanceType
{
    Routine,
    Preventive,
    Repair,
    Emergency,
    Inspection,
    Cleaning,
    FuelSystem,
    Brakes,
    Engine,
    Transmission
}