using DataProcessor.Service.Configuration;
using DataProcessor.Service.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DataProcessor.Service.Services;

public class RabbitMQConsumerService : IQueueConsumerService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly string _queueName;

    public RabbitMQConsumerService(
        IOptions<QueueConfig> queueConfig,
        ILogger<RabbitMQConsumerService> logger)
    {
        _logger = logger;
        var config = queueConfig.Value;
        _queueName = config.QueueName;

        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null).Wait();

        _logger.LogInformation("RabbitMQ Consumer service initialized for queue: {QueueName}", _queueName);
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_connection?.IsOpen == true);
    }

    public void StartConsuming<T>(
        Func<T, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default) where T : class
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var deserialized = JsonSerializer.Deserialize<T>(message);

                if (deserialized != null)
                {
                    await messageHandler(deserialized, cancellationToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger.LogDebug("Message processed and acknowledged");
                }
                else
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    _logger.LogWarning("Failed to deserialize message, message rejected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming messages from {QueueName}", _queueName);
    }

    public void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connections");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ Consumer service disposed");
        }
    }
}

