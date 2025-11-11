using DataIngestor.Service.Interfaces;
using System.Text.Json;

namespace DataIngestor.Service.Models;

public class WeakApiResponse : IWeakApiResponse
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public JsonElement Payload { get; set; }
}
