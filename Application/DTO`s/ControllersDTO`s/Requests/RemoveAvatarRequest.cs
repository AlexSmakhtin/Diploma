using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class RemoveAvatarRequest
{
    [Required]public Guid GameId { get; set; }
    public RemoveAvatarRequest(Guid gameId) => GameId = gameId;
}