using GreenSync.Lib.Models;

namespace GreenSync_app.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    public int TotalReports { get; set; }
    public int OpenReports { get; set; }
    public int AssignedReports { get; set; }
    public int CollectedReports { get; set; }
    
    public int TotalRoutes { get; set; }
    public int ActiveRoutes { get; set; }
    public int CompletedRoutes { get; set; }
    
    public decimal TotalEcoCredits { get; set; }
    
    public List<Report> RecentReports { get; set; } = new();
    public List<Report> HotspotReports { get; set; } = new();
    public List<GreenSync.Lib.Models.Route> ActiveRoutesList { get; set; } = new();
    public List<FleetVehicle> MockTrucks { get; set; } = new();
}
