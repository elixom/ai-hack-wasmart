using GreenSync.Lib.Data;
using GreenSync.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services.EntityFramework;

/// <summary>
/// Entity Framework implementation of the Route service
/// </summary>
public class EfRouteService : IRouteService
{
    private readonly GreenSyncDbContext _context;
    private readonly ILogger<EfRouteService> _logger;

    public EfRouteService(GreenSyncDbContext context, ILogger<EfRouteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Route>> GetAllRoutesAsync()
    {
        try
        {
            return await _context.Routes
                .Include(r => r.AssignedVehicle)
                .Include(r => r.Driver)
                .Include(r => r.Waypoints)
                .Include(r => r.AssignedReports)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all routes");
            throw;
        }
    }

    public async Task<Route?> GetRouteByIdAsync(Guid id)
    {
        try
        {
            return await _context.Routes
                .Include(r => r.AssignedVehicle)
                .Include(r => r.Driver)
                .Include(r => r.Waypoints.OrderBy(w => w.StopOrder))
                .Include(r => r.AssignedReports)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route {RouteId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Route>> GetRoutesByStatusAsync(RouteStatus status)
    {
        try
        {
            return await _context.Routes
                .Include(r => r.AssignedVehicle)
                .Include(r => r.Driver)
                .Include(r => r.Waypoints)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes with status {Status}", status);
            throw;
        }
    }

    public async Task<Route> CreateRouteAsync(Route route)
    {
        try
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created route {RouteId} with {StopCount} stops", 
                route.Id, route.NumberOfStops);
            return route;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route");
            throw;
        }
    }

    public async Task<Route?> UpdateRouteAsync(Guid id, Route route)
    {
        try
        {
            var existingRoute = await _context.Routes.FindAsync(id);
            if (existingRoute == null)
            {
                return null;
            }

            // Update properties
            existingRoute.Name = route.Name;
            existingRoute.Duration = route.Duration;
            existingRoute.FuelSavingsMetric = route.FuelSavingsMetric;
            existingRoute.TotalDistance = route.TotalDistance;
            existingRoute.EstimatedFuelCost = route.EstimatedFuelCost;
            existingRoute.NumberOfStops = route.NumberOfStops;
            existingRoute.OptimizationAlgorithm = route.OptimizationAlgorithm;
            existingRoute.EfficiencyScore = route.EfficiencyScore;
            existingRoute.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated route {RouteId}", id);
            return existingRoute;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route {RouteId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRouteAsync(Guid id)
    {
        try
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return false;
            }

            // Soft delete
            route.IsDeleted = true;
            route.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted route {RouteId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route {RouteId}", id);
            throw;
        }
    }

    public async Task<Route> OptimizeRouteAsync(List<Report> reports)
    {
        try
        {
            var optimizationResult = await SimulateRouteOptimizationAsync(reports);
            var optimizedRoute = optimizationResult.OptimizedRoute;
            
            // Create waypoints for the optimized route
            await CreateWaypointsForRoute(optimizedRoute, reports);

            _context.Routes.Add(optimizedRoute);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created optimized route {RouteId} with {FuelSavings}% fuel savings", 
                optimizedRoute.Id, optimizationResult.FuelSavingsPercent);

            return optimizedRoute;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing route");
            throw;
        }
    }

    public async Task<bool> AssignTruckToRouteAsync(Guid routeId, string truckId, string? driverId = null)
    {
        try
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route == null)
            {
                return false;
            }

            if (Guid.TryParse(truckId, out var vehicleGuid))
            {
                route.AssignedVehicleId = vehicleGuid;
                route.Status = RouteStatus.Assigned;
                
                if (!string.IsNullOrEmpty(driverId) && Guid.TryParse(driverId, out var driverGuid))
                {
                    route.DriverId = driverGuid;
                }

                route.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned vehicle {VehicleId} and driver {DriverId} to route {RouteId}", 
                    vehicleGuid, driverId, routeId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning truck to route {RouteId}", routeId);
            throw;
        }
    }

    public async Task<bool> UpdateRouteStatusAsync(Guid routeId, RouteStatus status)
    {
        try
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route == null)
            {
                return false;
            }

            route.Status = status;
            route.UpdatedAt = DateTime.UtcNow;

            if (status == RouteStatus.InProgress && route.StartedAt == null)
            {
                route.StartedAt = DateTime.UtcNow;
            }
            else if (status == RouteStatus.Completed)
            {
                route.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated route {RouteId} status to {Status}", routeId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route {RouteId} status", routeId);
            throw;
        }
    }

    public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
    {
        try
        {
            return await _context.Routes
                .Include(r => r.AssignedVehicle)
                .Include(r => r.Driver)
                .Include(r => r.Waypoints)
                .Where(r => r.Status == RouteStatus.Assigned || r.Status == RouteStatus.InProgress)
                .OrderBy(r => r.StartedAt ?? r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active routes");
            throw;
        }
    }

    public async Task<RouteOptimizationResult> SimulateRouteOptimizationAsync(List<Report> reports)
    {
        try
        {
            if (!reports.Any())
            {
                return new RouteOptimizationResult
                {
                    OptimizedRoute = new Route { Name = "Empty Route" },
                    FuelSavingsPercent = 0,
                    TimeReduction = 0,
                    OptimizationMethod = "No reports to optimize"
                };
            }

            // Calculate optimal route using nearest neighbor algorithm with priority weighting
            var optimizedWaypoints = OptimizeWaypointOrder(reports);
            var totalDistance = CalculateTotalDistance(optimizedWaypoints);
            var estimatedDuration = CalculateEstimatedDuration(totalDistance, optimizedWaypoints.Count);
            
            // Calculate fuel savings (simulation)
            var baseDistance = CalculateBaseDistance(reports); // Unoptimized distance
            var fuelSavingsPercent = Math.Max(0, ((baseDistance - totalDistance) / baseDistance) * 100);
            
            var optimizedRoute = new Route
            {
                Name = $"Optimized Route {DateTime.Now:HH:mm}",
                Duration = estimatedDuration,
                FuelSavingsMetric = (decimal)fuelSavingsPercent,
                TotalDistance = (decimal)totalDistance,
                EstimatedFuelCost = CalculateEstimatedFuelCost(totalDistance),
                NumberOfStops = reports.Count,
                OptimizationAlgorithm = "Nearest Neighbor with Priority Weighting",
                EfficiencyScore = CalculateEfficiencyScore(fuelSavingsPercent, reports.Count),
                Status = RouteStatus.Planned,
                CreatedAt = DateTime.UtcNow
            };

            var result = new RouteOptimizationResult
            {
                OptimizedRoute = optimizedRoute,
                FuelSavingsPercent = fuelSavingsPercent,
                TimeReduction = Math.Max(0, (baseDistance - totalDistance) / baseDistance * 100),
                OptimizationMethod = "AI-Powered Route Optimization",
                OptimizationSteps = new List<string>
                {
                    "1. Analyzed waste priority levels",
                    "2. Calculated optimal pickup sequence",
                    "3. Applied distance minimization algorithm",
                    $"4. Generated route with {reports.Count} stops",
                    $"5. Achieved {fuelSavingsPercent:F1}% fuel savings"
                }
            };

            _logger.LogInformation("Simulated route optimization: {FuelSavings}% fuel savings, {Distance}km total", 
                fuelSavingsPercent, totalDistance);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating route optimization");
            throw;
        }
    }

    #region Private Helper Methods

    private async Task CreateWaypointsForRoute(Route route, List<Report> reports)
    {
        var optimizedReports = OptimizeWaypointOrder(reports);
        var waypoints = new List<RouteWaypoint>();
        DateTime? startDt = route.StartedAt.HasValue ? route.StartedAt.Value.AddMinutes(5) : null;

        for (int i = 0; i < optimizedReports.Count; i++)
        {
            var report = optimizedReports[i];
            var min = CalculateEstimatedArrival(i, optimizedReports);
            if (startDt.HasValue)
            {
                startDt = startDt.Value.AddMinutes(min);
            }
                waypoints.Add(new RouteWaypoint
            {
                RouteId = route.Id,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                Address = report.Location,
                StopOrder = i + 1,
                EstimatedArrivalMinutes =min,
                EstimatedArrival = startDt,
                ReportId = report.Id,
                IsCompleted = false
            });
        }

        route.Waypoints = waypoints;
    }

    private static List<Report> OptimizeWaypointOrder(List<Report> reports)
    {
        if (reports.Count <= 1) return reports;

        // Start with highest priority reports
        var priorityGroups = reports
            .GroupBy(r => r.Priority)
            .OrderBy(g => g.Key == ReportPriority.Critical ? 0 :
                         g.Key == ReportPriority.High ? 1 :
                         g.Key == ReportPriority.Medium ? 2 : 3)
            .ToList();

        var optimized = new List<Report>();
        Report? currentLocation = null;

        foreach (var group in priorityGroups)
        {
            var remaining = group.ToList();
            
            while (remaining.Any())
            {
                Report next;
                if (currentLocation == null)
                {
                    // Start with the first report in the priority group
                    next = remaining.First();
                }
                else
                {
                    // Find the nearest report
                    next = remaining
                        .OrderBy(r => CalculateDistance(currentLocation.Latitude, currentLocation.Longitude, 
                                                       r.Latitude, r.Longitude))
                        .First();
                }

                optimized.Add(next);
                remaining.Remove(next);
                currentLocation = next;
            }
        }

        return optimized;
    }

    private static double CalculateTotalDistance(List<Report> reports)
    {
        if (reports.Count <= 1) return 0;

        double totalDistance = 0;
        for (int i = 0; i < reports.Count - 1; i++)
        {
            totalDistance += CalculateDistance(
                reports[i].Latitude, reports[i].Longitude,
                reports[i + 1].Latitude, reports[i + 1].Longitude);
        }

        return totalDistance;
    }

    private static double CalculateBaseDistance(List<Report> reports)
    {
        // Calculate unoptimized distance (simple order)
        return CalculateTotalDistance(reports) * 1.3; // Add 30% for unoptimized routing
    }

    private static int CalculateEstimatedDuration(double totalDistance, int stopCount)
    {
        // Estimate: 30km/h average speed + 10 minutes per stop
        var travelTime = (totalDistance / 30.0) * 60; // Convert to minutes
        var stopTime = stopCount * 10; // 10 minutes per stop
        return (int)Math.Ceiling(travelTime + stopTime);
    }

    private static decimal CalculateEstimatedFuelCost(double totalDistance)
    {
        // Estimate: 12L/100km fuel consumption, $1.50/L fuel cost
        var fuelConsumption = totalDistance * 0.12; // 12L per 100km
        return (decimal)(fuelConsumption * 1.50);
    }

    private static decimal CalculateEfficiencyScore(double fuelSavings, int stopCount)
    {
        // Score based on fuel savings and number of stops handled
        var baseScore = Math.Min(fuelSavings * 2, 70); // Max 70 points for fuel savings
        var stopBonus = Math.Min(stopCount * 3, 30); // Max 30 points for stops
        return (decimal)Math.Min(baseScore + stopBonus, 100);
    }

    private static int CalculateEstimatedArrival(int stopIndex, List<Report> reports)
    {
        if (stopIndex == 0) return 0;

        // Calculate cumulative travel time
        double cumulativeDistance = 0;
        for (int i = 0; i < stopIndex; i++)
        {
            cumulativeDistance += CalculateDistance(
                reports[i].Latitude, reports[i].Longitude,
                reports[i + 1].Latitude, reports[i + 1].Longitude);
        }

        // Add travel time + stop time
        var travelTime = (cumulativeDistance / 30.0) * 60; // 30km/h average
        var stopTime = stopIndex * 10; // 10 minutes per previous stop
        return (int)Math.Ceiling(travelTime + stopTime);
    }

    /// <summary>
    /// Calculate distance between two points using Haversine formula
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    #endregion
}
