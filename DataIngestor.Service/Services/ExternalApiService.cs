using DataIngestor.Service.Configuration;
using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DataIngestor.Service.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;
    private readonly ExternalApiConfig _config;

    public ExternalApiService(
        HttpClient httpClient,
        ILogger<ExternalApiService> logger,
        IOptions<ExternalApiConfig> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<List<SensorReading>> FetchDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching data from WeakApp API...");

            var metersUrl = $"{_config.BaseUrl}/meters";

            using var request = new HttpRequestMessage(HttpMethod.Get, metersUrl);
            request.Headers.Add("X-Api-Key", _config.Headers.XApiKey);

            _logger.LogDebug("Sending request to: {Url}", metersUrl);
            _logger.LogDebug("Headers: {Headers}", string.Join(", ", request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            _logger.LogDebug("Response status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("WeakApp API returned {StatusCode}", response.StatusCode);

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error response: {Error}", errorContent);

                return new List<SensorReading>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var weakApiResponses = JsonSerializer.Deserialize<List<WeakApiResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var sensorReadings = weakApiResponses?
                .Select(SensorReading.FromWeakApiResponse)
                .Where(reading => reading != null)
                .ToList() ?? new List<SensorReading>();

            _logger.LogInformation("Fetched {Count} sensor readings from WeakApp API", sensorReadings.Count);
            return sensorReadings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data from WeakApp API");
            throw;
        }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthUrl = $"{_config.BaseUrl}/health";

            using var request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
            request.Headers.Add("X-Api-Key", _config.Headers.XApiKey);

            _logger.LogDebug("Health check to: {Url}", healthUrl);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var isHealthy = response.IsSuccessStatusCode;

            if (!isHealthy)
            {
                _logger.LogWarning("WeakApp API health check failed: {StatusCode}", response.StatusCode);
            }
            else
            {
                _logger.LogDebug("WeakApp API health check passed");
            }

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WeakApp API health check failed with exception");
            return false;
        }
    }
}