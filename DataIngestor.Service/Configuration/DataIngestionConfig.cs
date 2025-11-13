namespace DataIngestor.Service.Configuration;

public class DataIngestionConfig
{
    public int IntervalSeconds { get; set; } = 15;
    public int InitialDelaySeconds { get; set; } = 5;
}

