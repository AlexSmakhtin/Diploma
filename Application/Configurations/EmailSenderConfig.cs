using System.ComponentModel.DataAnnotations;

namespace Application.Configurations;

public class EmailSenderConfig
{
    [Required] public int Port { get; set; }
    [Required] public string Host { get; set; } = null!;
    [Required] public string Login { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}