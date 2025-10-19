using GreenSync.Lib.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services;

/// <summary>
/// Local file system implementation of IFileStorageService.
/// Stores files in wwwroot/uploads/reports/{reportId}/ directory.
/// This is a placeholder implementation for development.
/// Production should use Azure Blob Storage implementation.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const string UploadFolder = "uploads/reports";
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<List<ReportImage>> SaveImagesAsync(IFormFileCollection files, Guid reportId)
    {
        var reportImages = new List<ReportImage>();

        if (files == null || files.Count == 0)
        {
            return reportImages;
        }

        // Create directory structure: wwwroot/uploads/reports/{reportId}/
        var reportUploadPath = Path.Combine(_environment.WebRootPath, UploadFolder, reportId.ToString());
        Directory.CreateDirectory(reportUploadPath);

        foreach (var file in files)
        {
            try
            {
                // Validate file
                if (!IsValidImageFile(file))
                {
                    _logger.LogWarning("Invalid file type rejected: {FileName}", file.FileName);
                    continue;
                }

                if (file.Length > MaxFileSize)
                {
                    _logger.LogWarning("File too large rejected: {FileName}, Size: {Size}", file.FileName, file.Length);
                    continue;
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(reportUploadPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create ReportImage entity
                var reportImage = new ReportImage
                {
                    ReportId = reportId,
                    FileName = file.FileName,
                    FilePath = Path.Combine(UploadFolder, reportId.ToString(), uniqueFileName),
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                };

                reportImages.Add(reportImage);
                _logger.LogInformation("Image saved: {FileName} for report {ReportId}", uniqueFileName, reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
            }
        }

        return reportImages;
    }

    public async Task<bool> DeleteImageAsync(ReportImage image)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, image.FilePath);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Image deleted: {FilePath}", image.FilePath);
                return true;
            }
            
            _logger.LogWarning("Image file not found: {FilePath}", image.FilePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {FilePath}", image.FilePath);
            return false;
        }
    }

    public async Task<bool> DeleteImagesAsync(IEnumerable<ReportImage> images)
    {
        var allDeleted = true;
        
        foreach (var image in images)
        {
            var deleted = await DeleteImageAsync(image);
            if (!deleted)
            {
                allDeleted = false;
            }
        }
        
        return allDeleted;
    }

    public string GetImageUrl(ReportImage image)
    {
        // Return relative URL path for local files
        // Format: /uploads/reports/{reportId}/{filename}
        return $"/{image.FilePath.Replace("\\", "/")}";
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return false;
        }

        // Check MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return false;
        }

        // Check file size
        if (file.Length > MaxFileSize)
        {
            return false;
        }

        return true;
    }
}
