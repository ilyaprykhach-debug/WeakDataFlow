using DataIngestor.Service.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataIngestor.Service.HealthChecks;

public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly IExternalApiService _apiService;
    private readonly ILogger<ExternalApiHealthCheck> _logger;

    public ExternalApiHealthCheck(
        IExternalApiService apiService,
        ILogger<ExternalApiHealthCheck> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _apiService.CheckHealthAsync(cancellationToken);

            if (isHealthy)
            {
                _logger.LogDebug("WeakApp API health check passed");
                return HealthCheckResult.Healthy("WeakApp API is responding correctly");
            }
            else
            {
                _logger.LogWarning("WeakApp API health check failed");
                return HealthCheckResult.Unhealthy("WeakApp API is not responding");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WeakApp API health check failed with exception");
            return HealthCheckResult.Unhealthy(
                "WeakApp API health check failed with exception",
                exception: ex);
        }
    }
}
