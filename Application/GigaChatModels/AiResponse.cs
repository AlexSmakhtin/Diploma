using System.Text.Json.Serialization;

namespace Application.GigaChatModels;

public class AiResponse
{
    [JsonPropertyName("choices")] public List<AiChoice> Choices { get; set; } = [];
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("model")] public string Model { get; set; } = null!;
    [JsonPropertyName("usage")] public AiUsage AiUsage { get; set; } = null!;
    [JsonPropertyName("object")] public string Object { get; set; } = null!;
}