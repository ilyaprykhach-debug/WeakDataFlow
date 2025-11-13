using DataProcessor.Service.Data;
using DataProcessor.Service.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net.Mime;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

try
{
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
            .WriteTo.File("logs/dataprocessor-.txt", rollingInterval: RollingInterval.Day));
    }

    builder.Services.AddDataProcessor(builder.Configuration);

    var app = builder.Build();

    // Apply database migrations
    if (!isTesting)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();
            try
            {
                dbContext.Database.Migrate();
                Log.Information("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply database migrations");
                throw;
            }
        }
    }

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

    if (!isTesting)
    {
        Log.Information("Starting DataProcessor service...");
        app.Run();
    }
}
catch (Exception ex)
{
    if (!isTesting)
    {
        Log.Fatal(ex, "Application terminated unexpectedly");
    }
    throw;
}
finally
{
    if (!isTesting)
    {
        Log.CloseAndFlush();
    }
}
