namespace DataProcessor.Service.Interfaces;

public interface IQueueConsumerService
{
    Task<bool> IsConnectedAsync();
    void StartConsuming<T>(Func<T, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default) where T : class;
}


