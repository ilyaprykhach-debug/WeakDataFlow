namespace DataProcessor.Service.Configuration;

public class DataProcessingConfig
{
    public int BatchSize { get; set; } = 10;
    public int ProcessingIntervalSeconds { get; set; } = 5;
    public int InitialDelaySeconds { get; set; } = 2;
}


