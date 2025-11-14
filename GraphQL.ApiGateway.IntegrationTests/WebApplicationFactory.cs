using GraphQL.ApiGateway.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.ApiGateway.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static int _instanceCount = 0;
    private readonly string _uniqueDbName;

    public CustomWebApplicationFactory()
    {
        _instanceCount++;
        _uniqueDbName = $"TestDb_{_instanceCount}_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:Host", "localhost" },
                { "Database:Port", "5432" },
                { "Database:Database", "testdb" },
                { "Database:Username", "testuser" },
                { "Database:Password", "testpass" }
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

            // Use unique database name for each factory instance to ensure test isolation
            services.AddDbContext<SensorDataDbContext>(options =>
            {
                options.UseInMemoryDatabase(_uniqueDbName);
            });
        });
    }
}

