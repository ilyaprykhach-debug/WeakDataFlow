using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Notification.Service.Controllers;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.UnitTests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<NotificationController>> _mockLogger;
    private readonly NotificationController _controller;

    public NotificationControllerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<NotificationController>>();
        _controller = new NotificationController(_mockNotificationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendNotification_ShouldReturnOk_WhenNotificationIsValid()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataReceivedFromApi",
            ServiceName = "TestService",
            Timestamp = DateTime.UtcNow,
            Message = "Test notification"
        };

        _mockNotificationService
            .Setup(x => x.NotifyAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SendNotification(notification);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockNotificationService.Verify(
            x => x.NotifyAsync(It.Is<NotificationEvent>(n => n == notification)),
            Times.Once);
    }

    [Fact]
    public async Task SendNotification_ShouldReturnBadRequest_WhenNotificationIsNull()
    {
        // Arrange
        NotificationEvent? notification = null;

        // Act
        var result = await _controller.SendNotification(notification!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Notification is required");
        _mockNotificationService.Verify(
            x => x.NotifyAsync(It.IsAny<NotificationEvent>()),
            Times.Never);
    }

    [Fact]
    public async Task SendNotification_ShouldLogInformation_WhenNotificationIsSent()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataSavedToDatabase",
            ServiceName = "DataProcessor",
            Timestamp = DateTime.UtcNow
        };

        _mockNotificationService
            .Setup(x => x.NotifyAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.SendNotification(notification);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Notification received and broadcasted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendNotification_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "TestEvent",
            ServiceName = "TestService",
            Timestamp = DateTime.UtcNow
        };

        _mockNotificationService
            .Setup(x => x.NotifyAsync(It.IsAny<NotificationEvent>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.SendNotification(notification);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Failed to process notification");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendNotification_ShouldHandleNotificationWithAllProperties()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataPublishedToQueue",
            ServiceName = "DataIngestor",
            Timestamp = DateTime.UtcNow,
            Data = new { QueueName = "sensor-data", MessageCount = 10 },
            Message = "Data published successfully"
        };

        _mockNotificationService
            .Setup(x => x.NotifyAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SendNotification(notification);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockNotificationService.Verify(
            x => x.NotifyAsync(It.Is<NotificationEvent>(n => 
                n.EventType == "DataPublishedToQueue" &&
                n.ServiceName == "DataIngestor" &&
                n.Message == "Data published successfully")),
            Times.Once);
    }
}

