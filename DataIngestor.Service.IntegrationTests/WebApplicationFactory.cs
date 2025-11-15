using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.RabbitMq;

namespace DataIngestor.Service.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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

    public async Task InitializeAsync()
    {
        // Ensure RabbitMQ container is started before configuring the application
        await _containerInitializer.Value;
    }

    public new async Task DisposeAsync()
    {
        // Don't dispose the container here - it's shared across all test instances
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Ensure container is initialized before getting the port
            // Use Task.Run to avoid potential deadlocks in synchronous context
            var container = Task.Run(async () => await _containerInitializer.Value).GetAwaiter().GetResult();
            var rabbitMqPort = container.GetMappedPublicPort(5672);
            
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ExternalApi:Connection:BaseUrl", "http://localhost:8080" },
                { "ExternalApi:Connection:TimeoutSeconds", "30" },
                { "ExternalApi:Retry:RetryCount", "3" },
                { "ExternalApi:Headers:XApiKey", "test-api-key" },
                { "Queue:Host", "localhost" },
                { "Queue:Port", rabbitMqPort.ToString() },
                { "Queue:QueueName", "sensor-data-test" },
                { "Queue:Username", "guest" },
                { "Queue:Password", "guest" },
                { "DataIngestion:IntervalSeconds", "60" },
                { "DataIngestion:InitialDelaySeconds", "0" }
            });
        });
    }

    public int RabbitMqPort
    {
        get
        {
            // Use Task.Run to avoid potential deadlocks in synchronous context
            var container = Task.Run(async () => await _containerInitializer.Value).GetAwaiter().GetResult();
            return container.GetMappedPublicPort(5672);
        }
    }
}

