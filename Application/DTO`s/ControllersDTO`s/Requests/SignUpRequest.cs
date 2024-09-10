using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class SignUpRequest
{
    [Required] public string Name { get; set; }
    [EmailAddress, Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
    [Required] public Statuses Status { get; set; }
    [Required] public DateTime Birthday { get; set; }

    public SignUpRequest(
        string name,
        string email,
        string password,
        Statuses status,
        DateTime birthday) => (Name, Email, Password, Status, Birthday) =
        (name, email, password, status, birthday);
}