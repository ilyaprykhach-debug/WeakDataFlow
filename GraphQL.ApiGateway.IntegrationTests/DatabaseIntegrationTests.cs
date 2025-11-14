using FluentAssertions;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.ApiGateway.IntegrationTests;

public class DatabaseIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Database_CanConnect_ShouldReturnTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_CanSaveAndRetrieveSensorReading()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();

        var reading = new SensorReading
        {
            Id = "integration-test-1",
            SensorId = "sensor-integration-1",
            Type = "energy",
            Location = "Test Office",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 150.75m
        };

        // Act
        context.SensorReadings.Add(reading);
        await context.SaveChangesAsync();

        var retrieved = await context.SensorReadings.FindAsync("integration-test-1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be("integration-test-1");
        retrieved.SensorId.Should().Be("sensor-integration-1");
        retrieved.Type.Should().Be("energy");
        retrieved.Location.Should().Be("Test Office");
        retrieved.EnergyConsumption.Should().Be(150.75m);
    }

    [Fact]
    public async Task Database_CanQueryWithFilters()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();

        var testIds = new[] { "filter-test-1", "filter-test-2" };
        
        context.SensorReadings.AddRange(new List<SensorReading>
        {
            new SensorReading
            {
                Id = "filter-test-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "Office A",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100m
            },
            new SensorReading
            {
                Id = "filter-test-2",
                SensorId = "sensor-2",
                Type = "air_quality",
                Location = "Office B",
                Timestamp = DateTime.UtcNow,
                Co2 = 400
            }
        });
        await context.SaveChangesAsync();

        // Act
        var energyReadings = await context.SensorReadings
            .Where(r => testIds.Contains(r.Id) && r.Type == "energy")
            .ToListAsync();

        var officeAReadings = await context.SensorReadings
            .Where(r => testIds.Contains(r.Id) && r.Location == "Office A")
            .ToListAsync();

        // Assert
        energyReadings.Should().HaveCount(1);
        energyReadings.First().Type.Should().Be("energy");
        officeAReadings.Should().HaveCount(1);
        officeAReadings.First().Location.Should().Be("Office A");
    }

    [Fact]
    public async Task Database_CanPerformAggregations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();

        var testIds = new[] { "agg-test-1", "agg-test-2" };
        
        context.SensorReadings.AddRange(new List<SensorReading>
        {
            new SensorReading
            {
                Id = "agg-test-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "Office A",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100m
            },
            new SensorReading
            {
                Id = "agg-test-2",
                SensorId = "sensor-2",
                Type = "energy",
                Location = "Office A",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 200m
            }
        });
        await context.SaveChangesAsync();

        // Act
        var average = await context.SensorReadings
            .Where(r => testIds.Contains(r.Id))
            .AverageAsync(r => r.EnergyConsumption);

        var count = await context.SensorReadings
            .Where(r => testIds.Contains(r.Id))
            .CountAsync();

        var sum = await context.SensorReadings
            .Where(r => testIds.Contains(r.Id))
            .SumAsync(r => r.EnergyConsumption);

        // Assert
        average.Should().Be(150m);
        count.Should().Be(2);
        sum.Should().Be(300m);
    }
}

