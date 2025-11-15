namespace Notification.Service.Models;

public class NotificationEvent
{
    public string EventType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public object? Data { get; set; }
    public string? Message { get; set; }
}

public enum EventType
{
    DataReceivedFromApi,
    DataPublishedToQueue,
    DataReadFromQueue,
    DataSavedToDatabase
}

