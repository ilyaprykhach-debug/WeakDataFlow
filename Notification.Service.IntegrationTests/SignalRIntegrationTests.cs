using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Notification.Service.Models;
using System.Net.Http.Json;

namespace Notification.Service.IntegrationTests;

public class SignalRIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private NotificationEvent? _receivedNotification;
    private readonly SemaphoreSlim _notificationReceived = new(0, 1);

    public SignalRIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var baseUrl = _httpClient.BaseAddress!.ToString().TrimEnd('/');
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/notificationHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        _hubConnection.On<NotificationEvent>("NotificationReceived", notification =>
        {
            _receivedNotification = notification;
            _notificationReceived.Release();
        });

        await _hubConnection.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        _httpClient.Dispose();
    }

    [Fact]
    public async Task NotificationHub_ShouldReceiveNotification_WhenNotificationIsSent()
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
        await _httpClient.PostAsJsonAsync("/api/Notification", notification);
        
        var received = await _notificationReceived.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        received.Should().BeTrue();
        _receivedNotification.Should().NotBeNull();
        _receivedNotification!.EventType.Should().Be(notification.EventType);
        _receivedNotification.ServiceName.Should().Be(notification.ServiceName);
        _receivedNotification.Message.Should().Be(notification.Message);
    }

    [Fact]
    public async Task NotificationHub_ShouldBroadcastToAllClients()
    {
        // Arrange
        var notification1 = new NotificationEvent
        {
            EventType = "DataSavedToDatabase",
            ServiceName = "DataProcessor",
            Timestamp = DateTime.UtcNow
        };

        var notification2 = new NotificationEvent
        {
            EventType = "DataPublishedToQueue",
            ServiceName = "DataIngestor",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _httpClient.PostAsJsonAsync("/api/Notification", notification1);
        var received1 = await _notificationReceived.WaitAsync(TimeSpan.FromSeconds(5));
        var firstNotification = _receivedNotification;
        _receivedNotification = null;

        await _httpClient.PostAsJsonAsync("/api/Notification", notification2);
        var received2 = await _notificationReceived.WaitAsync(TimeSpan.FromSeconds(5));
        var secondNotification = _receivedNotification;

        // Assert
        firstNotification.Should().NotBeNull();
        firstNotification!.EventType.Should().Be("DataSavedToDatabase");
        secondNotification.Should().NotBeNull();
        secondNotification!.EventType.Should().Be("DataPublishedToQueue");
    }

    [Fact]
    public async Task NotificationHub_ShouldHandleNotificationWithData()
    {
        // Arrange
        var notification = new NotificationEvent
        {
            EventType = "DataReadFromQueue",
            ServiceName = "DataProcessor",
            Timestamp = DateTime.UtcNow,
            Data = new { QueueName = "sensor-data", MessageCount = 5 },
            Message = "Messages processed"
        };

        // Act
        await _httpClient.PostAsJsonAsync("/api/Notification", notification);
        var received = await _notificationReceived.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        received.Should().BeTrue();
        _receivedNotification.Should().NotBeNull();
        _receivedNotification!.Data.Should().NotBeNull();
    }
}

