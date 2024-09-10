using System.Text.Json.Serialization;

namespace Application.GigaChatModels;

public class AiRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = null!;
    [JsonPropertyName("messages")] public List<AiMessage> Messages { get; set; } = [];

    [JsonIgnore]
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("top_p")] public float TopP { get; set; } = 0.9f;
    [JsonPropertyName("n")] public int AnswersCount { get; set; } = 1;
    [JsonPropertyName("stream")] public bool IsStream { get; set; } = false;
    [JsonPropertyName("update_interval")] public int UpdateInterval { get; set; } = 0;
    [JsonPropertyName("function_call")] public string FunctionCall { get; set; } = "auto";
}