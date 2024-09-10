using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Responses;

public record SignInResponse(Guid Id,string Name, string Email, string JwtToken, Statuses Status);