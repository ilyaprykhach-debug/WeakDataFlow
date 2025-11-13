using DataProcessor.Service.Models;

namespace DataProcessor.Service.Interfaces;

public interface IDatabaseService
{
    Task<bool> IsConnectedAsync();
    Task SaveSensorReadingsBatchAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default);
}


