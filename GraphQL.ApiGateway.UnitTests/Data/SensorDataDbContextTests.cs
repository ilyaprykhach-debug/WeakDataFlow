using FluentAssertions;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.ApiGateway.UnitTests.Data;

public class SensorDataDbContextTests
{
    [Fact]
    public void SensorDataDbContext_ShouldHaveSensorReadingsDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new SensorDataDbContext(options);

        // Assert
        context.SensorReadings.Should().NotBeNull();
    }

    [Fact]
    public void SensorDataDbContext_EntityConfiguration_ShouldMapToCorrectTable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new SensorDataDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(SensorReading));
        var tableName = entityType?.GetTableName();

        // Assert
        tableName.Should().Be("sensor_readings");
    }

    [Fact]
    public void SensorDataDbContext_EntityConfiguration_ShouldHaveCorrectPrimaryKey()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new SensorDataDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(SensorReading));
        var primaryKey = entityType?.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void SensorDataDbContext_EntityConfiguration_ShouldHaveIndexes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new SensorDataDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(SensorReading));
        var indexes = entityType?.GetIndexes();

        // Assert
        indexes.Should().NotBeNull();
        indexes!.Should().HaveCount(3);
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SensorId"));
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Timestamp"));
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Type"));
    }

    [Fact]
    public void SensorDataDbContext_CanSaveAndRetrieveSensorReading()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var reading = new SensorReading
        {
            Id = "test-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "Office A",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        // Act
        using (var context = new SensorDataDbContext(options))
        {
            context.SensorReadings.Add(reading);
            context.SaveChanges();
        }

        using (var context = new SensorDataDbContext(options))
        {
            var retrieved = context.SensorReadings.Find("test-1");

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.Id.Should().Be("test-1");
            retrieved.SensorId.Should().Be("sensor-1");
            retrieved.Type.Should().Be("energy");
            retrieved.Location.Should().Be("Office A");
            retrieved.EnergyConsumption.Should().Be(100.5m);
        }
    }

    [Fact]
    public void SensorDataDbContext_PropertyMappings_ShouldBeCorrect()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new SensorDataDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(SensorReading));

        // Act & Assert
        entityType?.FindProperty("Id")?.GetColumnName().Should().Be("id");
        entityType?.FindProperty("SensorId")?.GetColumnName().Should().Be("sensor_id");
        entityType?.FindProperty("Type")?.GetColumnName().Should().Be("type");
        entityType?.FindProperty("Location")?.GetColumnName().Should().Be("location");
        entityType?.FindProperty("Timestamp")?.GetColumnName().Should().Be("timestamp");
        entityType?.FindProperty("EnergyConsumption")?.GetColumnName().Should().Be("energy_consumption");
        entityType?.FindProperty("Co2")?.GetColumnName().Should().Be("co2");
        entityType?.FindProperty("Pm25")?.GetColumnName().Should().Be("pm25");
        entityType?.FindProperty("Humidity")?.GetColumnName().Should().Be("humidity");
        entityType?.FindProperty("MotionDetected")?.GetColumnName().Should().Be("motion_detected");
    }
}

