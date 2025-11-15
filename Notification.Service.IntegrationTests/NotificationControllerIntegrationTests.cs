using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Notification.Service.Models;
using Notification.Service.Services;
using System.Net;
using System.Net.Http.Json;

namespace Notification.Service.IntegrationTests;

public class NotificationControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public NotificationControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/Notification", notification);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendNotification_ShouldReturnBadRequest_WhenNotificationIsNull()
    {
        // Act
        var response = await _client.PostAsJsonAsync<NotificationEvent>("/api/Notification", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendNotification_ShouldProcessNotification_WithAllProperties()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataSavedToDatabase",
            ServiceName = "DataProcessor",
            Timestamp = DateTime.UtcNow,
            Data = new { RecordId = 123, Status = "Success" },
            Message = "Data saved successfully"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Notification", notification);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void NotificationService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<INotificationService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<INotificationService>();
    }

    [Fact]
    public async Task SendNotification_ShouldHandleMultipleNotifications()
    {
        // Arrange
        var notifications = new[]
        {
            new NotificationEvent
            {
                EventType = "DataReceivedFromApi",
                ServiceName = "DataIngestor",
                Timestamp = DateTime.UtcNow
            },
            new NotificationEvent
            {
                EventType = "DataPublishedToQueue",
                ServiceName = "DataIngestor",
                Timestamp = DateTime.UtcNow
            },
            new NotificationEvent
            {
                EventType = "DataReadFromQueue",
                ServiceName = "DataProcessor",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act & Assert
        foreach (var notification in notifications)
        {
            var response = await _client.PostAsJsonAsync("/api/Notification", notification);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

