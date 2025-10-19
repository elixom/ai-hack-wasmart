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
    private readonly IMapsService _mapsService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService, 
        IAuthService authService, 
        IEcoCreditService ecoCreditService, 
        IMapsService mapsService,
        IFileStorageService fileStorageService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _authService = authService;
        _ecoCreditService = ecoCreditService;
        _mapsService = mapsService;
        _fileStorageService = fileStorageService;
        _logger = logger;
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

        // Validate image files if provided
        if (Request.Form.Files.Count > 0)
        {
            foreach (var file in Request.Form.Files)
            {
                if (!_fileStorageService.IsValidImageFile(file))
                {
                    ModelState.AddModelError("ImageFiles", $"Invalid file type: {file.FileName}. Only JPG, PNG, GIF, and WebP images are allowed (max 10MB).");
                    return View(model);
                }
            }
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

        // Handle image file uploads
        if (Request.Form.Files.Count > 0)
        {
            try
            {
                var uploadedImages = await _fileStorageService.SaveImagesAsync(Request.Form.Files, createdReport.Id);
                createdReport.Images.AddRange(uploadedImages);
                
                // Update report with images
                await _reportService.UpdateReportAsync(createdReport.Id, createdReport);
                
                _logger.LogInformation("Uploaded {Count} images for report {ReportId}", uploadedImages.Count, createdReport.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images for report {ReportId}", createdReport.Id);
                TempData["Warning"] = "Report created but some images failed to upload.";
            }
        }

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

        // Pass Azure Maps subscription key for map rendering
        ViewBag.AzureMapsSubscriptionKey = _mapsService.GetSubscriptionKey();

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

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
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

        // Users can only edit their own reports
        if (report.UserId != currentUser.Id)
        {
            return Forbid();
        }

        // Only allow editing if status is still Reported
        if (report.Status != ReportStatus.Reported)
        {
            TempData["Warning"] = "This report cannot be edited as it has already been processed.";
        }

        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Report report)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id != report.Id)
        {
            return NotFound();
        }

        // Verify ownership
        var existingReport = await _reportService.GetReportByIdAsync(id);
        if (existingReport == null || existingReport.UserId != currentUser.Id)
        {
            return Forbid();
        }

        // Prevent editing if already assigned
        if (existingReport.Status != ReportStatus.Reported)
        {
            TempData["Error"] = "Cannot edit a report that has already been assigned or processed.";
            return RedirectToAction("Details", new { id });
        }

        // Validate image files if provided
        if (Request.Form.Files.Count > 0)
        {
            foreach (var file in Request.Form.Files)
            {
                if (!_fileStorageService.IsValidImageFile(file))
                {
                    ModelState.AddModelError("ImageFiles", $"Invalid file type: {file.FileName}. Only JPG, PNG, GIF, and WebP images are allowed (max 10MB).");
                    return View(report);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return View(report);
        }

        // Preserve important fields that shouldn't be changed via edit
        report.UserId = existingReport.UserId;
        report.Status = existingReport.Status;
        report.Timestamp = existingReport.Timestamp;
        report.AssignedVehicleId = existingReport.AssignedVehicleId;
        report.CollectedAt = existingReport.CollectedAt;
        report.Images = existingReport.Images; // Preserve existing images

        // Handle new image file uploads
        if (Request.Form.Files.Count > 0)
        {
            try
            {
                var uploadedImages = await _fileStorageService.SaveImagesAsync(Request.Form.Files, report.Id);
                report.Images.AddRange(uploadedImages);
                
                _logger.LogInformation("Uploaded {Count} additional images for report {ReportId}", uploadedImages.Count, report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images for report {ReportId}", report.Id);
                TempData["Warning"] = "Report updated but some images failed to upload.";
            }
        }

        var updatedReport = await _reportService.UpdateReportAsync(id, report);
        if (updatedReport == null)
        {
            TempData["Error"] = "Failed to update report. Please try again.";
            return View(report);
        }

        TempData["Success"] = "Report updated successfully!";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
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

        // Verify ownership
        if (report.UserId != currentUser.Id)
        {
            return Forbid();
        }

        // Prevent deletion if already assigned
        if (report.Status != ReportStatus.Reported)
        {
            TempData["Error"] = "Cannot delete a report that has already been assigned or processed.";
            return RedirectToAction("Details", new { id });
        }

        var success = await _reportService.DeleteReportAsync(id);
        if (success)
        {
            TempData["Success"] = "Report deleted successfully.";
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Failed to delete report. Please try again.";
        return RedirectToAction("Details", new { id });
    }
}
