using DataProcessor.Service.Data;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Service.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SensorDataDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(
        SensorDataDbContext context,
        ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogInformation("Start SaveSensorReadingsBatchAsync");

            var readingsList = readings.ToList();
            var ids = readingsList.Select(r => r.Id).ToList();

            _logger.LogInformation("Reading list created");

            var existingReadings = await _context.SensorReadings
                .Where(r => ids.Contains(r.Id))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("existingReadings created");

            var existingIds = existingReadings.Select(r => r.Id).ToHashSet();

            _logger.LogInformation("existingIds created");


            foreach (var reading in readingsList)
            {
                _logger.LogInformation("Start foreach");

                if (existingIds.Contains(reading.Id))
                {
                    _logger.LogInformation("Ids contained");

                    var existing = existingReadings.First(r => r.Id == reading.Id);
                    _context.Entry(existing).CurrentValues.SetValues(reading);
                    _logger.LogInformation("Set values");

                }
                else
                {
                    _logger.LogInformation("Adding to db");

                    await _context.SensorReadings.AddAsync(reading, cancellationToken);

                    _logger.LogInformation("Data added to db");

                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Batch of {Count} sensor readings saved", readingsList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save batch of sensor readings");
            throw;
        }
    }
}


