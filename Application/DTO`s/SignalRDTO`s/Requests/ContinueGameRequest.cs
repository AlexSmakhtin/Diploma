using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTO_s.SignalRDTO_s.Requests;

public class ContinueGameRequest
{
    [Required]  public string TimeZone { get; set; }
    [Required]  public string Language { get; set; }
    [Required] public Guid GameId { get; set; }
    [Required] public GameState GameState { get; set; }

    public ContinueGameRequest(string timeZone, string language, Guid gameId, GameState gameState)
    {
        TimeZone = timeZone;
        Language = language;
        GameId = gameId;
        GameState = gameState;
    }
}