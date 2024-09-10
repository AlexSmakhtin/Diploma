using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Responses;

public record UserGetResponse(string Name, string Email, Statuses Status);