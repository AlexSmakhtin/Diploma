using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class UserUpdateRequest
{
    [Required] public string Name { get; set; }
    [EmailAddress, Required] public string Email { get; set; }

    public UserUpdateRequest(string name, string email)
    {
        Name = name;
        Email = email;
    }
}