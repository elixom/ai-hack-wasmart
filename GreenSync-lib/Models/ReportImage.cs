using System.ComponentModel.DataAnnotations;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents an image associated with a waste report
/// </summary>
public class ReportImage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid ReportId { get; set; }
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// MIME type (e.g., image/jpeg, image/png)
    /// </summary>
    [Required]
    public string ContentType { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Azure Blob URL (will be populated when integrated with Azure Blob Storage)
    /// </summary>
    public string? BlobUrl { get; set; }
}
