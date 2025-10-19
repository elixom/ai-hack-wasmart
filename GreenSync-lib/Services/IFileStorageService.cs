using GreenSync.Lib.Models;
using Microsoft.AspNetCore.Http;

namespace GreenSync.Lib.Services;

/// <summary>
/// Interface for file storage operations. 
/// Local implementation stores files on disk.
/// Azure implementation will store files in Azure Blob Storage.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves uploaded image files and returns ReportImage entities
    /// </summary>
    /// <param name="files">Collection of uploaded files</param>
    /// <param name="reportId">ID of the associated report</param>
    /// <returns>List of ReportImage entities with file metadata</returns>
    Task<List<ReportImage>> SaveImagesAsync(IFormFileCollection files, Guid reportId);
    
    /// <summary>
    /// Deletes an image file
    /// </summary>
    /// <param name="image">ReportImage entity to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteImageAsync(ReportImage image);
    
    /// <summary>
    /// Deletes multiple image files
    /// </summary>
    /// <param name="images">Collection of ReportImage entities to delete</param>
    /// <returns>True if all deletions successful</returns>
    Task<bool> DeleteImagesAsync(IEnumerable<ReportImage> images);
    
    /// <summary>
    /// Gets the public URL or path for accessing an image
    /// </summary>
    /// <param name="image">ReportImage entity</param>
    /// <returns>Public URL or relative path</returns>
    string GetImageUrl(ReportImage image);
    
    /// <summary>
    /// Validates if the file is an acceptable image type
    /// </summary>
    /// <param name="file">File to validate</param>
    /// <returns>True if valid image type</returns>
    bool IsValidImageFile(IFormFile file);
}
