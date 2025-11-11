using DataIngestor.Service.Configuration;
using DataIngestor.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace DataIngestor.Service.Workers;

public class DataIngestionWorker : BackgroundService
{
    private readonly ISensorDataProcessor _dataProcessor;
    private readonly ILogger<DataIngestionWorker> _logger;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _initialDelay;

    public DataIngestionWorker(
        ISensorDataProcessor dataProcessor,
        ILogger<DataIngestionWorker> logger,
        IOptions<DataIngestionConfig> config)
    {
        _dataProcessor = dataProcessor;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(config.Value.IntervalSeconds);
        _initialDelay = TimeSpan.FromSeconds(config.Value.InitialDelaySeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Data Ingestion Worker started. Interval: {Interval}s, Initial delay: {Delay}s",
            _interval.TotalSeconds, _initialDelay.TotalSeconds);

        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _dataProcessor.ProcessDataAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in data ingestion cycle");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Data Ingestion Worker is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
