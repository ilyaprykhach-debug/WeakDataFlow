using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DataProcessor.Service.Data;
using Testcontainers.RabbitMq;

namespace DataProcessor.Service.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static int _instanceCount = 0;
    private readonly string _uniqueDbName;
    private static RabbitMqContainer? _sharedRabbitMqContainer;
    private static readonly SemaphoreSlim _containerLock = new(1, 1);
    private static readonly Lazy<Task<RabbitMqContainer>> _containerInitializer = new(async () =>
    {
        await _containerLock.WaitAsync();
        try
        {
            if (_sharedRabbitMqContainer == null)
            {
                _sharedRabbitMqContainer = new RabbitMqBuilder()
                    .WithImage("rabbitmq:3-management")
                    .WithPortBinding(5672, true)
                    .WithUsername("guest")
                    .WithPassword("guest")
                    .Build();
                await _sharedRabbitMqContainer.StartAsync();
            }
            return _sharedRabbitMqContainer;
        }
        finally
        {
            _containerLock.Release();
        }
    });

    public CustomWebApplicationFactory()
    {
        _instanceCount++;
        _uniqueDbName = $"TestDb_{_instanceCount}_{Guid.NewGuid()}";
    }

    public async Task InitializeAsync()
    {
        await _containerInitializer.Value;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var container = Task.Run(async () => await _containerInitializer.Value).GetAwaiter().GetResult();
            var rabbitMqPort = container.GetMappedPublicPort(5672);
            
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Queue:Host", "localhost" },
                { "Queue:Port", rabbitMqPort.ToString() },
                { "Queue:QueueName", "sensor-data-test" },
                { "Queue:Username", "guest" },
                { "Queue:Password", "guest" },
                { "Database:Host", "localhost" },
                { "Database:Port", "5432" },
                { "Database:Database", "testdb" },
                { "Database:Username", "testuser" },
                { "Database:Password", "testpass" },
                { "DataProcessing:BatchSize", "5" },
                { "DataProcessing:ProcessingIntervalSeconds", "30" },
                { "DataProcessing:InitialDelaySeconds", "2" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SensorDataDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<SensorDataDbContext>(options =>
            {
                options.UseInMemoryDatabase(_uniqueDbName);
            });
        });
    }

    public int RabbitMqPort
    {
        get
        {
            var container = Task.Run(async () => await _containerInitializer.Value).GetAwaiter().GetResult();
            return container.GetMappedPublicPort(5672);
        }
    }
}


