namespace DataProcessor.Service.Interfaces;

public interface IDataProcessor
{
    Task StartProcessingAsync(CancellationToken cancellationToken = default);
}


