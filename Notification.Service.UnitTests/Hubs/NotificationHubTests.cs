using FluentAssertions;
using Notification.Service.Hubs;

namespace Notification.Service.UnitTests.Hubs;

public class NotificationHubTests
{
    [Fact]
    public void NotificationHub_ShouldInheritFromHub()
    {
        // Arrange & Act
        var hub = new NotificationHub();

        // Assert
        hub.Should().BeAssignableTo<Microsoft.AspNetCore.SignalR.Hub>();
    }
}

