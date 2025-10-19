using GreenSync.Lib.Models;
using Microsoft.AspNetCore.Http;

namespace GreenSync_app.Models;

public class CreateReportViewModel
{
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    public double EstimatedVolume { get; set; }
    public WasteType WasteType { get; set; } = WasteType.General;
    
    /// <summary>
    /// Legacy field - kept for backward compatibility
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Multiple image file uploads
    /// </summary>
    public IFormFileCollection? ImageFiles { get; set; }
}
