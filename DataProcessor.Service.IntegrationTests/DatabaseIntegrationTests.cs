using DataProcessor.Service.Data;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataProcessor.Service.IntegrationTests;

public class DatabaseIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IServiceScope? _scope;
    private SensorDataDbContext? _context;
    private IDatabaseService? _databaseService;

    public DatabaseIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();
        _databaseService = _scope.ServiceProvider.GetRequiredService<IDatabaseService>();

        await _context.Database.EnsureCreatedAsync();
        
        _context.SensorReadings.RemoveRange(_context.SensorReadings);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
        _scope?.Dispose();
    }

    [Fact]
    public async Task IsConnectedAsync_ShouldReturnTrue_WhenDatabaseIsAccessible()
    {
        // Arrange
        Assert.NotNull(_databaseService);

        // Act
        var result = await _databaseService!.IsConnectedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldSaveReadingsToDatabase()
    {
        // Arrange
        Assert.NotNull(_databaseService);
        Assert.NotNull(_context);

        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = "reading-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100.5m
            },
            new SensorReading
            {
                Id = "reading-2",
                SensorId = "sensor-2",
                Type = "air_quality",
                Location = "location-2",
                Timestamp = DateTime.UtcNow,
                Co2 = 400,
                Pm25 = 20,
                Humidity = 50
            }
        };

        // Act
        await _databaseService!.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context!.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(2);
        savedReadings.Should().Contain(r => r.Id == "reading-1" && r.EnergyConsumption == 100.5m);
        savedReadings.Should().Contain(r => r.Id == "reading-2" && r.Co2 == 400);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldUpdateExistingReadings()
    {
        // Arrange
        Assert.NotNull(_databaseService);
        Assert.NotNull(_context);

        var existingReading = new SensorReading
        {
            Id = "reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        _context!.SensorReadings.Add(existingReading);
        await _context.SaveChangesAsync();

        var updatedReading = new SensorReading
        {
            Id = "reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 200.75m
        };

        // Act
        await _databaseService!.SaveSensorReadingsBatchAsync(new[] { updatedReading });

        // Assert
        var savedReading = await _context.SensorReadings.FindAsync("reading-1");
        savedReading.Should().NotBeNull();
        savedReading!.EnergyConsumption.Should().Be(200.75m);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleLargeBatch()
    {
        // Arrange
        Assert.NotNull(_databaseService);
        Assert.NotNull(_context);

        var readings = new List<SensorReading>();
        for (int i = 0; i < 100; i++)
        {
            readings.Add(new SensorReading
            {
                Id = $"reading-{i}",
                SensorId = $"sensor-{i}",
                Type = "energy",
                Location = $"location-{i}",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100.5m + i
            });
        }

        // Act
        await _databaseService!.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context!.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(100);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleMixedNewAndExistingReadings()
    {
        // Arrange
        Assert.NotNull(_databaseService);
        Assert.NotNull(_context);

        var existingReading = new SensorReading
        {
            Id = "reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        _context!.SensorReadings.Add(existingReading);
        await _context.SaveChangesAsync();

        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = "reading-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 150.5m
            },
            new SensorReading
            {
                Id = "reading-2",
                SensorId = "sensor-2",
                Type = "air_quality",
                Location = "location-2",
                Timestamp = DateTime.UtcNow,
                Co2 = 400
            }
        };

        // Act
        await _databaseService!.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(2);
        savedReadings.Should().Contain(r => r.Id == "reading-1" && r.EnergyConsumption == 150.5m);
        savedReadings.Should().Contain(r => r.Id == "reading-2" && r.Co2 == 400);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleAllSensorTypes()
    {
        // Arrange
        Assert.NotNull(_databaseService);
        Assert.NotNull(_context);

        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = "energy-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100.5m
            },
            new SensorReading
            {
                Id = "air-1",
                SensorId = "sensor-2",
                Type = "air_quality",
                Location = "location-2",
                Timestamp = DateTime.UtcNow,
                Co2 = 400,
                Pm25 = 20,
                Humidity = 50
            },
            new SensorReading
            {
                Id = "motion-1",
                SensorId = "sensor-3",
                Type = "motion",
                Location = "location-3",
                Timestamp = DateTime.UtcNow,
                MotionDetected = true
            }
        };

        // Act
        await _databaseService!.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context!.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(3);
        savedReadings.Should().Contain(r => r.Type == "energy" && r.EnergyConsumption == 100.5m);
        savedReadings.Should().Contain(r => r.Type == "air_quality" && r.Co2 == 400);
        savedReadings.Should().Contain(r => r.Type == "motion" && r.MotionDetected == true);
    }
}

