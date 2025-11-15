using DataIngestor.Service.Configuration;
using DataIngestor.Service.HealthChecks;
using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Services;
using DataIngestor.Service.Workers;
using Polly;
using Polly.Extensions.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataIngestor.Service.DependencyInjection;

public static class DataIngestorServiceCollectionExtensions
{
    public static IServiceCollection AddDataIngestor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExternalApiConnectionConfig>(configuration.GetSection("ExternalApi:Connection"));
        services.Configure<ExternalApiRetryConfig>(configuration.GetSection("ExternalApi:Retry"));
        services.Configure<ExternalApiHeadersConfig>(configuration.GetSection("ExternalApi:Headers"));
        services.Configure<QueueConfig>(configuration.GetSection("Queue"));
        services.Configure<DataIngestionConfig>(configuration.GetSection("DataIngestion"));

        services.AddHttpClient<IExternalApiService, ExternalApiService>((provider, client) =>
        {
            var connectionConfig = configuration.GetSection("ExternalApi:Connection");
            client.Timeout = TimeSpan.FromSeconds(connectionConfig.GetValue("TimeoutSeconds", 30));
        })
        .AddPolicyHandler(GetRetryPolicy(configuration))
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddHttpClient<INotificationClient, NotificationClient>();

        services.AddSingleton<IQueueService, RabbitMQService>();
        services.AddSingleton<IExternalApiService, ExternalApiService>();
        services.AddSingleton<ISensorDataProcessor, SensorDataProcessor>();

        services.AddSingleton<IHostedService, DataIngestionWorker>();

        services.AddHealthChecks()
            .AddCheck<ExternalApiHealthCheck>("weakapp-api")
            .AddCheck<QueueHealthCheck>("rabbitmq");

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IConfiguration configuration)
    {
        var retryConfig = configuration.GetSection("ExternalApi:Retry");
        var retryCount = retryConfig.GetValue("RetryCount", 3);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}