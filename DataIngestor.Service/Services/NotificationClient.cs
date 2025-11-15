using System.Text;
using System.Text.Json;

namespace DataIngestor.Service.Services;

public interface INotificationClient
{
    Task NotifyDataReceivedFromApiAsync(IEnumerable<object> data, CancellationToken cancellationToken = default);
    Task NotifyDataPublishedToQueueAsync(object data, CancellationToken cancellationToken = default);
}

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;
    private readonly string _notificationServiceUrl;

    public NotificationClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _notificationServiceUrl = configuration["NotificationService:BaseUrl"] ?? "http://localhost:5003";
    }

    public async Task NotifyDataReceivedFromApiAsync(IEnumerable<object> data, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                EventType = "DataReceivedFromApi",
                ServiceName = "DataIngestor.Service",
                Timestamp = DateTime.UtcNow,
                Data = data,
                Message = $"Received {data.Count()} items from external API"
            };

            await SendNotificationAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification about data received from API");
        }
    }

    public async Task NotifyDataPublishedToQueueAsync(object data, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                EventType = "DataPublishedToQueue",
                ServiceName = "DataIngestor.Service",
                Timestamp = DateTime.UtcNow,
                Data = data,
                Message = "Data published to RabbitMQ queue"
            };

            await SendNotificationAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification about data published to queue");
        }
    }

    private async Task SendNotificationAsync(object notification, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notification", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Notification service returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to notification service");
        }
    }
}

