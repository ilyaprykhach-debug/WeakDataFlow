using Microsoft.AspNetCore.SignalR;
using Notification.Service.Hubs;
using Notification.Service.Models;

namespace Notification.Service.Services;

public interface INotificationService
{
    Task NotifyAsync(NotificationEvent notification);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyAsync(NotificationEvent notification)
    {
        try
        {
            _logger.LogDebug(
                "Sending notification: {EventType} from {ServiceName}",
                notification.EventType,
                notification.ServiceName);

            await _hubContext.Clients.All.SendAsync("NotificationReceived", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
        }
    }
}

