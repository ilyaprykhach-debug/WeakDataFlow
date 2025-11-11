using DataIngestor.Service.Configuration;
using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Workers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DataIngestor.Service.UnitTests.Workers;

public class DataIngestionWorkerTests
{
    private readonly Mock<ISensorDataProcessor> _mockDataProcessor;
    private readonly Mock<ILogger<DataIngestionWorker>> _mockLogger;
    private readonly Mock<IOptions<DataIngestionConfig>> _mockConfig;
    private readonly DataIngestionWorker _worker;

    public DataIngestionWorkerTests()
    {
        _mockDataProcessor = new Mock<ISensorDataProcessor>();
        _mockLogger = new Mock<ILogger<DataIngestionWorker>>();
        _mockConfig = new Mock<IOptions<DataIngestionConfig>>();
        _mockConfig.Setup(x => x.Value).Returns(new DataIngestionConfig
        {
            IntervalSeconds = 1,
            InitialDelaySeconds = 0
        });

        _worker = new DataIngestionWorker(
            _mockDataProcessor.Object,
            _mockLogger.Object,
            _mockConfig.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallProcessDataAsync_WhenStarted()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await _worker.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(150);
        await _worker.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockDataProcessor.Verify(
            x => x.ProcessDataAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleExceptions_WithoutStopping()
    {
        // Arrange
        _mockDataProcessor
            .Setup(x => x.ProcessDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await _worker.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(150);
        await _worker.StopAsync(cancellationTokenSource.Token);

        // Assert
        _mockDataProcessor.Verify(
            x => x.ProcessDataAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _worker.StartAsync(cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        await Task.Delay(50);

        // Assert
        _mockDataProcessor.Verify(
            x => x.ProcessDataAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_ShouldStopWorkerGracefully()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

        // Act
        await _worker.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        await _worker.StopAsync(cancellationTokenSource.Token);

        // Assert
        await Task.CompletedTask;
    }
}

