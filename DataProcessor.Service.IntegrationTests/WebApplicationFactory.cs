using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DataProcessor.Service.Data;

namespace DataProcessor.Service.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Queue:Host", "localhost" },
                { "Queue:Port", "5672" },
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
                options.UseInMemoryDatabase("TestDb");
            });
        });
    }
}


