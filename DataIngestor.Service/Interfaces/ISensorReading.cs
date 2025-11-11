namespace DataIngestor.Service.Interfaces;

public interface ISensorReading
{
    string Id { get; set; }
    string SensorId { get; set; }
    string Type { get; set; }
    string Location { get; set; }
    DateTime Timestamp { get; set; }
    decimal? EnergyConsumption { get; set; }
    int? Co2 { get; set; }
    int? Pm25 { get; set; }
    int? Humidity { get; set; }
    bool? MotionDetected { get; set; }

    decimal? NumericValue { get; }
}
