using GreenSync.Lib.Services;
using GreenSync.Lib.Models;

namespace GreenSync_app.Models;

public class DashboardViewModel
{
    public User User { get; set; } = new();
    public List<Report> RecentReports { get; set; } = new();
    public EcoCredit? EcoCredit { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int CollectedReports { get; set; }
    public List<string> RecentAlerts { get; set; } = new()
    {
        "Your report from Downtown Main Street has been assigned to a collection truck",
        "Scheduled pickup for Park Avenue Commercial District tomorrow at 9:00 AM",
        "You earned 25 Eco-Credits for your recent recycling report!"
    };
    public string AzureMapsSubscriptionKey { get; set; } = string.Empty;
}
