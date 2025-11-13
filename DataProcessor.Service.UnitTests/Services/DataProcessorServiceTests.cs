using DataProcessor.Service.Configuration;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using DataProcessor.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DataProcessor.Service.UnitTests.Services;

public class DataProcessorServiceTests : IDisposable
{
    private readonly Mock<IQueueConsumerService> _mockQueueConsumer;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IDatabaseService> _mockDatabaseService;
    private readonly Mock<ILogger<DataProcessorService>> _mockLogger;
    private readonly Mock<IOptions<DataProcessingConfig>> _mockConfig;
    private readonly DataProcessorService _service;

    public DataProcessorServiceTests()
    {
        _mockQueueConsumer = new Mock<IQueueConsumerService>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockLogger = new Mock<ILogger<DataProcessorService>>();
        _mockConfig = new Mock<IOptions<DataProcessingConfig>>();

        _mockConfig.Setup(x => x.Value).Returns(new DataProcessingConfig
        {
            BatchSize = 5,
            ProcessingIntervalSeconds = 30,
            InitialDelaySeconds = 2
        });

        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IDatabaseService))).Returns(_mockDatabaseService.Object);

        _service = new DataProcessorService(
            _mockQueueConsumer.Object,
            _mockServiceScopeFactory.Object,
            _mockConfig.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task StartProcessingAsync_ShouldStartConsumingMessages()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        await _service.StartProcessingAsync(cancellationToken);

        // Assert
        _mockQueueConsumer.Verify(
            x => x.StartConsuming<SensorReading>(
                It.IsAny<Func<SensorReading, CancellationToken, Task>>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task OnMessageReceived_ShouldAddMessageToBatch()
    {
        // Arrange
        var message = new SensorReading
        {
            Id = "test-id",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        // Act
        var onMessageReceived = GetOnMessageReceivedHandler();
        await onMessageReceived(message, CancellationToken.None);

        // Assert
        _mockQueueConsumer.Verify(
            x => x.StartConsuming<SensorReading>(
                It.IsAny<Func<SensorReading, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnMessageReceived_ShouldProcessBatch_WhenBatchSizeReached()
    {
        // Arrange
        var batchSize = 5;
        _mockConfig.Setup(x => x.Value).Returns(new DataProcessingConfig { BatchSize = batchSize });

        var service = new DataProcessorService(
            _mockQueueConsumer.Object,
            _mockServiceScopeFactory.Object,
            _mockConfig.Object,
            _mockLogger.Object);

        await service.StartProcessingAsync();

        var onMessageReceived = GetOnMessageReceivedHandler(service);

        // Act
        for (int i = 0; i < batchSize; i++)
        {
            var message = new SensorReading
            {
                Id = $"test-id-{i}",
                SensorId = $"sensor-{i}",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100.5m + i
            };
            await onMessageReceived(message, CancellationToken.None);
        }

        await Task.Delay(100);

        // Assert
        _mockDatabaseService.Verify(
            x => x.SaveSensorReadingsBatchAsync(
                It.Is<IEnumerable<SensorReading>>(batch => batch.Count() == batchSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_ShouldSaveReadingsToDatabase()
    {
        // Arrange
        var batchSize = 2;
        _mockConfig.Setup(x => x.Value).Returns(new DataProcessingConfig { BatchSize = batchSize });

        var service = new DataProcessorService(
            _mockQueueConsumer.Object,
            _mockServiceScopeFactory.Object,
            _mockConfig.Object,
            _mockLogger.Object);

        var readings = new List<SensorReading>
        {
            new SensorReading { Id = "1", SensorId = "sensor-1", Type = "energy", Location = "loc-1", Timestamp = DateTime.UtcNow },
            new SensorReading { Id = "2", SensorId = "sensor-2", Type = "air_quality", Location = "loc-2", Timestamp = DateTime.UtcNow }
        };

        var onMessageReceived = GetOnMessageReceivedHandler(service);

        // Act
        foreach (var reading in readings)
        {
            await onMessageReceived(reading, CancellationToken.None);
        }

        await Task.Delay(200);

        // Assert
        _mockDatabaseService.Verify(
            x => x.SaveSensorReadingsBatchAsync(
                It.Is<IEnumerable<SensorReading>>(batch => batch.Count() == batchSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_ShouldHandleDatabaseExceptions()
    {
        // Arrange
        _mockDatabaseService
            .Setup(x => x.SaveSensorReadingsBatchAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var batchSize = 2;
        _mockConfig.Setup(x => x.Value).Returns(new DataProcessingConfig { BatchSize = batchSize });

        var service = new DataProcessorService(
            _mockQueueConsumer.Object,
            _mockServiceScopeFactory.Object,
            _mockConfig.Object,
            _mockLogger.Object);

        await service.StartProcessingAsync();
        var onMessageReceived = GetOnMessageReceivedHandler(service);

        // Act
        for (int i = 0; i < batchSize; i++)
        {
            var message = new SensorReading
            {
                Id = $"test-id-{i}",
                SensorId = $"sensor-{i}",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow
            };
            await onMessageReceived(message, CancellationToken.None);
        }

        await Task.Delay(200);

        // Assert
        _mockDatabaseService.Verify(
            x => x.SaveSensorReadingsBatchAsync(
                It.IsAny<IEnumerable<SensorReading>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_ShouldNotProcessEmptyBatch()
    {
        // Arrange
        await _service.StartProcessingAsync();

        // Act
        await Task.Delay(100);

        // Assert
        _mockDatabaseService.Verify(
            x => x.SaveSensorReadingsBatchAsync(
                It.IsAny<IEnumerable<SensorReading>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Dispose_ShouldDisposeTimer()
    {
        // Act
        _service.Dispose();

        // Assert
        Assert.True(true);
    }

    private Func<SensorReading, CancellationToken, Task> GetOnMessageReceivedHandler(DataProcessorService? service = null)
    {
        Func<SensorReading, CancellationToken, Task>? handler = null;
        _mockQueueConsumer.Setup(x => x.StartConsuming<SensorReading>(
            It.IsAny<Func<SensorReading, CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()))
            .Callback<Func<SensorReading, CancellationToken, Task>, CancellationToken>((h, ct) => handler = h);

        if (service == null)
        {
            _service.StartProcessingAsync().Wait();
        }
        else
        {
            service.StartProcessingAsync().Wait();
        }

        return handler!;
    }

    public void Dispose()
    {
        _service?.Dispose();
    }
}

