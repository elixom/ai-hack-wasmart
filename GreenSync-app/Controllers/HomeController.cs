using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GreenSync_app.Models;
using GreenSync.Lib.Services;

namespace GreenSync_app.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthService _authService;
    private readonly IReportService _reportService;
    private readonly IEcoCreditService _ecoCreditService;

    public HomeController(ILogger<HomeController> logger, IAuthService authService, 
                         IReportService reportService, IEcoCreditService ecoCreditService)
    {
        _logger = logger;
        _authService = authService;
        _reportService = reportService;
        _ecoCreditService = ecoCreditService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get user's reports and eco-credits
        var userReports = await _reportService.GetReportsByUserIdAsync(currentUser.Id);
        var ecoCredit = await _ecoCreditService.GetEcoCreditByUserIdAsync(currentUser.Id);

        var viewModel = new DashboardViewModel
        {
            User = currentUser,
            RecentReports = userReports.OrderByDescending(r => r.Timestamp).Take(5).ToList(),
            EcoCredit = ecoCredit,
            TotalReports = userReports.Count(),
            PendingReports = userReports.Count(r => r.Status == GreenSync.Lib.Models.ReportStatus.Reported),
            CollectedReports = userReports.Count(r => r.Status == GreenSync.Lib.Models.ReportStatus.Collected)
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
