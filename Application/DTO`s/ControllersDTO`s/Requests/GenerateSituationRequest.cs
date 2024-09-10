using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class GenerateSituationRequest
{
    [Required]public string Description { get; set; }
    public GenerateSituationRequest(string description) => Description = description;
}