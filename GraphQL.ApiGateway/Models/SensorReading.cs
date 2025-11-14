namespace GraphQL.ApiGateway.Models;

public class SensorReading
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SensorId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal? EnergyConsumption { get; set; }
    public int? Co2 { get; set; }
    public int? Pm25 { get; set; }
    public int? Humidity { get; set; }
    public bool? MotionDetected { get; set; }
}

