using System.Text.Json.Serialization;

namespace Application.GigaChatModels;

public class AiMessage
{
    [JsonPropertyName("role")] public string Role { get; set; } = null!;
    [JsonPropertyName("content")] public string Content { get; set; } = null!;
}