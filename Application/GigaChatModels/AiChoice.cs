using System.Text.Json.Serialization;

namespace Application.GigaChatModels;

public class AiChoice
{
    [JsonPropertyName("message")] public AiMessage Message { get; set; } = null!;
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; } = null!;
}