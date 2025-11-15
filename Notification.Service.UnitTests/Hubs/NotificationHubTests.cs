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

    [Fact]
    public void NotificationHub_ShouldHaveJoinGroupMethod()
    {
        // Arrange
        var hub = new NotificationHub();
        var method = typeof(NotificationHub).GetMethod("JoinGroup");

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void NotificationHub_ShouldHaveLeaveGroupMethod()
    {
        // Arrange
        var hub = new NotificationHub();
        var method = typeof(NotificationHub).GetMethod("LeaveGroup");

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void JoinGroup_ShouldAcceptStringParameter()
    {
        // Arrange
        var method = typeof(NotificationHub).GetMethod("JoinGroup");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[0].Name.Should().Be("groupName");
    }

    [Fact]
    public void LeaveGroup_ShouldAcceptStringParameter()
    {
        // Arrange
        var method = typeof(NotificationHub).GetMethod("LeaveGroup");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[0].Name.Should().Be("groupName");
    }
}

