using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Responses;

public record SignUpResponse(Guid Id, string Name, string Email, string JwtToken, Statuses Status);