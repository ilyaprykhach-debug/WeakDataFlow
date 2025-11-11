using DataIngestor.Service.HealthChecks;
using DataIngestor.Service.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataIngestor.Service.UnitTests.HealthChecks;

public class QueueHealthCheckTests
{
    private readonly Mock<IQueueService> _mockQueueService;
    private readonly Mock<ILogger<QueueHealthCheck>> _mockLogger;
    private readonly QueueHealthCheck _healthCheck;

    public QueueHealthCheckTests()
    {
        _mockQueueService = new Mock<IQueueService>();
        _mockLogger = new Mock<ILogger<QueueHealthCheck>>();
        _healthCheck = new QueueHealthCheck(_mockQueueService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenQueueIsConnected()
    {
        // Arrange
        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("RabbitMQ connection is active");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenQueueIsNotConnected()
    {
        // Arrange
        _mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("RabbitMQ connection is not available");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenExceptionOccurs()
    {
        // Arrange
        var exception = new Exception("Connection error");
        _mockQueueService.Setup(x => x.IsConnectedAsync()).ThrowsAsync(exception);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("RabbitMQ health check failed with exception");
        result.Exception.Should().Be(exception);
    }
}

