using System.ComponentModel.DataAnnotations;

namespace Application.Configurations;

public class GigaChatData
{
    [Required] public string Base64AuthData { get; set; } = null!;
    [Required] public string AuthUrl { get; set; } = null!;
    [Required] public string ChatUrl { get; set; } = null!;
}