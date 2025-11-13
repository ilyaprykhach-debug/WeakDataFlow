using System.Text.Json;

namespace DataIngestor.Service.Models;

public class WeakApiResponse
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public JsonElement Payload { get; set; }
}
