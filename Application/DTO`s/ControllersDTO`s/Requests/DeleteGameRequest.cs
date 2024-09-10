using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class DeleteGameRequest
{
    [Required] public Guid GameId { get; set; }
    public DeleteGameRequest(Guid gameId) => GameId = gameId;
}