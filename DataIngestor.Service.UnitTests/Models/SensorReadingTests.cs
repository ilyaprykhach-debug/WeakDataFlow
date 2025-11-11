using DataIngestor.Service.Models;
using FluentAssertions;
using System.Text.Json;

namespace DataIngestor.Service.UnitTests.Models;

public class SensorReadingTests
{
    [Fact]
    public void FromWeakApiResponse_ShouldCreateEnergyReading_WhenTypeIsEnergy()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { energy = 123.45m });
        var weakResponse = new WeakApiResponse
        {
            Type = "energy",
            Name = "Test Energy Meter",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("energy");
        result.Location.Should().Be("Test Energy Meter");
        result.EnergyConsumption.Should().Be(123.45m);
        result.SensorId.Should().Be("energy_Test_Energy_Meter");
        result.NumericValue.Should().Be(123.45m);
    }

    [Fact]
    public void FromWeakApiResponse_ShouldCreateAirQualityReading_WhenTypeIsAirQuality()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { co2 = 400, pm25 = 20, humidity = 50 });
        var weakResponse = new WeakApiResponse
        {
            Type = "air_quality",
            Name = "Air Quality Sensor",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("air_quality");
        result.Location.Should().Be("Air Quality Sensor");
        result.Co2.Should().Be(400);
        result.Pm25.Should().Be(20);
        result.Humidity.Should().Be(50);
        result.SensorId.Should().Be("air_quality_Air_Quality_Sensor");
        result.NumericValue.Should().Be(400);
    }

    [Fact]
    public void FromWeakApiResponse_ShouldCreateMotionReading_WhenTypeIsMotion()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { motionDetected = true });
        var weakResponse = new WeakApiResponse
        {
            Type = "motion",
            Name = "Motion Sensor",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("motion");
        result.Location.Should().Be("Motion Sensor");
        result.MotionDetected.Should().BeTrue();
        result.SensorId.Should().Be("motion_Motion_Sensor");
        result.NumericValue.Should().BeNull();
    }

    [Fact]
    public void FromWeakApiResponse_ShouldHandleMissingEnergyValue()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { });
        var weakResponse = new WeakApiResponse
        {
            Type = "energy",
            Name = "Test Meter",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.EnergyConsumption.Should().BeNull();
    }

    [Fact]
    public void FromWeakApiResponse_ShouldHandlePartialAirQualityData()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { co2 = 400 });
        var weakResponse = new WeakApiResponse
        {
            Type = "air_quality",
            Name = "Air Quality Sensor",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Co2.Should().Be(400);
        result.Pm25.Should().BeNull();
        result.Humidity.Should().BeNull();
        result.NumericValue.Should().Be(400);
    }

    [Fact]
    public void FromWeakApiResponse_ShouldHandleAirQualityWithOnlyPm25()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { pm25 = 25 });
        var weakResponse = new WeakApiResponse
        {
            Type = "air_quality",
            Name = "Air Quality Sensor",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Co2.Should().BeNull();
        result.Pm25.Should().Be(25);
        result.Humidity.Should().BeNull();
        result.NumericValue.Should().Be(25);
    }

    [Fact]
    public void FromWeakApiResponse_ShouldHandleAirQualityWithOnlyHumidity()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { humidity = 60 });
        var weakResponse = new WeakApiResponse
        {
            Type = "air_quality",
            Name = "Air Quality Sensor",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.Should().NotBeNull();
        result.Co2.Should().BeNull();
        result.Pm25.Should().BeNull();
        result.Humidity.Should().Be(60);
        result.NumericValue.Should().Be(60);
    }

    [Fact]
    public void NumericValue_ShouldReturnEnergyConsumption_ForEnergyType()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "energy",
            EnergyConsumption = 123.45m
        };

        // Act & Assert
        reading.NumericValue.Should().Be(123.45m);
    }

    [Fact]
    public void NumericValue_ShouldReturnCo2_ForAirQualityType()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "air_quality",
            Co2 = 400,
            Pm25 = 20,
            Humidity = 50
        };

        // Act & Assert
        reading.NumericValue.Should().Be(400);
    }

    [Fact]
    public void NumericValue_ShouldReturnPm25_WhenCo2IsNull()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "air_quality",
            Co2 = null,
            Pm25 = 20,
            Humidity = 50
        };

        // Act & Assert
        reading.NumericValue.Should().Be(20);
    }

    [Fact]
    public void NumericValue_ShouldReturnHumidity_WhenCo2AndPm25AreNull()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "air_quality",
            Co2 = null,
            Pm25 = null,
            Humidity = 50
        };

        // Act & Assert
        reading.NumericValue.Should().Be(50);
    }

    [Fact]
    public void NumericValue_ShouldReturnNull_ForMotionType()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "motion",
            MotionDetected = true
        };

        // Act & Assert
        reading.NumericValue.Should().BeNull();
    }

    [Fact]
    public void NumericValue_ShouldReturnNull_ForUnknownType()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "unknown"
        };

        // Act & Assert
        reading.NumericValue.Should().BeNull();
    }

    [Fact]
    public void SensorId_ShouldReplaceSpacesWithUnderscores()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { energy = 100m });
        var weakResponse = new WeakApiResponse
        {
            Type = "energy",
            Name = "Test Meter With Spaces",
            Payload = payload
        };

        // Act
        var result = SensorReading.FromWeakApiResponse(weakResponse);

        // Assert
        result.SensorId.Should().Be("energy_Test_Meter_With_Spaces");
    }
}

