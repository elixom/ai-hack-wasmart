using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public class InMemoryReportService : IReportService
{
    private readonly List<Report> _reports = new();

    public InMemoryReportService()
    {
        // Seed with sample data
        SeedSampleData();
    }

    public Task<IEnumerable<Report>> GetAllReportsAsync()
    {
        return Task.FromResult(_reports.AsEnumerable());
    }

    public Task<Report?> GetReportByIdAsync(Guid id)
    {
        var report = _reports.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(report);
    }

    public Task<IEnumerable<Report>> GetReportsByUserIdAsync(string userId)
    {
        var reports = _reports.Where(r => r.UserId == userId);
        return Task.FromResult(reports);
    }

    public Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
    {
        var reports = _reports.Where(r => r.Status == status);
        return Task.FromResult(reports);
    }

    public Task<Report> CreateReportAsync(Report report)
    {
        report.Id = Guid.NewGuid();
        report.Timestamp = DateTime.UtcNow;
        _reports.Add(report);
        return Task.FromResult(report);
    }

    public Task<Report?> UpdateReportAsync(Guid id, Report report)
    {
        var existingReport = _reports.FirstOrDefault(r => r.Id == id);
        if (existingReport == null)
            return Task.FromResult<Report?>(null);

        existingReport.Location = report.Location;
        existingReport.Latitude = report.Latitude;
        existingReport.Longitude = report.Longitude;
        existingReport.Status = report.Status;
        existingReport.Description = report.Description;
        existingReport.ImageUrl = report.ImageUrl;
        existingReport.Priority = report.Priority;
        existingReport.EstimatedVolume = report.EstimatedVolume;
        existingReport.WasteType = report.WasteType;
        existingReport.AssignedTruckId = report.AssignedTruckId;

        if (report.Status == ReportStatus.Collected)
            existingReport.CollectedAt = DateTime.UtcNow;

        return Task.FromResult<Report?>(existingReport);
    }

    public Task<bool> DeleteReportAsync(Guid id)
    {
        var report = _reports.FirstOrDefault(r => r.Id == id);
        if (report == null)
            return Task.FromResult(false);

        _reports.Remove(report);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Report>> GetUnassignedReportsAsync()
    {
        var reports = _reports.Where(r => r.Status == ReportStatus.Reported);
        return Task.FromResult(reports);
    }

    public Task<IEnumerable<Report>> GetReportsByLocationAsync(double latitude, double longitude, double radiusKm)
    {
        var reports = _reports.Where(r => CalculateDistance(latitude, longitude, r.Latitude, r.Longitude) <= radiusKm);
        return Task.FromResult(reports);
    }

    public Task<bool> AssignReportToTruckAsync(Guid reportId, string truckId)
    {
        var report = _reports.FirstOrDefault(r => r.Id == reportId);
        if (report == null)
            return Task.FromResult(false);

        report.AssignedTruckId = truckId;
        report.Status = ReportStatus.Assigned;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status)
    {
        var report = _reports.FirstOrDefault(r => r.Id == reportId);
        if (report == null)
            return Task.FromResult(false);

        report.Status = status;
        if (status == ReportStatus.Collected)
            report.CollectedAt = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    public Task<IEnumerable<Report>> GetHotspotReportsAsync()
    {
        // Group reports by location proximity and return areas with multiple reports
        var hotspots = _reports
            .Where(r => r.Status == ReportStatus.Reported)
            .GroupBy(r => new { 
                LatGroup = Math.Floor(r.Latitude * 100) / 100, 
                LonGroup = Math.Floor(r.Longitude * 100) / 100 
            })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g);

        return Task.FromResult(hotspots);
    }

    private void SeedSampleData()
    {
        var sampleReports = new List<Report>
        {
            new() {
                Id = Guid.NewGuid(),
                Location = "Downtown Main Street",
                Latitude = 4.6097,
                Longitude = -74.0817,
                Status = ReportStatus.Reported,
                Description = "Large pile of garbage bags overflowing from containers",
                UserId = "user1",
                Priority = ReportPriority.High,
                EstimatedVolume = 15.5,
                WasteType = WasteType.General,
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new() {
                Id = Guid.NewGuid(),
                Location = "Park Avenue Commercial District",
                Latitude = 4.6351,
                Longitude = -74.0703,
                Status = ReportStatus.Assigned,
                Description = "Recyclable materials scattered around dumpster",
                UserId = "user2",
                Priority = ReportPriority.Medium,
                EstimatedVolume = 8.2,
                WasteType = WasteType.Recyclable,
                AssignedTruckId = "TRUCK001",
                Timestamp = DateTime.UtcNow.AddHours(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                Location = "Residential Zone 5th Street",
                Latitude = 4.6234,
                Longitude = -74.0845,
                Status = ReportStatus.Reported,
                Description = "Organic waste spillage",
                UserId = "user1",
                Priority = ReportPriority.Medium,
                EstimatedVolume = 4.7,
                WasteType = WasteType.Organic,
                Timestamp = DateTime.UtcNow.AddMinutes(-45)
            },
            new() {
                Id = Guid.NewGuid(),
                Location = "Shopping Mall Parking Lot",
                Latitude = 4.6278,
                Longitude = -74.0761,
                Status = ReportStatus.Collected,
                Description = "Electronics disposal",
                UserId = "user3",
                Priority = ReportPriority.Low,
                EstimatedVolume = 2.1,
                WasteType = WasteType.Electronic,
                AssignedTruckId = "TRUCK002",
                CollectedAt = DateTime.UtcNow.AddMinutes(-15),
                Timestamp = DateTime.UtcNow.AddHours(-3)
            }
        };

        _reports.AddRange(sampleReports);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points on Earth
        const double R = 6371; // Radius of Earth in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
