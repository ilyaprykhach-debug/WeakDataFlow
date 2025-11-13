namespace DataIngestor.Service.Configuration;

public class QueueConfig
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string QueueName { get; set; } = "sensor-data";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

