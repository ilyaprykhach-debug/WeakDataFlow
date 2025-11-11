using System.Text.Json;

namespace DataIngestor.Service.Interfaces;

public interface IWeakApiResponse
{
    string Type { get; set; }
    string Name { get; set; }
    JsonElement Payload { get; set; }
}
