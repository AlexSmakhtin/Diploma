using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Responses;

public record UserUpdateResponse(string Name, string Email, Statuses Status, string JwtToken);