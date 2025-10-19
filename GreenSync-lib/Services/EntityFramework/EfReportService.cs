using GreenSync.Lib.Data;
using GreenSync.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services.EntityFramework;

/// <summary>
/// Entity Framework implementation of the Report service
/// </summary>
public class EfReportService : IReportService
{
    private readonly GreenSyncDbContext _context;
    private readonly ILogger<EfReportService> _logger;

    public EfReportService(GreenSyncDbContext context, ILogger<EfReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Report>> GetAllReportsAsync()
    {
        try
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedRoute)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all reports");
            throw;
        }
    }

    public async Task<Report?> GetReportByIdAsync(Guid id)
    {
        try
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedRoute)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report {ReportId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetReportsByUserIdAsync(Guid userId)
    {
        try
        { 

            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedRoute)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
    {
        try
        {
            return await _context.Reports
                .Include(r => r.User)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedRoute)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports with status {Status}", status);
            throw;
        }
    }

    public async Task<Report> CreateReportAsync(Report report)
    {
        try
        {
            // Generate eco-credits based on waste type and volume
            report.CreditsEarned = CalculateEcoCredits(report.WasteType, report.EstimatedVolume);
            
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created report {ReportId} for user {UserId}", report.Id, report.UserId);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report for user {UserId}", report.UserId);
            throw;
        }
    }

    public async Task<Report?> UpdateReportAsync(Guid id, Report report)
    {
        try
        {
            var existingReport = await _context.Reports.FindAsync(id);
            if (existingReport == null)
            {
                return null;
            }

            // Update properties
            existingReport.Location = report.Location;
            existingReport.Latitude = report.Latitude;
            existingReport.Longitude = report.Longitude;
            existingReport.Description = report.Description;
            existingReport.Priority = report.Priority;
            existingReport.WasteType = report.WasteType;
            existingReport.EstimatedVolume = report.EstimatedVolume;
            existingReport.UpdatedAt = DateTime.UtcNow;

            // Recalculate eco-credits if waste details changed
            existingReport.CreditsEarned = CalculateEcoCredits(report.WasteType, report.EstimatedVolume);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated report {ReportId}", id);
            return existingReport;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteReportAsync(Guid id)
    {
        try
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return false;
            }

            // Soft delete
            report.IsDeleted = true;
            report.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted report {ReportId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetUnassignedReportsAsync()
    {
        try
        {
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.Status == ReportStatus.Reported && r.AssignedVehicleId == null)
                .OrderBy(r => r.Priority == ReportPriority.Critical ? 0 :
                             r.Priority == ReportPriority.High ? 1 :
                             r.Priority == ReportPriority.Medium ? 2 : 3)
                .ThenBy(r => r.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unassigned reports");
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetReportsByLocationAsync(double latitude, double longitude, double radiusKm)
    {
        try
        {
            // Simple distance calculation using Haversine formula approximation
            var reports = await _context.Reports
                .Include(r => r.User)
                .ToListAsync();

            return reports.Where(r => 
            {
                var distance = CalculateDistance(latitude, longitude, r.Latitude, r.Longitude);
                return distance <= radiusKm;
            }).OrderBy(r => CalculateDistance(latitude, longitude, r.Latitude, r.Longitude));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports by location");
            throw;
        }
    }

    public async Task<bool> AssignReportToTruckAsync(Guid reportId, Guid vehicleGuid)
    {
        try
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return false;
            }
             
                report.AssignedVehicleId = vehicleGuid;
                report.Status = ReportStatus.Assigned;
                report.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Assigned report {ReportId} to vehicle {VehicleId}", reportId, vehicleGuid);
                return true;
           

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning report {ReportId} to truck {TruckId}", reportId, vehicleGuid);
            throw;
        }
    }

    public async Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status)
    {
        try
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return false;
            }

            report.Status = status;
            report.UpdatedAt = DateTime.UtcNow;

            if (status == ReportStatus.Collected)
            {
                report.CollectedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated report {ReportId} status to {Status}", reportId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId} status", reportId);
            throw;
        }
    }

    public async Task<IEnumerable<Report>> GetHotspotReportsAsync()
    {
        try
        {
            // Get reports from the last 7 days grouped by approximate location
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.Timestamp >= cutoffDate && r.Status != ReportStatus.Collected)
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.Timestamp)
                .Take(50) // Limit to top 50 for performance
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hotspot reports");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculate eco-credits based on waste type and volume
    /// </summary>
    private static decimal CalculateEcoCredits(WasteType wasteType, decimal volume)
    {
        var baseCredits = wasteType switch
        {
            WasteType.Recyclable => 10m,
            WasteType.Organic => 8m,
            WasteType.Electronic => 15m,
            WasteType.Hazardous => 20m,
            WasteType.General => 5m,
            WasteType.Bulky => 12m,
            _ => 5m
        };

        // Credit calculation: base credits + volume bonus
        var volumeBonus = Math.Min(volume * 2m, 20m); // Max 20 credits from volume
        return Math.Round(baseCredits + volumeBonus, 2);
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
