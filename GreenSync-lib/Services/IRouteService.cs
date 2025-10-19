using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public interface IRouteService
{
    Task<IEnumerable<Route>> GetAllRoutesAsync();
    Task<Route?> GetRouteByIdAsync(Guid id);
    Task<IEnumerable<Route>> GetRoutesByStatusAsync(RouteStatus status);
    Task<Route> CreateRouteAsync(Route route);
    Task<Route?> UpdateRouteAsync(Guid id, Route route);
    Task<bool> DeleteRouteAsync(Guid id);
    Task<Route> OptimizeRouteAsync(List<Report> reports);
    Task<bool> AssignTruckToRouteAsync(Guid routeId, string truckId, string? driverId = null);
    Task<bool> UpdateRouteStatusAsync(Guid routeId, RouteStatus status);
    Task<IEnumerable<Route>> GetActiveRoutesAsync();
    Task<RouteOptimizationResult> SimulateRouteOptimizationAsync(List<Report> reports);
}

public class RouteOptimizationResult
{
    public Route OptimizedRoute { get; set; } = new();
    public double FuelSavingsPercent { get; set; }
    public double TimeReduction { get; set; }
    public string OptimizationMethod { get; set; } = string.Empty;
    public List<string> OptimizationSteps { get; set; } = new();
}
