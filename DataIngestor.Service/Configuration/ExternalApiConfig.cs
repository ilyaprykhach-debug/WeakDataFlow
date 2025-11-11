namespace DataIngestor.Service.Configuration;

public class ExternalApiConfig
{
    public string BaseUrl { get; set; } = "http://weakapp:8080";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public ApiHeaders Headers { get; set; } = new();
}

public class ApiHeaders
{
    public string XApiKey { get; set; } = "supersecret";
}

public class QueueConfig
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string QueueName { get; set; } = "sensor-data";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

public class DataIngestionConfig
{
    public int IntervalSeconds { get; set; } = 15;
    public int InitialDelaySeconds { get; set; } = 5;
}
