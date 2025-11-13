using DataProcessor.Service.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataProcessor.Service.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly SensorDataDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        SensorDataDbContext context,
        ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                _logger.LogDebug("PostgreSQL health check passed");
                return HealthCheckResult.Healthy("PostgreSQL connection is active");
            }
            else
            {
                _logger.LogWarning("PostgreSQL health check failed");
                return HealthCheckResult.Unhealthy("PostgreSQL connection is not available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PostgreSQL health check failed with exception");
            return HealthCheckResult.Unhealthy(
                "PostgreSQL health check failed with exception",
                exception: ex);
        }
    }
}

