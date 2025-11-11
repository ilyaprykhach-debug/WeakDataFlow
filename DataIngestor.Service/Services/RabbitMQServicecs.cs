using DataIngestor.Service.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DataIngestor.Service.Services;

public class RabbitMQService : IQueueService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly string _queueName;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        var queueConfig = configuration.GetSection("Queue");
        _queueName = queueConfig["QueueName"] ?? "sensor-data";

        var factory = new ConnectionFactory
        {
            HostName = queueConfig["Host"] ?? "rabbitmq",
            Port = queueConfig.GetValue<int>("Port", 5672),
            UserName = queueConfig["Username"] ?? "guest",
            Password = queueConfig["Password"] ?? "guest",
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("RabbitMQ service initialized for queue: {QueueName}", _queueName);
    }

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Message published to {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {QueueName}", queueName);
            throw;
        }
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_connection?.IsOpen == true);
    }

    public void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        _channel?.Dispose();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ service disposed");
    }
}
