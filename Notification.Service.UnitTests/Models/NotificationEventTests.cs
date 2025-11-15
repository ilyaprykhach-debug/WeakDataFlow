using FluentAssertions;
using Notification.Service.Models;

namespace Notification.Service.UnitTests.Models;

public class NotificationEventTests
{
    [Fact]
    public void NotificationEvent_ShouldInitializeWithDefaultValues()
    {
        // Act
        var notification = new NotificationEvent();

        // Assert
        notification.EventType.Should().BeEmpty();
        notification.ServiceName.Should().BeEmpty();
        notification.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.Data.Should().BeNull();
        notification.Message.Should().BeNull();
    }

    [Fact]
    public void NotificationEvent_ShouldSetAllProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var data = new { SensorId = 123, Value = 45.6 };

        // Act
        var notification = new NotificationEvent
        {
            EventType = "DataReceivedFromApi",
            ServiceName = "DataIngestor",
            Timestamp = timestamp,
            Data = data,
            Message = "Test message"
        };

        // Assert
        notification.EventType.Should().Be("DataReceivedFromApi");
        notification.ServiceName.Should().Be("DataIngestor");
        notification.Timestamp.Should().Be(timestamp);
        notification.Data.Should().Be(data);
        notification.Message.Should().Be("Test message");
    }

    [Fact]
    public void EventType_Enum_ShouldContainAllExpectedValues()
    {
        // Act & Assert
        Enum.GetValues<EventType>().Should().Contain(EventType.DataReceivedFromApi);
        Enum.GetValues<EventType>().Should().Contain(EventType.DataPublishedToQueue);
        Enum.GetValues<EventType>().Should().Contain(EventType.DataReadFromQueue);
        Enum.GetValues<EventType>().Should().Contain(EventType.DataSavedToDatabase);
    }
}

