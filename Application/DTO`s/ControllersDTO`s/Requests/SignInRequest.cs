using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class SignInRequest
{
    [EmailAddress, Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
    public SignInRequest(string email, string password) => (Email, Password) = (email, password);
}