using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.SignalRDTO_s.Requests;

public class AcceptChoiceRequest
{
    [Required]public string TimeZone { get; set; }
    [Required] public string Language { get; set; }
    [Required] public string Choice { get; set; }
    [Required] public Guid GameId { get; set; }

    public AcceptChoiceRequest(string timeZone,string language, string choice, Guid gameId)
    {
        TimeZone = timeZone;
        Language = language;
        Choice = choice;
        GameId = gameId;
    }
}