using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GreenSync_admin.Models;
using GreenSync.Lib.Services;
using GreenSync.Lib.Models;

namespace GreenSync_admin.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReportService _reportService;
    private readonly IRouteService _routeService;
    private readonly IEcoCreditService _ecoCreditService;
    private readonly IAuthService _authService;

    public HomeController(ILogger<HomeController> logger, IReportService reportService, 
                         IRouteService routeService, IEcoCreditService ecoCreditService, 
                         IAuthService authService)
    {
        _logger = logger;
        _reportService = reportService;
        _routeService = routeService;
        _ecoCreditService = ecoCreditService;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            return RedirectToAction("Login", "Auth");
        }

        var allReports = await _reportService.GetAllReportsAsync();
        var allRoutes = await _routeService.GetAllRoutesAsync();
        var hotspotReports = await _reportService.GetHotspotReportsAsync();
        var activeRoutes = await _routeService.GetActiveRoutesAsync();
        var totalCredits = await _ecoCreditService.GetTotalCreditsInSystemAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalReports = allReports.Count(),
            OpenReports = allReports.Count(r => r.Status == ReportStatus.Reported),
            AssignedReports = allReports.Count(r => r.Status == ReportStatus.Assigned),
            CollectedReports = allReports.Count(r => r.Status == ReportStatus.Collected),
            
            TotalRoutes = allRoutes.Count(),
            ActiveRoutes = activeRoutes.Count(),
            CompletedRoutes = allRoutes.Count(r => r.Status == RouteStatus.Completed),
            
            TotalEcoCredits = totalCredits,
            
            RecentReports = allReports.OrderByDescending(r => r.Timestamp).Take(5).ToList(),
            HotspotReports = hotspotReports.Take(10).ToList(),
            ActiveRoutesList = activeRoutes.ToList(),
            
            MockTrucks = new List<MockTruck>
            {
                new() { Id = "TRUCK001", Status = "In Progress", Driver = "John Smith", Location = "Downtown Area", FuelLevel = 75 },
                new() { Id = "TRUCK002", Status = "Available", Driver = "Maria Garcia", Location = "Depot", FuelLevel = 90 },
                new() { Id = "TRUCK003", Status = "Maintenance", Driver = "Bob Wilson", Location = "Service Center", FuelLevel = 45 },
                new() { Id = "TRUCK004", Status = "In Progress", Driver = "Sarah Davis", Location = "Residential Zone", FuelLevel = 60 }
            }
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
