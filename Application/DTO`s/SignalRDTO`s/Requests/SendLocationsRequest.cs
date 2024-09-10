using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.SignalRDTO_s.Requests;

public class SendLocationsRequest
{
    [Required]public string TimeZone { get; set; }
    [Required] public string Language { get; set; }
    [Required] public Guid GameId { get; set; }

    public SendLocationsRequest(string timeZone,string language, Guid gameId)
    {
        TimeZone = timeZone;
        Language = language;
        GameId = gameId;
    }
}