using DataProcessor.Service.Configuration;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using Microsoft.Extensions.Options;

namespace DataProcessor.Service.Services;

public class DataProcessorService : IDataProcessor, IDisposable
{
    private readonly IQueueConsumerService _queueConsumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DataProcessorService> _logger;
    private readonly int _batchSize;
    private readonly List<SensorReading> _currentBatch = new();
    private readonly object _batchLock = new object();
    private readonly Timer _batchTimer;

    public DataProcessorService(
        IQueueConsumerService queueConsumer,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<DataProcessingConfig> config,
        ILogger<DataProcessorService> logger)
    {
        _queueConsumer = queueConsumer;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _batchSize = config.Value.BatchSize;

        _batchTimer = new Timer(ProcessBatchIfNeeded, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        _queueConsumer.StartConsuming<SensorReading>(
            OnMessageReceived,
            cancellationToken);

        _logger.LogInformation("Started processing messages with batch size: {BatchSize}", _batchSize);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SensorReading message, CancellationToken cancellationToken)
    {
        lock (_batchLock)
        {
            _currentBatch.Add(message);
            _logger.LogDebug("Added message to batch. Current batch size: {BatchSize}", _currentBatch.Count);
        }

        if (_currentBatch.Count >= _batchSize)
        {
            await ProcessBatchAsync(cancellationToken);
        }
    }

    private void ProcessBatchIfNeeded(object state)
    {
        lock (_batchLock)
        {
            if (_currentBatch.Count > 0)
            {
                _logger.LogInformation("Timer triggered - processing incomplete batch of {Count} messages", _currentBatch.Count);
                _ = ProcessBatchAsync(CancellationToken.None);
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start ProcessBatchAsync {batch}", _currentBatch);

        List<SensorReading> batchToProcess;
        lock (_batchLock)
        {
            batchToProcess = new List<SensorReading>(_currentBatch);
            _currentBatch.Clear();
        }

        _logger.LogInformation("Temp ProcessBatchAsync count: {Count}", batchToProcess.Count);

        if (batchToProcess.Any())
        {
            try
            {
                _logger.LogInformation("Processing batch of {Count} messages", batchToProcess.Count);

                using var scope = _serviceScopeFactory.CreateScope();
                _logger.LogInformation("Scope created");

                var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                _logger.LogInformation("DatabaseService created");

                await databaseService.SaveSensorReadingsBatchAsync(batchToProcess, cancellationToken);
                _logger.LogInformation("Successfully processed batch of {Count} messages", batchToProcess.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process batch of {Count} messages", batchToProcess.Count);
            }
        }
        else
        {
            _logger.LogWarning("BatchToProcess is empty!");
        }
    }

    public void Dispose()
    {
        _batchTimer?.Dispose();
    }
}

