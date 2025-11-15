using DataProcessor.Service.Data;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Service.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SensorDataDbContext _context;
    private readonly ILogger<DatabaseService> _logger;
    private readonly INotificationClient? _notificationClient;

    public DatabaseService(
        SensorDataDbContext context,
        ILogger<DatabaseService> logger,
        INotificationClient? notificationClient = null)
    {
        _context = context;
        _logger = logger;
        _notificationClient = notificationClient;
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check database connection");
            return false;
        }
    }

    public async Task SaveSensorReadingsBatchAsync(
        IEnumerable<SensorReading> readings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var readingsList = readings.ToList();
            var ids = readingsList.Select(r => r.Id).ToList();

            var existingReadings = await _context.SensorReadings
                .Where(r => ids.Contains(r.Id))
                .ToListAsync(cancellationToken);

            var existingIds = existingReadings.Select(r => r.Id).ToHashSet();

            foreach (var reading in readingsList)
            {
                if (existingIds.Contains(reading.Id))
                {
                    var existing = existingReadings.First(r => r.Id == reading.Id);
                    _context.Entry(existing).CurrentValues.SetValues(reading);
                }
                else
                {
                    await _context.SensorReadings.AddAsync(reading, cancellationToken);
                    _logger.LogInformation("Data added to db");

                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (_notificationClient != null)
            {
                _ = _notificationClient.NotifyDataSavedToDatabaseAsync(
                    readingsList.Select(r => new
                    {
                        r.Id,
                        r.SensorId,
                        r.Type,
                        r.Location,
                        r.Timestamp,
                        r.EnergyConsumption,
                        r.Co2,
                        r.Pm25,
                        r.Humidity,
                        r.MotionDetected
                    }),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save batch of sensor readings");
            throw;
        }
    }
}


