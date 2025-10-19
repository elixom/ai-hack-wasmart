using Azure.Maps.Search.Models;
using Azure.Core.GeoJson;

namespace GreenSync.Lib.Services;

/// <summary>
/// Interface for Azure Maps service operations
/// </summary>
public interface IMapsService
{
    /// <summary>
    /// Get coordinates for a given address
    /// </summary>
    /// <param name="address">The address to geocode</param>
    /// <returns>GeocodingResponse with coordinate information</returns>
    Task<GeocodingResponse?> GetCoordinatesAsync(string address);

    /// <summary>
    /// Get address for given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates to reverse geocode</param>
    /// <returns>GeocodingResponse with address information</returns>
    Task<GeocodingResponse?> GetAddressAsync(GeoPosition coordinates);

    /// <summary>
    /// Get the Azure Maps client ID for frontend map rendering
    /// </summary>
    /// <returns>The client ID for Azure Maps</returns>
    string GetClientId();
}
