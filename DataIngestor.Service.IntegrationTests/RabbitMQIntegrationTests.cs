using DataIngestor.Service.Models;
using FluentAssertions;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Testcontainers.RabbitMq;

namespace DataIngestor.Service.IntegrationTests;

public class RabbitMQIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer;
    private IConnection? _connection;
    private IChannel? _channel;
    private string _queueName = "sensor-data-test";

    public RabbitMQIntegrationTests()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await _rabbitMqContainer.DisposeAsync();
    }

    [Fact]
    public async Task PublishMessage_ShouldBeConsumable()
    {
        // Arrange
        var reading = new SensorReading
        {
            Type = "energy",
            Location = "Test Location",
            EnergyConsumption = 123.45m
        };

        var json = JsonSerializer.Serialize(reading);
        var body = Encoding.UTF8.GetBytes(json);

        // Act
        await _channel!.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);

        // Assert
        var result = await _channel.BasicGetAsync(_queueName, autoAck: true);
        result.Should().NotBeNull();
        result!.Body.ToArray().Should().NotBeEmpty();

        var consumedJson = Encoding.UTF8.GetString(result.Body.ToArray());
        var consumedReading = JsonSerializer.Deserialize<SensorReading>(consumedJson);
        consumedReading.Should().NotBeNull();
        consumedReading!.Type.Should().Be("energy");
        consumedReading.EnergyConsumption.Should().Be(123.45m);
    }
}

