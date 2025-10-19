using Microsoft.AspNetCore.Mvc;
using GreenSync.Lib.Services;
using GreenSync.Lib.Models;
using GreenSync_app.Models;

namespace GreenSync_app.Controllers;

public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly IAuthService _authService;
    private readonly IEcoCreditService _ecoCreditService;

    public ReportsController(IReportService reportService, IAuthService authService, IEcoCreditService ecoCreditService)
    {
        _reportService = reportService;
        _authService = authService;
        _ecoCreditService = ecoCreditService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var userReports = await _reportService.GetReportsByUserIdAsync(currentUser.Id);
        return View(userReports.OrderByDescending(r => r.Timestamp));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReportViewModel model)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var report = new Report
        {
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Description = model.Description,
            UserId = currentUser.Id,
            Status = ReportStatus.Reported,
            Priority = model.Priority,
            EstimatedVolume = model.EstimatedVolume,
            WasteType = model.WasteType,
            ImageUrl = model.ImageUrl
        };

        var createdReport = await _reportService.CreateReportAsync(report);

        // Award eco-credits for reporting
        await _ecoCreditService.AddCreditsAsync(
            currentUser.Id, 
            10, // Base credits for reporting
            $"Report submitted for {model.Location}",
            createdReport.Id
        );

        TempData["Success"] = "Report submitted successfully! You've earned 10 Eco-Credits.";
        return RedirectToAction("Details", new { id = createdReport.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        // Users can only view their own reports
        if (report.UserId != currentUser.Id)
        {
            return Forbid();
        }

        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> Track()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var userReports = await _reportService.GetReportsByUserIdAsync(currentUser.Id);
        var activeReports = userReports.Where(r => r.Status != ReportStatus.Collected && r.Status != ReportStatus.Cancelled);

        return View(activeReports.OrderByDescending(r => r.Timestamp));
    }
}
