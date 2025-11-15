using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Notification.Service.Hubs;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockClients.Setup(x => x.All).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);

        _service = new NotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task NotifyAsync_ShouldSendNotificationToAllClients()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataReceivedFromApi",
            ServiceName = "TestService",
            Timestamp = DateTime.UtcNow,
            Message = "Test message"
        };

        // Act
        await _service.NotifyAsync(notification);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "NotificationReceived",
                It.Is<object[]>(args => args[0] == notification),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_ShouldLogDebugMessage()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataSavedToDatabase",
            ServiceName = "DataProcessor",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _service.NotifyAsync(notification);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "TestEvent",
            ServiceName = "TestService",
            Timestamp = DateTime.UtcNow
        };

        _mockClientProxy
            .Setup(x => x.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SignalR error"));

        // Act
        await _service.NotifyAsync(notification);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_ShouldHandleNullNotificationData()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "TestEvent",
            ServiceName = "TestService",
            Timestamp = DateTime.UtcNow,
            Data = null
        };

        // Act
        await _service.NotifyAsync(notification);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "NotificationReceived",
                It.Is<object[]>(args => args[0] == notification),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_ShouldHandleNotificationWithData()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataReceivedFromApi",
            ServiceName = "DataIngestor",
            Timestamp = DateTime.UtcNow,
            Data = new { SensorId = 123, Value = 45.6 },
            Message = "Sensor data received"
        };

        // Act
        await _service.NotifyAsync(notification);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "NotificationReceived",
                It.Is<object[]>(args => 
                    args[0] != null &&
                    ((NotificationEvent)args[0]).EventType == "DataReceivedFromApi" &&
                    ((NotificationEvent)args[0]).ServiceName == "DataIngestor"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

