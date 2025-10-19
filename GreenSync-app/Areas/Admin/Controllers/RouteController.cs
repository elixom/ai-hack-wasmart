using Microsoft.AspNetCore.Mvc;
using GreenSync.Lib.Services;
using GreenSync.Lib.Models;
using GreenSync_app.Areas.Admin.Models;

namespace GreenSync_app.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Supervisor")]
public class RouteController : Controller
{
    private readonly IRouteService _routeService;
    private readonly IReportService _reportService;
    private readonly IAuthService _authService;
    private readonly IFleetVehicleService _fleetVehicleService;


    public RouteController(IRouteService routeService, IReportService reportService, IAuthService authService, IFleetVehicleService fleetVehicleService)
    {
        _routeService = routeService;
        _reportService = reportService;
        _authService = authService;
        _fleetVehicleService = fleetVehicleService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        var allRoutes = await _routeService.GetAllRoutesAsync();
        return View(allRoutes.OrderByDescending(r => r.CreatedAt));
    }

    [HttpGet]
    public async Task<IActionResult> Optimize()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        var unassignedReports = await _reportService.GetUnassignedReportsAsync();
        ViewBag.UnassignedReports = unassignedReports.Count();
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Optimize(bool includeAllReports = false)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        try
        {
            // Get reports to optimize
            var reportsToOptimize = includeAllReports 
                ? (await _reportService.GetReportsByStatusAsync(ReportStatus.Reported)).ToList()
                : (await _reportService.GetUnassignedReportsAsync()).Take(10).ToList();

            if (!reportsToOptimize.Any())
            {
                TempData["Error"] = "No reports available for route optimization.";
                return RedirectToAction("Optimize");
            }

            // Simulate AI-powered route optimization
            var optimizationResult = await _routeService.SimulateRouteOptimizationAsync(reportsToOptimize);


            var truck = await _fleetVehicleService.GetAvailableTruck();
            // Assign reports to the optimized route
            var ReportIds = optimizationResult.OptimizedRoute.Waypoints.Select(wp => wp.ReportId).ToList();
            foreach (var reportId in ReportIds)
            {
                if (reportId.HasValue)
                {
                    await _reportService.AssignReportToTruckAsync(reportId.Value, truck.Id);
                }
            }

            TempData["Success"] = $"Route optimization completed! Simulated {optimizationResult.FuelSavingsPercent}% fuel reduction. {reportsToOptimize.Count} reports assigned to optimized route.";
            
            return RedirectToAction("OptimizationResult", new { routeId = optimizationResult.OptimizedRoute.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Route optimization failed: {ex.Message}";
            return RedirectToAction("Optimize");
        }
    }

    [HttpGet]
    public async Task<IActionResult> OptimizationResult(Guid routeId)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        var route = await _routeService.GetRouteByIdAsync(routeId);
        if (route == null)
        {
            return NotFound();
        }

        // Get the reports included in this route
        var routeReports = new List<Report>();
        var ReportIds = route.AssignedReports.Select(wp => wp.Id).ToList();
        foreach (var reportId in ReportIds)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report != null)
            {
                routeReports.Add(report);
            }
        }

        var viewModel = new RouteOptimizationResultViewModel
        {
            Route = route,
            Reports = routeReports,
            OptimizationSteps = new List<string>
            {
                "1. Analyzed waste hotspot locations and priorities",
                "2. Applied machine learning distance optimization algorithm",
                "3. Factored in traffic patterns and road conditions",
                "4. Optimized for fuel efficiency and time reduction",
                "5. Generated optimal pickup sequence",
                $"6. Achieved {route.FuelSavingsMetric}% fuel reduction"
            }
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        var route = await _routeService.GetRouteByIdAsync(id);
        if (route == null)
        {
            return NotFound();
        }

        // Get the reports included in this route
        var routeReports = new List<Report>();
        var ReportIds = route.AssignedReports.Select(wp => wp.Id).ToList();
        foreach (var reportId in ReportIds)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report != null)
            {
                routeReports.Add(report);
            }
        }

        ViewBag.Reports = routeReports;
        return View(route);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, RouteStatus status)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var success = await _routeService.UpdateRouteStatusAsync(id, status);
        
        if (success)
        {
            return Json(new { success = true, message = $"Route status updated to {status}" });
        }

        return Json(new { success = false, message = "Failed to update route status" });
    }

    [HttpPost]
    public async Task<IActionResult> AssignTruck(Guid id, string truckId, string? driverId = null)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var success = await _routeService.AssignTruckToRouteAsync(id, truckId, driverId);
        
        if (success)
        {
            return Json(new { success = true, message = $"Truck {truckId} assigned to route" });
        }

        return Json(new { success = false, message = "Failed to assign truck to route" });
    }
}
