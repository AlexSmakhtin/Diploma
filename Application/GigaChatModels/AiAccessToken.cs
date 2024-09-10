using System.Text.Json.Serialization;

namespace Application.GigaChatModels;

public class AiAccessToken
{
    [JsonPropertyName("access_token")] public string Token { get; set; } = null!;
    [JsonPropertyName("expires_at")] public long ExpireAt { get; set; }
}