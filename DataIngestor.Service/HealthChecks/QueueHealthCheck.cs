using DataIngestor.Service.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataIngestor.Service.HealthChecks;

public class QueueHealthCheck : IHealthCheck
{
    private readonly IQueueService _queueService;
    private readonly ILogger<QueueHealthCheck> _logger;

    public QueueHealthCheck(
        IQueueService queueService,
        ILogger<QueueHealthCheck> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = await _queueService.IsConnectedAsync();

            if (isConnected)
            {
                _logger.LogDebug("RabbitMQ health check passed");
                return HealthCheckResult.Healthy("RabbitMQ connection is active");
            }
            else
            {
                _logger.LogWarning("RabbitMQ health check failed");
                return HealthCheckResult.Unhealthy("RabbitMQ connection is not available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ health check failed with exception");
            return HealthCheckResult.Unhealthy(
                "RabbitMQ health check failed with exception",
                exception: ex);
        }
    }
}