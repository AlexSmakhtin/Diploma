using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class GenerateImageRequest
{
    [Required] public string Description { get; set; }
    [Required]  public Guid GameId { get; set; }
    public GenerateImageRequest(string description, Guid gameId) => (Description, GameId) = (description, gameId);
}