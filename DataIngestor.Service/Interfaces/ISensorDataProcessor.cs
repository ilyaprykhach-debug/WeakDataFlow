namespace DataIngestor.Service.Interfaces;

public interface ISensorDataProcessor
{
    Task ProcessDataAsync(CancellationToken cancellationToken = default);
}
