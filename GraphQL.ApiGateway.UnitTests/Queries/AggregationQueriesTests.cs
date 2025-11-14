using FluentAssertions;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.GraphQL.Queries;
using GraphQL.ApiGateway.GraphQL.Types;
using GraphQL.ApiGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.ApiGateway.UnitTests.Queries;

public class AggregationQueriesTests
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
                Type = "energy",
                Location = "Office A",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                EnergyConsumption = 200.0m,
                Co2 = null,
                Pm25 = null,
                Humidity = null,
                MotionDetected = null
            },
            new SensorReading
            {
                Id = "3",
                SensorId = "sensor-3",
                Type = "air_quality",
                Location = "Office B",
                Timestamp = DateTime.UtcNow.AddHours(-3),
                EnergyConsumption = null,
                Co2 = 400,
                Pm25 = 25,
                Humidity = 60,
                MotionDetected = null
            },
            new SensorReading
            {
                Id = "4",
                SensorId = "sensor-4",
                Type = "air_quality",
                Location = "Office B",
                Timestamp = DateTime.UtcNow.AddHours(-4),
                EnergyConsumption = null,
                Co2 = 500,
                Pm25 = 30,
                Humidity = 65,
                MotionDetected = null
            }
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAggregationsByLocation_ShouldGroupByLocation()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByLocation(context);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.GroupBy == "Office A");
        result.Should().Contain(r => r.GroupBy == "Office B");
    }

    [Fact]
    public async Task GetAggregationsByLocation_WithTimeFilter_ShouldFilterByTime()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await queries.GetAggregationsByLocation(context, startTime, endTime);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Only Office A has readings in this time range
        result.First().GroupBy.Should().Be("Office A");
    }

    [Fact]
    public async Task GetAggregationsByType_ShouldGroupByType()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByType(context);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.GroupBy == "energy");
        result.Should().Contain(r => r.GroupBy == "air_quality");
    }

    [Fact]
    public async Task GetAggregationsByType_ShouldCalculateAverages()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByType(context);

        // Assert
        result.Should().NotBeNull();
        var energyResult = result.First(r => r.GroupBy == "energy");
        energyResult.AverageEnergyConsumption.Should().Be(150.25m); // (100.5 + 200.0) / 2
        energyResult.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetAggregationsByTimePeriod_WithHour_ShouldGroupByHour()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByTimePeriod(context, "hour");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAggregationsByTimePeriod_WithDay_ShouldGroupByDay()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByTimePeriod(context, "day");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAggregationsByTimePeriod_WithWeek_ShouldGroupByWeek()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByTimePeriod(context, "week");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAggregationsByTimePeriod_WithMonth_ShouldGroupByMonth()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act
        var result = await queries.GetAggregationsByTimePeriod(context, "month");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAggregationsByTimePeriod_WithInvalidPeriod_ShouldThrowException()
    {
        // Arrange
        using var context = CreateContext();
        SeedData(context);
        var queries = new SensorReadingQueries();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            queries.GetAggregationsByTimePeriod(context, "invalid"));
    }
}

