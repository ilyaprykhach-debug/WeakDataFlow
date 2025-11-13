using DataProcessor.Service.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataProcessor.Service.HealthChecks;

public class QueueHealthCheck : IHealthCheck
{
    private readonly IQueueConsumerService _queueConsumerService;
    private readonly ILogger<QueueHealthCheck> _logger;

    public QueueHealthCheck(
        IQueueConsumerService queueConsumerService,
        ILogger<QueueHealthCheck> logger)
    {
        _queueConsumerService = queueConsumerService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = await _queueConsumerService.IsConnectedAsync();

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


