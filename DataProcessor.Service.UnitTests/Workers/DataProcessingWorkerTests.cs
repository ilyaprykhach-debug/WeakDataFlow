using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Workers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataProcessor.Service.UnitTests.Workers;

public class DataProcessingWorkerTests
{
    private readonly Mock<IDataProcessor> _mockDataProcessor;
    private readonly Mock<ILogger<DataProcessingWorker>> _mockLogger;
    private readonly DataProcessingWorker _worker;

    public DataProcessingWorkerTests()
    {
        _mockDataProcessor = new Mock<IDataProcessor>();
        _mockLogger = new Mock<ILogger<DataProcessingWorker>>();
        _worker = new DataProcessingWorker(_mockDataProcessor.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStartDataProcessor()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockDataProcessor
            .Setup(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var executeTask = _worker.StartAsync(cancellationToken);
        
        await Task.Delay(100);
        cancellationTokenSource.Cancel();

        await Task.WhenAny(executeTask, Task.Delay(1000));

        // Assert
        _mockDataProcessor.Verify(
            x => x.StartProcessingAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleCancellation()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockDataProcessor
            .Setup(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var executeTask = _worker.StartAsync(cancellationToken);
        
        cancellationTokenSource.Cancel();

        await Task.Delay(100);

        // Assert
        _mockDataProcessor.Verify(
            x => x.StartProcessingAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationTokenToDataProcessor()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        CancellationToken? capturedToken = null;
        _mockDataProcessor
            .Setup(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => capturedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        var executeTask = _worker.StartAsync(cancellationToken);
        
        await Task.Delay(100);
        cancellationTokenSource.Cancel();

        await Task.WhenAny(executeTask, Task.Delay(1000));

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.Value.CanBeCanceled.Should().Be(cancellationToken.CanBeCanceled);
        capturedToken.Value.Should().NotBe(CancellationToken.None);
    }
}

