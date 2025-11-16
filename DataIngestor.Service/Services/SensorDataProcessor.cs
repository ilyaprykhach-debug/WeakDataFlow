using DataIngestor.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataIngestor.Service.Services;

public class SensorDataProcessor : ISensorDataProcessor
{
    private readonly IExternalApiService _apiService;
    private readonly IQueueService _queueService;
    private readonly ILogger<SensorDataProcessor> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SensorDataProcessor(
        IExternalApiService apiService,
        IQueueService queueService,
        ILogger<SensorDataProcessor> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _apiService = apiService;
        _queueService = queueService;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var notificationClient = scope.ServiceProvider.GetService<INotificationClient>();
                    if (notificationClient != null)
                    {
                        _ = notificationClient.NotifyDataPublishedToQueueAsync(
                            new
                            {
                                reading.Id,
                                reading.SensorId,
                                reading.Type,
                                reading.Location,
                                reading.Timestamp,
                                reading.EnergyConsumption,
                                reading.Co2,
                                reading.Pm25,
                                reading.Humidity,
                                reading.MotionDetected
                            },
                            cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send notification about data published to queue");
                }
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
