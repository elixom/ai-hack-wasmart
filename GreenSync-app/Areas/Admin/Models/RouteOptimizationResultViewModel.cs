using GreenSync.Lib.Models;

namespace GreenSync_app.Areas.Admin.Models;

public class RouteOptimizationResultViewModel
{
    public GreenSync.Lib.Models.Route Route { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
    public List<string> OptimizationSteps { get; set; } = new();
}
