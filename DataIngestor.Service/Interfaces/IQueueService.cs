namespace DataIngestor.Service.Interfaces;

public interface IQueueService
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync();
}
