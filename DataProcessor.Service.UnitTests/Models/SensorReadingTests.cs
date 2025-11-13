using DataProcessor.Service.Models;
using FluentAssertions;

namespace DataProcessor.Service.UnitTests.Models;

public class SensorReadingTests
{
    [Fact]
    public void SensorReading_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var reading = new SensorReading();

        // Assert
        reading.Id.Should().NotBeNullOrEmpty();
        reading.SensorId.Should().Be(string.Empty);
        reading.Type.Should().Be(string.Empty);
        reading.Location.Should().Be(string.Empty);
        reading.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        reading.EnergyConsumption.Should().BeNull();
        reading.Co2.Should().BeNull();
        reading.Pm25.Should().BeNull();
        reading.Humidity.Should().BeNull();
        reading.MotionDetected.Should().BeNull();
    }

    [Fact]
    public void SensorReading_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow;

        // Act
        var reading = new SensorReading
        {
            Id = id,
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = timestamp,
            EnergyConsumption = 100.5m,
            Co2 = 400,
            Pm25 = 20,
            Humidity = 50,
            MotionDetected = true
        };

        // Assert
        reading.Id.Should().Be(id);
        reading.SensorId.Should().Be("sensor-1");
        reading.Type.Should().Be("energy");
        reading.Location.Should().Be("location-1");
        reading.Timestamp.Should().Be(timestamp);
        reading.EnergyConsumption.Should().Be(100.5m);
        reading.Co2.Should().Be(400);
        reading.Pm25.Should().Be(20);
        reading.Humidity.Should().Be(50);
        reading.MotionDetected.Should().Be(true);
    }

    [Fact]
    public void SensorReading_ShouldSupportEnergyType()
    {
        // Arrange & Act
        var reading = new SensorReading
        {
            Id = "energy-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 123.45m
        };

        // Assert
        reading.Type.Should().Be("energy");
        reading.EnergyConsumption.Should().Be(123.45m);
        reading.Co2.Should().BeNull();
        reading.Pm25.Should().BeNull();
        reading.Humidity.Should().BeNull();
        reading.MotionDetected.Should().BeNull();
    }

    [Fact]
    public void SensorReading_ShouldSupportAirQualityType()
    {
        // Arrange & Act
        var reading = new SensorReading
        {
            Id = "air-1",
            SensorId = "sensor-2",
            Type = "air_quality",
            Location = "location-2",
            Timestamp = DateTime.UtcNow,
            Co2 = 400,
            Pm25 = 20,
            Humidity = 50
        };

        // Assert
        reading.Type.Should().Be("air_quality");
        reading.Co2.Should().Be(400);
        reading.Pm25.Should().Be(20);
        reading.Humidity.Should().Be(50);
        reading.EnergyConsumption.Should().BeNull();
        reading.MotionDetected.Should().BeNull();
    }

    [Fact]
    public void SensorReading_ShouldSupportMotionType()
    {
        // Arrange & Act
        var reading = new SensorReading
        {
            Id = "motion-1",
            SensorId = "sensor-3",
            Type = "motion",
            Location = "location-3",
            Timestamp = DateTime.UtcNow,
            MotionDetected = true
        };

        // Assert
        reading.Type.Should().Be("motion");
        reading.MotionDetected.Should().Be(true);
        reading.EnergyConsumption.Should().BeNull();
        reading.Co2.Should().BeNull();
        reading.Pm25.Should().BeNull();
        reading.Humidity.Should().BeNull();
    }

    [Fact]
    public void SensorReading_ShouldGenerateUniqueId()
    {
        // Arrange & Act
        var reading1 = new SensorReading();
        var reading2 = new SensorReading();

        // Assert
        reading1.Id.Should().NotBe(reading2.Id);
    }
}

