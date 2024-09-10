using System.ComponentModel.DataAnnotations;

namespace Application.Configurations;

public class ConnectionStrings
{
    [Required] public string PostgreSql { get; set; } = null!;
}