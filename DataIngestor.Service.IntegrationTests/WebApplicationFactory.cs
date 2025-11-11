using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DataIngestor.Service.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ExternalApi:BaseUrl", "http://localhost:8080" },
                { "ExternalApi:TimeoutSeconds", "30" },
                { "ExternalApi:RetryCount", "3" },
                { "ExternalApi:Headers:XApiKey", "test-api-key" },
                { "Queue:Host", "localhost" },
                { "Queue:Port", "5672" },
                { "Queue:QueueName", "sensor-data-test" },
                { "Queue:Username", "guest" },
                { "Queue:Password", "guest" },
                { "DataIngestion:IntervalSeconds", "60" },
                { "DataIngestion:InitialDelaySeconds", "0" }
            });
        });
    }
}

