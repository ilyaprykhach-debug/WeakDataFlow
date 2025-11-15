using Microsoft.AspNetCore.Mvc;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] NotificationEvent notification)
    {
        if (notification == null)
        {
            return BadRequest("Notification is required");
        }

        try
        {
            await _notificationService.NotifyAsync(notification);
            _logger.LogInformation(
                "Notification received and broadcasted: {EventType} from {ServiceName}",
                notification.EventType,
                notification.ServiceName);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification");
            return StatusCode(500, "Failed to process notification");
        }
    }
}

