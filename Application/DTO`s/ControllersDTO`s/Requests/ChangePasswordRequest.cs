using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class ChangePasswordRequest
{
    [Required] public string OldPassword { get; set; }
    [Required] public string NewPassword { get; set; }

    public ChangePasswordRequest(string oldPassword, string newPassword)
    {
        OldPassword = oldPassword;
        NewPassword = newPassword;
    }
}