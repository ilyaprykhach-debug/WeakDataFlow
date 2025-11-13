using DataProcessor.Service.Configuration;
using DataProcessor.Service.Data;
using DataProcessor.Service.HealthChecks;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Services;
using DataProcessor.Service.Workers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataProcessor.Service.DependencyInjection;

public static class DataProcessorServiceCollectionExtensions
{
    public static IServiceCollection AddDataProcessor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<QueueConfig>(configuration.GetSection("Queue"));
        services.Configure<DatabaseConfig>(configuration.GetSection("Database"));
        services.Configure<DataProcessingConfig>(configuration.GetSection("DataProcessing"));

        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();
        services.AddDbContext<SensorDataDbContext>(options =>
            options.UseNpgsql(databaseConfig.ConnectionString));

        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddSingleton<IQueueConsumerService, RabbitMQConsumerService>();
        services.AddSingleton<IDataProcessor, DataProcessorService>();

        services.AddSingleton<IHostedService, DataProcessingWorker>();

        services.AddHealthChecks()
            .AddCheck<QueueHealthCheck>("rabbitmq")
            .AddCheck<DatabaseHealthCheck>("postgresql");

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        return services;
    }
}

