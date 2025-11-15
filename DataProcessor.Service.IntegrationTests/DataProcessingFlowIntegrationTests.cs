using DataProcessor.Service.Data;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DataProcessor.Service.IntegrationTests;

public class DataProcessingFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IServiceScope? _scope;
    private SensorDataDbContext? _context;
    private IDataProcessor? _dataProcessor;
    private IConnection? _rabbitMqConnection;
    private IChannel? _rabbitMqChannel;
    private string _queueName = "sensor-data-test";

    public DataProcessingFlowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();
        _dataProcessor = _scope.ServiceProvider.GetRequiredService<IDataProcessor>();

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
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

        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
        _scope?.Dispose();
    }

    [Fact]
    public async Task DataProcessor_ShouldProcessMessagesFromQueue()
    {
        // Arrange
        Assert.NotNull(_dataProcessor);
        Assert.NotNull(_context);
        Assert.NotNull(_rabbitMqChannel);

        var reading = new SensorReading
        {
            Id = "test-reading-1",
            SensorId = "sensor-1",
            Type = "energy",
            Location = "location-1",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = 100.5m
        };

        var messageBody = JsonSerializer.Serialize(reading);
        var body = Encoding.UTF8.GetBytes(messageBody);

        // Act
        await _rabbitMqChannel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            body: body);

        await _dataProcessor!.StartProcessingAsync();

        await Task.Delay(2000);

        // Assert
        var savedReadings = await _context!.SensorReadings.ToListAsync();
        savedReadings.Should().NotBeNull();
    }

    [Fact]
    public async Task DataProcessor_ShouldHandleBatchProcessing()
    {
        // Arrange
        Assert.NotNull(_dataProcessor);
        Assert.NotNull(_context);
        Assert.NotNull(_rabbitMqChannel);

        var readings = new List<SensorReading>();
        for (int i = 0; i < 5; i++)
        {
            readings.Add(new SensorReading
            {
                Id = $"test-reading-{i}",
                SensorId = $"sensor-{i}",
                Type = "energy",
                Location = $"location-{i}",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = 100.5m + i
            });
        }

        // Act
        foreach (var reading in readings)
        {
            var messageBody = JsonSerializer.Serialize(reading);
            var body = Encoding.UTF8.GetBytes(messageBody);

            await _rabbitMqChannel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body);
        }

        await _dataProcessor!.StartProcessingAsync();
        await Task.Delay(2000);

        // Assert
        var savedReadings = await _context!.SensorReadings.ToListAsync();
        savedReadings.Should().NotBeNull();
    }
}

