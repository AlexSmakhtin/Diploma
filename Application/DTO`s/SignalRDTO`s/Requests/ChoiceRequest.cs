using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.SignalRDTO_s.Requests;

public class ChoiceRequest
{
    [Required] public string Choice { get; set; }
    [Required] public Guid GameId { get; set; }
    public ChoiceRequest(string choice, Guid gameId) 
        => (Choice, GameId) = (choice, gameId);
}