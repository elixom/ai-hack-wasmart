using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GreenSync_app.Areas.Admin.Models;
using GreenSync_app.Models;
using GreenSync.Lib.Services;
using GreenSync.Lib.Models;

namespace GreenSync_app.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReportService _reportService;
    private readonly IRouteService _routeService;
    private readonly IEcoCreditService _ecoCreditService;
    private readonly IAuthService _authService;
    private readonly IFleetVehicleService _fleetVehicleService;

    public HomeController(ILogger<HomeController> logger, IReportService reportService,
                         IRouteService routeService, IEcoCreditService ecoCreditService,
                         IAuthService authService, IFleetVehicleService fleetVehicleService)
    {
        _logger = logger;
        _reportService = reportService;
        _routeService = routeService;
        _ecoCreditService = ecoCreditService;
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
            
            MockTrucks = await _fleetVehicleService.GetAllVehiclesAsync(),
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View("~/Home/Privacy.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
