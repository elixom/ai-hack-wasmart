
using Azure.Core.GeoJson;
using Azure;
using Azure.Identity;
using Azure.Maps.Search;
using Azure.Maps.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services;

/// <summary>
/// Service for Azure Maps operations using Microsoft Entra authentication
/// </summary>
public class AzureMapsService : IMapsService
{
    private readonly MapsSearchClient _searchClient;
    private readonly string _clientId;
    private readonly ILogger<AzureMapsService> _logger;

    public AzureMapsService(IConfiguration configuration, ILogger<AzureMapsService> logger)
    {
        _logger = logger;
        _clientId = configuration["AzureMaps:ClientId"] ?? throw new InvalidOperationException("Azure Maps ClientId not configured");

        // Use DefaultAzureCredential for Microsoft Entra authentication
        // This will use environment variables: AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID
        var credential = new DefaultAzureCredential();
        _searchClient = new MapsSearchClient(credential, _clientId);

        _logger.LogInformation("Azure Maps service initialized with Microsoft Entra authentication");
    }

    /// <summary>
    /// Get coordinates for a given address (Geocoding)
    /// </summary>
    public async Task<GeocodingResponse?> GetCoordinatesAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Attempted to geocode empty address");
                return null;
            }

            Response<GeocodingResponse> result = await _searchClient.GetGeocodingAsync(address);
            
            if (result?.Value?.Features?.Count > 0)
            {
                _logger.LogInformation("Successfully geocoded address: {Address}", address);
                return result.Value;
            }

            _logger.LogWarning("No results found for address: {Address}", address);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return null;
        }
    }

    /// <summary>
    /// Get address for given coordinates (Reverse Geocoding)
    /// </summary>
    public async Task<GeocodingResponse?> GetAddressAsync(GeoPosition coordinates)
    {
        try
        {
            Response<GeocodingResponse> result = await _searchClient.GetReverseGeocodingAsync(coordinates);
            
            if (result?.Value?.Features?.Count > 0)
            {
                _logger.LogInformation("Successfully reverse geocoded coordinates: {Coordinates}", coordinates);
                return result.Value;
            }

            _logger.LogWarning("No address found for coordinates: {Coordinates}", coordinates);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reverse geocoding coordinates: {Coordinates}", coordinates);
            return null;
        }
    }

    /// <summary>
    /// Get the Azure Maps client ID for frontend map rendering
    /// </summary>
    public string GetClientId()
    {
        return _clientId;
    }
}
