using DataProcessor.Service.Data;
using DataProcessor.Service.Models;
using DataProcessor.Service.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataProcessor.Service.UnitTests.Services;

public class DatabaseServiceTests : IDisposable
{
    private readonly SensorDataDbContext _context;
    private readonly Mock<ILogger<DatabaseService>> _mockLogger;
    private readonly DatabaseService _service;

    public DatabaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SensorDataDbContext(options);
        _mockLogger = new Mock<ILogger<DatabaseService>>();
        _service = new DatabaseService(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task IsConnectedAsync_ShouldReturnTrue_WhenDatabaseIsAccessible()
    {
        // Act
        var result = await _service.IsConnectedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldSaveNewReadings()
    {
        // Arrange
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
        await _service.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(2);
        savedReadings.Should().Contain(r => r.Id == "reading-1" && r.EnergyConsumption == 100.5m);
        savedReadings.Should().Contain(r => r.Id == "reading-2" && r.Co2 == 400);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldUpdateExistingReadings()
    {
        // Arrange
        var existingReading = new SensorReading
        {
            Id = "reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        _context.SensorReadings.Add(existingReading);
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
        await _service.SaveSensorReadingsBatchAsync(new[] { updatedReading });

        // Assert
        var savedReading = await _context.SensorReadings.FindAsync("reading-1");
        savedReading.Should().NotBeNull();
        savedReading!.EnergyConsumption.Should().Be(200.75m);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleMixedNewAndExistingReadings()
    {
        // Arrange
        var existingReading = new SensorReading
        {
            Id = "reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        _context.SensorReadings.Add(existingReading);
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
        await _service.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(2);
        savedReadings.Should().Contain(r => r.Id == "reading-1" && r.EnergyConsumption == 150.5m);
        savedReadings.Should().Contain(r => r.Id == "reading-2" && r.Co2 == 400);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleEmptyBatch()
    {
        // Arrange
        var readings = new List<SensorReading>();

        // Act
        await _service.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleLargeBatch()
    {
        // Arrange
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
        await _service.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(100);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
    {
        // Arrange
        var context = new SensorDataDbContext(new DbContextOptionsBuilder<SensorDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);

        await context.DisposeAsync();

        var service = new DatabaseService(context, _mockLogger.Object);
        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = "reading-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => service.SaveSensorReadingsBatchAsync(readings));
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldHandleAllSensorTypes()
    {
        // Arrange
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
        await _service.SaveSensorReadingsBatchAsync(readings);

        // Assert
        var savedReadings = await _context.SensorReadings.ToListAsync();
        savedReadings.Should().HaveCount(3);
        savedReadings.Should().Contain(r => r.Type == "energy" && r.EnergyConsumption == 100.5m);
        savedReadings.Should().Contain(r => r.Type == "air_quality" && r.Co2 == 400);
        savedReadings.Should().Contain(r => r.Type == "motion" && r.MotionDetected == true);
    }

    [Fact]
    public async Task SaveSensorReadingsBatchAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = "reading-1",
                SensorId = "sensor-1",
                Type = "energy",
                Location = "location-1",
                Timestamp = DateTime.UtcNow
            }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.SaveSensorReadingsBatchAsync(readings, cts.Token));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

