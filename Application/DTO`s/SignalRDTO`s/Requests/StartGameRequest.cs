using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.SignalRDTO_s.Requests;

public class StartGameRequest
{
    [Required]public string TimeZone { get; set; }
    [Required] public Guid GameId { get; set; }
    [Required] public string Location { get; set; }
    [Required] public string Language { get; set; }
    [Required] public string CharName { get; set; }

    public StartGameRequest(string timeZone, Guid gameId, string location, string language, string charName)
    {
        TimeZone = timeZone;
        GameId = gameId;
        Location = location;
        Language = language;
        CharName = charName;
    }
}