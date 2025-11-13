namespace DataIngestor.Service.Configuration;

public class ExternalApiConnectionConfig
{
    public string BaseUrl { get; set; } = "http://weakapp:8080";
    public int TimeoutSeconds { get; set; } = 30;
}

