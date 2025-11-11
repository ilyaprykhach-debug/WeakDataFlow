using DataIngestor.Service.Interfaces;

namespace DataIngestor.Service.Services;

public class SensorDataProcessor : ISensorDataProcessor
{
    private readonly IExternalApiService _apiService;
    private readonly IQueueService _queueService;
    private readonly ILogger<SensorDataProcessor> _logger;

    public SensorDataProcessor(
        IExternalApiService apiService,
        IQueueService queueService,
        ILogger<SensorDataProcessor> logger)
    {
        _apiService = apiService;
        _queueService = queueService;
        _logger = logger;
    }

    public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        if (!await _queueService.IsConnectedAsync())
        {
            _logger.LogError("Queue connection is not available");
            return;
        }

        if (!await _apiService.CheckHealthAsync(cancellationToken))
        {
            _logger.LogWarning("WeakApp API is not healthy");
            return;
        }

        var sensorReadings = await _apiService.FetchDataAsync(cancellationToken);

        if (sensorReadings.Any())
        {
            foreach (var reading in sensorReadings)
            {
                await _queueService.PublishAsync(reading, "sensor-data", cancellationToken);
            }

            _logger.LogInformation("Successfully ingested {Count} sensor readings", sensorReadings.Count);

            var byType = sensorReadings.GroupBy(r => r.Type);
            foreach (var group in byType)
            {
                _logger.LogDebug("Type {Type}: {Count} readings", group.Key, group.Count());
            }
        }
        else
        {
            _logger.LogWarning("No data received from WeakApp API");
        }
    }
}
