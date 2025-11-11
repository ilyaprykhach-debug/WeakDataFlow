using DataIngestor.Service.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Net.Mime;
using System.Text.Json;

// Check if we're in testing environment
var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

try
{
    // Only configure Serilog if not in testing environment
    // WebApplicationFactory will handle running the app, so we skip app.Run() in Testing
    
    if (!isTesting)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/datainingestor-.txt", rollingInterval: RollingInterval.Day));
    }

    builder.Services.AddDataIngestor(builder.Configuration);

    var app = builder.Build();

    app.UseRouting();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            });
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(result);
        }
    });

    app.MapControllers();

    // Don't run the app if it's being used for integration tests
    // WebApplicationFactory will handle running the app through IHost
    if (!isTesting)
    {
        Log.Information("Starting DataIngestor service...");
        app.Run();
    }
}
catch (Exception ex)
{
    if (!isTesting)
    {
        Log.Fatal(ex, "Application terminated unexpectedly");
    }
    throw; // Re-throw in testing to let WebApplicationFactory handle it
}
finally
{
    if (!isTesting)
    {
        Log.CloseAndFlush();
    }
}