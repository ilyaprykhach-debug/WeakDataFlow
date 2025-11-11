using DataIngestor.Service.HealthChecks;
using DataIngestor.Service.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataIngestor.Service.UnitTests.HealthChecks;

public class ExternalApiHealthCheckTests
{
    private readonly Mock<IExternalApiService> _mockApiService;
    private readonly Mock<ILogger<ExternalApiHealthCheck>> _mockLogger;
    private readonly ExternalApiHealthCheck _healthCheck;

    public ExternalApiHealthCheckTests()
    {
        _mockApiService = new Mock<IExternalApiService>();
        _mockLogger = new Mock<ILogger<ExternalApiHealthCheck>>();
        _healthCheck = new ExternalApiHealthCheck(_mockApiService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenApiIsHealthy()
    {
        // Arrange
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("WeakApp API is responding correctly");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenApiIsNotHealthy()
    {
        // Arrange
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("WeakApp API is not responding");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenExceptionOccurs()
    {
        // Arrange
        var exception = new HttpRequestException("Connection failed");
        _mockApiService.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("WeakApp API health check failed with exception");
        result.Exception.Should().Be(exception);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _mockApiService.Setup(x => x.CheckHealthAsync(cancellationToken)).ReturnsAsync(true);
        var context = new HealthCheckContext();

        // Act
        await _healthCheck.CheckHealthAsync(context, cancellationToken);

        // Assert
        _mockApiService.Verify(x => x.CheckHealthAsync(cancellationToken), Times.Once);
    }
}

