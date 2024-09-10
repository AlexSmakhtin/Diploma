using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class GetAvatarRequest
{
    [Required]public string AvatarId { get; set; }
    public GetAvatarRequest(string avatarId) => AvatarId = avatarId;
}