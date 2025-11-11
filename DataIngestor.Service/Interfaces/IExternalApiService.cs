using DataIngestor.Service.Models;

namespace DataIngestor.Service.Interfaces;

public interface IExternalApiService
{
    Task<List<SensorReading>> FetchDataAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
}
