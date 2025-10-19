using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public interface IReportService
{
    Task<IEnumerable<Report>> GetAllReportsAsync();
    Task<Report?> GetReportByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetReportsByUserIdAsync(string userId);
    Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status);
    Task<Report> CreateReportAsync(Report report);
    Task<Report?> UpdateReportAsync(Guid id, Report report);
    Task<bool> DeleteReportAsync(Guid id);
    Task<IEnumerable<Report>> GetUnassignedReportsAsync();
    Task<IEnumerable<Report>> GetReportsByLocationAsync(double latitude, double longitude, double radiusKm);
    Task<bool> AssignReportToTruckAsync(Guid reportId, string truckId);
    Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status);
    Task<IEnumerable<Report>> GetHotspotReportsAsync();
}
