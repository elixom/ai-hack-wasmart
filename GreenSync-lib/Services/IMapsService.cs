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
    /// <returns>Geocoding result (placeholder for future implementation)</returns>
    Task<object?> GetCoordinatesAsync(string address);

    /// <summary>
    /// Get address for given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates to reverse geocode</param>
    /// <returns>Reverse geocoding result (placeholder for future implementation)</returns>
    Task<object?> GetAddressAsync(object coordinates);

    /// <summary>
    /// Get the Azure Maps subscription key for frontend map rendering
    /// </summary>
    /// <returns>The subscription key for Azure Maps</returns>
    string GetSubscriptionKey();
}
