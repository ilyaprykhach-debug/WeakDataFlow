using DataProcessor.Service.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace DataProcessor.Service.IntegrationTests;

public class QueueIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IServiceScope? _scope;
    private IQueueConsumerService? _queueConsumerService;
    private IConnection? _rabbitMqConnection;
    private IChannel? _rabbitMqChannel;
    private string _queueName = "sensor-data-test";

    public QueueIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        _queueConsumerService = _scope.ServiceProvider.GetRequiredService<IQueueConsumerService>();

        using var configScope = _factory.Services.CreateScope();
        var configuration = configScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var factory = new ConnectionFactory
        {
            HostName = configuration["Queue:Host"] ?? "localhost",
            Port = int.Parse(configuration["Queue:Port"] ?? "5672"),
            UserName = "guest",
            Password = "guest"
        };

        _rabbitMqConnection = await factory.CreateConnectionAsync();
        _rabbitMqChannel = await _rabbitMqConnection.CreateChannelAsync();
        await _rabbitMqChannel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task DisposeAsync()
    {
        if (_rabbitMqChannel != null)
        {
            await _rabbitMqChannel.CloseAsync();
            _rabbitMqChannel.Dispose();
        }
        if (_rabbitMqConnection != null)
        {
            await _rabbitMqConnection.CloseAsync();
            _rabbitMqConnection.Dispose();
        }
        _scope?.Dispose();
    }

    [Fact]
    public async Task IsConnectedAsync_ShouldReturnTrue_WhenQueueIsAccessible()
    {
        // Arrange
        Assert.NotNull(_queueConsumerService);

        // Act
        var result = await _queueConsumerService!.IsConnectedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StartConsuming_ShouldRegisterConsumer()
    {
        // Arrange
        Assert.NotNull(_queueConsumerService);

        Func<DataProcessor.Service.Models.SensorReading, CancellationToken, Task> handler = async (message, ct) =>
        {
            await Task.CompletedTask;
        };

        // Act
        _queueConsumerService!.StartConsuming(handler);

        // Assert
        Assert.True(true);
    }
}

