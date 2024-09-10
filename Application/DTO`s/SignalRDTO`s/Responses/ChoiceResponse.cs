using System.Text.Json.Serialization;

namespace Application.DTO_s.SignalRDTO_s.Responses;

public class ChoiceResponse
{
    [JsonPropertyName("answers")] public List<string> Answers { get; set; } = [];
}