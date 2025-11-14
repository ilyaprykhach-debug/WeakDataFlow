using FluentAssertions;
using GraphQL.ApiGateway.Models;

namespace GraphQL.ApiGateway.UnitTests.Models;

public class SensorReadingTests
{
    [Fact]
    public void SensorReading_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var reading = new SensorReading();

        // Assert
        reading.Id.Should().NotBeNullOrEmpty();
        reading.SensorId.Should().BeEmpty();
        reading.Type.Should().BeEmpty();
        reading.Location.Should().BeEmpty();
        reading.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SensorReading_Properties_ShouldBeSettable()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.AddHours(-1);

        // Act
        var reading = new SensorReading
        {
            Id = id,
            SensorId = "sensor-123",
            Type = "energy",
            Location = "Office A",
            Timestamp = timestamp,
            EnergyConsumption = 150.5m,
            Co2 = 400,
            Pm25 = 25,
            Humidity = 60,
            MotionDetected = true
        };

        // Assert
        reading.Id.Should().Be(id);
        reading.SensorId.Should().Be("sensor-123");
        reading.Type.Should().Be("energy");
        reading.Location.Should().Be("Office A");
        reading.Timestamp.Should().Be(timestamp);
        reading.EnergyConsumption.Should().Be(150.5m);
        reading.Co2.Should().Be(400);
        reading.Pm25.Should().Be(25);
        reading.Humidity.Should().Be(60);
        reading.MotionDetected.Should().BeTrue();
    }

    [Fact]
    public void SensorReading_NullableProperties_ShouldAllowNull()
    {
        // Act
        var reading = new SensorReading
        {
            Id = "1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "Office A",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        reading.EnergyConsumption.Should().BeNull();
        reading.Co2.Should().BeNull();
        reading.Pm25.Should().BeNull();
        reading.Humidity.Should().BeNull();
        reading.MotionDetected.Should().BeNull();
    }

    [Fact]
    public void SensorReading_EnergyType_ShouldHaveEnergyConsumption()
    {
        // Act
        var reading = new SensorReading
        {
            Id = "1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "Office A",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 200.0m
        };

        // Assert
        reading.Type.Should().Be("energy");
        reading.EnergyConsumption.Should().Be(200.0m);
    }

    [Fact]
    public void SensorReading_AirQualityType_ShouldHaveAirQualityData()
    {
        // Act
        var reading = new SensorReading
        {
            Id = "2",
            SensorId = "sensor-2",
            Type = "air_quality",
            Location = "Office B",
            Timestamp = DateTime.UtcNow,
            Co2 = 450,
            Pm25 = 30,
            Humidity = 65
        };

        // Assert
        reading.Type.Should().Be("air_quality");
        reading.Co2.Should().Be(450);
        reading.Pm25.Should().Be(30);
        reading.Humidity.Should().Be(65);
    }

    [Fact]
    public void SensorReading_MotionType_ShouldHaveMotionDetected()
    {
        // Act
        var reading = new SensorReading
        {
            Id = "3",
            SensorId = "sensor-3",
            Type = "motion",
            Location = "Office C",
            Timestamp = DateTime.UtcNow,
            MotionDetected = true
        };

        // Assert
        reading.Type.Should().Be("motion");
        reading.MotionDetected.Should().BeTrue();
    }
}

