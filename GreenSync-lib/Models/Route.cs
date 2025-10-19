using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GreenSync.Lib.Models;

public class Route
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [DisplayName("Route Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Optimized Path")]
    public List<GeoCoordinate> OptimizedPath { get; set; } = new();
    
    [Required]
    [DisplayName("Duration (Minutes)")]
    public int Duration { get; set; }
    
    [Required]
    [DisplayName("Fuel Savings (%)")]
    public double FuelSavingsMetric { get; set; }
    
    [DisplayName("Total Distance (km)")]
    public double TotalDistance { get; set; }
    
    [DisplayName("Estimated Fuel Cost")]
    public decimal EstimatedFuelCost { get; set; }
    
    [DisplayName("Number of Stops")]
    public int NumberOfStops { get; set; }
    
    [DisplayName("Assigned Truck ID")]
    public string? AssignedTruckId { get; set; }
    
    [DisplayName("Driver ID")]
    public string? DriverId { get; set; }
    
    [Required]
    [DisplayName("Route Status")]
    public RouteStatus Status { get; set; } = RouteStatus.Planned;
    
    [Required]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [DisplayName("Started At")]
    public DateTime? StartedAt { get; set; }
    
    [DisplayName("Completed At")]
    public DateTime? CompletedAt { get; set; }
    
    [DisplayName("Report IDs")]
    public List<Guid> ReportIds { get; set; } = new();
    
    [DisplayName("Optimization Algorithm")]
    public string OptimizationAlgorithm { get; set; } = "Distance-based";
    
    [DisplayName("Efficiency Score")]
    public double EfficiencyScore { get; set; }
}

public class GeoCoordinate
{
    [Required]
    [DisplayName("Latitude")]
    public double Latitude { get; set; }
    
    [Required]
    [DisplayName("Longitude")]
    public double Longitude { get; set; }
    
    [DisplayName("Address")]
    public string? Address { get; set; }
    
    [DisplayName("Order")]
    public int Order { get; set; }
    
    [DisplayName("Estimated Arrival Time")]
    public TimeSpan? EstimatedArrival { get; set; }
    
    [DisplayName("Report ID")]
    public Guid? ReportId { get; set; }
}

public enum RouteStatus
{
    Planned,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}
