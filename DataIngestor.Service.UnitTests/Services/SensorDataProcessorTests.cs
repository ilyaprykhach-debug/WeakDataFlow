using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Models;
using DataIngestor.Service.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataIngestor.Service.UnitTests.Services;

public class SensorDataProcessorTests
{
    private readonly Mock<IExternalApiService> _mockApiService;
    private readonly Mock<IQueueService> _mockQueueService;
    private readonly Mock<ILogger<SensorDataProcessor>> _mockLogger;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly SensorDataProcessor _processor;

    public SensorDataProcessorTests()
    {
        _mockApiService = new Mock<IExternalApiService>();
        _mockQueueService = new Mock<IQueueService>();
        _mockLogger = new Mock<ILogger<SensorDataProcessor>>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(INotificationClient))).Returns((INotificationClient?)null);

        _processor = new SensorDataProcessor(
            _mockApiService.Object,
            _mockQueueService.Object,
            _mockLogger.Object,
            _mockServiceScopeFactory.Object);
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldPublishAllReadings_WhenQueueIsConnectedAndApiIsHealthy()
    {
        // Arrange
        var readings = new List<SensorReading>
        {
            new SensorReading { Type = "energy", Location = "Location1", EnergyConsumption = 100m },
            new SensorReading { Type = "air_quality", Location = "Location2", Co2 = 400 }
        };

        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockApiService.Setup(x => x.FetchDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(readings);

        // Act
        await _processor.ProcessDataAsync();

        // Assert
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), "sensor-data", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _mockQueueService.Verify(
            x => x.PublishAsync(readings[0], "sensor-data", It.IsAny<CancellationToken>()),
            Times.Once);
        _mockQueueService.Verify(
            x => x.PublishAsync(readings[1], "sensor-data", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldNotPublish_WhenQueueIsNotConnected()
    {
        // Arrange
        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);

        // Act
        await _processor.ProcessDataAsync();

        // Assert
        _mockApiService.Verify(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockApiService.Verify(x => x.FetchDataAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldNotPublish_WhenApiIsNotHealthy()
    {
        // Arrange
        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        await _processor.ProcessDataAsync();

        // Assert
        _mockApiService.Verify(x => x.FetchDataAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldHandleEmptyReadings()
    {
        // Arrange
        var emptyReadings = new List<SensorReading>();

        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockApiService.Setup(x => x.FetchDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emptyReadings);

        // Act
        await _processor.ProcessDataAsync();

        // Assert
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldHandleMultipleReadingsOfSameType()
    {
        // Arrange
        var readings = new List<SensorReading>
        {
            new SensorReading { Type = "energy", Location = "Location1", EnergyConsumption = 100m },
            new SensorReading { Type = "energy", Location = "Location2", EnergyConsumption = 200m },
            new SensorReading { Type = "energy", Location = "Location3", EnergyConsumption = 300m }
        };

        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockApiService.Setup(x => x.FetchDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(readings);

        // Act
        await _processor.ProcessDataAsync();

        // Assert
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), "sensor-data", It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessDataAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var readings = new List<SensorReading>
        {
            new SensorReading { Type = "energy", Location = "Location1" }
        };

        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.CheckHealthAsync(cancellationToken)).ReturnsAsync(true);
        _mockApiService.Setup(x => x.FetchDataAsync(cancellationToken)).ReturnsAsync(readings);

        // Act
        await _processor.ProcessDataAsync(cancellationToken);

        // Assert
        _mockApiService.Verify(x => x.CheckHealthAsync(cancellationToken), Times.Once);
        _mockApiService.Verify(x => x.FetchDataAsync(cancellationToken), Times.Once);
        _mockQueueService.Verify(
            x => x.PublishAsync(It.IsAny<SensorReading>(), "sensor-data", cancellationToken),
            Times.Once);
    }
}

