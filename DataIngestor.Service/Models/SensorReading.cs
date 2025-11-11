using DataIngestor.Service.Interfaces;
using System.Text.Json;

namespace DataIngestor.Service.Models;

public class SensorReading : ISensorReading
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

    public decimal? NumericValue
    {
        get
        {
            return Type switch
            {
                "energy" => EnergyConsumption,
                "air_quality" => Co2 ?? Pm25 ?? Humidity,
                _ => null
            };
        }
    }

    public static SensorReading FromWeakApiResponse(IWeakApiResponse response)
    {
        var reading = new SensorReading
        {
            Type = response.Type,
            Location = response.Name,
            Timestamp = DateTime.UtcNow,
            SensorId = $"{response.Type}_{response.Name.Replace(" ", "_")}"
        };

        switch (response.Type)
        {
            case "energy":
                if (response.Payload.TryGetProperty("energy", out var energyValue) &&
                    energyValue.ValueKind == JsonValueKind.Number)
                {
                    reading.EnergyConsumption = energyValue.GetDecimal();
                }
                break;

            case "air_quality":
                if (response.Payload.TryGetProperty("co2", out var co2Value) &&
                    co2Value.ValueKind == JsonValueKind.Number)
                {
                    reading.Co2 = co2Value.GetInt32();
                }
                if (response.Payload.TryGetProperty("pm25", out var pm25Value) &&
                    pm25Value.ValueKind == JsonValueKind.Number)
                {
                    reading.Pm25 = pm25Value.GetInt32();
                }
                if (response.Payload.TryGetProperty("humidity", out var humidityValue) &&
                    humidityValue.ValueKind == JsonValueKind.Number)
                {
                    reading.Humidity = humidityValue.GetInt32();
                }
                break;

            case "motion":
                if (response.Payload.TryGetProperty("motionDetected", out var motionValue) &&
                    motionValue.ValueKind == JsonValueKind.True || motionValue.ValueKind == JsonValueKind.False)
                {
                    reading.MotionDetected = motionValue.GetBoolean();
                }
                break;
        }

        return reading;
    }
}
