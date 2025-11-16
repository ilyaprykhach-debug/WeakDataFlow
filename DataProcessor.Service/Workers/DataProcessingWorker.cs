using DataProcessor.Service.Interfaces;

namespace DataProcessor.Service.Workers;

public class DataProcessingWorker : BackgroundService
{
    private readonly IDataProcessor _dataProcessor;
    private readonly ILogger<DataProcessingWorker> _logger;

    public DataProcessingWorker(
        IDataProcessor dataProcessor,
        ILogger<DataProcessingWorker> logger)
    {
        _dataProcessor = dataProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Processing Worker started");

        await _dataProcessor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}


