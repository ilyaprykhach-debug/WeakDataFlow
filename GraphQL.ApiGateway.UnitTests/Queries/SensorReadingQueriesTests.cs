using FluentAssertions;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.GraphQL.Inputs;
using GraphQL.ApiGateway.GraphQL.Queries;
using GraphQL.ApiGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.ApiGateway.UnitTests.Queries;

public class SensorReadingQueriesTests
{
    private SensorDataDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new SensorDataDbContext(options);
    }

    private void SeedData(SensorDataDbContext context)
    {
        context.SensorReadings.AddRange(new List<SensorReading>
        {
            new SensorReading
            {
                Id = "1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "Office A",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                EnergyConsumption = 100.5m,
                Co2 = null,
                Pm25 = null,
                Humidity = null,
                MotionDetected = null
            },
            new SensorReading
            {
                Id = "2",
                SensorId = "sensor-2",
                Type = "air_quality",
                Location = "Office B",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                EnergyConsumption = null,
                Co2 = 400,
                Pm25 = 25,
                Humidity = 60,
                MotionDetected = null
            },
            new SensorReading
            {
                Id = "3",
                SensorId = "sensor-3",
                Type = "motion",
                Location = "Office A",
                Timestamp = DateTime.UtcNow.AddHours(-3),
                EnergyConsumption = null,
                Co2 = null,
                Pm25 = null,
                Humidity = null,
                MotionDetected = true
            }
        });
        context.SaveChanges();
    }

    [Fact]
    public void GetSensorReadings_ShouldReturnQueryable()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = queries.GetSensorReadings(context);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IQueryable<SensorReading>>();
        result.Count().Should().Be(3);
    }

    [Fact]
    public void GetSensorReadingsWithPagination_ShouldApplyPagination()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();
        var pagination = new PaginationInputData { Skip = 1, Take = 2 };

        // Act
        var result = queries.GetSensorReadingsWithPagination(context, pagination);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
    }

    [Fact]
    public void GetSensorReadingsWithPagination_WithoutPagination_ShouldReturnAll()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = queries.GetSensorReadingsWithPagination(context, null);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(3);
    }

    [Fact]
    public async Task GetSensorReadingById_ShouldReturnCorrectReading()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetSensorReadingById(context, "1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
        result.SensorId.Should().Be("sensor-1");
    }

    [Fact]
    public async Task GetSensorReadingById_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetSensorReadingById(context, "999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSensorReadingsByLocation_ShouldFilterByLocation()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = queries.GetSensorReadingsByLocation(context, "Office A");

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result.All(r => r.Location == "Office A").Should().BeTrue();
    }

    [Fact]
    public void GetSensorReadingsByType_ShouldFilterByType()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = queries.GetSensorReadingsByType(context, "energy");

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        result.First().Type.Should().Be("energy");
    }

    [Fact]
    public void GetSensorReadingsByTimeRange_ShouldFilterByTimeRange()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;

        // Act
        var result = queries.GetSensorReadingsByTimeRange(context, startTime, endTime);

        // Assert
        result.Should().NotBeNull();
        result.All(r => r.Timestamp >= startTime && r.Timestamp <= endTime).Should().BeTrue();
    }
}

